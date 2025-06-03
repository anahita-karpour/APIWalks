namespace NZWalks.API.Models.Domain
{
    public class Walk
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double LengthInKm { get; set; }
        public string? WalkImageUrl { get; set; }

        // Foreign Keys
        public Guid DifficultyId { get; set; }
        public Guid RegionId { get; set; }


        //Navigation properties
        //--this defines a one to one relationship between a walk and a difficuluty
        public Difficulty Difficulty { get; set; } //non-nullable 
                                                   //This tells the EF that a walk will have a difficulty and this is the
                                                   //difficulty ID for it. When we run entity framework code migrations, EF core
                                                   // is smart enough to give all those relationships that we want and create
                                                   // the database and tables as per the relationships that we are defining now.
                                                   //--this defines a one to one relationship between a walk and a region
        public Region Region { get; set; } //non-nullable
    }
}
