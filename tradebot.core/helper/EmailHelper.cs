using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace tradebot.core.helper
{
    public class EmailHelper : IEmailHelper
    {
        private ILogger<EmailHelper> _logger;
        private IConfiguration _configuration;
        public EmailHelper(ILogger<EmailHelper> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;
        }
        public async Task SendEmail(string subject, string content){
            var fromAddress = this._configuration["Email:EmailFrom"];
            var fromName = this._configuration["Email:FromName"];
            var toAddress = this._configuration["Email:EmailTo"];
            var toName = this._configuration["Email:ToName"];
            var apiKey = this._configuration["Email:ApiKey"];
            
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromAddress, fromName);
            var to = new EmailAddress(toAddress, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);

        }
    }
}