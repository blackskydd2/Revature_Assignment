using InvoiceManagement.BLL.Engines;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Models;
using InvoiceManagement.DAL.Repositories.Interfaces;

namespace InvoiceManagement.BLL.Services.Implementations
{
    /// <summary>
    /// Core invoice business logic.
    /// Orchestrates: InvoiceNumberEngine, InvoiceCalculationEngine, InvoiceStatusValidator.
    /// All invoice operations must flow through this service.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly InvoiceNumberEngine _numberEngine;
        private readonly InvoiceCalculationEngine _calcEngine;
        private readonly InvoiceStatusValidator _statusValidator;

        public InvoiceService(
            IInvoiceRepository invoiceRepo,
            InvoiceNumberEngine numberEngine,
            InvoiceCalculationEngine calcEngine,
            InvoiceStatusValidator statusValidator)
        {
            _invoiceRepo = invoiceRepo;
            _numberEngine = numberEngine;
            _calcEngine = calcEngine;
            _statusValidator = statusValidator;
        }

        /// <summary>
        /// Creates a new invoice atomically: generates number, calculates totals,
        /// sets due date from payment terms, and persists invoice + line items together.
        /// </summary>
        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice, List<InvoiceLineItem> lineItems)
        {
            // Step 1: Generate invoice number
            invoice.InvoiceNumber = await _numberEngine.GenerateAsync(invoice.InvoiceDate);

            // Step 2: Calculate due date from payment terms
            if (Enum.TryParse<PaymentTerms>(invoice.PaymentTerms, out var terms))
                invoice.DueDate = _calcEngine.CalculateDueDate(invoice.InvoiceDate, terms);

            // Step 3: Attach line items and calculate all totals
            invoice.LineItems = lineItems;
            foreach (var item in invoice.LineItems)
                _calcEngine.CalculateLineItem(item);

            _calcEngine.CalculateInvoiceTotals(invoice);

            // Step 4: Default status is Draft
            invoice.Status = InvoiceStatus.Draft.ToString();
            invoice.CreatedDate = DateTime.UtcNow;
            invoice.AmountPaid = 0;

            return await _invoiceRepo.AddAsync(invoice);
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _invoiceRepo.GetWithLineItemsAndPaymentsAsync(invoiceId);
        }

        public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            return await _invoiceRepo.GetByInvoiceNumberAsync(invoiceNumber);
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _invoiceRepo.GetAllAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId)
        {
            return await _invoiceRepo.GetByCustomerIdAsync(customerId);
        }

        /// <summary>
        /// Updates invoice status after validating the transition is allowed.
        /// Throws InvalidOperationException for illegal transitions.
        /// </summary>
        public async Task<Invoice> UpdateInvoiceStatusAsync(int invoiceId, InvoiceStatus newStatus)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            var currentStatus = _statusValidator.ParseStatus(invoice.Status);
            _statusValidator.ValidateTransition(currentStatus, newStatus); // Throws if invalid

            invoice.Status = newStatus.ToString();
            return await _invoiceRepo.UpdateAsync(invoice);
        }

        /// <summary>
        /// Adds a line item to an existing Draft invoice and recalculates totals.
        /// Only Draft invoices can be modified.
        /// </summary>
        public async Task<Invoice> AddLineItemAsync(int invoiceId, InvoiceLineItem lineItem)
        {
            var invoice = await _invoiceRepo.GetWithLineItemsAndPaymentsAsync(invoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            if (invoice.Status != InvoiceStatus.Draft.ToString())
                throw new InvalidOperationException("Line items can only be added to Draft invoices.");

            lineItem.InvoiceId = invoiceId;
            _calcEngine.CalculateLineItem(lineItem);
            invoice.LineItems.Add(lineItem);
            _calcEngine.CalculateInvoiceTotals(invoice);

            return await _invoiceRepo.UpdateAsync(invoice);
        }

        public async Task<bool> RemoveLineItemAsync(int invoiceId, int lineItemId)
        {
            var invoice = await _invoiceRepo.GetWithLineItemsAndPaymentsAsync(invoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            if (invoice.Status != InvoiceStatus.Draft.ToString())
                throw new InvalidOperationException("Line items can only be removed from Draft invoices.");

            var item = invoice.LineItems.FirstOrDefault(li => li.LineItemId == lineItemId)
                ?? throw new KeyNotFoundException($"Line item {lineItemId} not found.");

            invoice.LineItems.Remove(item);
            _calcEngine.CalculateInvoiceTotals(invoice);
            await _invoiceRepo.UpdateAsync(invoice);
            return true;
        }

        public async Task<bool> ArchiveInvoiceAsync(int invoiceId)
        {
            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            // Only Paid or Cancelled invoices should be archived
            if (invoice.Status != InvoiceStatus.Paid.ToString() &&
                invoice.Status != InvoiceStatus.Cancelled.ToString())
                throw new InvalidOperationException("Only Paid or Cancelled invoices can be archived.");

            return await _invoiceRepo.ArchiveInvoiceAsync(invoiceId);
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            return await _invoiceRepo.GetOverdueInvoicesAsync();
        }

        /// <summary>
        /// Generates aging report grouped into buckets: Current, 1-30, 31-60, 61-90, 90+
        /// </summary>
        public async Task<Dictionary<string, List<Invoice>>> GetAgingReportAsync()
        {
            var allUnpaid = await _invoiceRepo.GetByStatusAsync(InvoiceStatus.Sent.ToString());
            var overdue = await _invoiceRepo.GetOverdueInvoicesAsync();
            var partial = await _invoiceRepo.GetByStatusAsync(InvoiceStatus.PartiallyPaid.ToString());

            var allActive = allUnpaid.Concat(overdue).Concat(partial)
                .DistinctBy(i => i.InvoiceId);

            var report = new Dictionary<string, List<Invoice>>
            {
                ["Current"]           = new(),
                ["1-30 Days Overdue"] = new(),
                ["31-60 Days Overdue"]= new(),
                ["61-90 Days Overdue"]= new(),
                ["90+ Days Overdue"]  = new()
            };

            foreach (var invoice in allActive)
            {
                var bucket = _calcEngine.GetAgingBucket(invoice);
                if (report.ContainsKey(bucket))
                    report[bucket].Add(invoice);
            }

            return report;
        }

        /// <summary>
        /// Calculates Days Sales Outstanding for a given period.
        /// </summary>
        public async Task<decimal> GetDSOAsync(int periodDays = 30)
        {
            var allInvoices = await _invoiceRepo.GetAllAsync();
            decimal totalOutstanding = allInvoices.Sum(i => i.OutstandingBalance);
            decimal totalRevenue = allInvoices.Sum(i => i.GrandTotal);

            return _calcEngine.CalculateDSO(totalOutstanding, totalRevenue, periodDays);
        }

        /// <summary>
        /// Should be called daily (e.g., via a scheduled job) to auto-transition
        /// Sent invoices to Overdue when their DueDate has passed.
        /// </summary>
        public async Task UpdateOverdueStatusesAsync()
        {
            var sentInvoices = await _invoiceRepo.GetByStatusAsync(InvoiceStatus.Sent.ToString());
            var today = DateTime.UtcNow.Date;

            foreach (var invoice in sentInvoices.Where(i => i.DueDate.Date < today))
            {
                invoice.Status = InvoiceStatus.Overdue.ToString();
                await _invoiceRepo.UpdateAsync(invoice);
            }
        }
    }
}
