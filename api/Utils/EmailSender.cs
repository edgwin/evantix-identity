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

                    var smtpHost = Configuration.Config.GetSection("SmtpSettings:Host").Value;
                    var smtpPort = int.Parse(Configuration.Config.GetSection("SmtpSettings:Port").Value);
                    var smtpUser = Configuration.Config.GetSection("SmtpSettings:Username").Value;
                    var smtpPass = Configuration.Config.GetSection("SmtpSettings:Password").Value;
                    var enableSsl = bool.Parse(Configuration.Config.GetSection("SmtpSettings:EnableSsl").Value ?? "true");

                    using (var client = new SmtpClient(smtpHost, smtpPort))
                    {
                        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                        client.EnableSsl = enableSsl;
                        await client.SendMailAsync(message);
                    }
                }
        }

    }
}
