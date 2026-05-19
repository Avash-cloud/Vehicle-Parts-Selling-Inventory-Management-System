using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

/// <summary>
/// Handles all vehicle parts inventory operations including CRUD,
/// stock management, and low-stock notifications.
/// </summary>
public interface IPartService
{
    /// <summary>Returns all active parts, optionally filtered by search term or category.</summary>
    Task<List<PartDto>> GetAllPartsAsync(string? search = null, string? category = null);

    /// <summary>Returns a single part by its ID, or null if not found.</summary>
    Task<PartDto?> GetPartByIdAsync(int id);

    /// <summary>Creates a new part and persists it to the database.</summary>
    Task<PartDto> CreatePartAsync(CreatePartRequest request);

    /// <summary>Updates an existing part. Returns null if the part does not exist.</summary>
    Task<PartDto?> UpdatePartAsync(int id, UpdatePartRequest request);

    /// <summary>Soft-deletes a part by setting IsActive = false.</summary>
    Task<bool> DeletePartAsync(int id);

    /// <summary>Returns all parts whose stock is below their reorder level.</summary>
    Task<List<PartDto>> GetLowStockPartsAsync();
}

public class PartService : IPartService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<PartService> _logger;

    public PartService(AppDbContext db, IEmailService email, IConfiguration config, ILogger<PartService> logger)
    {
        _db     = db;
        _email  = email;
        _config = config;
        _logger = logger;
    }

    public async Task<List<PartDto>> GetAllPartsAsync(string? search = null, string? category = null)
    {
        var query = _db.Parts.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.PartNumber.Contains(search));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        return await query.Select(p => MapToDto(p)).ToListAsync();
    }

    public async Task<PartDto?> GetPartByIdAsync(int id)
    {
        var part = await _db.Parts.FindAsync(id);
        return part == null ? null : MapToDto(part);
    }

    public async Task<PartDto> CreatePartAsync(CreatePartRequest request)
    {
        // Validate selling price is not less than cost price
        if (request.SellingPrice < request.CostPrice)
            _logger.LogWarning("Part '{Name}' has selling price lower than cost price.", request.Name);

        var part = new Part
        {
            Name          = request.Name.Trim(),
            PartNumber    = request.PartNumber.Trim(),
            Category      = request.Category,
            Description   = request.Description,
            CostPrice     = request.CostPrice,
            SellingPrice  = request.SellingPrice,
            StockQuantity = request.StockQuantity,
            ReorderLevel  = request.ReorderLevel
        };

        _db.Parts.Add(part);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Part '{Name}' (ID: {Id}) created successfully.", part.Name, part.Id);
        return MapToDto(part);
    }

    public async Task<PartDto?> UpdatePartAsync(int id, UpdatePartRequest request)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part == null)
        {
            _logger.LogWarning("Attempted to update non-existent part ID: {Id}", id);
            return null;
        }

        part.Name          = request.Name.Trim();
        part.PartNumber    = request.PartNumber.Trim();
        part.Category      = request.Category;
        part.Description   = request.Description;
        part.CostPrice     = request.CostPrice;
        part.SellingPrice  = request.SellingPrice;
        part.StockQuantity = request.StockQuantity;
        part.ReorderLevel  = request.ReorderLevel;
        part.IsActive      = request.IsActive;
        part.UpdatedAt     = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Feature 15: Auto-notify admin when stock falls below reorder level
        if (part.StockQuantity < part.ReorderLevel)
        {
            _logger.LogWarning("Low stock alert: '{Name}' has {Stock} units (reorder level: {Level}).",
                part.Name, part.StockQuantity, part.ReorderLevel);

            var adminEmail = _config["Email:AdminEmail"] ?? "admin@vehicleparts.com";
            await _email.SendLowStockAlertAsync(adminEmail, part.Name, part.StockQuantity);

            _db.Notifications.Add(new Notification
            {
                Title   = "Low Stock Alert",
                Message = $"{part.Name} has only {part.StockQuantity} units remaining (reorder level: {part.ReorderLevel}).",
                Type    = "LowStock"
            });
            await _db.SaveChangesAsync();
        }

        return MapToDto(part);
    }

    public async Task<bool> DeletePartAsync(int id)
    {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return false;

        // Soft delete — preserves historical invoice data integrity
        part.IsActive  = false;
        part.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Part ID {Id} soft-deleted.", id);
        return true;
    }

    public async Task<List<PartDto>> GetLowStockPartsAsync() =>
        await _db.Parts
            .Where(p => p.IsActive && p.StockQuantity < p.ReorderLevel)
            .Select(p => MapToDto(p))
            .ToListAsync();

    /// <summary>Maps a Part entity to its DTO representation.</summary>
    private static PartDto MapToDto(Part p) => new()
    {
        Id            = p.Id,
        Name          = p.Name,
        PartNumber    = p.PartNumber,
        Category      = p.Category,
        Description   = p.Description,
        CostPrice     = p.CostPrice,
        SellingPrice  = p.SellingPrice,
        StockQuantity = p.StockQuantity,
        ReorderLevel  = p.ReorderLevel,
        IsActive      = p.IsActive
    };
}
