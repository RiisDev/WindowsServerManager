using System.Net;
using System.Net.Mail;

namespace WindowsServerManager.Libraries.NotificationClients.Services
{
    public static class MailService
    {
        public static async Task SendEmailAsync(string username, string password, int port, string fromEmail, string host, string toEmail, string subject, string htmlBody)
        {
            string bodyActual = $$"""
                                     <!DOCTYPE html>
                                     <html>
                                     <head>
                                         <meta charset="UTF-8">
                                         <title>Server Status Update</title>
                                         <style>
                                             body {
                                                 font-family: Arial, sans-serif;
                                                 background-color: #f4f4f4;
                                                 margin: 0;
                                                 padding: 20px;
                                             }
                                             .container {
                                                 max-width: 600px;
                                                 background: #ffffff;
                                                 padding: 20px;
                                                 border-radius: 8px;
                                                 box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                             }
                                             .header {
                                                 font-size: 20px;
                                                 font-weight: bold;
                                                 color: #333;
                                                 margin-bottom: 10px;
                                             }
                                             .content {
                                                 font-size: 16px;
                                                 color: #555;
                                             }
                                             .footer {
                                                 margin-top: 20px;
                                                 font-size: 12px;
                                                 color: #888;
                                             }
                                         </style>
                                     </head>
                                     <body>
                                         <div class="container">
                                             <div class="header">Update from Windows Server Manager</div>
                                             <div class="content">
                                                 <p>Hello,</p>
                                                 <p>This is a status update from Windows Server Server Manager.</p>
                                                 <p>{{htmlBody}}</p>
                                             </div>
                                             <div class="footer">
                                                 This is an automated message. Please do not reply.
                                             </div>
                                         </div>
                                     </body>
                                     </html>
                                     """;
            using SmtpClient client = new(host, port);
            using MailMessage mail = new(fromEmail, toEmail, subject, bodyActual);

            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;
            mail.IsBodyHtml = true;
            await client.SendMailAsync(mail);
        }
    }
}
