namespace VehiclePartsSystem.API.Models;

public enum PartRequestStatus { Pending, Fulfilled, Rejected }

public class PartRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Customer Customer { get; set; } = null!;
}
