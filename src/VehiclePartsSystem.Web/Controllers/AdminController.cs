using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Web.Models;

namespace VehiclePartsSystem.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    public AdminController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        var client = GetApiClient();
        try
        {
            var parts = await client.GetFromJsonAsync<List<PartViewModel>>("/api/parts") ?? new();
            var customers = await client.GetFromJsonAsync<List<CustomerViewModel>>("/api/customers") ?? new();
            var invoices = await client.GetFromJsonAsync<List<SalesInvoiceViewModel>>("/api/invoices/sales") ?? new();
            var lowStock = await client.GetFromJsonAsync<List<PartViewModel>>("/api/parts/low-stock") ?? new();
            var notifications = await client.GetFromJsonAsync<List<NotificationViewModel>>("/api/notifications") ?? new();

            ViewBag.TotalParts = parts.Count;
            ViewBag.TotalCustomers = customers.Count;
            ViewBag.TotalSales = invoices.Count;
            ViewBag.LowStockCount = lowStock.Count;
            ViewBag.TodayRevenue = invoices.Where(i => i.InvoiceDate.Date == DateTime.Today).Sum(i => i.TotalAmount);
            ViewBag.LowStockParts = lowStock.Take(5).ToList();
            ViewBag.UnreadNotifications = notifications.Count(n => !n.IsRead);
        }
        catch
        {
            ViewBag.TotalParts = 0;
            ViewBag.TotalCustomers = 0;
            ViewBag.TotalSales = 0;
            ViewBag.LowStockCount = 0;
            ViewBag.TodayRevenue = 0m;
            ViewBag.LowStockParts = new List<PartViewModel>();
            ViewBag.UnreadNotifications = 0;
        }
        return View();
    }

    // ===== PARTS =====
    public async Task<IActionResult> Parts([FromQuery] string? search)
    {
        var client = GetApiClient();
        var url = string.IsNullOrEmpty(search) ? "/api/parts" : $"/api/parts?search={Uri.EscapeDataString(search)}";
        var parts = await client.GetFromJsonAsync<List<PartViewModel>>(url) ?? new();
        ViewBag.Search = search;
        return View(parts);
    }

    public IActionResult CreatePart() => View(new PartViewModel());

    [HttpPost]
    public async Task<IActionResult> CreatePart(PartViewModel model)
    {
        var client = GetApiClient();
        var response = await client.PostAsJsonAsync("/api/parts", new
        {
            model.Name, model.PartNumber, model.Category, model.Description,
            model.CostPrice, model.SellingPrice, model.StockQuantity, model.ReorderLevel
        });
        if (response.IsSuccessStatusCode) { TempData["Success"] = "Part created."; return RedirectToAction("Parts"); }
        ModelState.AddModelError("", "Failed to create part.");
        return View(model);
    }

    public async Task<IActionResult> EditPart(int id)
    {
        var client = GetApiClient();
        var part = await client.GetFromJsonAsync<PartViewModel>($"/api/parts/{id}");
        return part == null ? NotFound() : View(part);
    }

    [HttpPost]
    public async Task<IActionResult> EditPart(int id, PartViewModel model)
    {
        var client = GetApiClient();
        var response = await client.PutAsJsonAsync($"/api/parts/{id}", new
        {
            model.Name, model.PartNumber, model.Category, model.Description,
            model.CostPrice, model.SellingPrice, model.StockQuantity, model.ReorderLevel, model.IsActive
        });
        if (response.IsSuccessStatusCode) { TempData["Success"] = "Part updated."; return RedirectToAction("Parts"); }
        ModelState.AddModelError("", "Failed to update part.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeletePart(int id)
    {
        var client = GetApiClient();
        await client.DeleteAsync($"/api/parts/{id}");
        TempData["Success"] = "Part deleted.";
        return RedirectToAction("Parts");
    }

    // ===== VENDORS =====
    public async Task<IActionResult> Vendors()
    {
        var client = GetApiClient();
        var vendors = await client.GetFromJsonAsync<List<VendorViewModel>>("/api/vendors") ?? new();
        return View(vendors);
    }

    public IActionResult CreateVendor() => View(new VendorViewModel());

    [HttpPost]
    public async Task<IActionResult> CreateVendor(VendorViewModel model)
    {
        var client = GetApiClient();
        var response = await client.PostAsJsonAsync("/api/vendors", new
        { model.Name, model.ContactPerson, model.Phone, model.Email, model.Address });
        if (response.IsSuccessStatusCode) { TempData["Success"] = "Vendor created."; return RedirectToAction("Vendors"); }
        ModelState.AddModelError("", "Failed to create vendor.");
        return View(model);
    }

    public async Task<IActionResult> EditVendor(int id)
    {
        var client = GetApiClient();
        var vendor = await client.GetFromJsonAsync<VendorViewModel>($"/api/vendors/{id}");
        return vendor == null ? NotFound() : View(vendor);
    }

    [HttpPost]
    public async Task<IActionResult> EditVendor(int id, VendorViewModel model)
    {
        var client = GetApiClient();
        var response = await client.PutAsJsonAsync($"/api/vendors/{id}", new
        { model.Name, model.ContactPerson, model.Phone, model.Email, model.Address });
        if (response.IsSuccessStatusCode) { TempData["Success"] = "Vendor updated."; return RedirectToAction("Vendors"); }
        ModelState.AddModelError("", "Failed to update vendor.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        var client = GetApiClient();
        await client.DeleteAsync($"/api/vendors/{id}");
        TempData["Success"] = "Vendor deleted.";
        return RedirectToAction("Vendors");
    }

    // ===== STAFF =====
    public async Task<IActionResult> Staff()
    {
        var client = GetApiClient();
        var staff = await client.GetFromJsonAsync<List<StaffViewModel>>("/api/auth/staff") ?? new();
        return View(staff);
    }

    public IActionResult CreateStaff() => View();

    [HttpPost]
    public async Task<IActionResult> CreateStaff(string fullName, string email, string password, string phone, string position)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["Error"] = "Full name, email and password are required.";
            return RedirectToAction("Staff");
        }

        var client = GetApiClient();
        var payload = new
        {
            FullName = fullName,
            Email    = email,
            Password = password,
            Phone    = phone ?? "",
            Position = position ?? "Staff"
        };
        var response = await client.PostAsJsonAsync("/api/auth/register-staff", payload);
        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = $"Staff '{fullName}' registered successfully.";
        }
        else
        {
            var body = await response.Content.ReadAsStringAsync();
            TempData["Error"] = response.StatusCode == System.Net.HttpStatusCode.BadRequest
                ? "Email already exists. Please use a different email."
                : $"Failed to register staff. ({(int)response.StatusCode})";
        }
        return RedirectToAction("Staff");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStaff(int userId)
    {
        var client = GetApiClient();
        await client.PutAsync($"/api/auth/staff/{userId}/toggle", null);
        TempData["Success"] = "Staff status updated.";
        return RedirectToAction("Staff");
    }

    // ===== PURCHASE INVOICES =====
    public async Task<IActionResult> PurchaseInvoices()
    {
        var client = GetApiClient();
        var invoices = await client.GetFromJsonAsync<List<PurchaseInvoiceViewModel>>("/api/invoices/purchases") ?? new();
        return View(invoices);
    }

    public async Task<IActionResult> CreatePurchaseInvoice()
    {
        var client = GetApiClient();
        ViewBag.Vendors = await client.GetFromJsonAsync<List<VendorViewModel>>("/api/vendors") ?? new();
        ViewBag.Parts = await client.GetFromJsonAsync<List<PartViewModel>>("/api/parts") ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchaseInvoice(int vendorId, string notes, List<int> partIds, List<int> quantities, List<decimal> unitCosts)
    {
        var client = GetApiClient();
        var items = partIds.Select((pid, i) => new { PartId = pid, Quantity = quantities[i], UnitCost = unitCosts[i] }).ToList();
        var response = await client.PostAsJsonAsync("/api/invoices/purchases", new { vendorId, notes, items });
        if (response.IsSuccessStatusCode) { TempData["Success"] = "Purchase invoice created."; return RedirectToAction("PurchaseInvoices"); }
        TempData["Error"] = "Failed to create invoice.";
        return RedirectToAction("CreatePurchaseInvoice");
    }

    // ===== FINANCIAL REPORTS =====
    public async Task<IActionResult> FinancialReport([FromQuery] string period = "monthly")
    {
        var client = GetApiClient();
        var report = await client.GetFromJsonAsync<FinancialReportViewModel>($"/api/reports/financial?period={period}");
        ViewBag.Period = period;
        return View(report);
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

    // ===== APPOINTMENTS =====
    public async Task<IActionResult> Appointments()
    {
        var client = GetApiClient();
        var appts = await client.GetFromJsonAsync<List<AppointmentViewModel>>("/api/appointments") ?? new();
        return View(appts);
    }

    // ===== NOTIFICATIONS =====
    public async Task<IActionResult> Notifications()
    {
        var client = GetApiClient();
        var notifications = await client.GetFromJsonAsync<List<NotificationViewModel>>("/api/notifications") ?? new();
        return View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> SendCreditReminders()
    {
        var client = GetApiClient();
        await client.PostAsync("/api/reports/send-credit-reminders", null);
        TempData["Success"] = "Credit reminders sent.";
        return RedirectToAction("Index");
    }
}
