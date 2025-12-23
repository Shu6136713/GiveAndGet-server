using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Repositories.Entity;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Mock
{
    public class GiveAndGetDataBase : DbContext, IContext
    {

        //public GiveAndGetDB(DbContextOptions<GiveAndGetDB> options)
        //   : base(options)
        //{
        //}

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Talent> Talents { get; set; }
        public DbSet<TalentRequest> TalentRequests { get; set; }
        public DbSet<TalentUser> TalentUser { get; set; }



        //when using Dependency Injection OnConfiguring is not needed

        public void Save()
        {
            SaveChanges();
        }

        public async Task SaveAsync()
        {
            await SaveChangesAsync();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=sql;database=GiveAndGetDataB;trusted_connection=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // unique indexes for the models
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.HashPwd)
                .IsUnique();

            modelBuilder.Entity<Talent>()
                .HasIndex(t => t.TalentName)
                .IsUnique();

            modelBuilder.Entity<TalentUser>()
           .HasKey(tu => new { tu.UserId, tu.TalentId });
        }
    }
}
