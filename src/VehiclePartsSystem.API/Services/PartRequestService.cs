using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

public class PartRequestCreateDto
{
    public int CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PartRequestDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IPartRequestService
{
    Task<PartRequestDto> CreateAsync(PartRequestCreateDto request);
    Task<List<PartRequestDto>> GetAllAsync();
    Task<List<PartRequestDto>> GetByCustomerAsync(int customerId);
    Task<bool> UpdateStatusAsync(int id, string status);
}

public class PartRequestService : IPartRequestService
{
    private readonly AppDbContext _db;

    public PartRequestService(AppDbContext db) => _db = db;

    public async Task<PartRequestDto> CreateAsync(PartRequestCreateDto request)
    {
        var pr = new PartRequest
        {
            CustomerId = request.CustomerId,
            PartName = request.PartName,
            Description = request.Description
        };
        _db.PartRequests.Add(pr);
        await _db.SaveChangesAsync();

        var customer = await _db.Customers.Include(c => c.User).FirstAsync(c => c.Id == request.CustomerId);
        return new PartRequestDto
        {
            Id = pr.Id,
            CustomerName = customer.User.FullName,
            PartName = pr.PartName,
            Description = pr.Description,
            Status = pr.Status.ToString(),
            CreatedAt = pr.CreatedAt
        };
    }

    public async Task<List<PartRequestDto>> GetAllAsync()
    {
        return await _db.PartRequests
            .Include(pr => pr.Customer).ThenInclude(c => c.User)
            .OrderByDescending(pr => pr.CreatedAt)
            .Select(pr => new PartRequestDto
            {
                Id = pr.Id,
                CustomerName = pr.Customer.User.FullName,
                PartName = pr.PartName,
                Description = pr.Description,
                Status = pr.Status.ToString(),
                CreatedAt = pr.CreatedAt
            }).ToListAsync();
    }

    public async Task<List<PartRequestDto>> GetByCustomerAsync(int customerId)
    {
        return await _db.PartRequests
            .Include(pr => pr.Customer).ThenInclude(c => c.User)
            .Where(pr => pr.CustomerId == customerId)
            .OrderByDescending(pr => pr.CreatedAt)
            .Select(pr => new PartRequestDto
            {
                Id = pr.Id,
                CustomerName = pr.Customer.User.FullName,
                PartName = pr.PartName,
                Description = pr.Description,
                Status = pr.Status.ToString(),
                CreatedAt = pr.CreatedAt
            }).ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var pr = await _db.PartRequests.FindAsync(id);
        if (pr == null) return false;
        if (Enum.TryParse<PartRequestStatus>(status, true, out var parsed))
            pr.Status = parsed;
        await _db.SaveChangesAsync();
        return true;
    }
}
