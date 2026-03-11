using MongoDB.Bson.Serialization.Attributes;

public class Student
{
    [BsonId]
    public Object Id { get; set; }   

    [BsonElement("name")]
    public string Name{get;set;}

    [BsonElement ("age")]
    public int Age { get; set; }

    [BsonElement ("course")]
    public string Course { get; set; }   

}