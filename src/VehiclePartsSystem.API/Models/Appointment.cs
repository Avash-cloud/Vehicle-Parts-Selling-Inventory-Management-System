namespace VehiclePartsSystem.API.Models;

public enum AppointmentStatus { Pending, Confirmed, Completed, Cancelled }

public class Appointment
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Customer Customer { get; set; } = null!;
}
