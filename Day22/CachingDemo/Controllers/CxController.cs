using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/[controller]")]
public class CxController : ControllerBase
{
    CrmDbContext _dbContext;
    public CxController(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var contextId = _dbContext.ContextId;
        var customer = await _dbContext.Customers.ToListAsync();

        return Ok(
            new
            {
                ContextId = contextId,
                Customers = customer
            }
        );
    }
}