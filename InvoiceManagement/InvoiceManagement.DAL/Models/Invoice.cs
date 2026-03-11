using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InvoiceManagement.DAL.Models
{
    public class Invoice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string InvoiceNumber { get; set; }

        public DateTime CreatedDate { get; set; }

        public decimal TotalAmount { get; set; }

        public InvoiceStatus Status { get; set; }
    }
}   