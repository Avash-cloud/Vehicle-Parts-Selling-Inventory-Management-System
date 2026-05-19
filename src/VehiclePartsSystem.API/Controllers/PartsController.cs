using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartsController : ControllerBase
{
    private readonly IPartService _parts;

    public PartsController(IPartService parts) => _parts = parts;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category)
        => Ok(await _parts.GetAllPartsAsync(search, category));

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var part = await _parts.GetPartByIdAsync(id);
        return part == null ? NotFound() : Ok(part);
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetLowStock()
        => Ok(await _parts.GetLowStockPartsAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePartRequest request)
    {
        var part = await _parts.CreatePartAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartRequest request)
    {
        var part = await _parts.UpdatePartAsync(id, request);
        return part == null ? NotFound() : Ok(part);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _parts.DeletePartAsync(id);
        return result ? Ok(new { message = "Part deleted." }) : NotFound();
    }
}
