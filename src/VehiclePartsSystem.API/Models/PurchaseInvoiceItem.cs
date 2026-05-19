namespace VehiclePartsSystem.API.Models;

public class PurchaseInvoiceItem
{
    public int Id { get; set; }
    public int PurchaseInvoiceId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    // Navigation
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
