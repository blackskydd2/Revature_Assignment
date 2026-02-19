using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.BLL.Validators
{
    /// <summary>
    /// Enforces the Invoice Status State Machine.
    /// 
    /// VALID TRANSITIONS:
    ///
    ///   Draft ──────────→ Sent
    ///   Draft ──────────→ Cancelled
    ///
    ///   Sent ───────────→ Overdue         (when DueDate has passed)
    ///   Sent ───────────→ PartiallyPaid   (when partial payment received)
    ///   Sent ───────────→ Paid            (when full payment received)
    ///   Sent ───────────→ Cancelled
    ///
    ///   Overdue ────────→ PartiallyPaid
    ///   Overdue ────────→ Paid
    ///   Overdue ────────→ Cancelled
    ///
    ///   PartiallyPaid ──→ Paid            (remaining balance cleared)
    ///   PartiallyPaid ──→ Cancelled
    ///
    ///   Paid ───────────→ (terminal — no transitions allowed)
    ///   Cancelled ──────→ (terminal — no transitions allowed)
    /// </summary>
    public class InvoiceStatusValidator
    {
        // Transition map: current status → list of valid next statuses
        private static readonly Dictionary<InvoiceStatus, List<InvoiceStatus>> _validTransitions = new()
        {
            [InvoiceStatus.Draft] = new()
            {
                InvoiceStatus.Sent,
                InvoiceStatus.Cancelled
            },
            [InvoiceStatus.Sent] = new()
            {
                InvoiceStatus.Overdue,
                InvoiceStatus.PartiallyPaid,
                InvoiceStatus.Paid,
                InvoiceStatus.Cancelled
            },
            [InvoiceStatus.Overdue] = new()
            {
                InvoiceStatus.PartiallyPaid,
                InvoiceStatus.Paid,
                InvoiceStatus.Cancelled
            },
            [InvoiceStatus.PartiallyPaid] = new()
            {
                InvoiceStatus.Paid,
                InvoiceStatus.Cancelled
            },
            [InvoiceStatus.Paid]      = new(),   // Terminal — no further transitions
            [InvoiceStatus.Cancelled] = new()    // Terminal — no further transitions
        };

        /// <summary>
        /// Returns true if transitioning from currentStatus to newStatus is allowed.
        /// </summary>
        public bool IsValidTransition(InvoiceStatus currentStatus, InvoiceStatus newStatus)
        {
            if (!_validTransitions.TryGetValue(currentStatus, out var allowed))
                return false;

            return allowed.Contains(newStatus);
        }

        /// <summary>
        /// Throws InvalidOperationException if the transition is not allowed.
        /// Use this in service methods before updating status.
        /// </summary>
        public void ValidateTransition(InvoiceStatus currentStatus, InvoiceStatus newStatus)
        {
            if (!IsValidTransition(currentStatus, newStatus))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition: {currentStatus} → {newStatus}. " +
                    $"Allowed from {currentStatus}: [{string.Join(", ", _validTransitions[currentStatus])}]");
            }
        }

        /// <summary>
        /// Parses a status string from DB and returns the enum value.
        /// </summary>
        public InvoiceStatus ParseStatus(string status)
        {
            if (Enum.TryParse<InvoiceStatus>(status, out var result))
                return result;

            throw new ArgumentException($"'{status}' is not a valid InvoiceStatus.");
        }
    }
}
