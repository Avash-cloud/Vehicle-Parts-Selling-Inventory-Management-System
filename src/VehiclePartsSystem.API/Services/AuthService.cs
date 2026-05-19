using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Helpers;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterCustomerAsync(RegisterCustomerRequest request);
    Task<bool> RegisterStaffAsync(RegisterStaffRequest request);
    Task<List<User>> GetAllStaffAsync();
    Task<bool> ToggleStaffStatusAsync(int userId);
    Task<bool> UpdateStaffRoleAsync(int userId, string position);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;

    public AuthService(AppDbContext db, JwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return new LoginResponse
        {
            Token = _jwt.GenerateToken(user),
            Role = user.Role.ToString(),
            FullName = user.FullName,
            UserId = user.Id
        };
    }

    public async Task<bool> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return false;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = UserRole.Customer
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var customer = new Customer
        {
            UserId = user.Id,
            Address = request.Address
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        if (request.Vehicle != null)
        {
            var vehicle = new Vehicle
            {
                CustomerId = customer.Id,
                VehicleNumber = request.Vehicle.VehicleNumber,
                Make = request.Vehicle.Make,
                Model = request.Vehicle.VehicleModel,
                Year = request.Vehicle.Year,
                FuelType = request.Vehicle.FuelType,
                Mileage = request.Vehicle.Mileage,
                LastServiceDate = request.Vehicle.LastServiceDate,
                Notes = request.Vehicle.Notes
            };
            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> RegisterStaffAsync(RegisterStaffRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return false;

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = UserRole.Staff
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var staff = new Staff
        {
            UserId = user.Id,
            Position = request.Position,
            HireDate = DateTime.UtcNow
        };
        _db.Staff.Add(staff);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<List<User>> GetAllStaffAsync()
    {
        return await _db.Users
            .Where(u => u.Role == UserRole.Staff)
            .Include(u => u.Staff)
            .ToListAsync();
    }

    public async Task<bool> ToggleStaffStatusAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStaffRoleAsync(int userId, string position)
    {
        var staff = await _db.Staff.FirstOrDefaultAsync(s => s.UserId == userId);
        if (staff == null) return false;
        staff.Position = position;
        await _db.SaveChangesAsync();
        return true;
    }
}
