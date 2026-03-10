using Microsoft.AspNetCore.Mvc;

/// <summary>
/// CustomerController is responsible for handling HTTP requests related to customers. It provides endpoints for retrieving and creating customer data.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CustomerController : ControllerBase
{
    /// <summary>
    /// Returns a list of all customers.
    /// </summary>
    /// <returns>A list of customers.</returns>
    [HttpGet]

    public IActionResult Get()
    {
        return Ok("Customers Get");
    }

    [HttpPost]
    public IActionResult Post()
    {
        return Ok("Customers Get");
    }
}