using Microsoft.EntityFrameworkCore;
using Nous_Tale_API.Model.Entities;
using Microsoft.Extensions.Configuration;

namespace Nous_Tale_API.Model
{
    public class NousContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer();

        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Tale> Tales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tale>()
                .Property(t => t.TaleID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Room>()
                .Property(t => t.RoomID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Player>()
                .Property(t => t.PlayerID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Chapter>()
                .Property(t => t.ChapterID)
                .ValueGeneratedOnAdd();
        }
    }
}
