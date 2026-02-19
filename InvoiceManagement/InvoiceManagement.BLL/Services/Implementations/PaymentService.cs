using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Models;
using InvoiceManagement.DAL.Repositories.Interfaces;

namespace InvoiceManagement.BLL.Services.Implementations
{
    /// <summary>
    /// Handles payment recording and reconciliation.
    /// After each payment, updates Invoice.AmountPaid, OutstandingBalance,
    /// and transitions invoice status to PartiallyPaid or Paid automatically.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly InvoiceStatusValidator _statusValidator;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IInvoiceRepository invoiceRepo,
            InvoiceStatusValidator statusValidator)
        {
            _paymentRepo = paymentRepo;
            _invoiceRepo = invoiceRepo;
            _statusValidator = statusValidator;
        }

        /// <summary>
        /// Records a payment against an invoice.
        /// Supports partial payments — call multiple times for installments.
        /// Automatically updates invoice status based on remaining balance.
        /// </summary>
        public async Task<Payment> RecordPaymentAsync(
            int invoiceId, decimal amount, int methodId, string? reference)
        {
            var invoice = await _invoiceRepo.GetWithLineItemsAndPaymentsAsync(invoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            // Cannot pay a cancelled invoice
            if (invoice.Status == InvoiceStatus.Cancelled.ToString())
                throw new InvalidOperationException("Cannot record payment for a Cancelled invoice.");

            // Cannot overpay
            if (amount > invoice.OutstandingBalance)
                throw new InvalidOperationException(
                    $"Payment amount ({amount:C}) exceeds outstanding balance ({invoice.OutstandingBalance:C}).");

            // Create payment record
            var payment = new Payment
            {
                InvoiceId       = invoiceId,
                MethodId        = methodId,
                PaymentAmount   = amount,
                PaymentDate     = DateTime.UtcNow,
                ReceivedDate    = DateTime.UtcNow,
                ReferenceNumber = reference,
                CreatedDate     = DateTime.UtcNow
            };

            await _paymentRepo.AddAsync(payment);

            // Update invoice financials
            invoice.AmountPaid += amount;
            invoice.OutstandingBalance = invoice.GrandTotal - invoice.AmountPaid;

            // Auto-transition status
            var currentStatus = _statusValidator.ParseStatus(invoice.Status);
            InvoiceStatus newStatus;

            if (invoice.OutstandingBalance <= 0)
                newStatus = InvoiceStatus.Paid;
            else
                newStatus = InvoiceStatus.PartiallyPaid;

            // Validate transition before applying
            if (_statusValidator.IsValidTransition(currentStatus, newStatus))
                invoice.Status = newStatus.ToString();

            await _invoiceRepo.UpdateAsync(invoice);

            return payment;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceAsync(int invoiceId)
        {
            return await _paymentRepo.GetPaymentsByInvoiceIdAsync(invoiceId);
        }

        public async Task<decimal> GetOutstandingBalanceAsync(int invoiceId)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            return invoice.OutstandingBalance;
        }
    }
}
