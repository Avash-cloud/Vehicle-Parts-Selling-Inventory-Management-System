using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Web.Models;

namespace VehiclePartsSystem.Web.Controllers;

[Authorize(Roles = "Customer")]
public class CustomerController : BaseController
{
    public CustomerController(IHttpClientFactory http) : base(http) { }

    private int GetUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

    private async Task<CustomerViewModel?> GetMyProfile()
    {
        var client = GetApiClient();
        return await client.GetFromJsonAsync<CustomerViewModel>($"/api/customers/by-user/{GetUserId()}");
    }

    public async Task<IActionResult> Dashboard()
    {
        var profile = await GetMyProfile();
        if (profile == null) return RedirectToAction("Login", "Account");
        var client = GetApiClient();
        var history = await client.GetFromJsonAsync<List<SalesInvoiceViewModel>>($"/api/customers/{profile.Id}/history") ?? new();
        var appts = await client.GetFromJsonAsync<List<AppointmentViewModel>>($"/api/appointments/customer/{profile.Id}") ?? new();
        ViewBag.History = history.Take(5).ToList();
        ViewBag.Appointments = appts.Take(3).ToList();
        ViewBag.TotalSpent = profile.TotalSpent;
        ViewBag.CreditBalance = profile.CreditBalance;
        return View(profile);
    }

    public async Task<IActionResult> Profile()
    {
        var profile = await GetMyProfile();
        return profile == null ? RedirectToAction("Login", "Account") : View(profile);
    }

    public async Task<IActionResult> History()
    {
        var profile = await GetMyProfile();
        if (profile == null) return RedirectToAction("Login", "Account");
        var client = GetApiClient();
        var history = await client.GetFromJsonAsync<List<SalesInvoiceViewModel>>($"/api/customers/{profile.Id}/history") ?? new();
        return View(history);
    }

    public async Task<IActionResult> Appointments()
    {
        var profile = await GetMyProfile();
        if (profile == null) return RedirectToAction("Login", "Account");
        var client = GetApiClient();
        var appts = await client.GetFromJsonAsync<List<AppointmentViewModel>>($"/api/appointments/customer/{profile.Id}") ?? new();
        ViewBag.CustomerId = profile.Id;
        return View(appts);
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment(int customerId, string serviceType, DateTime appointmentDate, string notes)
    {
        var client = GetApiClient();
        var response = await client.PostAsJsonAsync("/api/appointments", new { customerId, serviceType, appointmentDate, notes });
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? "Appointment booked!" : "Failed to book appointment.";
        return RedirectToAction("Appointments");
    }

    public async Task<IActionResult> AiPrediction()
    {
        var profile = await GetMyProfile();
        if (profile == null) return RedirectToAction("Login", "Account");
        var client = GetApiClient();
        var prediction = await client.GetFromJsonAsync<AiPredictionViewModel>($"/api/customers/{profile.Id}/ai-prediction");
        return View(prediction);
    }

    public async Task<IActionResult> PartRequests()
    {
        var profile = await GetMyProfile();
        if (profile == null) return RedirectToAction("Login", "Account");
        var client = GetApiClient();
        var requests = await client.GetFromJsonAsync<List<PartRequestViewModel>>($"/api/partrequests/customer/{profile.Id}") ?? new();
        ViewBag.CustomerId = profile.Id;
        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitPartRequest(int customerId, string partName, string description)
    {
        var client = GetApiClient();
        await client.PostAsJsonAsync("/api/partrequests", new { customerId, partName, description });
        TempData["Success"] = "Part request submitted.";
        return RedirectToAction("PartRequests");
    }

    public async Task<IActionResult> Reviews()
    {
        var client = GetApiClient();
        var reviews = await client.GetFromJsonAsync<List<ReviewViewModel>>("/api/reviews") ?? new();
        var profile = await GetMyProfile();
        ViewBag.CustomerId = profile?.Id;
        return View(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReview(int customerId, int rating, string comment)
    {
        var client = GetApiClient();
        await client.PostAsJsonAsync("/api/reviews", new { customerId, rating, comment });
        TempData["Success"] = "Review submitted. Thank you!";
        return RedirectToAction("Reviews");
    }

    public async Task<IActionResult> AddVehicle()
    {
        var profile = await GetMyProfile();
        ViewBag.CustomerId = profile?.Id;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddVehicle(int customerId, string vehicleNumber, string make, string model,
        int year, string fuelType, int mileage, DateTime lastServiceDate, string notes)
    {
        var client = GetApiClient();
        var response = await client.PostAsJsonAsync($"/api/customers/{customerId}/vehicles", new
        { vehicleNumber, make, model, year, fuelType, mileage, lastServiceDate, notes });
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? "Vehicle added." : "Failed to add vehicle.";
        return RedirectToAction("Profile");
    }
}
