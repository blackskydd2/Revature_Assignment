using InvoiceManagement.BLL.Engines;
using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Documents;
using InvoiceManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceManagement.BLL.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ICalculationEngine _calcEngine;
        private readonly IInvoiceStateMachine _stateMachine;
        private readonly IInvoiceValidator _validator;
        private readonly IAuditRepository _auditRepo;

        public InvoiceService(
            IInvoiceRepository invoiceRepo,
            ICustomerRepository customerRepo,
            ICalculationEngine calcEngine,
            IInvoiceStateMachine stateMachine,
            IInvoiceValidator validator,
            IAuditRepository auditRepo)
        {
            _invoiceRepo = invoiceRepo;
            _customerRepo = customerRepo;
            _calcEngine = calcEngine;
            _stateMachine = stateMachine;
            _validator = validator;
            _auditRepo = auditRepo;
        }

        public async Task<InvoiceDocument> CreateAsync(CreateInvoiceRequest request)
        {
            // Validate
            var validation = _validator.ValidateCreate(request);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join("; ", validation.Errors));

            // Load customer
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId)
                ?? throw new KeyNotFoundException($"Customer '{request.CustomerId}' not found.");

            // Calculate financials
            var calc = _calcEngine.Calculate(request.LineItems, request.InvoiceDiscountAmount, request.InvoiceTaxRatePercent);

            // Determine due date
            var dueDate = ParseDueDate(request.InvoiceDate, request.PaymentTerms);

            // Build invoice document
            var invoice = new InvoiceDocument
            {
                CustomerId = customer.Id,
                CustomerSnapshot = new CustomerSnapshot
                {
                    CustomerName = customer.CustomerName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    BillingAddress = customer.BillingAddress != null
                        ? $"{customer.BillingAddress.Street}, {customer.BillingAddress.City}, {customer.BillingAddress.State} {customer.BillingAddress.PostalCode}"
                        : null
                },
                QuoteId = request.QuoteId,
                InvoiceDate = request.InvoiceDate,
                DueDate = dueDate,
                Status = InvoiceStatus.Draft,
                PaymentTerms = request.PaymentTerms,
                Notes = request.Notes,
                IsRecurring = request.IsRecurring,
                RecurringFrequency = request.RecurringFrequency,
                SubTotal = calc.SubTotal,
                DiscountAmount = calc.DiscountAmount,
                TaxAmount = calc.TaxAmount,
                GrandTotal = calc.GrandTotal,
                AmountPaid = 0,
                CreatedBy = request.CreatedBy,
                LineItems = calc.LineItems.Select(li => new InvoiceLineItemDocument
                {
                    ProductId = li.ProductId,
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    DiscountPercent = li.DiscountPercent,
                    DiscountAmount = li.DiscountAmount,
                    TaxRatePercent = li.TaxRatePercent,
                    TaxAmount = li.TaxAmount,
                    LineTotal = li.LineTotal
                }).ToList()
            };

            // Generate unique invoice number (atomic, no race conditions)
            invoice.InvoiceNumber = await _invoiceRepo.GenerateInvoiceNumberAsync();

            var created = await _invoiceRepo.CreateAsync(invoice);

            await _auditRepo.LogAsync(new AuditLogDocument
            {
                InvoiceId = created.Id,
                InvoiceNumber = created.InvoiceNumber,
                CustomerId = created.CustomerId,
                EventType = "InvoiceCreated",
                EventDescription = $"Invoice {created.InvoiceNumber} created. Total: {created.GrandTotal:C}",
                NewValue = InvoiceStatus.Draft,
                PerformedBy = request.CreatedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["GrandTotal"] = created.GrandTotal,
                    ["LineItems"] = created.LineItems.Count,
                    ["PaymentTerms"] = created.PaymentTerms
                }
            });

            return created;
        }

        public async Task<InvoiceDocument?> GetByIdAsync(string id) =>
            await _invoiceRepo.GetByIdAsync(id);

        public async Task<InvoiceDocument?> GetByNumberAsync(string invoiceNumber) =>
            await _invoiceRepo.GetByInvoiceNumberAsync(invoiceNumber);

        public async Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync(bool includeArchived = false)
        {
            var list = await _invoiceRepo.GetAllAsync(includeArchived);
            return list.Select(ToSummary);
        }

        public async Task<IEnumerable<InvoiceSummaryDto>> GetByCustomerAsync(string customerId)
        {
            var list = await _invoiceRepo.GetByCustomerIdAsync(customerId);
            return list.Select(ToSummary);
        }

        public async Task<IEnumerable<InvoiceSummaryDto>> GetOverdueAsync()
        {
            var list = await _invoiceRepo.GetOverdueAsync();
            return list.Select(ToSummary);
        }

        public async Task<InvoiceDocument> ChangeStatusAsync(string id, string newStatus, string performedBy = "System")
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Invoice '{id}' not found.");

            var transition = _stateMachine.Validate(invoice.Status, newStatus);
            if (!transition.IsValid)
                throw new InvalidOperationException(transition.Reason);

            var prev = invoice.Status;
            invoice.Status = newStatus;
            var updated = await _invoiceRepo.UpdateAsync(invoice);

            await _auditRepo.LogAsync(new AuditLogDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                EventType = "StatusChanged",
                EventDescription = $"Status changed: {prev} → {newStatus}",
                PreviousValue = prev,
                NewValue = newStatus,
                PerformedBy = performedBy
            });

            return updated;
        }

        public async Task<InvoiceDocument> MarkSentAsync(string id, string recipientEmail, string performedBy = "System")
        {
            var invoice = await ChangeStatusAsync(id, InvoiceStatus.Sent, performedBy);
            invoice.SentDate = DateTime.UtcNow;
            await _invoiceRepo.UpdateAsync(invoice);

            await _auditRepo.LogAsync(new AuditLogDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                EventType = "InvoiceSent",
                EventDescription = $"Invoice sent to {recipientEmail}",
                PerformedBy = performedBy,
                Metadata = new Dictionary<string, object> { ["Recipient"] = recipientEmail }
            });

            return invoice;
        }

        public async Task<bool> ArchiveAsync(string id, string performedBy = "System")
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Invoice '{id}' not found.");

            await _auditRepo.LogAsync(new AuditLogDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                EventType = "Archived",
                EventDescription = "Invoice archived",
                PreviousValue = invoice.Status,
                NewValue = InvoiceStatus.Archived,
                PerformedBy = performedBy
            });

            return await _invoiceRepo.ArchiveAsync(id);
        }

        public async Task<bool> DeleteDraftAsync(string id)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Invoice '{id}' not found.");

            if (invoice.Status != InvoiceStatus.Draft)
                throw new InvalidOperationException("Only Draft invoices can be permanently deleted.");

            await _auditRepo.LogAsync(new AuditLogDocument
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = invoice.CustomerId,
                EventType = "Deleted",
                EventDescription = "Draft invoice permanently deleted"
            });

            return await _invoiceRepo.DeleteAsync(id);
        }

        public async Task UpdateOverdueStatusesAsync()
        {
            var candidates = await _invoiceRepo.GetOverdueAsync();
            foreach (var inv in candidates)
            {
                if (inv.Status == InvoiceStatus.Overdue) continue;
                inv.Status = InvoiceStatus.Overdue;
                await _invoiceRepo.UpdateAsync(inv);
                await _auditRepo.LogAsync(new AuditLogDocument
                {
                    InvoiceId = inv.Id,
                    InvoiceNumber = inv.InvoiceNumber,
                    CustomerId = inv.CustomerId,
                    EventType = "MarkedOverdue",
                    EventDescription = $"Auto-marked overdue. Due: {inv.DueDate:yyyy-MM-dd}",
                    PerformedBy = "System"
                });
            }
        }

        // ── Helpers ──────────────────────────────────────────────

        private static DateTime ParseDueDate(DateTime invoiceDate, string paymentTerms) =>
            paymentTerms switch
            {
                "Due on Receipt" => invoiceDate,
                "Net 15"         => invoiceDate.AddDays(15),
                "Net 30"         => invoiceDate.AddDays(30),
                "Net 45"         => invoiceDate.AddDays(45),
                "Net 60"         => invoiceDate.AddDays(60),
                _                => invoiceDate.AddDays(30)
            };

        private static InvoiceSummaryDto ToSummary(InvoiceDocument inv)
        {
            var today = DateTime.UtcNow.Date;
            return new InvoiceSummaryDto
            {
                Id = inv.Id,
                InvoiceNumber = inv.InvoiceNumber,
                CustomerName = inv.CustomerSnapshot.CustomerName,
                InvoiceDate = inv.InvoiceDate,
                DueDate = inv.DueDate,
                Status = inv.Status,
                GrandTotal = inv.GrandTotal,
                AmountPaid = inv.AmountPaid,
                BalanceDue = inv.GrandTotal - inv.AmountPaid,
                DaysOverdue = inv.DueDate.Date < today ? (int)(today - inv.DueDate.Date).TotalDays : 0,
                LineItemCount = inv.LineItems.Count
            };
        }
    }
}
