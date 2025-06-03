namespace NZWalks.API.Models.Domain
{
    public class Region
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? RegionImageUrl { get; set; } //string? means a nullable string type

        //Navigation properties
        public ICollection<Walk> Walks { get; set; } = new List<Walk>(); //one to many relationship 
        // The navigation property ICollection<Walk> Walks is a C# concept that helps you navigate the object graph.
        // The foreign key property RegionId is the database concept that establishes the relationship between the Region and Walk tables.
        // Therefore, you are completely correct that the ICollection does not create a table or Column in the database.
        // It is a tool for object navigation.
    }
}
