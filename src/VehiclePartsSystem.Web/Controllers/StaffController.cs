using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Web.Models;

namespace VehiclePartsSystem.Web.Controllers;

[Authorize(Roles = "Staff,Admin")]
public class StaffController : BaseController
{
    public StaffController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        var client = GetApiClient();
        var customers = await client.GetFromJsonAsync<List<CustomerViewModel>>("/api/customers") ?? new();
        var invoices = await client.GetFromJsonAsync<List<SalesInvoiceViewModel>>("/api/invoices/sales") ?? new();
        ViewBag.TotalCustomers = customers.Count;
        ViewBag.TodaySales = invoices.Count(i => i.InvoiceDate.Date == DateTime.Today);
        ViewBag.TodayRevenue = invoices.Where(i => i.InvoiceDate.Date == DateTime.Today).Sum(i => i.TotalAmount);
        return View();
    }

    // ===== CUSTOMERS =====
    public async Task<IActionResult> Customers([FromQuery] string? search)
    {
        var client = GetApiClient();
        List<CustomerViewModel> customers;
        if (!string.IsNullOrEmpty(search))
            customers = await client.GetFromJsonAsync<List<CustomerViewModel>>($"/api/customers/search?q={Uri.EscapeDataString(search)}") ?? new();
        else
            customers = await client.GetFromJsonAsync<List<CustomerViewModel>>("/api/customers") ?? new();
        ViewBag.Search = search;
        return View(customers);
    }

    public async Task<IActionResult> CustomerDetail(int id)
    {
        var client = GetApiClient();
        var customer = await client.GetFromJsonAsync<CustomerViewModel>($"/api/customers/{id}");
        if (customer == null) return NotFound();
        var history = await client.GetFromJsonAsync<List<SalesInvoiceViewModel>>($"/api/customers/{id}/history") ?? new();
        ViewBag.History = history;
        return View(customer);
    }

    public IActionResult RegisterCustomer() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> RegisterCustomer(RegisterViewModel model)
    {
        var client = GetApiClient();
        object? vehiclePayload = null;
        if (!string.IsNullOrEmpty(model.VehicleNumber))
        {
            vehiclePayload = new
            {
                vehicleNumber = model.VehicleNumber,
                make = model.Make ?? "",
                model = model.VehicleModel ?? "",
                year = model.Year ?? DateTime.Now.Year,
                fuelType = model.FuelType ?? "Petrol",
                mileage = 0,
                lastServiceDate = DateTime.UtcNow,
                notes = ""
            };
        }
        var payload = new
        {
            fullName = model.FullName,
            email = model.Email,
            password = model.Password,
            phone = model.Phone,
            address = model.Address,
            vehicle = vehiclePayload
        };
        var response = await client.PostAsJsonAsync("/api/auth/register", payload);
        if (response.IsSuccessStatusCode) { TempData["Success"] = "Customer registered."; return RedirectToAction("Customers"); }
        ModelState.AddModelError("", "Registration failed. Email may already exist.");
        return View(model);
    }

    // ===== SALES INVOICES =====
    public async Task<IActionResult> SalesInvoices()
    {
        var client = GetApiClient();
        var invoices = await client.GetFromJsonAsync<List<SalesInvoiceViewModel>>("/api/invoices/sales") ?? new();
        return View(invoices);
    }

    public async Task<IActionResult> CreateSalesInvoice()
    {
        var client = GetApiClient();
        ViewBag.Customers = await client.GetFromJsonAsync<List<CustomerViewModel>>("/api/customers") ?? new();
        ViewBag.Parts = await client.GetFromJsonAsync<List<PartViewModel>>("/api/parts") ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesInvoice(int customerId, bool isCreditSale, string notes, List<int> partIds, List<int> quantities)
    {
        var client = GetApiClient();
        var items = partIds.Select((pid, i) => new { PartId = pid, Quantity = quantities[i] }).ToList();
        var response = await client.PostAsJsonAsync("/api/invoices/sales", new { customerId, isCreditSale, notes, items });
        if (response.IsSuccessStatusCode)
        {
            var inv = await response.Content.ReadFromJsonAsync<SalesInvoiceViewModel>();
            TempData["Success"] = $"Invoice {inv?.InvoiceNumber} created. Total: Rs. {inv?.TotalAmount:N2}" +
                (inv?.DiscountAmount > 0 ? $" (10% loyalty discount applied: Rs. {inv.DiscountAmount:N2})" : "");
            return RedirectToAction("SalesInvoices");
        }
        var err = await response.Content.ReadAsStringAsync();
        TempData["Error"] = "Failed: " + err;
        return RedirectToAction("CreateSalesInvoice");
    }

    [HttpPost]
    public async Task<IActionResult> EmailInvoice(int id)
    {
        var client = GetApiClient();
        var response = await client.PostAsync($"/api/invoices/sales/{id}/email", null);
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? "Invoice emailed." : "Failed to send email.";
        return RedirectToAction("SalesInvoices");
    }

    // ===== REPORTS =====
    public async Task<IActionResult> Reports()
    {
        var client = GetApiClient();
        var topSpenders = await client.GetFromJsonAsync<List<TopSpenderViewModel>>("/api/reports/top-spenders") ?? new();
        var pendingCredits = await client.GetFromJsonAsync<List<PendingCreditViewModel>>("/api/reports/pending-credits") ?? new();
        var regularCustomers = await client.GetFromJsonAsync<List<CustomerViewModel>>("/api/reports/regular-customers") ?? new();
        ViewBag.TopSpenders = topSpenders;
        ViewBag.PendingCredits = pendingCredits;
        ViewBag.RegularCustomers = regularCustomers;
        return View();
    }

    // ===== PART REQUESTS =====
    public async Task<IActionResult> PartRequests()
    {
        var client = GetApiClient();
        var requests = await client.GetFromJsonAsync<List<PartRequestViewModel>>("/api/partrequests") ?? new();
        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePartRequestStatus(int id, string status)
    {
        var client = GetApiClient();
        await client.PutAsJsonAsync($"/api/partrequests/{id}/status", status);
        TempData["Success"] = "Status updated.";
        return RedirectToAction("PartRequests");
    }

    // ===== APPOINTMENTS =====
    public async Task<IActionResult> Appointments()
    {
        var client = GetApiClient();
        var appts = await client.GetFromJsonAsync<List<AppointmentViewModel>>("/api/appointments") ?? new();
        return View(appts);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
    {
        var client = GetApiClient();
        await client.PutAsJsonAsync($"/api/appointments/{id}/status", status);
        TempData["Success"] = "Appointment status updated.";
        return RedirectToAction("Appointments");
    }
}
