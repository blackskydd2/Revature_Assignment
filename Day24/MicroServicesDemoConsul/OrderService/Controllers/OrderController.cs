using Consul;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

[ApiController]
[Route("api/v1/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IConsulClient _consulClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderController(IConsulClient consulClient, IHttpClientFactory httpClientFactory)
    {
        _consulClient = consulClient;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        // Discover Customer Service
        var services = await _consulClient.Health.Service("customer-service", "", true);
        var service = services.Response.First().Service;
        var url = $"http://{service.Address}:{service.Port}/api/v1/customer/{id}";

        var client = _httpClientFactory.CreateClient();
        var customer = await client.GetFromJsonAsync<object>(url);

        return Ok(new { OrderId = id, Customer = customer });
    }
}