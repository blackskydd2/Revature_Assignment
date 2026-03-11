using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookStoreApi.Models
{
    public class Books
    {
        [BsonId] // Marks this property as the document’s primary key
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("Title")]
        public string Title { get; set; } = null!;

        [BsonElement("Author")]
        public string Author { get; set; } = null!;

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("Category")]
        public string Category { get; set; } = null!;
    }
}