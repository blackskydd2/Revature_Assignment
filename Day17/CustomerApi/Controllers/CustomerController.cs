using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController : ControllerBase
{
    // // Temporary in-memory list (acts like database)
    private static List<Customer> customers = new ()
    {
        new Customer { Id = 1, Name = "Alice", Email = "alice@mail.com" },
        new Customer { Id = 2, Name = "Bob", Email = "bob@mail.com" }
    };

    // =========================
    // GET ALL
    // =========================
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(customers);
    }

    // =========================
    // GET BY ID
    // =========================
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var customer = customers.FirstOrDefault(c => c.Id == id);

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    // =========================
    // POST (Create)
    // =========================
    [HttpPost]
    public IActionResult Create(Customer customer)
    {
        customer.Id = customers.Max(c => c.Id) + 1;
        customers.Add(customer);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    // =========================
    // PUT (Update)
    // =========================
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Customer updatedCustomer)
    {
        var customer = customers.FirstOrDefault(c => c.Id == id);

        if (customer == null)
            return NotFound();

        customer.Name = updatedCustomer.Name;
        customer.Email = updatedCustomer.Email;

        return NoContent();
    }

    // =========================
    // DELETE
    // =========================
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var customer = customers.FirstOrDefault(c => c.Id == id);

        if (customer == null)
            return NotFound();

        customers.Remove(customer);

        return NoContent();
    }
}