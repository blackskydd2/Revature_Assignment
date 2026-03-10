using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDemo.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCustomer()
    {
        return Ok(new { Name = "John Doe", Age = 30 });
    }
}