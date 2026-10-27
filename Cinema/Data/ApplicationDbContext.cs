
using Cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }

        public DbSet<Cinema.Models.Cinema> Cinemas { get; set; }

        public DbSet<Actor> Actors { get; set; }

        public DbSet<Movie> Movies { get; set; }

        public DbSet<MovieSubImage> MovieSubImages { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(
                "Data Source=LAPTOP-7D13UVG2;Initial Catalog=Cinema;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True");
        }

       
protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Actors)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "MovieActors",
                    j => j
                        .HasOne<Actor>()
                        .WithMany()
                        .HasForeignKey("ActorId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Movie>()
                        .WithMany()
                        .HasForeignKey("MovieId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("MovieId", "ActorId");
                    });

        }
    }
}
