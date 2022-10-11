using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Core;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using ZyxMeBridge.Models.Common;
using ZyxMeBridge.Models.Controllers.Scheduler;
using ZyxMeBridge.Models.Controllers.Scheduler.Input;
using ZyxMeBridge.Models.Controllers.Scheduler.Output;
using ZyxMeBridge.Processors;

namespace ZyxMeBridge.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class ProcessSchedulerController : Controller
    {
        private readonly AppSettings AppSettings;

        private readonly Logger Logger;

        public ProcessSchedulerController(IOptions<AppSettings> AppSettings)
        {
            Logger = LoggerProcessor.ConfigureLogger(AppSettings.Value, "Scheduler");

            this.AppSettings = AppSettings.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Json(new { controller = "Scheduler" });
        }

        [HttpPost]
        [Route("SendMail")]
        public ActionResult<SendMailOutput> SendMail([FromBody] object RawInput)
        {
            Logger.ForContext("Context", "POST: SendMail").Information("Input: {RawInput}", JsonConvert.SerializeObject(RawInput));

            SendMailOutput Output = new SendMailOutput();

            try
            {
                SendMailInput Input = JsonConvert.DeserializeObject<SendMailInput>(RawInput.ToString());

                MailCredentials DefaultCredentials = new MailCredentials()
                {
                    Password = AppSettings.MailSettings.Password,
                    Username = AppSettings.MailSettings.Address,
                    DefaultCredentials = false,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    Port = "587"
                };

                if (!string.IsNullOrWhiteSpace(Input.MailBody))
                {
                    Input.MailBody = Input.MailBody.Replace(Environment.NewLine, "<br />");
                }

                if (!string.IsNullOrWhiteSpace(Input.MailCredentials))
                {
                    MailCredentials IncomingCredentials = JsonConvert.DeserializeObject<MailCredentials>(Input.MailCredentials);

                    if (!string.IsNullOrWhiteSpace(IncomingCredentials.Username) && !string.IsNullOrWhiteSpace(IncomingCredentials.Password))
                    {
                        DefaultCredentials = IncomingCredentials;
                    }
                }

                MailMessage MailMessage = new MailMessage()
                {
                    From = new MailAddress(DefaultCredentials.Username),
                    Subject = Input.MailTitle,
                    Body = Input.MailBody,
                    IsBodyHtml = true
                };

                if (!string.IsNullOrWhiteSpace(Input.MailAttachmentName) && Input.MailAttachmentData != null)
                {
                    Attachment Attachment = new Attachment(new MemoryStream(Input.MailAttachmentData), Input.MailAttachmentName);

                    MailMessage.Attachments.Add(Attachment);
                }

                if (Input.Attachments != null)
                {
                    foreach (var MailAttachment in Input.Attachments)
                    {
                        if (!string.IsNullOrWhiteSpace(MailAttachment.Value))
                        {
                            switch (MailAttachment.Type.ToUpper())
                            {
                                case "FILE":
                                case "URL":
                                    using (WebClient WebClient = new WebClient())
                                    {
                                        byte[] ByteArray = WebClient.DownloadData(MailAttachment.Value);

                                        string AttachmentName = MailAttachment.Value.Split("?").First().Split("/").Last();

                                        Attachment AttachmentData = new Attachment(new MemoryStream(ByteArray), AttachmentName);

                                        MailMessage.Attachments.Add(AttachmentData);
                                    };
                                    break;

                                case "DATA":
                                    if (!string.IsNullOrWhiteSpace(MailAttachment.Name))
                                    {
                                        byte[] ByteArray = Convert.FromBase64String(MailAttachment.Value);

                                        Attachment AttachmentData = new Attachment(new MemoryStream(ByteArray), MailAttachment.Name);

                                        MailMessage.Attachments.Add(AttachmentData);
                                    }
                                    break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(Input.MailBlindAddress))
                {
                    foreach (var Address in Input.MailBlindAddress.Split(","))
                    {
                        MailMessage.Bcc.Add(Address);
                    }

                    foreach (var Address in Input.MailBlindAddress.Split(";"))
                    {
                        MailMessage.Bcc.Add(Address);
                    }
                }

                if (!string.IsNullOrWhiteSpace(Input.MailCopyAddress))
                {
                    foreach (var Address in Input.MailCopyAddress.Split(","))
                    {
                        MailMessage.CC.Add(Address);
                    }

                    foreach (var Address in Input.MailCopyAddress.Split(";"))
                    {
                        MailMessage.CC.Add(Address);
                    }
                }

                if (!string.IsNullOrWhiteSpace(Input.MailAddress))
                {
                    foreach (var Address in Input.MailAddress.Split(","))
                    {
                        MailMessage.To.Add(Address);
                    }

                    foreach (var Address in Input.MailAddress.Split(";"))
                    {
                        MailMessage.To.Add(Address);
                    }
                }

                SmtpClient SmtpClient = new SmtpClient(DefaultCredentials.Host)
                {
                    UseDefaultCredentials = DefaultCredentials.DefaultCredentials,
                    Port = int.Parse(DefaultCredentials.Port),
                    EnableSsl = DefaultCredentials.EnableSsl,
                    Credentials = new NetworkCredential(DefaultCredentials.Username, DefaultCredentials.Password)
                };

                SmtpClient.Send(MailMessage);

                Output.Success = true;

                Input.MailAttachmentData = null;

                Logger.ForContext("Context", "POST: SendMail").Debug("Sent mail: {Input}", JsonConvert.SerializeObject(Input));
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "POST: SendMail").Error(Exception, "Exception found:");

                Output.OperationMessage = Exception.Message;

                return BadRequest(Output);
            }

            Logger.ForContext("Context", "POST: SendMail").Information("Output: {@Output}", Output);

            return Ok(Output);
        }
    }
}