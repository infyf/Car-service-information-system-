using AutoServ.Core.Entities;
using AutoServ.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
        public DbSet<ServiceDetail> ServiceDetails => Set<ServiceDetail>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingService> BookingServices => Set<BookingService>();
        public DbSet<Master> Masters => Set<Master>();
        public DbSet<Recommendation> Recommendations => Set<Recommendation>();
        public DbSet<Consultation> Consultations => Set<Consultation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<UserProfile>().ToTable("user_profiles");
            modelBuilder.Entity<Service>().ToTable("services");
            modelBuilder.Entity<ServiceItem>().ToTable("serviceitems");
            modelBuilder.Entity<ServiceDetail>().ToTable("servicedetails");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<BookingService>().ToTable("booking_services");
            modelBuilder.Entity<Master>().ToTable("masters");
            modelBuilder.Entity<Recommendation>().ToTable("recommendations");
            modelBuilder.Entity<Consultation>().ToTable("consultations");

            modelBuilder.Entity<Booking>().Property(b => b.TotalPrice).HasPrecision(18, 2);
            modelBuilder.Entity<BookingService>().Property(bs => bs.PriceAtBooking).HasPrecision(18, 2);

            
            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion(
                    v => v.ToString().ToLower(), 
                    v => (BookingStatus)Enum.Parse(typeof(BookingStatus), v.Replace("_", ""), true) 
                );

            modelBuilder.Entity<Booking>().HasOne(b => b.Master).WithMany(m => m.Bookings).HasForeignKey(b => b.MasterId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<BookingService>().HasOne(bs => bs.Booking).WithMany(b => b.Services).HasForeignKey(bs => bs.BookingId);
            modelBuilder.Entity<BookingService>().HasOne(bs => bs.ServiceItem).WithMany().HasForeignKey(bs => bs.ServiceItemId);
            modelBuilder.Entity<Recommendation>().HasOne(r => r.Booking).WithMany(b => b.Recommendations).HasForeignKey(r => r.BookingId);
            modelBuilder.Entity<Recommendation>().HasOne(r => r.ServiceItem).WithMany().HasForeignKey(r => r.ServiceItemId);
            modelBuilder.Entity<Consultation>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Consultation>().HasOne(c => c.Master).WithMany().HasForeignKey(c => c.MasterId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Master>().HasMany(m => m.ServiceItems).WithMany(s => s.Masters).UsingEntity<Dictionary<string, object>>(
                "masterspecializations",
                j => j.HasOne<ServiceItem>().WithMany().HasForeignKey("ServiceItemId"),
                j => j.HasOne<Master>().WithMany().HasForeignKey("MasterId"),
                j => { j.Property<DateTime>("CreatedAt").HasDefaultValueSql("CURRENT_TIMESTAMP"); j.HasKey("MasterId", "ServiceItemId"); });
        }
    }
}