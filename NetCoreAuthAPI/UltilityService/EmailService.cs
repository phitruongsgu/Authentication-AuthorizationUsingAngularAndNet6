using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using NetCoreAuthAPI.Models;

namespace NetCoreAuthAPI.UltilityService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;   
        public EmailService(IConfiguration configuration)
        {

            _configuration = configuration; 
        }
        public void SendEmail(EmailModel emailModel)
        {
            var emailMessage = new MimeMessage();
            var from = _configuration["EmailSettings:From"];
            emailMessage.From.Add(new MailboxAddress("Phi Trường", from));
            emailMessage.To.Add(new MailboxAddress(emailModel.To, emailModel.To));
            emailMessage.Subject = emailModel.Subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = string.Format(emailModel.Content)
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_configuration["EmailSettings:SmtpServer"], 465, true);
                    client.Authenticate(_configuration["EmailSettings:From"], _configuration["EmailSettings:Password"]);
                    client.Send(emailMessage);
                }catch (Exception ex) {
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
