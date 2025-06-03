namespace NZWalks.API.Models.DTO
{
    //this class is a subset of the model class.
    /// <summary>
    /// Using this class we are decoupling our Region Domain model from the view layer of the API
    /// </summary>
    public class RegionDtoV1
    {
        //properties that you want to expose to the client
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? RegionImageUrl { get; set; } //string? means a nullable string type
    }
    public class RegionDtoV2
    {
        //properties that you want to expose to the client
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string RegionName { get; set; }
        public string? RegionImageUrl { get; set; } //string? means a nullable string type
    }
}
