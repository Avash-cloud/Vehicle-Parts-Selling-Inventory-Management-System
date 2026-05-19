using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.API.DTOs;
using VehiclePartsSystem.API.Services;

namespace VehiclePartsSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoices;

    public InvoicesController(IInvoiceService invoices) => _invoices = invoices;

    // Sales Invoices
    [HttpGet("sales")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAllSales()
        => Ok(await _invoices.GetAllSalesInvoicesAsync());

    [HttpGet("sales/{id}")]
    public async Task<IActionResult> GetSalesById(int id)
    {
        var inv = await _invoices.GetSalesInvoiceByIdAsync(id);
        return inv == null ? NotFound() : Ok(inv);
    }

    [HttpPost("sales")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateSales([FromBody] CreateSalesInvoiceRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var inv = await _invoices.CreateSalesInvoiceAsync(request, userId);
            return inv == null ? BadRequest(new { message = "Staff not found." }) : CreatedAtAction(nameof(GetSalesById), new { id = inv.Id }, inv);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sales/{id}/email")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> EmailSalesInvoice(int id)
    {
        var result = await _invoices.EmailSalesInvoiceAsync(id);
        return result ? Ok(new { message = "Invoice emailed successfully." }) : NotFound();
    }

    // Purchase Invoices
    [HttpGet("purchases")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPurchases()
        => Ok(await _invoices.GetAllPurchaseInvoicesAsync());

    [HttpGet("purchases/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPurchaseById(int id)
    {
        var inv = await _invoices.GetPurchaseInvoiceByIdAsync(id);
        return inv == null ? NotFound() : Ok(inv);
    }

    [HttpPost("purchases")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseInvoiceRequest request)
    {
        var inv = await _invoices.CreatePurchaseInvoiceAsync(request);
        return inv == null ? BadRequest(new { message = "Vendor not found." }) : CreatedAtAction(nameof(GetPurchaseById), new { id = inv.Id }, inv);
    }
}
