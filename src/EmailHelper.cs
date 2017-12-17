using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace tradebot
{
    public static class EmailHelper
    {
        public async static Task SendEmail(string subject, string toAddress, string content)
        {
            var apiKey = Program.Configuration["Email:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("phi.huynh@nashtechglobal.com", "Phi Huynh");
            var to = new EmailAddress(toAddress, "Phi Huynh");
            var plainTextContent = content;
            var htmlContent = content;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}