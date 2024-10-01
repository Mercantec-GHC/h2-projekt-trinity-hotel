using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Blazor.Services
{
    public class MailServices
    {
        private readonly string ConfirmationEmail;
        private readonly string AppPassword;

        public MailServices(IConfiguration configuration)
        {
            // Read the email settings from the configuration
            ConfirmationEmail = configuration["EmailSettings:ConfirmationEmail"];
            AppPassword = configuration["EmailSettings:AppPassword"];
        }

        public async Task SendEmail(string toEmail, string subject, string message)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(ConfirmationEmail, AppPassword)
            };

            try
            {
                await client.SendMailAsync(new MailMessage(ConfirmationEmail, toEmail, subject, message));
                Console.WriteLine("Mail sent successfully.");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Failed to send Email: {ex.Message}");
                Console.WriteLine($"Status Code: {ex.StatusCode}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            }
        }
    }
}









