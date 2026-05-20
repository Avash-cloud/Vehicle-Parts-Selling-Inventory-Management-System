using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Web.Models;

namespace VehiclePartsSystem.Web.Controllers;

public class HomeController : BaseController
{
    public HomeController(IHttpClientFactory http) : base(http) { }

    public async Task<IActionResult> Index()
    {
        var client = GetApiClient();
        var parts = await client.GetFromJsonAsync<List<PartViewModel>>("/api/parts") ?? new();
        var reviews = await client.GetFromJsonAsync<List<ReviewViewModel>>("/api/reviews") ?? new();
        ViewBag.Parts = parts.Take(6).ToList();
        ViewBag.Reviews = reviews.Take(5).ToList();
        return View();
    }

    public IActionResult About() => View();

    public async Task<IActionResult> Parts([FromQuery] string? search, [FromQuery] string? category)
    {
        var client = GetApiClient();
        var url = "/api/parts";
        var qs = new List<string>();
        if (!string.IsNullOrEmpty(search)) qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrEmpty(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
        if (qs.Any()) url += "?" + string.Join("&", qs);

        var parts = await client.GetFromJsonAsync<List<PartViewModel>>(url) ?? new();
        ViewBag.Search = search;
        ViewBag.Category = category;
        return View(parts);
    }

    public async Task<IActionResult> Reviews()
    {
        var client = GetApiClient();
        var reviews = await client.GetFromJsonAsync<List<ReviewViewModel>>("/api/reviews") ?? new();
        return View(reviews);
    }

    public IActionResult Error() => View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
