using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.DAL.Documents;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagement.API.Controllers
{
    /// <summary>
    /// Create and manage invoices
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _svc;
        public InvoicesController(IInvoiceService svc) => _svc = svc;

        // GET api/invoices
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InvoiceSummaryDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] bool includeArchived = false)
        {
            var list = await _svc.GetAllAsync(includeArchived);
            return Ok(list);
        }

        // GET api/invoices/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InvoiceDocument), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(string id)
        {
            var inv = await _svc.GetByIdAsync(id);
            return inv is null ? NotFound(new { message = $"Invoice '{id}' not found." }) : Ok(inv);
        }

        // GET api/invoices/number/{invoiceNumber}
        [HttpGet("number/{invoiceNumber}")]
        [ProducesResponseType(typeof(InvoiceDocument), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByNumber(string invoiceNumber)
        {
            var inv = await _svc.GetByNumberAsync(invoiceNumber);
            return inv is null ? NotFound(new { message = $"Invoice '{invoiceNumber}' not found." }) : Ok(inv);
        }

        // GET api/invoices/customer/{customerId}
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(IEnumerable<InvoiceSummaryDto>), 200)]
        public async Task<IActionResult> GetByCustomer(string customerId)
        {
            var list = await _svc.GetByCustomerAsync(customerId);
            return Ok(list);
        }

        // GET api/invoices/overdue
        [HttpGet("overdue")]
        [ProducesResponseType(typeof(IEnumerable<InvoiceSummaryDto>), 200)]
        public async Task<IActionResult> GetOverdue()
        {
            await _svc.UpdateOverdueStatusesAsync();   // auto-mark any newly overdue
            var list = await _svc.GetOverdueAsync();
            return Ok(list);
        }

        // POST api/invoices
        [HttpPost]
        [ProducesResponseType(typeof(InvoiceDocument), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
        {
            try
            {
                var inv = await _svc.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = inv.Id }, inv);
            }
            catch (ArgumentException ex)   { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex){ return NotFound(new { message = ex.Message }); }
        }

        // POST api/invoices/{id}/send
        [HttpPost("{id}/send")]
        [ProducesResponseType(typeof(InvoiceDocument), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Send(string id, [FromBody] SendInvoiceRequest request)
        {
            try
            {
                var inv = await _svc.MarkSentAsync(id, request.RecipientEmail, request.SentBy ?? "API");
                return Ok(inv);
            }
            catch (KeyNotFoundException ex)  { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PATCH api/invoices/{id}/status
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(InvoiceDocument), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeStatusRequest request)
        {
            try
            {
                var inv = await _svc.ChangeStatusAsync(id, request.NewStatus, request.PerformedBy ?? "API");
                return Ok(inv);
            }
            catch (KeyNotFoundException ex)  { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // POST api/invoices/{id}/archive
        [HttpPost("{id}/archive")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Archive(string id)
        {
            try
            {
                await _svc.ArchiveAsync(id, "API");
                return Ok(new { message = $"Invoice '{id}' archived." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // DELETE api/invoices/{id}  (Draft only)
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _svc.DeleteDraftAsync(id);
                return Ok(new { message = $"Draft invoice '{id}' deleted." });
            }
            catch (KeyNotFoundException ex)  { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }
    }

    // ── Request DTOs specific to invoice actions ─────────────────
    public class SendInvoiceRequest
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string? SentBy { get; set; }
    }

    public class ChangeStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? PerformedBy { get; set; }
    }
}
