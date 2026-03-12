using InvoiceManagement.BLL;
using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.DAL.Context;
using InvoiceManagement.DAL.Documents;
using InvoiceManagement.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static IServiceProvider _provider = null!;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var mongoSettings = config.GetSection("MongoDB").Get<MongoDbSettings>() ?? new MongoDbSettings();
        var services = new ServiceCollection();
        services.AddInvoiceManagement(mongoSettings);
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        _provider = services.BuildServiceProvider();

        PrintHeader("Invoice Management System  MongoDB");

        await SeedPaymentMethodsAsync();
        await SeedDummyDataAsync();
        await RunMenuAsync();
    }

    // ══════════════════════════════════════════════════════════════
    //  SEED  Payment Methods
    // ══════════════════════════════════════════════════════════════
    static async Task SeedPaymentMethodsAsync()
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentMethodRepository>();
        var existing = (await repo.GetAllActiveAsync()).ToList();
        if (existing.Any()) return;

        Console.Write("Seeding payment methods... ");
        foreach (var name in new[] { "Cash", "Check", "Credit Card", "Bank Transfer", "Online Payment" })
            await repo.CreateAsync(new PaymentMethodDocument { MethodName = name });
        Println("Done", ConsoleColor.Green);
    }

    // ══════════════════════════════════════════════════════════════
    //  SEED  Rich Dummy Data (runs only once)
    // ══════════════════════════════════════════════════════════════
    static async Task SeedDummyDataAsync()
    {
        using var scope = _provider.CreateScope();
        var invoiceSvc  = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        var customerSvc = scope.ServiceProvider.GetRequiredService<ICustomerService>();
        var paymentSvc  = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var methodRepo  = scope.ServiceProvider.GetRequiredService<IPaymentMethodRepository>();

        var existing = (await invoiceSvc.GetAllAsync(true)).ToList();
        if (existing.Count > 0)
        {
            Println($"  Found {existing.Count} existing invoices  skipping seed.\n", ConsoleColor.DarkGray);
            return;
        }

        Console.WriteLine("\nSeeding dummy data...");
        var methods      = (await methodRepo.GetAllActiveAsync()).ToList();
        var bankTransfer = methods.First(m => m.MethodName == "Bank Transfer");
        var creditCard   = methods.First(m => m.MethodName == "Credit Card");
        var online       = methods.First(m => m.MethodName == "Online Payment");
        var cash         = methods.First(m => m.MethodName == "Cash");

        // ── 7 Customers ──────────────────────────────────────────
        var customers = new List<CustomerDocument>();
        var seedCustomers = new[]
        {
            new CreateCustomerRequest { CustomerName = "Acme Corporation",      Email = "billing@acme.com",         Phone = "+1-555-0101", Industry = "Technology",    BillingStreet = "123 Main St",        BillingCity = "New York",    BillingState = "NY", BillingPostalCode = "10001", BillingCountry = "USA" },
            new CreateCustomerRequest { CustomerName = "Globex Industries",     Email = "accounts@globex.com",      Phone = "+1-555-0202", Industry = "Manufacturing", BillingStreet = "456 Industrial Ave", BillingCity = "Chicago",     BillingState = "IL", BillingPostalCode = "60601", BillingCountry = "USA" },
            new CreateCustomerRequest { CustomerName = "Initech Solutions",     Email = "finance@initech.io",       Phone = "+1-555-0303", Industry = "Consulting",    BillingStreet = "789 Commerce Blvd",  BillingCity = "Austin",      BillingState = "TX", BillingPostalCode = "73301", BillingCountry = "USA" },
            new CreateCustomerRequest { CustomerName = "Umbrella Enterprises",  Email = "billing@umbrella.net",     Phone = "+1-555-0404", Industry = "Healthcare",    BillingStreet = "321 Park Ave",       BillingCity = "Boston",      BillingState = "MA", BillingPostalCode = "02101", BillingCountry = "USA" },
            new CreateCustomerRequest { CustomerName = "Stark Industries",      Email = "ap@stark.com",             Phone = "+1-555-0505", Industry = "Defense",       BillingStreet = "1 Stark Tower",      BillingCity = "Los Angeles", BillingState = "CA", BillingPostalCode = "90001", BillingCountry = "USA" },
            new CreateCustomerRequest { CustomerName = "Wayne Enterprises",     Email = "finance@wayne.com",        Phone = "+1-555-0606", Industry = "Conglomerate",  BillingStreet = "1007 Mountain Dr",   BillingCity = "Gotham",      BillingState = "NJ", BillingPostalCode = "07001", BillingCountry = "USA" },
            new CreateCustomerRequest { CustomerName = "Dunder Mifflin Paper",  Email = "billing@dundermifflin.com",Phone = "+1-555-0707", Industry = "Paper Goods",   BillingStreet = "1725 Slough Ave",    BillingCity = "Scranton",    BillingState = "PA", BillingPostalCode = "18503", BillingCountry = "USA" },
        };
        foreach (var cr in seedCustomers)
            customers.Add(await customerSvc.CreateAsync(cr));
        Console.WriteLine($"  + {customers.Count} customers");

        // ── Helper: build invoice, send it, optionally pay it ────
        async Task MakeInvoice(
            string cid, DateTime date, string terms,
            List<CreateLineItemRequest> items,
            decimal disc = 0, decimal taxPct = 0,
            string? payMethodId = null, decimal payFraction = 1m,
            bool forceOverdue = false)
        {
            var inv = await invoiceSvc.CreateAsync(new CreateInvoiceRequest
            {
                CustomerId = cid, InvoiceDate = date, PaymentTerms = terms,
                InvoiceDiscountAmount = disc, InvoiceTaxRatePercent = taxPct,
                LineItems = items, CreatedBy = "Seed", Notes = "Seeded invoice"
            });
            await invoiceSvc.ChangeStatusAsync(inv.Id, InvoiceStatus.Sent, "Seed");
            if (forceOverdue) await invoiceSvc.UpdateOverdueStatusesAsync();
            if (payMethodId != null)
            {
                inv = (await invoiceSvc.GetByIdAsync(inv.Id))!;
                var amount = Math.Round(inv.GrandTotal * payFraction, 2);
                await paymentSvc.ApplyPaymentAsync(new ApplyPaymentRequest
                {
                    InvoiceId = inv.Id, PaymentAmount = amount,
                    PaymentDate = DateTime.UtcNow, PaymentMethodId = payMethodId,
                    ReferenceNumber = $"REF-{inv.InvoiceNumber}", ReceivedBy = "Seed"
                });
            }
        }

        // INV-1  Acme  PAID in full
        await MakeInvoice(customers[0].Id, DateTime.UtcNow.AddDays(-20), "Net 30", new() {
            new() { Description = "Enterprise Software License (Annual)", Quantity = 3,  UnitPrice = 1200m, DiscountPercent = 5,  TaxRatePercent = 10 },
            new() { Description = "Cloud Infrastructure Setup",           Quantity = 1,  UnitPrice = 2500m, TaxRatePercent = 10 },
            new() { Description = "Priority Support (12 months)",         Quantity = 1,  UnitPrice = 800m,  DiscountPercent = 10 }
        }, disc: 150, payMethodId: bankTransfer.Id, payFraction: 1m);

        // INV-2  Acme  PARTIALLY PAID
        await MakeInvoice(customers[0].Id, DateTime.UtcNow.AddDays(-10), "Net 30", new() {
            new() { Description = "Data Analytics Module",      Quantity = 2, UnitPrice = 950m,  TaxRatePercent = 10 },
            new() { Description = "Training Sessions (5 days)", Quantity = 5, UnitPrice = 400m,  DiscountPercent = 5 }
        }, taxPct: 5, payMethodId: creditCard.Id, payFraction: 0.5m);

        // INV-3  Acme  DRAFT (recurring SaaS)
        await invoiceSvc.CreateAsync(new CreateInvoiceRequest {
            CustomerId = customers[0].Id, InvoiceDate = DateTime.UtcNow, PaymentTerms = "Net 15",
            IsRecurring = true, RecurringFrequency = "Monthly", CreatedBy = "Seed",
            LineItems = new() { new() { Description = "SaaS Platform Subscription (Monthly)", Quantity = 1, UnitPrice = 499m, TaxRatePercent = 10 } }
        });

        // INV-4  Globex  PAID
        await MakeInvoice(customers[1].Id, DateTime.UtcNow.AddDays(-35), "Net 15", new() {
            new() { Description = "Industrial IoT Sensors (x50)",  Quantity = 50, UnitPrice = 75m,   TaxRatePercent = 8 },
            new() { Description = "Installation & Commissioning",  Quantity = 1,  UnitPrice = 3000m, TaxRatePercent = 8 },
            new() { Description = "Annual Maintenance Contract",   Quantity = 1,  UnitPrice = 1500m, DiscountPercent = 10, TaxRatePercent = 8 }
        }, payMethodId: online.Id, payFraction: 1m);

        // INV-5  Globex  OVERDUE (31-60 days)
        await MakeInvoice(customers[1].Id, DateTime.UtcNow.AddDays(-60), "Net 15", new() {
            new() { Description = "Robotics Assembly Module",   Quantity = 2, UnitPrice = 4500m, TaxRatePercent = 8 },
            new() { Description = "Remote Diagnostics License", Quantity = 1, UnitPrice = 600m }
        }, forceOverdue: true);

        // INV-6  Initech  OVERDUE (90+ days)
        await MakeInvoice(customers[2].Id, DateTime.UtcNow.AddDays(-120), "Net 30", new() {
            new() { Description = "Business Process Consulting (40 hrs)", Quantity = 40, UnitPrice = 250m },
            new() { Description = "Process Documentation Package",        Quantity = 1,  UnitPrice = 1200m }
        }, forceOverdue: true);

        // INV-7  Initech  DRAFT
        await invoiceSvc.CreateAsync(new CreateInvoiceRequest {
            CustomerId = customers[2].Id, InvoiceDate = DateTime.UtcNow, PaymentTerms = "Net 30", CreatedBy = "Seed",
            LineItems = new() {
                new() { Description = "ERP Integration Services", Quantity = 20, UnitPrice = 180m },
                new() { Description = "User License (5 seats)",   Quantity = 5,  UnitPrice = 300m, DiscountPercent = 10 }
            }
        });

        // INV-8  Umbrella  PAID
        await MakeInvoice(customers[3].Id, DateTime.UtcNow.AddDays(-25), "Net 45", new() {
            new() { Description = "Patient Management Software", Quantity = 1, UnitPrice = 8500m, TaxRatePercent = 5 },
            new() { Description = "HIPAA Compliance Audit",      Quantity = 1, UnitPrice = 3200m, TaxRatePercent = 5 },
            new() { Description = "Staff Training (2 days)",     Quantity = 2, UnitPrice = 600m }
        }, disc: 500, payMethodId: bankTransfer.Id, payFraction: 1m);

        // INV-9  Umbrella  PARTIALLY PAID
        await MakeInvoice(customers[3].Id, DateTime.UtcNow.AddDays(-5), "Net 30", new() {
            new() { Description = "Telemedicine Platform License", Quantity = 1, UnitPrice = 5000m, TaxRatePercent = 5 },
            new() { Description = "API Integration (HL7 FHIR)",    Quantity = 1, UnitPrice = 2200m, TaxRatePercent = 5 }
        }, payMethodId: creditCard.Id, payFraction: 0.3m);

        // INV-10  Stark  PAID
        await MakeInvoice(customers[4].Id, DateTime.UtcNow.AddDays(-15), "Net 15", new() {
            new() { Description = "Advanced AI Research License", Quantity = 1, UnitPrice = 25000m, DiscountPercent = 15, TaxRatePercent = 10 },
            new() { Description = "Quantum Computing Module",     Quantity = 1, UnitPrice = 12000m, TaxRatePercent = 10 }
        }, payMethodId: bankTransfer.Id, payFraction: 1m);

        // INV-11  Stark  OVERDUE (31-60 days)
        await MakeInvoice(customers[4].Id, DateTime.UtcNow.AddDays(-50), "Net 15", new() {
            new() { Description = "Cybersecurity Penetration Testing", Quantity = 1, UnitPrice = 7500m },
            new() { Description = "Security Audit Report",             Quantity = 1, UnitPrice = 2000m }
        }, forceOverdue: true);

        // INV-12  Wayne  SENT (current, large deal)
        await MakeInvoice(customers[5].Id, DateTime.UtcNow.AddDays(-3), "Net 60", new() {
            new() { Description = "Gotham Smart City Infrastructure", Quantity = 1, UnitPrice = 50000m, DiscountPercent = 5, TaxRatePercent = 8 },
            new() { Description = "Fiber Optic Network Installation",  Quantity = 1, UnitPrice = 15000m, TaxRatePercent = 8 },
            new() { Description = "Command Center Software (Annual)",  Quantity = 1, UnitPrice = 8000m,  TaxRatePercent = 8 }
        }, disc: 2000);

        // INV-13  Wayne  PARTIALLY PAID
        await MakeInvoice(customers[5].Id, DateTime.UtcNow.AddDays(-30), "Net 30", new() {
            new() { Description = "Bat-Signal Maintenance Contract", Quantity = 1, UnitPrice = 1200m },
            new() { Description = "Encrypted Comms License",         Quantity = 5, UnitPrice = 350m, DiscountPercent = 5 }
        }, payMethodId: cash.Id, payFraction: 0.6m);

        // INV-14  Dunder Mifflin  PAID
        await MakeInvoice(customers[6].Id, DateTime.UtcNow.AddDays(-40), "Net 30", new() {
            new() { Description = "Office Management Software (1 yr)", Quantity = 1, UnitPrice = 1800m, TaxRatePercent = 6 },
            new() { Description = "Printer Fleet Maintenance",          Quantity = 3, UnitPrice = 250m,  TaxRatePercent = 6 },
            new() { Description = "Paper Inventory System Plugin",      Quantity = 1, UnitPrice = 400m,  DiscountPercent = 15, TaxRatePercent = 6 }
        }, payMethodId: online.Id, payFraction: 1m);

        // INV-15  Dunder Mifflin  OVERDUE (1-30 days)
        await MakeInvoice(customers[6].Id, DateTime.UtcNow.AddDays(-40), "Net 15", new() {
            new() { Description = "HR Portal License (Annual)",  Quantity = 1, UnitPrice = 950m },
            new() { Description = "Payroll Integration Module",  Quantity = 1, UnitPrice = 600m, TaxRatePercent = 6 }
        }, forceOverdue: true);

        var allInv = (await invoiceSvc.GetAllAsync(true)).ToList();
        Console.WriteLine($"  + {allInv.Count} invoices (Paid, PartiallyPaid, Overdue, Sent, Draft)");
        Println("  Dummy data seed complete!\n", ConsoleColor.Green);
    }

    // ══════════════════════════════════════════════════════════════
    //  MAIN MENU
    // ══════════════════════════════════════════════════════════════
    static async Task RunMenuAsync()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n+==========================================+");
            Console.WriteLine("|     INVOICE MANAGEMENT  MAIN MENU      |");
            Console.WriteLine("+==========================================+");
            Console.ResetColor();
            Console.WriteLine("|  CUSTOMERS                               |");
            Console.WriteLine("|   1.  Create New Customer                |");
            Console.WriteLine("|   2.  List All Customers                 |");
            Console.WriteLine("|   3.  Search Customer by Name            |");
            Console.WriteLine("|   4.  View Customer Invoices             |");
            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|  INVOICES                                |");
            Console.WriteLine("|   5.  Create New Invoice                 |");
            Console.WriteLine("|   6.  List All Invoices                  |");
            Console.WriteLine("|   7.  View Invoice Details               |");
            Console.WriteLine("|   8.  Mark Invoice as Sent               |");
            Console.WriteLine("|   9.  Change Invoice Status              |");
            Console.WriteLine("|   10. Archive Invoice                    |");
            Console.WriteLine("|   11. Delete Draft Invoice               |");
            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|  PAYMENTS                                |");
            Console.WriteLine("|   12. Apply Payment to Invoice           |");
            Console.WriteLine("|   13. View Payments for Invoice          |");
            Console.WriteLine("|   14. Reverse a Payment                  |");
            Console.WriteLine("|   15. Payments by Date Range             |");
            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|  REPORTS & ANALYTICS                     |");
            Console.WriteLine("|   16. Aging Report                       |");
            Console.WriteLine("|   17. DSO Report                         |");
            Console.WriteLine("|   18. Overdue Invoices                   |");
            Console.WriteLine("|   19. Monthly Analytics Snapshot         |");
            Console.WriteLine("|   20. Audit Trail for Invoice            |");
            Console.WriteLine("|   21. Reconciliation Records             |");
            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|   0.  Exit                               |");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("+==========================================+");
            Console.ResetColor();
            Console.Write("\n  Enter option: ");
            var input = Console.ReadLine()?.Trim();

            try
            {
                switch (input)
                {
                    case "1":  await Menu_CreateCustomer();      break;
                    case "2":  await Menu_ListCustomers();       break;
                    case "3":  await Menu_SearchCustomer();      break;
                    case "4":  await Menu_CustomerInvoices();    break;
                    case "5":  await Menu_CreateInvoice();       break;
                    case "6":  await Menu_ListInvoices();        break;
                    case "7":  await Menu_ViewInvoiceDetails();  break;
                    case "8":  await Menu_MarkSent();            break;
                    case "9":  await Menu_ChangeStatus();        break;
                    case "10": await Menu_Archive();             break;
                    case "11": await Menu_DeleteDraft();         break;
                    case "12": await Menu_ApplyPayment();        break;
                    case "13": await Menu_ViewPayments();        break;
                    case "14": await Menu_ReversePayment();      break;
                    case "15": await Menu_PaymentsByDate();      break;
                    case "16": await Menu_AgingReport();         break;
                    case "17": await Menu_DsoReport();           break;
                    case "18": await Menu_OverdueInvoices();     break;
                    case "19": await Menu_MonthlySnapshot();     break;
                    case "20": await Menu_AuditTrail();          break;
                    case "21": await Menu_Reconciliation();      break;
                    case "0":  Println("\n  Goodbye!", ConsoleColor.Green); return;
                    default:   Println("  Invalid option. Enter 0-21.", ConsoleColor.Red); break;
                }
            }
            catch (Exception ex)
            {
                Println($"\n  [ERROR] {ex.Message}", ConsoleColor.Red);
            }

            Console.Write("\n  Press Enter to return to menu...");
            Console.ReadLine();
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  CUSTOMER HANDLERS
    // ══════════════════════════════════════════════════════════════

    static async Task Menu_CreateCustomer()
    {
        Section("Create New Customer");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        Console.Write("  Customer Name : "); var name = Required();
        Console.Write("  Email         : "); var email = Console.ReadLine();
        Console.Write("  Phone         : "); var phone = Console.ReadLine();
        Console.Write("  Industry      : "); var industry = Console.ReadLine();
        Console.Write("  Street        : "); var street = Console.ReadLine();
        Console.Write("  City          : "); var city = Console.ReadLine();
        Console.Write("  State         : "); var state = Console.ReadLine();
        Console.Write("  Postal Code   : "); var postal = Console.ReadLine();
        Console.Write("  Country       : "); var country = Console.ReadLine();

        var c = await svc.CreateAsync(new CreateCustomerRequest {
            CustomerName = name, Email = email, Phone = phone, Industry = industry,
            BillingStreet = street, BillingCity = city, BillingState = state,
            BillingPostalCode = postal, BillingCountry = country
        });

        Println($"\n  Customer created!", ConsoleColor.Green);
        Console.WriteLine($"    ID   : {c.Id}");
        Console.WriteLine($"    Name : {c.CustomerName}");
        Console.WriteLine($"    Email: {c.Email}");
    }

    static async Task Menu_ListCustomers()
    {
        Section("All Customers");
        using var scope = _provider.CreateScope();
        var list = (await scope.ServiceProvider.GetRequiredService<ICustomerService>().GetAllAsync()).ToList();

        Console.WriteLine($"\n  {"#",-4} {"Name",-28} {"Email",-30} {"Industry",-18} {"Phone"}");
        Console.WriteLine(new string('-', 95));
        for (int i = 0; i < list.Count; i++)
            Console.WriteLine($"  {i+1,-4} {list[i].CustomerName,-28} {list[i].Email ?? "-",-30} {list[i].Industry ?? "-",-18} {list[i].Phone ?? "-"}");
        Console.WriteLine($"\n  Total: {list.Count} active customers");
    }

    static async Task Menu_SearchCustomer()
    {
        Section("Search Customer by Name");
        using var scope = _provider.CreateScope();
        Console.Write("  Search: "); var q = Required();
        var results = (await scope.ServiceProvider.GetRequiredService<ICustomerService>().SearchAsync(q)).ToList();

        if (!results.Any()) { Println("  No customers found.", ConsoleColor.Yellow); return; }
        foreach (var c in results)
        {
            Console.WriteLine($"\n  ID    : {c.Id}");
            Console.WriteLine($"  Name  : {c.CustomerName}");
            Console.WriteLine($"  Email : {c.Email}   Phone: {c.Phone}");
            Console.WriteLine($"  Addr  : {c.BillingAddress?.Street}, {c.BillingAddress?.City}, {c.BillingAddress?.State}");
        }
    }

    static async Task Menu_CustomerInvoices()
    {
        Section("Customer Invoices");
        using var scope = _provider.CreateScope();
        var customerSvc = scope.ServiceProvider.GetRequiredService<ICustomerService>();
        var invoiceSvc  = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

        var customers = (await customerSvc.GetAllAsync()).ToList();
        Console.WriteLine("\n  Select Customer:");
        for (int i = 0; i < customers.Count; i++)
            Console.WriteLine($"    {i+1}. {customers[i].CustomerName}");

        Console.Write("\n  Number: ");
        if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > customers.Count)
        { Println("  Invalid selection.", ConsoleColor.Red); return; }

        var c = customers[idx - 1];
        var invoices = (await invoiceSvc.GetByCustomerAsync(c.Id)).ToList();
        Console.WriteLine($"\n  {c.CustomerName} — {invoices.Count} invoice(s):\n");
        Console.WriteLine($"  {"Invoice #",-22} {"Date",-12} {"Status",-16} {"Total",10} {"Balance",10} {"Due",12}");
        Console.WriteLine(new string('-', 86));
        foreach (var inv in invoices)
        {
            Console.ForegroundColor = StatusColor(inv.Status);
            Console.WriteLine($"  {inv.InvoiceNumber,-22} {inv.InvoiceDate:yyyy-MM-dd,-12} {inv.Status,-16} {inv.GrandTotal,10:C} {inv.BalanceDue,10:C} {inv.DueDate:yyyy-MM-dd,12}");
            Console.ResetColor();
        }
        Console.WriteLine($"\n  Revenue: {invoices.Sum(i => i.GrandTotal):C}   Collected: {invoices.Sum(i => i.AmountPaid):C}   Outstanding: {invoices.Sum(i => i.BalanceDue):C}");
    }

    // ══════════════════════════════════════════════════════════════
    //  INVOICE HANDLERS
    // ══════════════════════════════════════════════════════════════

    static async Task Menu_CreateInvoice()
    {
        Section("Create New Invoice");
        using var scope = _provider.CreateScope();
        var customerSvc = scope.ServiceProvider.GetRequiredService<ICustomerService>();
        var invoiceSvc  = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

        var customers = (await customerSvc.GetAllAsync()).ToList();
        Console.WriteLine("\n  Customers:");
        for (int i = 0; i < customers.Count; i++)
            Console.WriteLine($"    {i+1}. {customers[i].CustomerName}");

        Console.Write("\n  Select customer: ");
        if (!int.TryParse(Console.ReadLine(), out int cidx) || cidx < 1 || cidx > customers.Count)
        { Println("  Invalid.", ConsoleColor.Red); return; }

        Console.Write("  Payment Terms [Net 30]: ");
        var terms = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(terms)) terms = "Net 30";

        Console.Write("  Invoice discount amount [0]: ");
        decimal.TryParse(Console.ReadLine(), out decimal disc);
        Console.Write("  Invoice tax rate % [0]: ");
        decimal.TryParse(Console.ReadLine(), out decimal taxPct);
        Console.Write("  Notes: ");
        var notes = Console.ReadLine();

        var items = new List<CreateLineItemRequest>();
        Console.WriteLine("\n  Add Line Items (blank description = done):");
        int n = 1;
        while (true)
        {
            Console.Write($"  Item {n} Description  : ");
            var desc = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(desc)) break;
            Console.Write($"  Item {n} Quantity     : "); decimal.TryParse(Console.ReadLine(), out decimal qty);
            Console.Write($"  Item {n} Unit Price   : "); decimal.TryParse(Console.ReadLine(), out decimal price);
            Console.Write($"  Item {n} Discount %   [0]: "); decimal.TryParse(Console.ReadLine(), out decimal itemDisc);
            Console.Write($"  Item {n} Tax %        [0]: "); decimal.TryParse(Console.ReadLine(), out decimal itemTax);
            items.Add(new() { Description = desc, Quantity = qty, UnitPrice = price, DiscountPercent = itemDisc, TaxRatePercent = itemTax });
            n++;
        }

        if (!items.Any()) { Println("  No items — cancelled.", ConsoleColor.Yellow); return; }

        var inv = await invoiceSvc.CreateAsync(new CreateInvoiceRequest {
            CustomerId = customers[cidx-1].Id, InvoiceDate = DateTime.UtcNow,
            PaymentTerms = terms, InvoiceDiscountAmount = disc, InvoiceTaxRatePercent = taxPct,
            Notes = notes, LineItems = items, CreatedBy = "User"
        });

        Println($"\n  Invoice created: {inv.InvoiceNumber}", ConsoleColor.Green);
        Console.WriteLine($"  Status: {inv.Status}  |  Total: {inv.GrandTotal:C}  |  Due: {inv.DueDate:yyyy-MM-dd}");
    }

    static async Task Menu_ListInvoices()
    {
        Section("All Invoices");
        using var scope = _provider.CreateScope();
        Console.Write("  Include archived? (y/N): ");
        bool archived = Console.ReadLine()?.Trim().ToLower() == "y";
        var list = (await scope.ServiceProvider.GetRequiredService<IInvoiceService>().GetAllAsync(archived)).ToList();

        Console.WriteLine($"\n  {"#",-4} {"Invoice #",-22} {"Customer",-24} {"Status",-16} {"Total",10} {"Balance",10} {"Due",12}");
        Console.WriteLine(new string('-', 102));
        for (int i = 0; i < list.Count; i++)
        {
            Console.ForegroundColor = StatusColor(list[i].Status);
            Console.WriteLine($"  {i+1,-4} {list[i].InvoiceNumber,-22} {list[i].CustomerName,-24} {list[i].Status,-16} {list[i].GrandTotal,10:C} {list[i].BalanceDue,10:C} {list[i].DueDate:yyyy-MM-dd,12}");
            Console.ResetColor();
        }
        Console.WriteLine($"\n  {list.Count} invoices  |  Revenue: {list.Sum(i=>i.GrandTotal):C}  |  Outstanding: {list.Sum(i=>i.BalanceDue):C}");
    }

    static async Task Menu_ViewInvoiceDetails()
    {
        Section("Invoice Details");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();

        Console.Write("  Invoice Number: ");
        var inv = await svc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }

        Console.WriteLine();
        Console.WriteLine($"  Invoice  : {inv.InvoiceNumber}");
        Console.WriteLine($"  Customer : {inv.CustomerSnapshot.CustomerName}");
        Console.WriteLine($"  Email    : {inv.CustomerSnapshot.Email}");
        Console.WriteLine($"  Address  : {inv.CustomerSnapshot.BillingAddress}");
        Console.WriteLine($"  Date     : {inv.InvoiceDate:yyyy-MM-dd}   Due: {inv.DueDate:yyyy-MM-dd}   Terms: {inv.PaymentTerms}");
        Console.ForegroundColor = StatusColor(inv.Status);
        Console.WriteLine($"  Status   : {inv.Status}");
        Console.ResetColor();
        Console.WriteLine($"  Recurring: {(inv.IsRecurring ? $"Yes ({inv.RecurringFrequency})" : "No")}");
        Console.WriteLine($"  Notes    : {inv.Notes ?? "-"}");

        Console.WriteLine($"\n  {"Description",-40} {"Qty",5} {"Price",10} {"Disc",9} {"Tax",9} {"Total",10}");
        Console.WriteLine(new string('-', 87));
        foreach (var li in inv.LineItems)
            Console.WriteLine($"  {li.Description,-40} {li.Quantity,5} {li.UnitPrice,10:C} {li.DiscountAmount,9:C} {li.TaxAmount,9:C} {li.LineTotal,10:C}");

        Console.WriteLine(new string('-', 87));
        Console.WriteLine($"  {"Sub Total",-55} {inv.SubTotal,10:C}");
        Console.WriteLine($"  {"Discount",-55} {inv.DiscountAmount,10:C}");
        Console.WriteLine($"  {"Tax",-55} {inv.TaxAmount,10:C}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {"GRAND TOTAL",-55} {inv.GrandTotal,10:C}");
        Console.ResetColor();
        Console.ForegroundColor = inv.BalanceDue > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
        Console.WriteLine($"  {"Amount Paid",-55} {inv.AmountPaid,10:C}");
        Console.WriteLine($"  {"BALANCE DUE",-55} {inv.BalanceDue,10:C}");
        Console.ResetColor();
    }

    static async Task Menu_MarkSent()
    {
        Section("Mark Invoice as Sent");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        Console.Write("  Invoice Number: ");
        var inv = await svc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }
        Console.Write("  Recipient Email: ");
        await svc.MarkSentAsync(inv.Id, Required(), "User");
        Println($"  Invoice {inv.InvoiceNumber} marked Sent.", ConsoleColor.Green);
    }

    static async Task Menu_ChangeStatus()
    {
        Section("Change Invoice Status");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        Console.Write("  Invoice Number: ");
        var inv = await svc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }
        Console.WriteLine($"  Current: {inv.Status}");
        Console.WriteLine("  Options: Draft | Sent | Viewed | PartiallyPaid | Paid | Overdue | Cancelled | Archived");
        Console.Write("  New Status: ");
        await svc.ChangeStatusAsync(inv.Id, Required(), "User");
        Println("  Status updated.", ConsoleColor.Green);
    }

    static async Task Menu_Archive()
    {
        Section("Archive Invoice");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        Console.Write("  Invoice Number: ");
        var inv = await svc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }
        Console.Write($"  Archive {inv.InvoiceNumber}? (y/N): ");
        if (Console.ReadLine()?.Trim().ToLower() != "y") { Console.WriteLine("  Cancelled."); return; }
        await svc.ArchiveAsync(inv.Id, "User");
        Println("  Archived.", ConsoleColor.Green);
    }

    static async Task Menu_DeleteDraft()
    {
        Section("Delete Draft Invoice");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        Console.Write("  Invoice Number: ");
        var inv = await svc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }
        if (inv.Status != InvoiceStatus.Draft) { Println($"  Only Drafts can be deleted. Status: {inv.Status}", ConsoleColor.Red); return; }
        Console.Write($"  Permanently delete {inv.InvoiceNumber}? (y/N): ");
        if (Console.ReadLine()?.Trim().ToLower() != "y") { Console.WriteLine("  Cancelled."); return; }
        await svc.DeleteDraftAsync(inv.Id);
        Println("  Deleted.", ConsoleColor.Green);
    }

    // ══════════════════════════════════════════════════════════════
    //  PAYMENT HANDLERS
    // ══════════════════════════════════════════════════════════════

    static async Task Menu_ApplyPayment()
    {
        Section("Apply Payment");
        using var scope = _provider.CreateScope();
        var invoiceSvc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        var paymentSvc = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        Console.Write("  Invoice Number: ");
        var inv = await invoiceSvc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }
        if (inv.BalanceDue <= 0) { Println("  Already fully paid.", ConsoleColor.Green); return; }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Grand Total: {inv.GrandTotal:C}   Paid: {inv.AmountPaid:C}   Balance: {inv.BalanceDue:C}");
        Console.ResetColor();

        var methods = (await paymentSvc.GetPaymentMethodsAsync()).ToList();
        Console.WriteLine("\n  Payment Methods:");
        for (int i = 0; i < methods.Count; i++)
            Console.WriteLine($"    {i+1}. {methods[i].MethodName}");

        Console.Write("\n  Method: ");
        if (!int.TryParse(Console.ReadLine(), out int midx) || midx < 1 || midx > methods.Count)
        { Println("  Invalid.", ConsoleColor.Red); return; }

        Console.Write($"  Amount (max {inv.BalanceDue:C}): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
        { Println("  Invalid amount.", ConsoleColor.Red); return; }

        Console.Write("  Reference #: ");
        var refNum = Console.ReadLine();

        await paymentSvc.ApplyPaymentAsync(new ApplyPaymentRequest {
            InvoiceId = inv.Id, PaymentAmount = amount, PaymentDate = DateTime.UtcNow,
            PaymentMethodId = methods[midx-1].Id, ReferenceNumber = refNum, ReceivedBy = "User"
        });

        var updated = (await invoiceSvc.GetByIdAsync(inv.Id))!;
        Println($"\n  Payment of {amount:C} applied via {methods[midx-1].MethodName}", ConsoleColor.Green);
        Console.WriteLine($"  Status: {updated.Status}   Balance Remaining: {updated.BalanceDue:C}");
    }

    static async Task Menu_ViewPayments()
    {
        Section("Payments for Invoice");
        using var scope = _provider.CreateScope();
        var invoiceSvc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        var paymentSvc = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        Console.Write("  Invoice Number: ");
        var inv = await invoiceSvc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }

        var payments = (await paymentSvc.GetByInvoiceAsync(inv.Id)).ToList();
        Console.WriteLine($"\n  {payments.Count} payment(s) for {inv.InvoiceNumber}:\n");
        if (!payments.Any()) { Console.WriteLine("  No payments recorded."); return; }

        Console.WriteLine($"  {"#",-4} {"Date",-12} {"Amount",12} {"Method",-18} {"Reference",-20} {"Received"}");
        Console.WriteLine(new string('-', 82));
        for (int i = 0; i < payments.Count; i++)
            Console.WriteLine($"  {i+1,-4} {payments[i].PaymentDate:yyyy-MM-dd,-12} {payments[i].PaymentAmount,12:C} {payments[i].PaymentMethodName,-18} {payments[i].ReferenceNumber ?? "-",-20} {payments[i].ReceivedDate:yyyy-MM-dd}");

        Console.WriteLine($"\n  Total Paid: {payments.Sum(p => p.PaymentAmount):C}   Balance: {inv.BalanceDue:C}");
    }

    static async Task Menu_ReversePayment()
    {
        Section("Reverse Payment");
        using var scope = _provider.CreateScope();
        var invoiceSvc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        var paymentSvc = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        Console.Write("  Invoice Number: ");
        var inv = await invoiceSvc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }

        var payments = (await paymentSvc.GetByInvoiceAsync(inv.Id)).ToList();
        if (!payments.Any()) { Println("  No payments.", ConsoleColor.Yellow); return; }

        for (int i = 0; i < payments.Count; i++)
            Console.WriteLine($"  {i+1}. {payments[i].PaymentDate:yyyy-MM-dd}  {payments[i].PaymentAmount:C}  {payments[i].PaymentMethodName}  Ref: {payments[i].ReferenceNumber ?? "-"}");

        Console.Write("\n  Select payment to reverse: ");
        if (!int.TryParse(Console.ReadLine(), out int pidx) || pidx < 1 || pidx > payments.Count)
        { Println("  Invalid.", ConsoleColor.Red); return; }

        Console.Write("  Reason: ");
        await paymentSvc.ReversePaymentAsync(payments[pidx-1].Id, Required(), "User");
        Println("  Payment reversed.", ConsoleColor.Green);
    }

    static async Task Menu_PaymentsByDate()
    {
        Section("Payments by Date Range");
        using var scope = _provider.CreateScope();
        var paymentSvc = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        Console.Write("  From (yyyy-MM-dd) [30 days ago]: ");
        var fromStr = Console.ReadLine();
        var from = string.IsNullOrWhiteSpace(fromStr) ? DateTime.UtcNow.AddDays(-30) : DateTime.Parse(fromStr);

        Console.Write("  To   (yyyy-MM-dd) [today]:       ");
        var toStr = Console.ReadLine();
        var to = string.IsNullOrWhiteSpace(toStr) ? DateTime.UtcNow : DateTime.Parse(toStr);

        var payments = (await paymentSvc.GetByDateRangeAsync(from, to)).ToList();
        Console.WriteLine($"\n  {payments.Count} payments from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}:\n");
        if (!payments.Any()) { Console.WriteLine("  None found."); return; }

        Console.WriteLine($"  {"Date",-12} {"Invoice",-22} {"Amount",12} {"Method",-18} {"Reference"}");
        Console.WriteLine(new string('-', 80));
        foreach (var p in payments)
            Console.WriteLine($"  {p.PaymentDate:yyyy-MM-dd,-12} {p.InvoiceNumber,-22} {p.PaymentAmount,12:C} {p.PaymentMethodName,-18} {p.ReferenceNumber ?? "-"}");

        Console.WriteLine($"\n  Total Collected: {payments.Sum(p => p.PaymentAmount):C}");
    }

    // ══════════════════════════════════════════════════════════════
    //  REPORT HANDLERS
    // ══════════════════════════════════════════════════════════════

    static async Task Menu_AgingReport()
    {
        Section("Accounts Receivable Aging Report");
        using var scope = _provider.CreateScope();
        var report = await scope.ServiceProvider.GetRequiredService<IReportingService>().GetAgingReportAsync();

        Console.WriteLine($"\n  As of: {report.ReportDate:yyyy-MM-dd}\n");
        Console.WriteLine($"  {"Bucket",-14} {"Amount",14}  {"% of Total"}");
        Console.WriteLine(new string('-', 42));

        void Row(string label, decimal amount, ConsoleColor c)
        {
            var pct = report.TotalOutstanding > 0 ? amount / report.TotalOutstanding * 100 : 0;
            Console.ForegroundColor = c;
            Console.WriteLine($"  {label,-14} {amount,14:C}  {pct,8:F1}%");
            Console.ResetColor();
        }

        Row("Current",    report.CurrentAmount, ConsoleColor.Green);
        Row("1-30 days",  report.Days1To30,     ConsoleColor.Yellow);
        Row("31-60 days", report.Days31To60,    ConsoleColor.DarkYellow);
        Row("61-90 days", report.Days61To90,    ConsoleColor.Red);
        Row("90+ days",   report.Over90Days,    ConsoleColor.DarkRed);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {new string('-', 42)}");
        Console.WriteLine($"  {"TOTAL",-14} {report.TotalOutstanding,14:C}");
        Console.ResetColor();

        if (report.Details.Any())
        {
            Console.WriteLine($"\n  Outstanding Invoices Detail ({report.Details.Count}):\n");
            Console.WriteLine($"  {"Invoice",-22} {"Customer",-24} {"Due",12} {"Balance",10} {"Days OD",8} {"Bucket"}");
            Console.WriteLine(new string('-', 86));
            foreach (var d in report.Details.OrderByDescending(x => x.DaysOverdue))
            {
                Console.ForegroundColor = d.AgingBucket == "Current" ? ConsoleColor.Green :
                                          d.AgingBucket == "1-30"    ? ConsoleColor.Yellow :
                                          d.AgingBucket == "90+"     ? ConsoleColor.DarkRed : ConsoleColor.Red;
                Console.WriteLine($"  {d.InvoiceNumber,-22} {d.CustomerName,-24} {d.DueDate:yyyy-MM-dd,12} {d.BalanceDue,10:C} {d.DaysOverdue,8} {d.AgingBucket}");
                Console.ResetColor();
            }
        }
    }

    static async Task Menu_DsoReport()
    {
        Section("Days Sales Outstanding (DSO)");
        using var scope = _provider.CreateScope();

        Console.Write("  Period in days [30]: ");
        if (!int.TryParse(Console.ReadLine(), out int days) || days <= 0) days = 30;

        var dso = await scope.ServiceProvider.GetRequiredService<IReportingService>().GetDsoAsync(days);

        Console.WriteLine($"\n  Period           : Last {dso.PeriodDays} days");
        Console.WriteLine($"  Total Receivables: {dso.TotalReceivables:C}");
        Console.WriteLine($"  Avg Daily Revenue: {dso.AverageDailyRevenue:C}");
        Console.WriteLine();

        var color = dso.DaysSalesOutstanding > 45 ? ConsoleColor.Red :
                    dso.DaysSalesOutstanding > 30 ? ConsoleColor.Yellow : ConsoleColor.Green;
        Console.ForegroundColor = color;
        Console.WriteLine($"  DSO: {dso.DaysSalesOutstanding:F1} days  " +
            (dso.DaysSalesOutstanding < 30 ? "(Excellent)" :
             dso.DaysSalesOutstanding < 45 ? "(Good)" : "(Needs Attention)"));
        Console.ResetColor();
        Console.WriteLine("  Benchmark: <30 Excellent | 30-45 Good | >45 Review needed");
    }

    static async Task Menu_OverdueInvoices()
    {
        Section("Overdue Invoices");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        await svc.UpdateOverdueStatusesAsync();
        var list = (await svc.GetOverdueAsync()).OrderByDescending(i => i.DaysOverdue).ToList();

        Console.WriteLine($"\n  {list.Count} overdue invoice(s):\n");
        if (!list.Any()) { Println("  No overdue invoices! Great job.", ConsoleColor.Green); return; }

        Console.WriteLine($"  {"Invoice",-22} {"Customer",-24} {"Due Date",12} {"Balance",12} {"Days Overdue"}");
        Console.WriteLine(new string('-', 86));
        foreach (var inv in list)
        {
            Console.ForegroundColor = inv.DaysOverdue > 90 ? ConsoleColor.DarkRed :
                                      inv.DaysOverdue > 60 ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.WriteLine($"  {inv.InvoiceNumber,-22} {inv.CustomerName,-24} {inv.DueDate:yyyy-MM-dd,12} {inv.BalanceDue,12:C} {inv.DaysOverdue} days");
            Console.ResetColor();
        }
        Console.WriteLine($"\n  Total Overdue Amount: {list.Sum(i => i.BalanceDue):C}");
    }

    static async Task Menu_MonthlySnapshot()
    {
        Section("Monthly Analytics Snapshot");
        using var scope = _provider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IReportingService>();

        Console.Write($"  Year  [{DateTime.UtcNow.Year}]: ");
        var yStr = Console.ReadLine();
        int year = string.IsNullOrWhiteSpace(yStr) ? DateTime.UtcNow.Year : int.Parse(yStr);

        Console.Write($"  Month [{DateTime.UtcNow.Month}]: ");
        var mStr = Console.ReadLine();
        int month = string.IsNullOrWhiteSpace(mStr) ? DateTime.UtcNow.Month : int.Parse(mStr);

        Console.Write($"\n  Generating {year}-{month:D2} snapshot... ");
        var snap = await svc.GenerateMonthlySnapshotAsync(year, month);
        Println("Saved to MongoDB", ConsoleColor.Green);

        Console.WriteLine($"\n  Period        : {snap.Period}");
        Console.WriteLine($"  Total Invoices: {snap.TotalInvoices}");
        Console.WriteLine($"  Revenue       : {snap.TotalRevenue:C}");
        Console.WriteLine($"  Collected     : {snap.TotalPaid:C}");
        Console.WriteLine($"  Outstanding   : {snap.TotalOutstanding:C}");
        Console.WriteLine($"  DSO           : {snap.DaysSalesOutstanding:F1} days");

        Console.WriteLine("\n  Status Breakdown:");
        foreach (var kv in snap.StatusBreakdown.OrderBy(k => k.Key))
            Console.WriteLine($"    {kv.Key,-18}: {kv.Value}");

        Console.WriteLine("\n  Aging Buckets:");
        Console.WriteLine($"    Current   : {snap.AgingBuckets.Current:C}");
        Console.WriteLine($"    1-30 days : {snap.AgingBuckets.Days1To30:C}");
        Console.WriteLine($"    31-60 days: {snap.AgingBuckets.Days31To60:C}");
        Console.WriteLine($"    61-90 days: {snap.AgingBuckets.Days61To90:C}");
        Console.WriteLine($"    90+ days  : {snap.AgingBuckets.Over90Days:C}");

        if (snap.TopCustomers.Any())
        {
            Console.WriteLine("\n  Top Customers:");
            int rank = 1;
            foreach (var c in snap.TopCustomers)
                Console.WriteLine($"    {rank++}. {c.CustomerName,-30} {c.TotalRevenue:C}  ({c.InvoiceCount} inv)");
        }
    }

    static async Task Menu_AuditTrail()
    {
        Section("Audit Trail");
        using var scope = _provider.CreateScope();
        var invoiceSvc   = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        var reportingSvc = scope.ServiceProvider.GetRequiredService<IReportingService>();

        Console.Write("  Invoice Number: ");
        var inv = await invoiceSvc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }

        var logs = (await reportingSvc.GetAuditTrailAsync(inv.Id)).ToList();
        Console.WriteLine($"\n  {logs.Count} audit events for {inv.InvoiceNumber}:\n");
        if (!logs.Any()) { Console.WriteLine("  No events."); return; }

        foreach (var log in logs)
        {
            Console.ForegroundColor = log.EventType switch {
                "InvoiceCreated"  => ConsoleColor.Cyan,
                "PaymentApplied"  => ConsoleColor.Green,
                "StatusChanged"   => ConsoleColor.Yellow,
                "PaymentReversed" => ConsoleColor.Red,
                "Archived"        => ConsoleColor.DarkGray,
                _                 => ConsoleColor.White
            };
            Console.Write($"  [{log.Timestamp:yyyy-MM-dd HH:mm:ss}] {log.EventType,-20} ");
            Console.ResetColor();
            Console.WriteLine(log.EventDescription);
            if (log.PreviousValue != null)
                Console.WriteLine($"    {log.PreviousValue}  =>  {log.NewValue}");
        }
    }

    static async Task Menu_Reconciliation()
    {
        Section("Reconciliation Records");
        using var scope = _provider.CreateScope();
        var invoiceSvc   = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        var reportingSvc = scope.ServiceProvider.GetRequiredService<IReportingService>();

        Console.Write("  Invoice Number: ");
        var inv = await invoiceSvc.GetByNumberAsync(Required());
        if (inv == null) { Println("  Not found.", ConsoleColor.Red); return; }

        var records = (await reportingSvc.GetReconciliationsAsync(inv.Id)).ToList();
        Console.WriteLine($"\n  {records.Count} reconciliation record(s) for {inv.InvoiceNumber}:\n");
        if (!records.Any()) { Console.WriteLine("  No records."); return; }

        Console.WriteLine($"  {"Date",-12} {"Amount",12} {"Method",-16} {"Before",12} {"After",12} {"Settled"}");
        Console.WriteLine(new string('-', 76));
        foreach (var r in records)
        {
            Console.ForegroundColor = r.IsFullyPaid ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"  {r.PaymentDate:yyyy-MM-dd,-12} {r.PaymentAmount,12:C} {r.PaymentMethod,-16} {r.BalanceBefore,12:C} {r.BalanceAfter,12:C} {(r.IsFullyPaid ? "YES" : "NO")}");
            Console.ResetColor();
        }
        Console.WriteLine($"\n  Invoice Total: {inv.GrandTotal:C}  |  Paid: {records.Sum(r => r.PaymentAmount):C}  |  Balance: {inv.BalanceDue:C}");
    }

    // ══════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════

    static void PrintHeader(string t)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n" + new string('=', 55));
        Console.WriteLine($"  {t}");
        Console.WriteLine(new string('=', 55) + "\n");
        Console.ResetColor();
    }

    static void Section(string t)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n--- {t} ---");
        Console.ResetColor();
    }

    static void Println(string msg, ConsoleColor c)
    {
        Console.ForegroundColor = c;
        Console.WriteLine(msg);
        Console.ResetColor();
    }

    static string Required()
    {
        var v = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(v)) throw new ArgumentException("Input cannot be empty.");
        return v;
    }

    static ConsoleColor StatusColor(string status) => status switch
    {
        "Paid"          => ConsoleColor.Green,
        "PartiallyPaid" => ConsoleColor.Yellow,
        "Overdue"       => ConsoleColor.Red,
        "Cancelled"     => ConsoleColor.DarkGray,
        "Archived"      => ConsoleColor.DarkGray,
        "Draft"         => ConsoleColor.DarkCyan,
        _               => ConsoleColor.White
    };
}
