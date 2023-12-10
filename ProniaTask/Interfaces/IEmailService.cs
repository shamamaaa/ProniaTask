using System;
namespace ProniaTask.Interfaces
{
    public interface IEmailService
    {
        Task SendMailAsync(string emailTo, string subject, string body, bool isHTML = false);
    }
}

