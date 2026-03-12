using MongoDB.Driver;

var connectionString =
"mongodb://admin:temppwd123@localhost:27017/crmdb?authSource=admin&authMechanism=SCRAM-SHA-1";

var client = new MongoClient(connectionString);

var db = client.GetDatabase("crmdb");

Console.WriteLine("Connected to MongoDB");

// get customers collection
var customersCollection = db.GetCollection<Customer>("customers");

// create a new customer
var newCustomer = new Customer
{
    Name = "Sarah Smith",
    Email = "sarah.smith@example.com",
    Age = 20
};

// insert the new customer into the collection
await customersCollection.InsertOneAsync(newCustomer);
Console.WriteLine("Inserted new customer: " + newCustomer.Name);

// read all customers

var customers = await customersCollection.Find(_ => true).ToListAsync();

Console.WriteLine("Customers in database:");
foreach (var customer in customers)
{
    Console.WriteLine($"- {customer.Name} ({customer.Email})");
}

class Customer
{
    // declare Object ID with BSON data annotation
    [MongoDB.Bson.Serialization.Attributes.BsonId]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)
    ]
    public MongoDB.Bson.ObjectId Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public int Age { get; set; }
}