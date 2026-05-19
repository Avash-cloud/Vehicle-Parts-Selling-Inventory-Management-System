namespace VehiclePartsSystem.API.DTOs;

public class SalesInvoiceItemRequest
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
}

public class CreateSalesInvoiceRequest
{
    public int CustomerId { get; set; }
    public bool IsCreditSale { get; set; } = false;
    public string Notes { get; set; } = string.Empty;
    public List<SalesInvoiceItemRequest> Items { get; set; } = new();
}

public class SalesInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCreditSale { get; set; }
    public bool IsPaid { get; set; }
    public DateTime InvoiceDate { get; set; }
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
}

public class SalesInvoiceItemDto
{
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class PurchaseInvoiceItemRequest
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreatePurchaseInvoiceRequest
{
    public int VendorId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseInvoiceItemRequest> Items { get; set; } = new();
}

public class PurchaseInvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public List<PurchaseInvoiceItemDto> Items { get; set; } = new();
}

public class PurchaseInvoiceItemDto
{
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
}
