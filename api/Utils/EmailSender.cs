using IdentityService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace IdentityService.Utils
{
    public static class EmailSender
    {
        public static async Task SendAsync(EmailDto emailInfo)
        {
                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(emailInfo.ToAddress, emailInfo.ToName));
                    message.From = new MailAddress(emailInfo.FromAddress, emailInfo.FromName);
                    message.Subject = emailInfo.Subject;
                    message.Body = emailInfo.Body;
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.IsBodyHtml = true;

                    using (var client = new SmtpClient("smtp.gmail.com"))
                    {
                        client.Port = 587;
                        client.Credentials = new NetworkCredential("edgwin@gmail.com", "H@d@lind@2019");
                        client.EnableSsl = true;
                        await client.SendMailAsync(message);
                    }
                }
        }

    }
}
