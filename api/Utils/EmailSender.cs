using IdentityService.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
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
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.IsBodyHtml = true;

                    // Wrap body with branded template including logo
                    var brandedBody = $@"
                        <div style='max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif;'>
                            <div style='text-align: center; padding: 20px 0; background: #1a1a2e; border-radius: 8px 8px 0 0;'>
                                <img src='cid:evantix-logo' alt='Evantix' width='120' style='display: inline-block;' />
                            </div>
                            <div style='padding: 20px; background: #ffffff; border: 1px solid #eee; border-top: none; border-radius: 0 0 8px 8px;'>
                                {emailInfo.Body}
                            </div>
                            <div style='text-align: center; padding: 15px; color: #999; font-size: 11px;'>
                                <p>© {DateTime.Now.Year} Evantix. Todos los derechos reservados.</p>
                            </div>
                        </div>";

                    // Embed logo as inline attachment
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "Evantix.png");
                    if (File.Exists(logoPath))
                    {
                        var htmlView = AlternateView.CreateAlternateViewFromString(brandedBody, System.Text.Encoding.UTF8, "text/html");
                        var logoResource = new LinkedResource(logoPath, "image/png")
                        {
                            ContentId = "evantix-logo",
                            TransferEncoding = TransferEncoding.Base64
                        };
                        htmlView.LinkedResources.Add(logoResource);
                        message.AlternateViews.Add(htmlView);
                    }
                    else
                    {
                        message.Body = brandedBody;
                    }

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
