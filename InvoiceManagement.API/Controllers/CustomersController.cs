using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.DAL.Documents;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagement.API.Controllers
{
    /// <summary>
    /// Manage CRM customers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _svc;
        public CustomersController(ICustomerService svc) => _svc = svc;

        // GET api/customers
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CustomerDocument>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
        {
            var list = await _svc.GetAllAsync(activeOnly);
            return Ok(list);
        }

        // GET api/customers/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerDocument), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(string id)
        {
            var c = await _svc.GetByIdAsync(id);
            return c is null ? NotFound(new { message = $"Customer '{id}' not found." }) : Ok(c);
        }

        // GET api/customers/search?name=acme
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDocument>), 200)]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Search 'name' query param is required." });

            var results = await _svc.SearchAsync(name);
            return Ok(results);
        }

        // POST api/customers
        [HttpPost]
        [ProducesResponseType(typeof(CustomerDocument), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            try
            {
                var created = await _svc.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/customers/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CustomerDocument), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(string id, [FromBody] CustomerDocument customer)
        {
            var existing = await _svc.GetByIdAsync(id);
            if (existing is null) return NotFound(new { message = $"Customer '{id}' not found." });

            customer.Id = id;
            var updated = await _svc.UpdateAsync(customer);
            return Ok(updated);
        }

        // DELETE api/customers/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _svc.GetByIdAsync(id);
            if (existing is null) return NotFound(new { message = $"Customer '{id}' not found." });

            await _svc.DeleteAsync(id);
            return Ok(new { message = $"Customer '{existing.CustomerName}' deactivated." });
        }
    }
}
