using CineManage.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineManage.API.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
    }
}
