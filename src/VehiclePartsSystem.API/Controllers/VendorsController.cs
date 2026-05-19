using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendors;

    public VendorsController(IVendorService vendors) => _vendors = vendors;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _vendors.GetAllVendorsAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var v = await _vendors.GetVendorByIdAsync(id);
        return v == null ? NotFound() : Ok(v);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorRequest request)
    {
        var v = await _vendors.CreateVendorAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = v.Id }, v);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateVendorRequest request)
    {
        var v = await _vendors.UpdateVendorAsync(id, request);
        return v == null ? NotFound() : Ok(v);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _vendors.DeleteVendorAsync(id);
        return result ? Ok(new { message = "Vendor deleted." }) : NotFound();
    }
}
