using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Services;

public interface IVendorService
{
    Task<List<VendorDto>> GetAllVendorsAsync();
    Task<VendorDto?> GetVendorByIdAsync(int id);
    Task<VendorDto> CreateVendorAsync(CreateVendorRequest request);
    Task<VendorDto?> UpdateVendorAsync(int id, CreateVendorRequest request);
    Task<bool> DeleteVendorAsync(int id);
}

public class VendorService : IVendorService
{
    private readonly AppDbContext _db;

    public VendorService(AppDbContext db) => _db = db;

    public async Task<List<VendorDto>> GetAllVendorsAsync() =>
        await _db.Vendors.Where(v => v.IsActive).Select(v => MapToDto(v)).ToListAsync();

    public async Task<VendorDto?> GetVendorByIdAsync(int id)
    {
        var v = await _db.Vendors.FindAsync(id);
        return v == null ? null : MapToDto(v);
    }

    public async Task<VendorDto> CreateVendorAsync(CreateVendorRequest request)
    {
        var vendor = new Vendor
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address
        };
        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();
        return MapToDto(vendor);
    }

    public async Task<VendorDto?> UpdateVendorAsync(int id, CreateVendorRequest request)
    {
        var vendor = await _db.Vendors.FindAsync(id);
        if (vendor == null) return null;

        vendor.Name = request.Name;
        vendor.ContactPerson = request.ContactPerson;
        vendor.Phone = request.Phone;
        vendor.Email = request.Email;
        vendor.Address = request.Address;

        await _db.SaveChangesAsync();
        return MapToDto(vendor);
    }

    public async Task<bool> DeleteVendorAsync(int id)
    {
        var vendor = await _db.Vendors.FindAsync(id);
        if (vendor == null) return false;
        vendor.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    private static VendorDto MapToDto(Vendor v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        ContactPerson = v.ContactPerson,
        Phone = v.Phone,
        Email = v.Email,
        Address = v.Address,
        IsActive = v.IsActive
    };
}
