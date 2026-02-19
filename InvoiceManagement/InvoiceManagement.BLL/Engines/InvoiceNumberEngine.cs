using InvoiceManagement.DAL.Repositories.Interfaces;

namespace InvoiceManagement.BLL.Engines
{
    /// <summary>
    /// Generates unique, sequential invoice numbers.
    /// 
    /// FORMAT: INV-YYYYMM-XXXXX
    ///   INV    = fixed prefix
    ///   YYYYMM = year and month of invoice date
    ///   XXXXX  = 5-digit sequence number, resets each month
    /// 
    /// EXAMPLES:
    ///   INV-202502-00001  (first invoice in February 2025)
    ///   INV-202502-00002  (second invoice in February 2025)
    ///   INV-202503-00001  (first invoice in March 2025 — resets)
    /// </summary>
    public class InvoiceNumberEngine
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceNumberEngine(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Generates the next invoice number for the given date.
        /// Counts existing invoices in the same month and increments.
        /// </summary>
        public async Task<string> GenerateAsync(DateTime invoiceDate)
        {
            int count = await _invoiceRepository.GetInvoiceCountForMonthAsync(
                invoiceDate.Year, invoiceDate.Month);

            int sequence = count + 1;
            string yearMonth = invoiceDate.ToString("yyyyMM");

            return $"INV-{yearMonth}-{sequence:D5}";
        }
    }
}
