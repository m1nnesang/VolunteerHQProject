using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using VolunteerHQ.Core.Settings;
using VolunteerHQ.Infrastructure.Services.Interfaces;

namespace VolunteerHQ.Infrastructure.Services.Classes;

public class EmailService :  IEmailService
{
    private readonly EmailSettings _settings;
    
    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var message = new MimeKit.MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.Username));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
    
}
