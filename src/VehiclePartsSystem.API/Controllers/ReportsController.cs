using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    [HttpGet("financial")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFinancial(
        [FromQuery] string period = "monthly",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
        => Ok(await _reports.GetFinancialReportAsync(period, from, to));

    [HttpGet("top-spenders")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetTopSpenders([FromQuery] int top = 10)
        => Ok(await _reports.GetTopSpendersAsync(top));

    [HttpGet("pending-credits")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetPendingCredits()
        => Ok(await _reports.GetPendingCreditsAsync());

    [HttpGet("regular-customers")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetRegularCustomers([FromQuery] int minPurchases = 3)
        => Ok(await _reports.GetRegularCustomersAsync(minPurchases));

    [HttpPost("send-credit-reminders")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendCreditReminders()
    {
        await _reports.SendCreditRemindersAsync();
        return Ok(new { message = "Credit reminders sent." });
    }
}
