using Microsoft.EntityFrameworkCore;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Data
{
    public class NZWalksDbContext : DbContext

    {
        public NZWalksDbContext(DbContextOptions<NZWalksDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        //Constructor
        //public NZWalksDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        //{

        //}


        //DbSet: is a property of DbContext class that represents a collection of entities in the database
        //in our application we have three entities or domain models: Difficulty, Region, Walk
        //we want to represent the DbSets of each of these entities as a collection or tables in our database
        //these represents the collections inside our database
        public DbSet<Difficulty> Difficulties { get; set; }

        public DbSet<Region> Regions { get; set; }
        public DbSet<Walk> Walks { get; set; }

        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Seed data for Difficulties;
            // Easy, Medium, Hard

            var difficulties = new List<Difficulty>()
            {
                new Difficulty() {
                    Id = Guid.Parse("5b4a90b7-b8e7-4bd7-bda0-876e87bb762c"),
                    Name = "Easy"
                },
                new Difficulty() {
                    Id = Guid.Parse("9d728f86-8cd5-466e-9407-5baad65a2e7e"),
                    Name = "Medium"
                },
                new Difficulty() {
                    Id = Guid.Parse("d52c2278-004b-4590-ac1d-209c010e91d1"),
                    Name = "Hard"
                }
            };
            //Seed difficulties to the database
            modelBuilder.Entity<Difficulty>().HasData(difficulties);

            //Seed data for Regions
            var regions = new List<Region>()
            {
                new Region() {
                    Id = Guid.Parse("6944c405-78d6-4f2e-9bf9-ef7000dab607"),
                    Code = "AKL",
                    Name = "Auckland",
                    RegionImageUrl = "https://images.unsplash.com/photo-1604367078108-3ed095682132?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                },
                new Region() {
                    Id = Guid.Parse("b870c489-695c-4da0-b5dc-1ffc0bde439b"),
                    Code = "NTL",
                    Name = "Northland",
                    RegionImageUrl = "https://images.unsplash.com/photo-1622943032318-8f65b0de0acd?q=80&w=1931&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                },
                new Region() {
                    Id = Guid.Parse("5e588842-6eba-42a7-bb03-4288b9cfe7af"),
                    Code = "BOP",
                    Name = "Bay of Plenty",
                    RegionImageUrl = "https://plus.unsplash.com/premium_photo-1669752276750-f2e6034db8d8?q=80&w=1926&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                },
                new Region() {
                    Id = Guid.Parse("4a1c9875-fd9e-417f-824b-cab25453a907"),
                    Code = "WGN",
                    Name = "Wellington",
                    RegionImageUrl = "https://media.istockphoto.com/id/464378599/photo/wellington-bay-and-harbour.jpg?s=2048x2048&w=is&k=20&c=AYHd5NUaAT6F_YXGqPHFs3k8_Im2B7kaJuNJBgMSYMI="
                },
                new Region() {
                    Id = Guid.Parse("331c6de1-cf59-484b-8e9e-a8169400933c"),
                    Code = "NSN",
                    Name = "Nelson",
                    RegionImageUrl = "https://images.unsplash.com/photo-1589446482864-4623c4301397?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                },
                new Region() {
                    Id = Guid.Parse("c13b7f34-fe98-468e-8b3b-9a9d587de5b1"),
                    Code = "STL",
                    Name = "Southland",
                    RegionImageUrl = "https://images.unsplash.com/photo-1730800672210-9ed7e0e1b789?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                },
                new Region() {
                    Id = Guid.Parse("C7B2F25A-E6B0-4FC0-B5D0-08DD62B97FAE"),
                    Code = "WKT",
                    Name = "Waikato",
                    RegionImageUrl = "https://images.unsplash.com/photo-1599636578877-1689486b6243?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
                },
            };
            //this will insert the regions data inside the regions table
            modelBuilder.Entity<Region>().HasData(regions);

            //modelBuilder.Entity<Walk>(b =>
            //{
            //    b.Property(e => e.LengthInKm).HasPrecision(5, 2); // Example: 18 digits total, 2 after the decimal
            //});
        }
    }
}
