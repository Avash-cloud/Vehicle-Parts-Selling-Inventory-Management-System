namespace VehiclePartsSystem.API.Models;

public enum PaymentStatus { Paid, Pending, Overdue }

public class PurchaseInvoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Vendor Vendor { get; set; } = null!;
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}
