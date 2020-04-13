using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Services
{
    public class EmailSender : IEmailSender
    {
        //Recall I registered my EmailOptions class in the startup.cs by configuring it and because I did not register it with an interface(container), the default microsoft Interface(which is Ioptions) was used.
        //this means that for me to have access to this class I registered above, I have to use DI i.e IOptions<EmailOptions> <variableName>, this is EmailOptions because that was the class I registered.

        //to get access to the actual values of this EmailOptions, unlike the DB context or any other service registered wir=th their own interface, that we just do "_db = db", here we have to use Value because it is registered in Ioption as a key value pair where the Key <EmailOptions>, so the value is now <ClassName>.Value;
        public EmailOptions Options { get; set; }
        public EmailSender(IOptions<EmailOptions> emailOptions)
        {
            Options = emailOptions.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            await Execute(Options.SendGridKey, subject, message, email);
        }

        private async Task Execute(string sendGridKey, string subject, string message, string email)
        {
            //for the send grid configuration

            //1. configure the client which is us with the sendgrid api key
            var client = new SendGridClient(sendGridKey);

            //2. Construct the message
            var msg = new SendGridMessage()
            {
                //                      must be a legit email and added to the sendgrid authenticated users 
                From = new EmailAddress("okafor.c.chidimma@gmail.com", "Spice Restaurant"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            //3. Add the receivers email to the message object
            msg.AddTo(new EmailAddress(email));
            try
            {
                //4. Send the mail
                var response = await client.SendEmailAsync(msg);
                var error = response.Body.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new Exception($"This Email could not be sent because {ex}");
            }
           
        }
    }
}
