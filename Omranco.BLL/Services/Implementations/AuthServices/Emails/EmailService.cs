using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Omranco.BLL.Services.Implementations.AuthServices.Emails;
public class EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger) : IEmailSender
{
    private readonly MailSettings _mailSettings = mailSettings.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage
        {
            Subject = subject
        };
        message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
        message.To.Add(MailboxAddress.Parse(email));

        var builder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        _logger.LogInformation("Sending email to {email}", email);

        await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}