using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        // ===================== DbSets =====================
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SlaConfiguration> SlaConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== User =====================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.IsActive).IsRequired();
            });

            // ===================== Ticket =====================
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);
                
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).IsRequired();
                entity.Property(t => t.Status).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Priority).IsRequired().HasMaxLength(50);

                // CreatedBy relationship
                entity.HasOne(t => t.CreatedBy)
                      .WithMany(u => u.CreatedTickets)
                      .HasForeignKey(t => t.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);

                // AssignedTo relationship
                entity.HasOne(t => t.AssignedTo)
                      .WithMany(u => u.AssignedTickets)
                      .HasForeignKey(t => t.AssignedToId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Category relationship
                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Tickets)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== Category =====================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            });

            // ===================== TicketComment =====================
            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.HasKey(tc => tc.Id);

                entity.HasOne(tc => tc.Ticket)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(tc => tc.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tc => tc.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(tc => tc.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== TicketHistory =====================
            modelBuilder.Entity<TicketHistory>(entity =>
            {
                entity.HasKey(th => th.Id);

                entity.HasOne(th => th.Ticket)
                      .WithMany(t => t.History)
                      .HasForeignKey(th => th.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(th => th.User)
                      .WithMany(u => u.TicketHistories)
                      .HasForeignKey(th => th.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== Notification =====================
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Ticket)
                      .WithMany()
                      .HasForeignKey(n => n.TicketId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // ===================== SLA Configuration =====================
            modelBuilder.Entity<SlaConfiguration>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Priority).IsRequired().HasMaxLength(20);
            });

            SeedData(modelBuilder);
        }

        // ===================== Seed Data =====================
        private static void SeedData(ModelBuilder modelBuilder)
        {
            var fixedDate = new DateTime(2024, 1, 1);

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Hardware Issue", SlaHours = 24, IsActive = true, CreatedAt = fixedDate },
                new Category { Id = 2, Name = "Software Issue", SlaHours = 48, IsActive = true, CreatedAt = fixedDate },
                new Category { Id = 3, Name = "Network Issue", SlaHours = 12, IsActive = true, CreatedAt = fixedDate },
                new Category { Id = 4, Name = "Access Request", SlaHours = 72, IsActive = true, CreatedAt = fixedDate }
            );

            // SLA Configurations
            modelBuilder.Entity<SlaConfiguration>().HasData(
                new SlaConfiguration { Id = 1, Priority = "Low", ResponseTimeHours = 24, ResolutionTimeHours = 96, IsActive = true, CreatedAt = fixedDate },
                new SlaConfiguration { Id = 2, Priority = "Medium", ResponseTimeHours = 8, ResolutionTimeHours = 48, IsActive = true, CreatedAt = fixedDate },
                new SlaConfiguration { Id = 3, Priority = "High", ResponseTimeHours = 4, ResolutionTimeHours = 24, IsActive = true, CreatedAt = fixedDate },
                new SlaConfiguration { Id = 4, Priority = "Critical", ResponseTimeHours = 1, ResolutionTimeHours = 12, IsActive = true, CreatedAt = fixedDate }
            );

            // Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "System Administrator",
                    Email = "admin@smartticket.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = fixedDate
                }
            );
        }
    }
}