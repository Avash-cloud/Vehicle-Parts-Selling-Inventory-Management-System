using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace VehiclePartsSystem.Web.Controllers;

public abstract class BaseController : Controller
{
    protected readonly IHttpClientFactory HttpFactory;

    protected BaseController(IHttpClientFactory http) => HttpFactory = http;

    protected HttpClient GetApiClient()
    {
        var client = HttpFactory.CreateClient("API");
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
