# Invoice Management System
### CRM Project — Module 5 | .NET 8 | EF Core Code-First | Layered Architecture

---

## Project Structure

```
InvoiceManagement/
│
├── InvoiceManagement.sln
│
├── InvoiceManagement.DAL/              ← Data Access Layer
│   ├── Models/
│   │   ├── InvoiceStatus.cs            Enums: InvoiceStatus, PaymentTerms
│   │   ├── Invoice.cs                  Core invoice entity
│   │   ├── InvoiceLineItem.cs          Line items (products/services)
│   │   ├── Payment.cs                  Payment records
│   │   └── PaymentMethod.cs            Lookup table (Cash, UPI, etc.)
│   ├── Context/
│   │   └── AppDbContext.cs             EF Core DbContext
│   └── Repositories/
│       ├── Interfaces/
│       │   ├── IRepository.cs          Generic CRUD interface
│       │   ├── IInvoiceRepository.cs   Invoice-specific queries
│       │   └── IPaymentRepository.cs   Payment-specific queries
│       └── Implementations/
│           ├── InvoiceRepository.cs    EF Core implementation
│           └── PaymentRepository.cs    EF Core implementation
│
├── InvoiceManagement.BLL/              ← Business Logic Layer
│   ├── Engines/
│   │   ├── InvoiceNumberEngine.cs      Auto-generates INV-YYYYMM-XXXXX
│   │   └── InvoiceCalculationEngine.cs Financial math + aging + DSO
│   ├── Validators/
│   │   └── InvoiceStatusValidator.cs   State machine for status transitions
│   └── Services/
│       ├── Interfaces/
│       │   ├── IInvoiceService.cs
│       │   └── IPaymentService.cs
│       └── Implementations/
│           ├── InvoiceService.cs       Core invoice orchestration
│           └── PaymentService.cs       Payment + reconciliation logic
│
└── InvoiceManagement.Console/          ← Presentation Layer
    └── Program.cs                      Interactive menu-driven demo
```

---

## Entity Relationship Diagram

```
PaymentMethod (1) ──────────────────────────── (N) Payment
                        Restrict delete

Invoice (1) ────────────────────────────────── (N) InvoiceLineItem
                        Cascade delete

Invoice (1) ────────────────────────────────── (N) Payment
                        Cascade delete

Invoice (N) ──── CustomerId ────→ [Customer Management Module]
Invoice (N) ──── QuoteId   ────→ [Quotation Management Module]
InvoiceLineItem (N) ── ProductId → [Stock/Inventory Module]
```

---

## Annotation vs Fluent API — What's Used Where

| Configuration          | Where Handled     | Why                                |
|------------------------|-------------------|------------------------------------|
| Primary Keys           | `[Key]`           | Simple, self-documenting           |
| Required fields        | `[Required]`      | Annotation sufficient              |
| String max lengths     | `[MaxLength]`     | Annotation sufficient              |
| Decimal column types   | `[Column(TypeName)]` | Annotation sufficient           |
| FK declarations        | `[ForeignKey]`    | Annotation sufficient              |
| Range validation       | `[Range]`         | Annotation sufficient              |
| Cascade Delete         | Fluent API        | Annotations default to Cascade — Restrict needs Fluent |
| Unique Index           | Fluent API        | Annotations cannot create unique indexes |
| Seed Data              | Fluent API        | `HasData()` has no annotation equivalent |
| Performance Indexes    | Fluent API        | No annotation for non-unique indexes |

---

## Invoice Status State Machine

```
         ┌─────────┐
         │  Draft  │──────────────────────────────┐
         └────┬────┘                              │
              │ Send                              │
         ┌────▼────┐                              │
         │  Sent   │──── DueDate passed ──→ ┌────▼──────┐
         └────┬────┘                        │  Overdue  │
              │                             └────┬──────┘
    ┌─────────┼──────────────────────────────────┤
    │         │                                  │
    ▼         ▼                                  ▼
┌───────┐ ┌──────────────┐              ┌────────────────┐
│  Paid │ │PartiallyPaid │──────────→   │   Cancelled    │
└───────┘ └──────────────┘  Cancelled   └────────────────┘
    ▲              │
    └──────────────┘  (full payment received)
```

---

## Invoice Number Format

```
INV - YYYYMM - XXXXX
 │      │        │
 │      │        └── 5-digit sequence, resets each month
 │      └─────────── Year + Month of invoice date
 └────────────────── Fixed prefix

Examples:
  INV-202502-00001   First invoice in February 2025
  INV-202502-00002   Second invoice in February 2025
  INV-202503-00001   First invoice in March 2025 (resets)
```

---

## Financial Formula Chain

```
LineTotal   = (Quantity × UnitPrice) - LineDiscount + LineTax
LineTax     = (Quantity × UnitPrice - LineDiscount) × TaxRate / 100

SubTotal    = SUM(all raw line amounts before tax)
Tax         = SUM(all LineTax amounts)
GrandTotal  = SubTotal - InvoiceDiscount + Tax
Outstanding = GrandTotal - AmountPaid
```

---

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- SQL Server Express (`.\SQLEXPRESS`)
- Visual Studio 2022 or VS Code

### Steps

```bash
# 1. Clone / open the solution
cd InvoiceManagement

# 2. Run migrations (from DAL project)
cd InvoiceManagement.DAL
dotnet ef migrations add InitialCreate --startup-project ../InvoiceManagement.Console
dotnet ef database update --startup-project ../InvoiceManagement.Console

# 3. Run the console app
cd ../InvoiceManagement.Console
dotnet run
```

### Connection String
Edit in `AppDbContext.cs` → `OnConfiguring()`:
```
Server=.\SQLEXPRESS;Database=InvoiceManagementDB;Trusted_Connection=True;TrustServerCertificate=True
```

---

## Key Design Decisions

1. **Soft Delete over Hard Delete** — Invoices are never truly deleted; they are archived. This maintains audit trails and supports financial reconciliation.

2. **Computed fields stored in DB** — `OutstandingBalance` and `GrandTotal` are recalculated and stored (not purely computed at query time) for performance in aging and reporting queries.

3. **Cross-module references via IDs only** — `CustomerId`, `QuoteId`, `ProductId` are stored as plain integers with no EF navigation properties. This keeps modules decoupled for Phase 2 (API integration).

4. **Status as string in DB** — Stored as `"Draft"`, `"Paid"` etc. for human-readable database inspection. Parsed to enum in BLL for compile-time safety.

5. **PaymentMethod as Restrict** — Cannot delete a payment method if payments exist against it. Prevents data integrity issues.

---

## Phase 2 Ready
When building the ASP.NET Core Web API layer:
- Replace manual wiring in `Program.cs` with `services.AddScoped<>()`
- Replace `AppDbContext` constructor configuration with `appsettings.json` + `builder.Services.AddDbContext<>()`
- Repository interfaces are already abstracted — swap implementations for mocks in unit tests
