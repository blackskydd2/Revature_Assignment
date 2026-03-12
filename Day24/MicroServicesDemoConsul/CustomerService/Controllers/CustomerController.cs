using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/v1/controller")]
public class CustomerController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetCustomer(int id)
    {
        return Ok(new {Id = id, Name = $"Customer {id}"});
    }


}