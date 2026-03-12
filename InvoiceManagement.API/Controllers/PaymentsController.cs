using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.DAL.Documents;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagement.API.Controllers
{
    /// <summary>
    /// Apply and manage payments against invoices
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _svc;
        public PaymentsController(IPaymentService svc) => _svc = svc;

        // GET api/payments/invoice/{invoiceId}
        [HttpGet("invoice/{invoiceId}")]
        [ProducesResponseType(typeof(IEnumerable<PaymentDocument>), 200)]
        public async Task<IActionResult> GetByInvoice(string invoiceId)
        {
            var payments = await _svc.GetByInvoiceAsync(invoiceId);
            return Ok(payments);
        }

        // GET api/payments/range?from=2024-01-01&to=2024-12-31
        [HttpGet("range")]
        [ProducesResponseType(typeof(IEnumerable<PaymentDocument>), 200)]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime from,
            [FromQuery] DateTime? to)
        {
            var toDate = to ?? DateTime.UtcNow;
            var payments = await _svc.GetByDateRangeAsync(from, toDate);
            return Ok(payments);
        }

        // GET api/payments/methods
        [HttpGet("methods")]
        [ProducesResponseType(typeof(IEnumerable<PaymentMethodDocument>), 200)]
        public async Task<IActionResult> GetMethods()
        {
            var methods = await _svc.GetPaymentMethodsAsync();
            return Ok(methods);
        }

        // POST api/payments
        [HttpPost]
        [ProducesResponseType(typeof(PaymentDocument), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ApplyPayment([FromBody] ApplyPaymentRequest request)
        {
            try
            {
                var payment = await _svc.ApplyPaymentAsync(request);
                return CreatedAtAction(null, new { id = payment.Id }, payment);
            }
            catch (ArgumentException ex)     { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex)  { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // POST api/payments/{paymentId}/reverse
        [HttpPost("{paymentId}/reverse")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Reverse(string paymentId, [FromBody] ReversePaymentRequest request)
        {
            try
            {
                await _svc.ReversePaymentAsync(paymentId, request.Reason, request.PerformedBy ?? "API");
                return Ok(new { message = $"Payment '{paymentId}' reversed. Reason: {request.Reason}" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }

    public class ReversePaymentRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? PerformedBy { get; set; }
    }
}
