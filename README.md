# Invoice Management System — MongoDB + YARP Gateway

A fully **MongoDB-only** .NET 8 enterprise Invoice Management system with a **YARP API Gateway**.

---

## Architecture Overview

```
  Client (Browser / Postman / React)
         |
         v  port 5000
  +-----------------------------+
  |  InvoiceManagement.Gateway  |  <- YARP Reverse Proxy
  |  - Route matching           |     Rate Limiting (100 req/min)
  |  - Rate limiting            |     Request/Response Logging
  |  - CORS                     |     Active Health Check Polling
  +-------------+---------------+
                |  forwards to port 5001
                v
  +-----------------------------+
  |  InvoiceManagement.API      |  <- ASP.NET Core Web API
  |  - REST endpoints           |     Swagger UI at /swagger
  |  - Swagger docs             |     Global error middleware
  |  - Global error handler     |
  +-------------+---------------+
                |
  +-------------v---------------+
  |  InvoiceManagement.BLL      |  <- Business Logic Layer
  |  - Invoice lifecycle        |
  |  - Payment logic            |
  |  - Reports / Analytics      |
  +-------------+---------------+
                |
  +-------------v---------------+
  |  InvoiceManagement.DAL      |  <- Data Access Layer (MongoDB only)
  |  - 8 MongoDB repositories   |
  +-------------+---------------+
                |
  +-------------v---------------+
  |  MongoDB  port 27017        |
  |  InvoiceManagementDB        |
  +-----------------------------+
```

---

## Project Structure

```
InvoiceManagement/
|-- InvoiceManagement.DAL/            # MongoDB documents + repositories
|-- InvoiceManagement.BLL/            # Business logic, engines, services
|-- InvoiceManagement.API/            # ASP.NET Core Web API (port 5001)
|   |-- Controllers/
|   |   |-- CustomersController.cs
|   |   |-- InvoicesController.cs
|   |   |-- PaymentsController.cs
|   |   +-- ReportsController.cs
|   |-- Middleware/
|   |   +-- GlobalExceptionMiddleware.cs
|   |-- Program.cs
|   +-- appsettings.json
|-- InvoiceManagement.Gateway/        # YARP Gateway (port 5000)
|   |-- Program.cs
|   +-- appsettings.json             <- Routes configured here
|-- InvoiceManagement.Console/        # Interactive CLI demo
+-- InvoiceManagement.Tests/          # xUnit tests (30+ cases)
```

---

## How to Run

### Prerequisites
- .NET 8 SDK
- MongoDB running on localhost:27017

### Step 1 — Start the API  (Terminal 1)
```bash
cd InvoiceManagement.API
dotnet run
# API listening on:  http://localhost:5001
# Swagger UI:        http://localhost:5001/swagger
```

### Step 2 — Start the Gateway  (Terminal 2)
```bash
cd InvoiceManagement.Gateway
dotnet run
# Gateway on:    http://localhost:5000
# Gateway info:  http://localhost:5000/gateway/info
# Gateway health:http://localhost:5000/gateway/health
```

### Step 3 — Make requests through the Gateway
All client requests go to port **5000**. YARP forwards them to the API on port **5001**.

```
http://localhost:5000/api/customers
http://localhost:5000/api/invoices
http://localhost:5000/api/payments
http://localhost:5000/api/reports/aging
http://localhost:5000/swagger          <- Swagger proxied too
```

---

## REST API Reference

### Customers
| Method | Route | Description |
|--------|-------|-------------|
| GET    | /api/customers | List all customers |
| GET    | /api/customers/{id} | Get by ID |
| GET    | /api/customers/search?name= | Search by name |
| POST   | /api/customers | Create customer |
| PUT    | /api/customers/{id} | Update customer |
| DELETE | /api/customers/{id} | Soft-delete customer |

### Invoices
| Method | Route | Description |
|--------|-------|-------------|
| GET    | /api/invoices | List all invoices |
| GET    | /api/invoices/{id} | Get by ID |
| GET    | /api/invoices/number/{num} | Get by invoice number |
| GET    | /api/invoices/customer/{cid} | Invoices for customer |
| GET    | /api/invoices/overdue | All overdue invoices |
| POST   | /api/invoices | Create invoice |
| POST   | /api/invoices/{id}/send | Mark as sent |
| PATCH  | /api/invoices/{id}/status | Change status |
| POST   | /api/invoices/{id}/archive | Archive |
| DELETE | /api/invoices/{id} | Delete draft only |

### Payments
| Method | Route | Description |
|--------|-------|-------------|
| GET    | /api/payments/invoice/{id} | Payments for invoice |
| GET    | /api/payments/range?from=&to= | By date range |
| GET    | /api/payments/methods | Payment methods list |
| POST   | /api/payments | Apply payment |
| POST   | /api/payments/{id}/reverse | Reverse payment |

### Reports
| Method | Route | Description |
|--------|-------|-------------|
| GET    | /api/reports/aging | AR Aging report |
| GET    | /api/reports/dso?periodDays=30 | DSO calculation |
| POST   | /api/reports/snapshot?year=&month= | Generate monthly snapshot |
| GET    | /api/reports/snapshots?year= | Get saved snapshots |
| GET    | /api/reports/audit/{invoiceId} | Audit trail |
| GET    | /api/reports/reconciliation/{invoiceId} | Reconciliation records |

### Gateway Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET    | /gateway/health | Gateway own health |
| GET    | /gateway/info | All routes and clusters |
| GET    | /health | API health (proxied) |

---

## YARP Gateway Features

### Config-Driven Routing
Routes and clusters live entirely in appsettings.json — no code changes needed:
```json
"Routes": {
  "invoice-api-route": {
    "ClusterId": "invoice-api-cluster",
    "Match": { "Path": "/api/{**catch-all}" }
  }
},
"Clusters": {
  "invoice-api-cluster": {
    "LoadBalancingPolicy": "RoundRobin",
    "Destinations": {
      "invoice-api-1": { "Address": "http://localhost:5001/" }
    }
  }
}
```

### Horizontal Scaling
Add more API instances just by adding destinations:
```json
"Destinations": {
  "invoice-api-1": { "Address": "http://localhost:5001/" },
  "invoice-api-2": { "Address": "http://localhost:5002/" },
  "invoice-api-3": { "Address": "http://localhost:5003/" }
}
```

### Rate Limiting
- Read requests: 100/min (global-limit)
- Write requests: 20/min (write-limit)
- Returns HTTP 429 with JSON body when exceeded

### Active Health Checks
Polls GET /health on the API every 10 seconds.
If the API is down, the Gateway stops routing to it automatically.

### Request Logging
```
[Gateway] POST /api/payments -> forwarding
[Gateway] POST /api/payments <- 201 (42ms)
```

---

## Sample curl Requests

```bash
# List all customers
curl http://localhost:5000/api/customers

# Create a customer
curl -X POST http://localhost:5000/api/customers \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Test Corp","email":"test@corp.com","phone":"+1-555-9999"}'

# Create an invoice
curl -X POST http://localhost:5000/api/invoices \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "<id>",
    "paymentTerms": "Net 30",
    "lineItems": [{"description":"License","quantity":1,"unitPrice":500,"taxRatePercent":10}]
  }'

# Apply payment
curl -X POST http://localhost:5000/api/payments \
  -H "Content-Type: application/json" \
  -d '{"invoiceId":"<id>","paymentAmount":550,"paymentDate":"2024-03-01","paymentMethodId":"<id>"}'

# Aging report
curl http://localhost:5000/api/reports/aging

# DSO
curl "http://localhost:5000/api/reports/dso?periodDays=60"

# Gateway info
curl http://localhost:5000/gateway/info
```
