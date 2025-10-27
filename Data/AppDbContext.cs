using IMS.Models;
using System.Data.Entity;

namespace IMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("IMSConnection") { }

        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>().ToTable("Users");
            modelBuilder.Entity<Admin>().Property(a => a.Username).HasColumnName("Username");
            modelBuilder.Entity<Admin>().Property(a => a.PasswordHash).HasColumnName("PasswordHash");
            modelBuilder.Entity<Admin>().Property(a => a.Role).HasColumnName("Role");
        }
    }
}