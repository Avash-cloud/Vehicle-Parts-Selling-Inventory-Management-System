namespace VehiclePartsSystem.API.Models;

public class Staff
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
}
