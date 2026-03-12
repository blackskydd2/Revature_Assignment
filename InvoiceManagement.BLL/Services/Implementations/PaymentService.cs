using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Documents;
using InvoiceManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceManagement.BLL.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IPaymentMethodRepository _methodRepo;
        private readonly IInvoiceValidator _validator;
        private readonly IAuditRepository _auditRepo;
        private readonly IReconciliationRepository _reconRepo;

        public PaymentService(
            IPaymentRepository paymentRepo,
            IInvoiceRepository invoiceRepo,
            IPaymentMethodRepository methodRepo,
            IInvoiceValidator validator,
            IAuditRepository auditRepo,
            IReconciliationRepository reconRepo)
        {
            _paymentRepo = paymentRepo;
            _invoiceRepo = invoiceRepo;
            _methodRepo = methodRepo;
            _validator = validator;
            _auditRepo = auditRepo;
            _reconRepo = reconRepo;
        }

        public async Task<PaymentDocument> ApplyPaymentAsync(ApplyPaymentRequest request)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(request.InvoiceId)
                ?? throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' not found.");

            if (invoice.Status == InvoiceStatus.Paid)
                throw new InvalidOperationException("Invoice is already fully paid.");

            if (invoice.Status is InvoiceStatus.Cancelled or InvoiceStatus.Archived)
                throw new InvalidOperationException($"Cannot apply payment to a {invoice.Status} invoice.");

            var balance = invoice.GrandTotal - invoice.AmountPaid;

            var validation = _validator.ValidatePayment(request, balance);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join("; ", validation.Errors));

            var method = await _methodRepo.GetByIdAsync(request.PaymentMethodId)
                ?? throw new KeyNotFoundException($"Payment method '{request.PaymentMethodId}' not found.");

            // Create payment record
            var payment = new PaymentDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                PaymentAmount = request.PaymentAmount,
                PaymentDate = request.PaymentDate,
                PaymentMethodId = method.Id,
                PaymentMethodName = method.MethodName,
                ReferenceNumber = request.ReferenceNumber,
                Notes = request.Notes,
                ReceivedDate = DateTime.UtcNow
            };
            var saved = await _paymentRepo.CreateAsync(payment);

            // Update invoice balance
            var balanceBefore = balance;
            invoice.AmountPaid += request.PaymentAmount;
            var balanceAfter = invoice.GrandTotal - invoice.AmountPaid;
            var fullyPaid = balanceAfter <= 0;

            invoice.Status = fullyPaid ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
            await _invoiceRepo.UpdateAsync(invoice);

            // Audit log
            await _auditRepo.LogAsync(new AuditLogDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                EventType = "PaymentApplied",
                EventDescription = $"Payment {request.PaymentAmount:C} via {method.MethodName}. Balance: {balanceBefore:C} → {balanceAfter:C}",
                PreviousValue = $"Balance {balanceBefore:C}",
                NewValue = $"Balance {balanceAfter:C}",
                PerformedBy = request.ReceivedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["PaymentId"] = saved.Id,
                    ["PaymentAmount"] = request.PaymentAmount,
                    ["FullyPaid"] = fullyPaid
                }
            });

            // Reconciliation record
            await _reconRepo.CreateAsync(new ReconciliationDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                PaymentId = saved.Id,
                PaymentAmount = request.PaymentAmount,
                PaymentDate = request.PaymentDate,
                PaymentMethod = method.MethodName,
                InvoiceTotal = invoice.GrandTotal,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                IsFullyPaid = fullyPaid
            });

            return saved;
        }

        public async Task<IEnumerable<PaymentDocument>> GetByInvoiceAsync(string invoiceId) =>
            await _paymentRepo.GetByInvoiceIdAsync(invoiceId);

        public async Task<IEnumerable<PaymentDocument>> GetByDateRangeAsync(DateTime from, DateTime to) =>
            await _paymentRepo.GetByDateRangeAsync(from, to);

        public async Task<bool> ReversePaymentAsync(string paymentId, string reason, string performedBy = "System")
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId)
                ?? throw new KeyNotFoundException($"Payment '{paymentId}' not found.");

            var invoice = await _invoiceRepo.GetByIdAsync(payment.InvoiceId)
                ?? throw new KeyNotFoundException("Invoice not found for this payment.");

            invoice.AmountPaid = Math.Max(0, invoice.AmountPaid - payment.PaymentAmount);
            var newBalance = invoice.GrandTotal - invoice.AmountPaid;
            invoice.Status = invoice.AmountPaid > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Sent;

            await _invoiceRepo.UpdateAsync(invoice);
            await _paymentRepo.MarkReversedAsync(paymentId, reason);

            await _auditRepo.LogAsync(new AuditLogDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                EventType = "PaymentReversed",
                EventDescription = $"Payment {payment.PaymentAmount:C} reversed. Reason: {reason}",
                PerformedBy = performedBy
            });

            return true;
        }

        public async Task<IEnumerable<PaymentMethodDocument>> GetPaymentMethodsAsync() =>
            await _methodRepo.GetAllActiveAsync();
    }
}
