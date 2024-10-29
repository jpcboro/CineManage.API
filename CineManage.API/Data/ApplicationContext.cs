using CineManage.API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Data
{
    public class ApplicationContext : IdentityDbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MovieGenre>().HasKey(m => new { m.GenreId, m.MovieId });
            modelBuilder.Entity<CinemaScreening>().HasKey(m => new { m.MovieTheaterId, m.MovieId });
            modelBuilder.Entity<MovieActor>().HasKey(m => new { m.ActorId, m.MovieId });
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieTheater> MovieTheaters { get; set;}
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<CinemaScreening> CinemaScreenings { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<Rating> MovieRatings { get; set; }
    }
}
