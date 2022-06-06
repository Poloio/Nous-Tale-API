 using Microsoft.EntityFrameworkCore;
using Nous_Tale_API.Model.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Nous_Tale_API.Model
{
    public class NousContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseSqlServer("Server=HOESMACHINE;Database=NousTaleDB;User Id=apiuser;Password=080899_Ap;Integrated Security = False")                .UseLazyLoadingProxies()
                .ConfigureWarnings(b => b.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning));
        }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Player> Players { get; set; } 
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Tale> Tales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tale>()
                .Property(t => t.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Room>()
                .Property(t => t.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Player>()
                .Property(t => t.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Chapter>()
                .Property(t => t.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Chapter>(chapter =>
            {
                chapter.HasOne(c => c.Tale)
                .WithMany(t => t.Chapters)
                .HasForeignKey(c => c.TaleID)
                .OnDelete(DeleteBehavior.NoAction);

                chapter.HasOne(c => c.Player)
                .WithMany(p => p.Chapters)
                .HasForeignKey(c => c.PlayerID)
                .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Player>(player =>
            {
                player.HasOne(p => p.Room)
                .WithMany(r => r.Players)
                .HasForeignKey(p => p.RoomID)
                .OnDelete(DeleteBehavior.Cascade);

                player.HasMany(p => p.Chapters)
                .WithOne(c => c.Player);
            });

            modelBuilder.Entity<Room>(room =>
            {
                room.HasMany(r => r.Players)
                .WithOne(p => p.Room);
                room.HasMany(r => r.Tales)
                .WithOne(t => t.Room);
            });

            modelBuilder.Entity<Tale>(tale =>
            {
                tale.HasOne(t => t.Room)
                .WithMany(r => r.Tales)
                .HasForeignKey(t => t.RoomID)
                .OnDelete(DeleteBehavior.Cascade);

                tale.HasMany(t => t.Chapters)
                .WithOne(c => c.Tale);
            });

                
            
        }
    }
}
