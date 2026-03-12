using InvoiceManagement.BLL.Models;
using System;
using System.Linq;

namespace InvoiceManagement.BLL.Validators
{
    public interface IInvoiceValidator
    {
        ValidationResult ValidateCreate(CreateInvoiceRequest request);
        ValidationResult ValidatePayment(ApplyPaymentRequest request, decimal currentBalance);
        ValidationResult ValidateCustomer(CreateCustomerRequest request);
    }

    public class InvoiceValidator : IInvoiceValidator
    {
        public ValidationResult ValidateCreate(CreateInvoiceRequest request)
        {
            var r = new ValidationResult();

            if (string.IsNullOrWhiteSpace(request.CustomerId))
                r.AddError("CustomerId is required.");

            if (request.InvoiceDate == default)
                r.AddError("InvoiceDate is required.");

            if (request.InvoiceDate.Date > DateTime.UtcNow.Date.AddDays(1))
                r.AddError("InvoiceDate cannot be set more than 1 day in the future.");

            if (!request.LineItems.Any())
                r.AddError("Invoice must contain at least one line item.");

            for (int i = 0; i < request.LineItems.Count; i++)
            {
                var item = request.LineItems[i];
                var n = i + 1;
                if (string.IsNullOrWhiteSpace(item.Description))
                    r.AddError($"Line item {n}: Description is required.");
                if (item.Quantity <= 0)
                    r.AddError($"Line item {n}: Quantity must be > 0.");
                if (item.UnitPrice < 0)
                    r.AddError($"Line item {n}: UnitPrice cannot be negative.");
                if (item.DiscountPercent < 0 || item.DiscountPercent > 100)
                    r.AddError($"Line item {n}: DiscountPercent must be 0–100.");
                if (item.TaxRatePercent < 0 || item.TaxRatePercent > 100)
                    r.AddError($"Line item {n}: TaxRatePercent must be 0–100.");
            }

            if (request.InvoiceDiscountAmount < 0)
                r.AddError("Invoice discount cannot be negative.");

            if (request.InvoiceTaxRatePercent < 0 || request.InvoiceTaxRatePercent > 100)
                r.AddError("Invoice tax rate must be 0–100.");

            return r;
        }

        public ValidationResult ValidatePayment(ApplyPaymentRequest request, decimal currentBalance)
        {
            var r = new ValidationResult();

            if (string.IsNullOrWhiteSpace(request.InvoiceId))
                r.AddError("InvoiceId is required.");

            if (request.PaymentAmount <= 0)
                r.AddError("PaymentAmount must be > 0.");

            if (request.PaymentAmount > currentBalance)
                r.AddError($"PaymentAmount ({request.PaymentAmount:C}) exceeds outstanding balance ({currentBalance:C}).");

            if (request.PaymentDate == default)
                r.AddError("PaymentDate is required.");

            if (request.PaymentDate.Date > DateTime.UtcNow.Date.AddDays(1))
                r.AddError("PaymentDate cannot be in the future.");

            if (string.IsNullOrWhiteSpace(request.PaymentMethodId))
                r.AddError("PaymentMethodId is required.");

            return r;
        }

        public ValidationResult ValidateCustomer(CreateCustomerRequest request)
        {
            var r = new ValidationResult();

            if (string.IsNullOrWhiteSpace(request.CustomerName))
                r.AddError("CustomerName is required.");

            if (request.Email is not null && !request.Email.Contains('@'))
                r.AddError("Email format is invalid.");

            return r;
        }
    }
}
