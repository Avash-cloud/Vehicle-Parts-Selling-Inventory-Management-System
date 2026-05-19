using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        if (result == null) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
    {
        var success = await _auth.RegisterCustomerAsync(request);
        if (!success) return BadRequest(new { message = "Email already exists." });
        return Ok(new { message = "Registration successful." });
    }

    [HttpPost("register-staff")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffRequest request)
    {
        var success = await _auth.RegisterStaffAsync(request);
        if (!success) return BadRequest(new { message = "Email already exists." });
        return Ok(new { message = "Staff registered successfully." });
    }

    [HttpGet("staff")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllStaff()
    {
        var staff = await _auth.GetAllStaffAsync();
        return Ok(staff.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.Phone,
            u.IsActive,
            u.CreatedAt,
            Position = u.Staff?.Position
        }));
    }

    [HttpPut("staff/{userId}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStaff(int userId)
    {
        var result = await _auth.ToggleStaffStatusAsync(userId);
        if (!result) return NotFound();
        return Ok(new { message = "Staff status updated." });
    }

    [HttpPut("staff/{userId}/position")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePosition(int userId, [FromBody] string position)
    {
        var result = await _auth.UpdateStaffRoleAsync(userId, position);
        if (!result) return NotFound();
        return Ok(new { message = "Position updated." });
    }
}
