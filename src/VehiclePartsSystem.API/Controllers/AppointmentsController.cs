using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;

    public AppointmentsController(IAppointmentService appointments) => _appointments = appointments;

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll() => Ok(await _appointments.GetAllAsync());

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
        => Ok(await _appointments.GetByCustomerAsync(customerId));

    [HttpPost]
    [Authorize(Roles = "Customer,Staff,Admin")]
    public async Task<IActionResult> Create([FromBody] AppointmentRequest request)
    {
        var result = await _appointments.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var result = await _appointments.UpdateStatusAsync(id, status);
        return result ? Ok(new { message = "Status updated." }) : NotFound();
    }
}
