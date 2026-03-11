using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InvoiceManagement.DAL.Models
{
    public class Payment
    {
        [BsonId] // MongoDB primary key (_id)
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("InvoiceId")]
        public string InvoiceId { get; set; } = null!; // Reference to Invoice document

        [BsonElement("MethodId")]
        public string MethodId { get; set; } = null!; // Reference to PaymentMethod document

        [BsonElement("PaymentAmount")]
        public decimal PaymentAmount { get; set; }

        [BsonElement("PaymentDate")]
        public DateTime PaymentDate { get; set; }

        [BsonElement("ReceivedDate")]
        public DateTime ReceivedDate { get; set; }

        [BsonElement("ReferenceNumber")]
        public string? ReferenceNumber { get; set; }

        [BsonElement("Notes")]
        public string? Notes { get; set; }

        [BsonElement("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation-like references (MongoDB doesn’t enforce FK, but you can embed or reference)
        [BsonElement("Invoice")]
        public Invoice? Invoice { get; set; }

        [BsonElement("Method")]
        public PaymentMethod? Method { get; set; }
    }
}