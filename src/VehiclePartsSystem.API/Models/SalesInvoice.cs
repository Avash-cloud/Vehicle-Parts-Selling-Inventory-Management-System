namespace VehiclePartsSystem.API.Models;

public class SalesInvoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int StaffId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public bool IsCreditSale { get; set; } = false;
    public bool IsPaid { get; set; } = true;
    public DateTime? PaymentDueDate { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
}
