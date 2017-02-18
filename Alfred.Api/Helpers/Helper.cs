using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Alfred.Api.Helpers
{
    public class Helper
    {
        public static async Task SendEmail(string apiKey, string message, string recipient, string cc, string from, string subject)
        {
            try
            {
                // Add multiple addresses to the To field.
                var sg = new SendGridAPIClient(apiKey);

                Content content = new Content("text/html", message);
                Mail mail = new Mail(new Email(from, "The Wynn Las Vegas"), subject, new Email(recipient), content);
                mail.TemplateId = "92f28402-ec59-4255-8146-fb8c180b87b5";

                var response = await sg.client.mail.send.post(requestBody: mail.Get());
            }
            catch (Exception ex)
            {
                //TODO
            }
        }
    }
}