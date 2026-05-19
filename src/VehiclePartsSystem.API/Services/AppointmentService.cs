using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

public class AppointmentRequest
{
    public int CustomerId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class AppointmentDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(AppointmentRequest request);
    Task<List<AppointmentDto>> GetAllAsync();
    Task<List<AppointmentDto>> GetByCustomerAsync(int customerId);
    Task<bool> UpdateStatusAsync(int id, string status);
}

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;

    public AppointmentService(AppDbContext db) => _db = db;

    public async Task<AppointmentDto> CreateAsync(AppointmentRequest request)
    {
        var appt = new Appointment
        {
            CustomerId = request.CustomerId,
            ServiceType = request.ServiceType,
            AppointmentDate = request.AppointmentDate,
            Notes = request.Notes
        };
        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();
        return await MapAsync(appt.Id);
    }

    public async Task<List<AppointmentDto>> GetAllAsync()
    {
        return await _db.Appointments
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer.User.FullName,
                ServiceType = a.ServiceType,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status.ToString(),
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            }).ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetByCustomerAsync(int customerId)
    {
        return await _db.Appointments
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer.User.FullName,
                ServiceType = a.ServiceType,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status.ToString(),
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            }).ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var appt = await _db.Appointments.FindAsync(id);
        if (appt == null) return false;
        if (Enum.TryParse<AppointmentStatus>(status, true, out var parsed))
            appt.Status = parsed;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<AppointmentDto> MapAsync(int id)
    {
        var a = await _db.Appointments
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .FirstAsync(a => a.Id == id);
        return new AppointmentDto
        {
            Id = a.Id,
            CustomerId = a.CustomerId,
            CustomerName = a.Customer.User.FullName,
            ServiceType = a.ServiceType,
            AppointmentDate = a.AppointmentDate,
            Status = a.Status.ToString(),
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        };
    }
}
