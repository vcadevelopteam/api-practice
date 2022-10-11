using Serilog.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Cores;

namespace TaskInitializer.Services
{
    public class MailService
    {
        public static void SendMail(AppSettings AppSettings, Logger Logger, SendMailBody SendMailBody)
        {
            try
            {
                Logger.ForContext("Context", "SendMail").Debug("Sending message: {MailTitle}", SendMailBody.MailTitle);

                MailMessage MailMessage = new MailMessage()
                {
                    From = new MailAddress(AppSettings.MailSettings.Address),
                    Subject = SendMailBody.MailTitle,
                    Body = SendMailBody.MailBody,
                    IsBodyHtml = true
                };

                if (!string.IsNullOrWhiteSpace(SendMailBody.MailAttachmentName) && SendMailBody.MailAttachmentData != null)
                {
                    Attachment Attachment = new Attachment(new MemoryStream(SendMailBody.MailAttachmentData), SendMailBody.MailAttachmentName);

                    MailMessage.Attachments.Add(Attachment);
                }

                if (!string.IsNullOrWhiteSpace(SendMailBody.MailBlindAddress))
                {
                    foreach (var Address in SendMailBody.MailBlindAddress.Split(","))
                    {
                        MailMessage.Bcc.Add(Address);
                    }

                    foreach (var Address in SendMailBody.MailBlindAddress.Split(";"))
                    {
                        MailMessage.Bcc.Add(Address);
                    }
                }

                if (!string.IsNullOrWhiteSpace(SendMailBody.MailCopyAddress))
                {
                    foreach (var Address in SendMailBody.MailCopyAddress.Split(","))
                    {
                        MailMessage.CC.Add(Address);
                    }

                    foreach (var Address in SendMailBody.MailCopyAddress.Split(";"))
                    {
                        MailMessage.CC.Add(Address);
                    }
                }

                if (!string.IsNullOrWhiteSpace(SendMailBody.MailAddress))
                {
                    foreach (var Address in SendMailBody.MailAddress.Split(","))
                    {
                        MailMessage.To.Add(Address);
                    }

                    foreach (var Address in SendMailBody.MailAddress.Split(";"))
                    {
                        MailMessage.To.Add(Address);
                    }
                }

                SmtpClient SmtpClient = new SmtpClient("smtp.gmail.com")
                {
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Port = 587,
                    Credentials = new NetworkCredential(AppSettings.MailSettings.Address, AppSettings.MailSettings.Password)
                };

                SmtpClient.Send(MailMessage);

                Logger.ForContext("Context", "SendMail").Debug("Message sent: {MailTitle}", SendMailBody.MailTitle);
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "SendMail").Error(Exception, "Exception found:");
            }
        }
    }
}