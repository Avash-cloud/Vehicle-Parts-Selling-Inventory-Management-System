using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartRequestsController : ControllerBase
{
    private readonly IPartRequestService _partRequests;

    public PartRequestsController(IPartRequestService partRequests) => _partRequests = partRequests;

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll() => Ok(await _partRequests.GetAllAsync());

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
        => Ok(await _partRequests.GetByCustomerAsync(customerId));

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] PartRequestCreateDto request)
    {
        var result = await _partRequests.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var result = await _partRequests.UpdateStatusAsync(id, status);
        return result ? Ok(new { message = "Status updated." }) : NotFound();
    }
}
