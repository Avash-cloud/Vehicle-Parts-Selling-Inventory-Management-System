using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace VehiclePartsSystem.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    Task SendInvoiceEmailAsync(string toEmail, string toName, string invoiceNumber, decimal total);
    Task SendCreditReminderAsync(string toEmail, string toName, decimal creditBalance);
    Task SendLowStockAlertAsync(string adminEmail, string partName, int currentStock);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:SenderName"] ?? "Vehicle Parts System",
                _config["Email:SenderEmail"] ?? "noreply@vehicleparts.com"));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Email:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_config["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }

    public async Task SendInvoiceEmailAsync(string toEmail, string toName, string invoiceNumber, decimal total)
    {
        var html = $@"
        <html><body style='font-family:Arial,sans-serif;'>
        <h2 style='color:#2c3e50;'>Vehicle Parts System - Invoice</h2>
        <p>Dear {toName},</p>
        <p>Thank you for your purchase. Your invoice details:</p>
        <table border='1' cellpadding='8' style='border-collapse:collapse;'>
            <tr><td><strong>Invoice Number</strong></td><td>{invoiceNumber}</td></tr>
            <tr><td><strong>Total Amount</strong></td><td>Rs. {total:N2}</td></tr>
            <tr><td><strong>Date</strong></td><td>{DateTime.Now:dd MMM yyyy}</td></tr>
        </table>
        <p>Thank you for choosing us!</p>
        <p><em>Vehicle Parts & Service Center</em></p>
        </body></html>";

        await SendEmailAsync(toEmail, toName, $"Invoice #{invoiceNumber}", html);
    }

    public async Task SendCreditReminderAsync(string toEmail, string toName, decimal creditBalance)
    {
        var html = $@"
        <html><body style='font-family:Arial,sans-serif;'>
        <h2 style='color:#e74c3c;'>Payment Reminder</h2>
        <p>Dear {toName},</p>
        <p>This is a reminder that you have an outstanding credit balance of <strong>Rs. {creditBalance:N2}</strong> 
        which is overdue by more than 1 month.</p>
        <p>Please visit our service center or contact us to clear your dues.</p>
        <p><em>Vehicle Parts & Service Center</em></p>
        </body></html>";

        await SendEmailAsync(toEmail, toName, "Payment Reminder - Outstanding Credit Balance", html);
    }

    public async Task SendLowStockAlertAsync(string adminEmail, string partName, int currentStock)
    {
        var html = $@"
        <html><body style='font-family:Arial,sans-serif;'>
        <h2 style='color:#e67e22;'>Low Stock Alert</h2>
        <p>The following part has fallen below the reorder level:</p>
        <table border='1' cellpadding='8' style='border-collapse:collapse;'>
            <tr><td><strong>Part Name</strong></td><td>{partName}</td></tr>
            <tr><td><strong>Current Stock</strong></td><td>{currentStock} units</td></tr>
        </table>
        <p>Please reorder this part immediately.</p>
        <p><em>Vehicle Parts System - Auto Notification</em></p>
        </body></html>";

        await SendEmailAsync(adminEmail, "Admin", $"Low Stock Alert: {partName}", html);
    }
}
