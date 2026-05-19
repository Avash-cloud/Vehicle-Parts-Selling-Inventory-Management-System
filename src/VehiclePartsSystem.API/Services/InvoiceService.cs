using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

/// <summary>
/// Manages sales and purchase invoice operations including creation,
/// retrieval, email dispatch, and stock updates.
/// </summary>
public interface IInvoiceService
{
    Task<SalesInvoiceDto?> CreateSalesInvoiceAsync(CreateSalesInvoiceRequest request, int staffUserId);
    Task<List<SalesInvoiceDto>> GetAllSalesInvoicesAsync();
    Task<SalesInvoiceDto?> GetSalesInvoiceByIdAsync(int id);
    Task<bool> EmailSalesInvoiceAsync(int invoiceId);
    Task<PurchaseInvoiceDto?> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceRequest request);
    Task<List<PurchaseInvoiceDto>> GetAllPurchaseInvoicesAsync();
    Task<PurchaseInvoiceDto?> GetPurchaseInvoiceByIdAsync(int id);
}

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(AppDbContext db, IEmailService email, IConfiguration config, ILogger<InvoiceService> logger)
    {
        _db     = db;
        _email  = email;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Creates a sales invoice. Applies 10% loyalty discount if subtotal exceeds Rs. 5000 (Feature 16).
    /// Deducts stock and triggers low-stock notifications (Feature 15).
    /// </summary>
    public async Task<SalesInvoiceDto?> CreateSalesInvoiceAsync(CreateSalesInvoiceRequest request, int staffUserId)
    {
        // Validate staff exists
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.UserId == staffUserId);
        if (staff == null)
        {
            _logger.LogWarning("Sales invoice creation failed: staff user ID {Id} not found.", staffUserId);
            return null;
        }

        // Validate customer exists
        var customer = await _db.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId);
        if (customer == null)
            throw new InvalidOperationException($"Customer ID {request.CustomerId} not found.");

        if (!request.Items.Any())
            throw new InvalidOperationException("Invoice must contain at least one item.");

        decimal subTotal   = 0;
        var invoiceItems   = new List<SalesInvoiceItem>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException("Item quantity must be greater than zero.");

            var part = await _db.Parts.FindAsync(item.PartId);
            if (part == null)
                throw new InvalidOperationException($"Part ID {item.PartId} not found.");

            if (part.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{part.Name}'. Available: {part.StockQuantity}, Requested: {item.Quantity}.");

            var lineTotal = part.SellingPrice * item.Quantity;
            subTotal += lineTotal;

            invoiceItems.Add(new SalesInvoiceItem
            {
                PartId     = item.PartId,
                Quantity   = item.Quantity,
                UnitPrice  = part.SellingPrice,
                TotalPrice = lineTotal
            });

            // Deduct stock
            part.StockQuantity -= item.Quantity;
            part.UpdatedAt      = DateTime.UtcNow;

            // Feature 15: Low stock notification
            if (part.StockQuantity < part.ReorderLevel)
            {
                _logger.LogWarning("Low stock after sale: '{Name}' now has {Stock} units.", part.Name, part.StockQuantity);
                _db.Notifications.Add(new Notification
                {
                    Title   = "Low Stock Alert",
                    Message = $"{part.Name} has only {part.StockQuantity} units left after sale.",
                    Type    = "LowStock"
                });
                var adminEmail = _config["Email:AdminEmail"] ?? "admin@vehicleparts.com";
                await _email.SendLowStockAlertAsync(adminEmail, part.Name, part.StockQuantity);
            }
        }

        // Feature 16: Loyalty discount — 10% if subtotal > Rs. 5000
        decimal discount = subTotal > 5000m ? Math.Round(subTotal * 0.10m, 2) : 0m;
        decimal total    = subTotal - discount;

        if (discount > 0)
            _logger.LogInformation("Loyalty discount of Rs. {Discount} applied for customer ID {CustomerId}.",
                discount, request.CustomerId);

        var invoiceNumber = $"SI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var invoice = new SalesInvoice
        {
            InvoiceNumber  = invoiceNumber,
            CustomerId     = request.CustomerId,
            StaffId        = staff.Id,
            SubTotal       = subTotal,
            DiscountAmount = discount,
            TotalAmount    = total,
            IsCreditSale   = request.IsCreditSale,
            IsPaid         = !request.IsCreditSale,
            PaymentDueDate = request.IsCreditSale ? DateTime.UtcNow.AddDays(30) : null,
            Notes          = request.Notes,
            Items          = invoiceItems
        };

        _db.SalesInvoices.Add(invoice);

        // Update customer totals
        customer.TotalSpent += total;
        if (request.IsCreditSale)
        {
            customer.CreditBalance += total;
            customer.CreditDueDate  = DateTime.UtcNow.AddDays(30);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Sales invoice {Number} created. Total: Rs. {Total}.", invoiceNumber, total);

        return await GetSalesInvoiceByIdAsync(invoice.Id);
    }

    public async Task<List<SalesInvoiceDto>> GetAllSalesInvoicesAsync() =>
        await _db.SalesInvoices
            .Include(si => si.Items).ThenInclude(i => i.Part)
            .Include(si => si.Staff).ThenInclude(s => s.User)
            .Include(si => si.Customer).ThenInclude(c => c.User)
            .OrderByDescending(si => si.InvoiceDate)
            .Select(si => MapSalesDto(si))
            .ToListAsync();

    public async Task<SalesInvoiceDto?> GetSalesInvoiceByIdAsync(int id)
    {
        var si = await _db.SalesInvoices
            .Include(si => si.Items).ThenInclude(i => i.Part)
            .Include(si => si.Staff).ThenInclude(s => s.User)
            .Include(si => si.Customer).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(si => si.Id == id);
        return si == null ? null : MapSalesDto(si);
    }

    /// <summary>Feature 11: Emails a sales invoice to the customer.</summary>
    public async Task<bool> EmailSalesInvoiceAsync(int invoiceId)
    {
        var invoice = await _db.SalesInvoices
            .Include(si => si.Customer).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(si => si.Id == invoiceId);

        if (invoice == null) return false;

        await _email.SendInvoiceEmailAsync(
            invoice.Customer.User.Email,
            invoice.Customer.User.FullName,
            invoice.InvoiceNumber,
            invoice.TotalAmount);

        _logger.LogInformation("Invoice {Number} emailed to {Email}.", invoice.InvoiceNumber, invoice.Customer.User.Email);
        return true;
    }

    /// <summary>Creates a purchase invoice and increases stock quantities.</summary>
    public async Task<PurchaseInvoiceDto?> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceRequest request)
    {
        var vendor = await _db.Vendors.FindAsync(request.VendorId);
        if (vendor == null)
        {
            _logger.LogWarning("Purchase invoice creation failed: vendor ID {Id} not found.", request.VendorId);
            return null;
        }

        decimal total = 0;
        var items     = new List<PurchaseInvoiceItem>();

        foreach (var item in request.Items)
        {
            var part = await _db.Parts.FindAsync(item.PartId);
            if (part == null) continue;

            var lineCost = item.Quantity * item.UnitCost;
            total += lineCost;

            // Increase stock on purchase
            part.StockQuantity += item.Quantity;
            part.UpdatedAt      = DateTime.UtcNow;

            items.Add(new PurchaseInvoiceItem
            {
                PartId    = item.PartId,
                Quantity  = item.Quantity,
                UnitCost  = item.UnitCost,
                TotalCost = lineCost
            });
        }

        var invoiceNumber = $"PI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var invoice = new PurchaseInvoice
        {
            InvoiceNumber = invoiceNumber,
            VendorId      = request.VendorId,
            TotalAmount   = total,
            Notes         = request.Notes,
            Items         = items
        };

        _db.PurchaseInvoices.Add(invoice);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Purchase invoice {Number} created. Total: Rs. {Total}.", invoiceNumber, total);
        return await GetPurchaseInvoiceByIdAsync(invoice.Id);
    }

    public async Task<List<PurchaseInvoiceDto>> GetAllPurchaseInvoicesAsync() =>
        await _db.PurchaseInvoices
            .Include(pi => pi.Items).ThenInclude(i => i.Part)
            .Include(pi => pi.Vendor)
            .OrderByDescending(pi => pi.InvoiceDate)
            .Select(pi => MapPurchaseDto(pi))
            .ToListAsync();

    public async Task<PurchaseInvoiceDto?> GetPurchaseInvoiceByIdAsync(int id)
    {
        var pi = await _db.PurchaseInvoices
            .Include(pi => pi.Items).ThenInclude(i => i.Part)
            .Include(pi => pi.Vendor)
            .FirstOrDefaultAsync(pi => pi.Id == id);
        return pi == null ? null : MapPurchaseDto(pi);
    }

    private static SalesInvoiceDto MapSalesDto(SalesInvoice si) => new()
    {
        Id             = si.Id,
        InvoiceNumber  = si.InvoiceNumber,
        CustomerName   = si.Customer?.User?.FullName ?? "",
        StaffName      = si.Staff?.User?.FullName ?? "",
        SubTotal       = si.SubTotal,
        DiscountAmount = si.DiscountAmount,
        TotalAmount    = si.TotalAmount,
        IsCreditSale   = si.IsCreditSale,
        IsPaid         = si.IsPaid,
        InvoiceDate    = si.InvoiceDate,
        Items          = si.Items?.Select(i => new SalesInvoiceItemDto
        {
            PartName   = i.Part?.Name ?? "",
            Quantity   = i.Quantity,
            UnitPrice  = i.UnitPrice,
            TotalPrice = i.TotalPrice
        }).ToList() ?? new()
    };

    private static PurchaseInvoiceDto MapPurchaseDto(PurchaseInvoice pi) => new()
    {
        Id            = pi.Id,
        InvoiceNumber = pi.InvoiceNumber,
        VendorName    = pi.Vendor?.Name ?? "",
        TotalAmount   = pi.TotalAmount,
        PaymentStatus = pi.PaymentStatus.ToString(),
        InvoiceDate   = pi.InvoiceDate,
        Items         = pi.Items?.Select(i => new PurchaseInvoiceItemDto
        {
            PartName  = i.Part?.Name ?? "",
            Quantity  = i.Quantity,
            UnitCost  = i.UnitCost,
            TotalCost = i.TotalCost
        }).ToList() ?? new()
    };
}
