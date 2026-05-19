namespace VehiclePartsSystem.API.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public decimal CreditBalance { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public List<VehicleDto> Vehicles { get; set; } = new();
}

public class VehicleDto
{
    public int Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public DateTime LastServiceDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreateVehicleRequest
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonPropertyName("model")]
    public string VehicleModel { get; set; } = string.Empty;
    public int Year { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public DateTime LastServiceDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class RegisterCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public CreateVehicleRequest? Vehicle { get; set; }
}
