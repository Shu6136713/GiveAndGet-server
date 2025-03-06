using System;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;

namespace Services.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string subject, string body, string dest)
        {
            try
            {
                // הגדרת פרטי המייל ושליחתו
                string source = _configuration["EmailSettings:SenderEmail"];

                // שליחת המייל (השתמש ב-MailKit או ב-Outlook לפי הצורך)
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sender", source));
                emailMessage.To.Add(new MailboxAddress("", dest));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("plain") { Text = body };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(_configuration["EmailSettings:SmtpHost"], int.Parse(_configuration["EmailSettings:SmtpPort"]), true);
                    smtpClient.Authenticate(source, _configuration["EmailSettings:SenderPassword"]);
                    smtpClient.Send(emailMessage);
                    smtpClient.Disconnect(true);
                }

                Console.WriteLine($"Email sent successfully to {dest}");
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה, הדפס את השגיאה וזרוק אותה כדי שהקוד יוכל לתפוס ולהגיב בשגיאה
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw new Exception(ex.Message); // שים לב ש-throw לא משאיר את השגיאה ללא טיפול, ומאפשר לקוד שמזמין את המתודה לדעת על השגיאה
            }
        }

    }
}
