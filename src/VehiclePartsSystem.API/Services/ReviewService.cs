using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

public class ReviewRequest
{
    public int CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ReviewDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(ReviewRequest request);
    Task<List<ReviewDto>> GetAllAsync();
}

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db) => _db = db;

    public async Task<ReviewDto> CreateAsync(ReviewRequest request)
    {
        var review = new Review
        {
            CustomerId = request.CustomerId,
            Rating = Math.Clamp(request.Rating, 1, 5),
            Comment = request.Comment
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var customer = await _db.Customers.Include(c => c.User).FirstAsync(c => c.Id == request.CustomerId);
        return new ReviewDto
        {
            Id = review.Id,
            CustomerName = customer.User.FullName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<List<ReviewDto>> GetAllAsync()
    {
        return await _db.Reviews
            .Include(r => r.Customer).ThenInclude(c => c.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                CustomerName = r.Customer.User.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToListAsync();
    }
}
