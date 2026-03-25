using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Vml;
using LibraryMS.BLL.Models;
using MailKit.Security;
using MimeKit;

namespace LibraryMS.BLL.Services
{
    public sealed class EmailSenderService
    {
        private readonly EmailSettings _settings;

        public EmailSenderService(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            var secureSocket = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocket);

            if (!string.IsNullOrWhiteSpace(_settings.UserName))
                await client.AuthenticateAsync(_settings.UserName, _settings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
