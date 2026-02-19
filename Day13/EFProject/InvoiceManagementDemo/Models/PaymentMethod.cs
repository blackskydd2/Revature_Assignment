namespace InvoiceManagementDemo.Models
{
    public class PaymentMethod
    {
        public int MethodId { get; set; }
        public string MethodName { get; set; } = string.Empty;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
