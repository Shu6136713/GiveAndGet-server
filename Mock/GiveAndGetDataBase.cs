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



        // אין צורך ב-OnConfiguring כאשר משתמשים ב-Dependency Injection

        public void Save()
        {
            SaveChanges();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=sql;database=GiveAndGetDataBase;trusted_connection=true");
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // הגדרת אינדקסים ייחודיים עבור המודל User
        //    modelBuilder.Entity<User>()
        //        .HasIndex(u => u.UserName)
        //        .IsUnique();

        //    modelBuilder.Entity<User>()
        //        .HasIndex(u => u.Email)
        //        .IsUnique();

        //    modelBuilder.Entity<User>()
        //        .HasIndex(u => u.PhoneNumber)
        //        .IsUnique();

        //    modelBuilder.Entity<User>()
        //        .HasIndex(u => u.HashPwd)
        //        .IsUnique();
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // הגדרת אינדקסים ייחודיים עבור המודל User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            //    // טבלת ביניים לכשרונות שמוצעים
            //    modelBuilder.Entity<User>()
            //        .HasMany(u => u.TalentsOffered)
            //        .WithMany()
            //        .UsingEntity(j => j.ToTable("UserTalentsOffered"));

            //    // טבלת ביניים לכשרונות שמחפשים
            //    modelBuilder.Entity<User>()
            //        .HasMany(u => u.TalentsWanted)
            //        .WithMany()
            //        .UsingEntity(j => j.ToTable("UserTalentsWanted"));
        }
    }
}