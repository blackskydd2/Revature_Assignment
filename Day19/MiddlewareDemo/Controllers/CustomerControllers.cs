using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

[ApiController]
[Route("api/v1/[controller]")]
[EnableCors("AllowAll")]
public class CustomerController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCustomer()
    {
        throw new Exception("This is a custom exception in CustomerController.GetCustomer");
        return Ok("Customer data");
    }

    [HttpPost]
    [DisableCors]
    public IActionResult PostCustomer()
    {
        return Ok("Customer created");
    }
}