using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto?> GetCustomerByUserIdAsync(int userId);
    Task<List<CustomerDto>> SearchCustomersAsync(string query);
    Task<CustomerDto?> GetCustomerByVehicleNumberAsync(string vehicleNumber);
    Task<bool> AddVehicleAsync(int customerId, CreateVehicleRequest request);
    Task<List<SalesInvoiceDto>> GetCustomerHistoryAsync(int customerId);
    Task<AiPredictionDto?> GetAiPredictionAsync(int customerId);
}

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db) => _db = db;

    public async Task<List<CustomerDto>> GetAllCustomersAsync()
    {
        return await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var c = await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);
        return c == null ? null : MapToDto(c);
    }

    public async Task<CustomerDto?> GetCustomerByUserIdAsync(int userId)
    {
        var c = await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        return c == null ? null : MapToDto(c);
    }

    public async Task<List<CustomerDto>> SearchCustomersAsync(string query)
    {
        var lower = query.ToLower();
        return await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Where(c =>
                c.User.FullName.ToLower().Contains(lower) ||
                c.User.Phone.Contains(query) ||
                c.User.Email.ToLower().Contains(lower) ||
                c.Id.ToString() == query)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetCustomerByVehicleNumberAsync(string vehicleNumber)
    {
        var vehicle = await _db.Vehicles
            .Include(v => v.Customer)
            .ThenInclude(c => c.User)
            .Include(v => v.Customer)
            .ThenInclude(c => c.Vehicles)
            .FirstOrDefaultAsync(v => v.VehicleNumber.ToLower() == vehicleNumber.ToLower());

        return vehicle == null ? null : MapToDto(vehicle.Customer);
    }

    public async Task<bool> AddVehicleAsync(int customerId, CreateVehicleRequest request)
    {
        var customer = await _db.Customers.FindAsync(customerId);
        if (customer == null) return false;

        _db.Vehicles.Add(new Vehicle
        {
            CustomerId = customerId,
            VehicleNumber = request.VehicleNumber,
            Make = request.Make,
            Model = request.VehicleModel,
            Year = request.Year,
            FuelType = request.FuelType,
            Mileage = request.Mileage,
            LastServiceDate = request.LastServiceDate,
            Notes = request.Notes
        });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<SalesInvoiceDto>> GetCustomerHistoryAsync(int customerId)
    {
        return await _db.SalesInvoices
            .Include(si => si.Items).ThenInclude(i => i.Part)
            .Include(si => si.Staff).ThenInclude(s => s.User)
            .Include(si => si.Customer).ThenInclude(c => c.User)
            .Where(si => si.CustomerId == customerId)
            .OrderByDescending(si => si.InvoiceDate)
            .Select(si => new SalesInvoiceDto
            {
                Id = si.Id,
                InvoiceNumber = si.InvoiceNumber,
                CustomerName = si.Customer.User.FullName,
                StaffName = si.Staff.User.FullName,
                SubTotal = si.SubTotal,
                DiscountAmount = si.DiscountAmount,
                TotalAmount = si.TotalAmount,
                IsCreditSale = si.IsCreditSale,
                IsPaid = si.IsPaid,
                InvoiceDate = si.InvoiceDate,
                Items = si.Items.Select(i => new SalesInvoiceItemDto
                {
                    PartName = i.Part.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.Quantity * i.UnitPrice
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<AiPredictionDto?> GetAiPredictionAsync(int customerId)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.CustomerId == customerId)
            .ToListAsync();

        if (!vehicles.Any()) return null;

        var vehicle = vehicles.First();
        var predictions = new List<string>();
        var riskLevel = "Low";

        // AI logic based on mileage, age, and service history
        var age = DateTime.Now.Year - vehicle.Year;
        var daysSinceService = (DateTime.Now - vehicle.LastServiceDate).Days;

        if (vehicle.Mileage > 80000)
            predictions.Add("Timing belt replacement recommended");
        if (vehicle.Mileage > 50000)
            predictions.Add("Brake pad inspection needed");
        if (daysSinceService > 180)
            predictions.Add("Oil change overdue");
        if (age > 5)
            predictions.Add("Battery health check recommended");
        if (vehicle.Mileage > 100000)
            predictions.Add("Transmission fluid change needed");
        if (daysSinceService > 365)
            predictions.Add("Full service inspection required");

        if (predictions.Count >= 4) riskLevel = "High";
        else if (predictions.Count >= 2) riskLevel = "Medium";

        if (!predictions.Any())
            predictions.Add("Vehicle is in good condition. Keep up with regular maintenance.");

        return new AiPredictionDto
        {
            VehicleNumber = vehicle.VehicleNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            PredictedFailures = predictions,
            RiskLevel = riskLevel,
            Recommendation = riskLevel == "High"
                ? "Immediate service visit recommended."
                : riskLevel == "Medium"
                    ? "Schedule a service appointment soon."
                    : "Continue regular maintenance schedule."
        };
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id,
        UserId = c.UserId,
        FullName = c.User?.FullName ?? "",
        Email = c.User?.Email ?? "",
        Phone = c.User?.Phone ?? "",
        Address = c.Address,
        TotalSpent = c.TotalSpent,
        CreditBalance = c.CreditBalance,
        CreditDueDate = c.CreditDueDate,
        Vehicles = c.Vehicles?.Select(v => new VehicleDto
        {
            Id = v.Id,
            VehicleNumber = v.VehicleNumber,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            FuelType = v.FuelType,
            Mileage = v.Mileage,
            LastServiceDate = v.LastServiceDate,
            Notes = v.Notes
        }).ToList() ?? new()
    };
}
