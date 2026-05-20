using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using VehiclePartsSystem.Web.Models;

namespace VehiclePartsSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _http;

    public AccountController(IHttpClientFactory http) => _http = http;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _http.CreateClient("API");
        var response = await client.PostAsJsonAsync("/api/auth/login", new { model.Email, model.Password });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result == null) { ModelState.AddModelError("", "Login failed."); return View(model); }

        // Store JWT in session
        HttpContext.Session.SetString("JwtToken", result.Token);
        HttpContext.Session.SetString("UserRole", result.Role);
        HttpContext.Session.SetString("UserName", result.FullName);
        HttpContext.Session.SetInt32("UserId", result.UserId);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, result.FullName),
            new(ClaimTypes.Email, model.Email),
            new(ClaimTypes.Role, result.Role),
            new(ClaimTypes.NameIdentifier, result.UserId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return result.Role switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "Staff" => RedirectToAction("Index", "Staff"),
            _ => RedirectToAction("Dashboard", "Customer")
        };
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var client = _http.CreateClient("API");

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
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Registration failed. Email may already exist.");
            return View(model);
        }

        TempData["Success"] = "Registration successful! Please login.";
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();

    private class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
