using Microsoft.EntityFrameworkCore;
using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure
{
    public class MemberDbContext : DbContext
    {
        public MemberDbContext(DbContextOptions<MemberDbContext> options) : base(options)
        {
        }

        public DbSet<User> MemberUserAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("WW_INT_REG_USER_INFO").HasKey(u => u.UserName);
                entity
                    .Property(c => c.UserName)
                    .HasColumnName("USERID")
                    .IsRequired();
                entity
                    .Property(c => c.BusinessGroup)
                    .HasColumnName("BGROUP");
                entity
                    .Property(c => c.ReferenceNumber)
                    .HasColumnName("REFNO");
                entity
                    .Property(c => c.Status)
                    .HasColumnName("USTATUS");
                entity
                    .Property(c => c.Type)
                    .HasColumnName("USERTYPE");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}