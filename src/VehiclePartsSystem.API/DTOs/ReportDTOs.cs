namespace VehiclePartsSystem.API.DTOs;

public class FinancialReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }
    public int TotalSalesCount { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<DailySaleDto> DailySales { get; set; } = new();
}

public class DailySaleDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int SalesCount { get; set; }
}

public class TopSpenderDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int PurchaseCount { get; set; }
}

public class PendingCreditDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal CreditBalance { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public bool IsOverdue { get; set; }
}

public class AiPredictionDto
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public List<string> PredictedFailures { get; set; } = new();
    public string RiskLevel { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
