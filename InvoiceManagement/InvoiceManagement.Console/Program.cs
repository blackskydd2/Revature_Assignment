using InvoiceManagement.BLL.Engines;
using InvoiceManagement.BLL.Services.Implementations;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Context;
using InvoiceManagement.DAL.Models;
using InvoiceManagement.DAL.Repositories.Implementations;

// ════════════════════════════════════════════════════════════════════════════
//  DEPENDENCY SETUP
//  Wiring up the layers manually (no DI container in console — kept simple)
//  In Phase 2 (ASP.NET Core), use services.AddScoped<> for each of these.
// ════════════════════════════════════════════════════════════════════════════
var context         = new AppDbContext();
var invoiceRepo     = new InvoiceRepository(context);
var paymentRepo     = new PaymentRepository(context);
var numberEngine    = new InvoiceNumberEngine(invoiceRepo);
var calcEngine      = new InvoiceCalculationEngine();
var statusValidator = new InvoiceStatusValidator();
var invoiceService  = new InvoiceService(invoiceRepo, numberEngine, calcEngine, statusValidator);
var paymentService  = new PaymentService(paymentRepo, invoiceRepo, statusValidator);

// ════════════════════════════════════════════════════════════════════════════
//  ENSURE DB IS CREATED
// ════════════════════════════════════════════════════════════════════════════
await context.Database.EnsureCreatedAsync();
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("       INVOICE MANAGEMENT SYSTEM — Demo Console        ");
Console.WriteLine("═══════════════════════════════════════════════════════\n");

bool running = true;
while (running)
{
    ShowMenu();
    var choice = Console.ReadLine()?.Trim();

    switch (choice)
    {
        case "1": await CreateInvoiceFlow(); break;
        case "2": await ViewInvoiceFlow(); break;
        case "3": await RecordPaymentFlow(); break;
        case "4": await UpdateStatusFlow(); break;
        case "5": await ViewAllInvoicesFlow(); break;
        case "6": await AgingReportFlow(); break;
        case "7": await ArchiveInvoiceFlow(); break;
        case "8": await OverdueCheckFlow(); break;
        case "0": running = false; Console.WriteLine("\nGoodbye!"); break;
        default:  Console.WriteLine("Invalid option. Try again.\n"); break;
    }
}

// ── MENU ─────────────────────────────────────────────────────────────────────
void ShowMenu()
{
    Console.WriteLine("\n╔══════════════════════════════╗");
    Console.WriteLine("║         MAIN MENU            ║");
    Console.WriteLine("╠══════════════════════════════╣");
    Console.WriteLine("║  1. Create Invoice           ║");
    Console.WriteLine("║  2. View Invoice by Number   ║");
    Console.WriteLine("║  3. Record Payment           ║");
    Console.WriteLine("║  4. Update Invoice Status    ║");
    Console.WriteLine("║  5. View All Invoices        ║");
    Console.WriteLine("║  6. Aging Report             ║");
    Console.WriteLine("║  7. Archive Invoice          ║");
    Console.WriteLine("║  8. Check & Mark Overdue     ║");
    Console.WriteLine("║  0. Exit                     ║");
    Console.WriteLine("╚══════════════════════════════╝");
    Console.Write("Enter choice: ");
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 1: CREATE INVOICE
// ════════════════════════════════════════════════════════════════════════════
async Task CreateInvoiceFlow()
{
    Console.WriteLine("\n── CREATE INVOICE ──────────────────────────────────");

    Console.Write("Customer ID       : ");
    int customerId = int.Parse(Console.ReadLine() ?? "1");

    Console.Write("Quote ID (or 0 for manual): ");
    int quoteIdInput = int.Parse(Console.ReadLine() ?? "0");
    int? quoteId = quoteIdInput == 0 ? null : quoteIdInput;

    Console.Write("Payment Terms (Immediate/Net15/Net30/Net60/Net90) [Net30]: ");
    string termsInput = Console.ReadLine()?.Trim() ?? "Net30";
    if (!Enum.TryParse<PaymentTerms>(termsInput, out var terms))
        terms = PaymentTerms.Net30;

    Console.Write("Notes (optional): ");
    string? notes = Console.ReadLine();

    // Build invoice shell
    var invoice = new Invoice
    {
        CustomerId   = customerId,
        QuoteId      = quoteId,
        InvoiceDate  = DateTime.UtcNow,
        PaymentTerms = terms.ToString(),
        Notes        = string.IsNullOrWhiteSpace(notes) ? null : notes,
        CreatedBy    = "ConsoleUser"
    };

    // Collect line items
    var lineItems = new List<InvoiceLineItem>();
    Console.WriteLine("\nAdd Line Items (type 'done' when finished):");

    while (true)
    {
        Console.WriteLine($"\n  Line Item #{lineItems.Count + 1}");
        Console.Write("  Description (or 'done'): ");
        string? desc = Console.ReadLine();
        if (desc?.ToLower() == "done") break;

        Console.Write("  SKU (optional): ");
        string? sku = Console.ReadLine();

        Console.Write("  Quantity        : ");
        int qty = int.Parse(Console.ReadLine() ?? "1");

        Console.Write("  Unit Price (₹)  : ");
        decimal price = decimal.Parse(Console.ReadLine() ?? "0");

        Console.Write("  Discount (₹)    : ");
        decimal discount = decimal.Parse(Console.ReadLine() ?? "0");

        Console.Write("  Tax Rate (%)    : ");
        decimal taxRate = decimal.Parse(Console.ReadLine() ?? "0");

        lineItems.Add(new InvoiceLineItem
        {
            Description = desc ?? "",
            SKU         = string.IsNullOrWhiteSpace(sku) ? null : sku,
            Quantity    = qty,
            UnitPrice   = price,
            Discount    = discount,
            TaxRate     = taxRate
        });
    }

    if (lineItems.Count == 0)
    {
        Console.WriteLine("No line items added. Invoice not created.");
        return;
    }

    try
    {
        var created = await invoiceService.CreateInvoiceAsync(invoice, lineItems);
        Console.WriteLine("\n✓ Invoice Created Successfully!");
        PrintInvoiceSummary(created);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ Error: {ex.Message}");
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 2: VIEW INVOICE
// ════════════════════════════════════════════════════════════════════════════
async Task ViewInvoiceFlow()
{
    Console.WriteLine("\n── VIEW INVOICE ────────────────────────────────────");
    Console.Write("Enter Invoice Number (e.g. INV-202502-00001): ");
    string? num = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(num)) return;

    var invoice = await invoiceService.GetInvoiceByNumberAsync(num);
    if (invoice == null)
    {
        Console.WriteLine("Invoice not found.");
        return;
    }

    PrintInvoiceDetail(invoice);
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 3: RECORD PAYMENT
// ════════════════════════════════════════════════════════════════════════════
async Task RecordPaymentFlow()
{
    Console.WriteLine("\n── RECORD PAYMENT ──────────────────────────────────");
    Console.Write("Invoice ID: ");
    int invoiceId = int.Parse(Console.ReadLine() ?? "0");

    var invoice = await invoiceService.GetInvoiceByIdAsync(invoiceId);
    if (invoice == null) { Console.WriteLine("Invoice not found."); return; }

    Console.WriteLine($"Invoice:     {invoice.InvoiceNumber}");
    Console.WriteLine($"Grand Total: ₹{invoice.GrandTotal:N2}");
    Console.WriteLine($"Amount Paid: ₹{invoice.AmountPaid:N2}");
    Console.WriteLine($"Outstanding: ₹{invoice.OutstandingBalance:N2}");

    Console.Write("\nPayment Amount (₹): ");
    decimal amount = decimal.Parse(Console.ReadLine() ?? "0");

    Console.WriteLine("\nPayment Methods:");
    var methods = context.PaymentMethods.Where(m => m.IsActive).ToList();
    foreach (var m in methods)
        Console.WriteLine($"  {m.MethodId}. {m.MethodName}");
    Console.Write("Method ID: ");
    int methodId = int.Parse(Console.ReadLine() ?? "1");

    Console.Write("Reference Number (optional): ");
    string? reference = Console.ReadLine();

    try
    {
        var payment = await paymentService.RecordPaymentAsync(
            invoiceId, amount, methodId, reference);
        Console.WriteLine($"\n✓ Payment of ₹{payment.PaymentAmount:N2} recorded (ID: {payment.PaymentId})");

        var updated = await invoiceService.GetInvoiceByIdAsync(invoiceId);
        Console.WriteLine($"  Invoice Status now: {updated?.Status}");
        Console.WriteLine($"  Outstanding Balance: ₹{updated?.OutstandingBalance:N2}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ Error: {ex.Message}");
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 4: UPDATE STATUS
// ════════════════════════════════════════════════════════════════════════════
async Task UpdateStatusFlow()
{
    Console.WriteLine("\n── UPDATE INVOICE STATUS ───────────────────────────");
    Console.Write("Invoice ID: ");
    int id = int.Parse(Console.ReadLine() ?? "0");

    var invoice = await invoiceService.GetInvoiceByIdAsync(id);
    if (invoice == null) { Console.WriteLine("Invoice not found."); return; }

    Console.WriteLine($"Current Status: {invoice.Status}");
    Console.WriteLine("New Status (Draft/Sent/Overdue/PartiallyPaid/Paid/Cancelled): ");
    string? statusInput = Console.ReadLine()?.Trim();

    if (!Enum.TryParse<InvoiceStatus>(statusInput, out var newStatus))
    {
        Console.WriteLine("Invalid status.");
        return;
    }

    try
    {
        await invoiceService.UpdateInvoiceStatusAsync(id, newStatus);
        Console.WriteLine($"✓ Status updated to: {newStatus}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error: {ex.Message}");
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 5: VIEW ALL INVOICES
// ════════════════════════════════════════════════════════════════════════════
async Task ViewAllInvoicesFlow()
{
    Console.WriteLine("\n── ALL INVOICES ────────────────────────────────────");
    var invoices = await invoiceService.GetAllInvoicesAsync();

    if (!invoices.Any()) { Console.WriteLine("No invoices found."); return; }

    Console.WriteLine($"\n{"#",-4} {"Invoice No",-22} {"Customer",-10} {"Date",-12} {"Status",-15} {"Grand Total",12} {"Outstanding",12}");
    Console.WriteLine(new string('─', 95));

    foreach (var inv in invoices)
    {
        Console.WriteLine($"{inv.InvoiceId,-4} {inv.InvoiceNumber,-22} {inv.CustomerId,-10} " +
                          $"{inv.InvoiceDate:dd-MM-yyyy,-12} {inv.Status,-15} " +
                          $"₹{inv.GrandTotal,11:N2} ₹{inv.OutstandingBalance,11:N2}");
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 6: AGING REPORT
// ════════════════════════════════════════════════════════════════════════════
async Task AgingReportFlow()
{
    Console.WriteLine("\n── AGING REPORT ────────────────────────────────────");
    var report = await invoiceService.GetAgingReportAsync();
    decimal dso = await invoiceService.GetDSOAsync(30);

    foreach (var bucket in report)
    {
        Console.WriteLine($"\n  [{bucket.Key}] — {bucket.Value.Count} invoice(s)");
        decimal total = bucket.Value.Sum(i => i.OutstandingBalance);
        Console.WriteLine($"  Total Outstanding: ₹{total:N2}");

        foreach (var inv in bucket.Value)
            Console.WriteLine($"    {inv.InvoiceNumber} | Customer {inv.CustomerId} | Due: {inv.DueDate:dd-MM-yyyy} | ₹{inv.OutstandingBalance:N2}");
    }

    Console.WriteLine($"\n  DSO (30-day period): {dso:N1} days");
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 7: ARCHIVE
// ════════════════════════════════════════════════════════════════════════════
async Task ArchiveInvoiceFlow()
{
    Console.WriteLine("\n── ARCHIVE INVOICE ─────────────────────────────────");
    Console.Write("Invoice ID to archive: ");
    int id = int.Parse(Console.ReadLine() ?? "0");

    try
    {
        await invoiceService.ArchiveInvoiceAsync(id);
        Console.WriteLine("✓ Invoice archived.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error: {ex.Message}");
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  FLOW 8: OVERDUE CHECK
// ════════════════════════════════════════════════════════════════════════════
async Task OverdueCheckFlow()
{
    Console.WriteLine("\n── OVERDUE STATUS UPDATE ───────────────────────────");
    await invoiceService.UpdateOverdueStatusesAsync();
    var overdue = await invoiceService.GetOverdueInvoicesAsync();
    Console.WriteLine($"✓ Done. {overdue.Count()} invoice(s) currently overdue.");
}

// ════════════════════════════════════════════════════════════════════════════
//  PRINT HELPERS
// ════════════════════════════════════════════════════════════════════════════
void PrintInvoiceSummary(Invoice inv)
{
    Console.WriteLine($"\n  Invoice No   : {inv.InvoiceNumber}");
    Console.WriteLine($"  Customer ID  : {inv.CustomerId}");
    Console.WriteLine($"  Invoice Date : {inv.InvoiceDate:dd-MM-yyyy}");
    Console.WriteLine($"  Due Date     : {inv.DueDate:dd-MM-yyyy}");
    Console.WriteLine($"  Terms        : {inv.PaymentTerms}");
    Console.WriteLine($"  Status       : {inv.Status}");
    Console.WriteLine($"  Sub Total    : ₹{inv.SubTotal:N2}");
    Console.WriteLine($"  Discount     : ₹{inv.Discount:N2}");
    Console.WriteLine($"  Tax          : ₹{inv.Tax:N2}");
    Console.WriteLine($"  Grand Total  : ₹{inv.GrandTotal:N2}");
}

void PrintInvoiceDetail(Invoice inv)
{
    PrintInvoiceSummary(inv);

    Console.WriteLine($"\n  ── Line Items ({inv.LineItems.Count}) ──");
    foreach (var li in inv.LineItems)
    {
        Console.WriteLine($"    [{li.LineItemId}] {li.Description}");
        Console.WriteLine($"         Qty:{li.Quantity} × ₹{li.UnitPrice:N2}  Disc:₹{li.Discount:N2}  Tax:{li.TaxRate}%  → ₹{li.LineTotal:N2}");
    }

    Console.WriteLine($"\n  ── Payments ({inv.Payments.Count}) ──");
    foreach (var p in inv.Payments)
    {
        Console.WriteLine($"    [{p.PaymentId}] {p.PaymentDate:dd-MM-yyyy} | {p.Method?.MethodName ?? "?"} | ₹{p.PaymentAmount:N2} | Ref: {p.ReferenceNumber ?? "—"}");
    }
}
