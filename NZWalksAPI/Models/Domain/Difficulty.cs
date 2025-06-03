namespace NZWalks.API.Models.Domain
{
    public class Difficulty
    {
        public Guid Id { get; set; } //non-nullable = meaning we need a value for these
        public string Name { get; set; } //non-nullable
                                         // to create property faster, type prop and double tab
    }
}
