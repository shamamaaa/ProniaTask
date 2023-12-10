using System;
using System.Net;
using System.Net.Mail;
using ProniaTask.Interfaces;

namespace ProniaTask.Services
{
	public class EmailService:IEmailService
	{
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendMailAsync(string emailTo, string subject, string body, bool isHTML = false)
        {
            SmtpClient smtp = new SmtpClient(_configuration["Email:Host"], Convert.ToInt32(_configuration["Email:Port"]));
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(_configuration["Email:LoginEmail"], _configuration["Email:Password"]);

            MailAddress from = new MailAddress(_configuration["Email:LoginEmail"], "Pronia Team");
            MailAddress to = new MailAddress(emailTo);

            MailMessage message = new MailMessage(from, to);

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHTML;
            await smtp.SendMailAsync(message);
        }
    }
}

