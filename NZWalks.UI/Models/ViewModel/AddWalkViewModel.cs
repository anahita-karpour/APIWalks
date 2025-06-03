namespace NZWalks.UI.Models.ViewModel
{
    // properties that you want from the client
    public class AddWalkViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double LengthInKm { get; set; }
        public string WalkImageUrl { get; set; }
        public Guid RegionId { get; set; }
        public Guid DifficultyId { get; set; }

    }
}
