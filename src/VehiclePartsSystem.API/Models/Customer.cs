namespace VehiclePartsSystem.API.Models;

public class Customer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; } = 0;
    public decimal CreditBalance { get; set; } = 0;
    public DateTime? CreditDueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
}
