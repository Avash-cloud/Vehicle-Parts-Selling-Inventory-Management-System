using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.API.Data;
using VehiclePartsSystem.API.DTOs;

namespace VehiclePartsSystem.API.Services;

public interface IReportService
{
    Task<FinancialReportDto> GetFinancialReportAsync(string period, DateTime? from = null, DateTime? to = null);
    Task<List<TopSpenderDto>> GetTopSpendersAsync(int top = 10);
    Task<List<PendingCreditDto>> GetPendingCreditsAsync();
    Task<List<CustomerDto>> GetRegularCustomersAsync(int minPurchases = 3);
    Task SendCreditRemindersAsync();
}

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;

    public ReportService(AppDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(string period, DateTime? from = null, DateTime? to = null)
    {
        DateTime start, end;
        var now = DateTime.UtcNow;

        switch (period.ToLower())
        {
            case "daily":
                start = now.Date;
                end = now.Date.AddDays(1);
                break;
            case "monthly":
                start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                end = start.AddMonths(1);
                break;
            case "yearly":
                start = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                end = start.AddYears(1);
                break;
            case "custom":
                start = from ?? now.Date;
                end = to ?? now.Date.AddDays(1);
                break;
            default:
                start = now.Date;
                end = now.Date.AddDays(1);
                break;
        }

        var invoices = await _db.SalesInvoices
            .Include(si => si.Items).ThenInclude(i => i.Part)
            .Where(si => si.InvoiceDate >= start && si.InvoiceDate < end)
            .ToListAsync();

        var totalRevenue = invoices.Sum(si => si.TotalAmount);
        var totalCost = invoices.SelectMany(si => si.Items)
            .Sum(i => i.Part.CostPrice * i.Quantity);

        var dailySales = invoices
            .GroupBy(si => si.InvoiceDate.Date)
            .Select(g => new DailySaleDto
            {
                Date = g.Key,
                Revenue = g.Sum(si => si.TotalAmount),
                SalesCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new FinancialReportDto
        {
            TotalRevenue = totalRevenue,
            TotalCost = totalCost,
            GrossProfit = totalRevenue - totalCost,
            TotalSalesCount = invoices.Count,
            PeriodStart = start,
            PeriodEnd = end,
            DailySales = dailySales
        };
    }

    public async Task<List<TopSpenderDto>> GetTopSpendersAsync(int top = 10)
    {
        return await _db.Customers
            .Include(c => c.User)
            .Include(c => c.SalesInvoices)
            .OrderByDescending(c => c.TotalSpent)
            .Take(top)
            .Select(c => new TopSpenderDto
            {
                CustomerId = c.Id,
                CustomerName = c.User.FullName,
                Phone = c.User.Phone,
                TotalSpent = c.TotalSpent,
                PurchaseCount = c.SalesInvoices.Count
            })
            .ToListAsync();
    }

    public async Task<List<PendingCreditDto>> GetPendingCreditsAsync()
    {
        return await _db.Customers
            .Include(c => c.User)
            .Where(c => c.CreditBalance > 0)
            .Select(c => new PendingCreditDto
            {
                CustomerId = c.Id,
                CustomerName = c.User.FullName,
                Email = c.User.Email,
                Phone = c.User.Phone,
                CreditBalance = c.CreditBalance,
                CreditDueDate = c.CreditDueDate,
                IsOverdue = c.CreditDueDate.HasValue && c.CreditDueDate.Value < DateTime.UtcNow
            })
            .ToListAsync();
    }

    public async Task<List<CustomerDto>> GetRegularCustomersAsync(int minPurchases = 3)
    {
        return await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Include(c => c.SalesInvoices)
            .Where(c => c.SalesInvoices.Count >= minPurchases)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User.FullName,
                Email = c.User.Email,
                Phone = c.User.Phone,
                Address = c.Address,
                TotalSpent = c.TotalSpent,
                CreditBalance = c.CreditBalance
            })
            .ToListAsync();
    }

    public async Task SendCreditRemindersAsync()
    {
        var overdueDate = DateTime.UtcNow.AddMonths(-1);
        var overdueCustomers = await _db.Customers
            .Include(c => c.User)
            .Where(c => c.CreditBalance > 0 && c.CreditDueDate.HasValue && c.CreditDueDate.Value < overdueDate)
            .ToListAsync();

        foreach (var customer in overdueCustomers)
        {
            await _email.SendCreditReminderAsync(
                customer.User.Email,
                customer.User.FullName,
                customer.CreditBalance);

            _db.Notifications.Add(new Models.Notification
            {
                Title = "Credit Reminder Sent",
                Message = $"Reminder sent to {customer.User.FullName} for Rs. {customer.CreditBalance:N2}",
                Type = "CreditReminder",
                UserId = customer.UserId
            });
        }

        await _db.SaveChangesAsync();
    }
}
