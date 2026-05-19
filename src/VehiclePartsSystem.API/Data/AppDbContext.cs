using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Models;

namespace VehiclePartsSystem.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Staff -> User (one-to-one)
        modelBuilder.Entity<Staff>()
            .HasOne(s => s.User)
            .WithOne(u => u.Staff)
            .HasForeignKey<Staff>(s => s.UserId);

        // Customer -> User (one-to-one)
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne(u => u.Customer)
            .HasForeignKey<Customer>(c => c.UserId);

        // Vehicle -> Customer
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId);

        // SalesInvoice -> Customer
        modelBuilder.Entity<SalesInvoice>()
            .HasOne(si => si.Customer)
            .WithMany(c => c.SalesInvoices)
            .HasForeignKey(si => si.CustomerId);

        // SalesInvoice -> Staff
        modelBuilder.Entity<SalesInvoice>()
            .HasOne(si => si.Staff)
            .WithMany(s => s.SalesInvoices)
            .HasForeignKey(si => si.StaffId);

        // SalesInvoiceItem -> SalesInvoice
        modelBuilder.Entity<SalesInvoiceItem>()
            .HasOne(i => i.SalesInvoice)
            .WithMany(si => si.Items)
            .HasForeignKey(i => i.SalesInvoiceId);

        // SalesInvoiceItem -> Part
        modelBuilder.Entity<SalesInvoiceItem>()
            .HasOne(i => i.Part)
            .WithMany(p => p.SalesInvoiceItems)
            .HasForeignKey(i => i.PartId);

        // PurchaseInvoice -> Vendor
        modelBuilder.Entity<PurchaseInvoice>()
            .HasOne(pi => pi.Vendor)
            .WithMany(v => v.PurchaseInvoices)
            .HasForeignKey(pi => pi.VendorId);

        // PurchaseInvoiceItem -> PurchaseInvoice
        modelBuilder.Entity<PurchaseInvoiceItem>()
            .HasOne(i => i.PurchaseInvoice)
            .WithMany(pi => pi.Items)
            .HasForeignKey(i => i.PurchaseInvoiceId);

        // PurchaseInvoiceItem -> Part
        modelBuilder.Entity<PurchaseInvoiceItem>()
            .HasOne(i => i.Part)
            .WithMany(p => p.PurchaseInvoiceItems)
            .HasForeignKey(i => i.PartId);

        // Appointment -> Customer
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId);

        // Review -> Customer
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CustomerId);

        // PartRequest -> Customer
        modelBuilder.Entity<PartRequest>()
            .HasOne(pr => pr.Customer)
            .WithMany(c => c.PartRequests)
            .HasForeignKey(pr => pr.CustomerId);

        // Decimal precision (ignored by SQLite, kept for documentation)
        // modelBuilder.Entity<Part>().Property(p => p.CostPrice).HasPrecision(18, 2);

        // Seed admin user (password: Admin@123)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            FullName = "System Admin",
            Email = "admin@vehicleparts.com",
            PasswordHash = "$2a$11$UMc6K1LUnXM.6ufF8hG.uuWlZ3nzhCQX7PpmCnpnwz/pZ7O431rHK",
            Phone = "9800000000",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
