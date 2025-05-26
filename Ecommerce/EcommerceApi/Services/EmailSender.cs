using Ecommerce.Configurations;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;


namespace Ecommerce.Services;
public class EmailSender(IOptions<EmailSettings> options,ILogger<EmailSender> logger)
{
    private readonly EmailSettings _settings = options.Value;

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, false);

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }
}
