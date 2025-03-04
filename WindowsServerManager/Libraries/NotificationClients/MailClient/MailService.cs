using System.Net;
using System.Net.Mail;

namespace WindowsServerManager.Libraries.NotificationClients.MailClient
{
    public record MailSettings(string Username, string Password, int Port, string FromEmail, string Host);

    public class MailService(MailSettings mailConfig)
    {
        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            using SmtpClient client = new(mailConfig.Host, mailConfig.Port);
            using MailMessage mail = new(mailConfig.FromEmail, toEmail, subject, htmlBody);

            client.Credentials = new NetworkCredential(mailConfig.Username, mailConfig.Password);
            client.EnableSsl = true;
            mail.IsBodyHtml = true;
            await client.SendMailAsync(mail);
        }
    }
}
