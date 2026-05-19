using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;

    public CustomersController(ICustomerService customers) => _customers = customers;

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll() => Ok(await _customers.GetAllCustomersAsync());

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _customers.GetCustomerByIdAsync(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var c = await _customers.GetCustomerByUserIdAsync(userId);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Search([FromQuery] string q)
        => Ok(await _customers.SearchCustomersAsync(q));

    [HttpGet("by-vehicle/{vehicleNumber}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetByVehicle(string vehicleNumber)
    {
        var c = await _customers.GetCustomerByVehicleNumberAsync(vehicleNumber);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost("{id}/vehicles")]
    public async Task<IActionResult> AddVehicle(int id, [FromBody] CreateVehicleRequest request)
    {
        var result = await _customers.AddVehicleAsync(id, request);
        return result ? Ok(new { message = "Vehicle added." }) : NotFound();
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
        => Ok(await _customers.GetCustomerHistoryAsync(id));

    [HttpGet("{id}/ai-prediction")]
    public async Task<IActionResult> GetAiPrediction(int id)
    {
        var prediction = await _customers.GetAiPredictionAsync(id);
        return prediction == null ? NotFound(new { message = "No vehicles found." }) : Ok(prediction);
    }
}
