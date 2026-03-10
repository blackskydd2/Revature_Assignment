using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceDTODemo.Data;
using InvoiceDTODemo.DTOs;

namespace InvoiceDTODemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoiceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Select(i => new
                {
                    i.InvoiceId,
                    i.InvoiceDate,
                    i.Amount,
                    Customer = new CustomerDTO
                    {
                        Name = i.Customer.Name,
                        Email = i.Customer.Email
                    }
                })
                .ToListAsync();

            return Ok(invoices);
        }
    }
}