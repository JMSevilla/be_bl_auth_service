using Microsoft.EntityFrameworkCore;
using WTW.AuthenticationService.Domain;

namespace WTW.AuthenticationService.Infrastructure
{
    public class AuthenticationDbContext : DbContext
    {
        public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens").HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("Id")
                    .IsRequired();
                entity
                    .Property(c => c.UserId)
                    .HasColumnName("UserId")
                    .HasMaxLength(255)
                    .IsRequired();
                entity
                    .Property(c => c.SessionId)
                    .HasColumnName("SessionId")
                    .HasMaxLength(255)
                    .IsRequired();
                entity
                    .Property(c => c.Value)
                    .HasMaxLength(255)
                    .HasColumnName("Value");
                entity
                    .Property(c => c.ExpiresAt)
                    .HasColumnName("ExpiresAt");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}