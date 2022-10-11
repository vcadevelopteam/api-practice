using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog.Core;
using System;
using System.Net;
using System.Text;
using ZyxMeBridge.Models.Common;
using ZyxMeBridge.Processors;

namespace ZyxMeBridge.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class WebController : Controller
    {
        private readonly AppSettings AppSettings;

        private readonly Logger Logger;

        public WebController(IOptions<AppSettings> AppSettings)
        {
            Logger = LoggerProcessor.ConfigureLogger(AppSettings.Value, "Web");

            this.AppSettings = AppSettings.Value;
        }

        [HttpPost]
        [Route("BuildClaroChat")]
        public ContentResult BuildClaroChat([FromForm] string name, string email, string phone, string rut, string idtrans)
        {
            Logger.ForContext("Context", "POST: BuildClaroChat").Information("Input: {FormData}", $"Name: {name} - Mail: {email} - Phone: {phone} - Rut: {rut} - Transacción: {idtrans}");

            try
            {
                if (AppSettings.ChatSettings != null)
                {
                    if (!string.IsNullOrWhiteSpace(AppSettings.ChatSettings.ClaroEndpoint))
                    {
                        WebClient WebClient = new WebClient();

                        string SiteString = WebClient.DownloadString(AppSettings.ChatSettings.ClaroEndpoint);

                        if (!string.IsNullOrWhiteSpace(idtrans))
                        {
                            idtrans = Encoding.UTF8.GetString(Convert.FromBase64String(idtrans));
                        }

                        if (!string.IsNullOrWhiteSpace(phone))
                        {
                            phone = Encoding.UTF8.GetString(Convert.FromBase64String(phone));
                        }

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            email = Encoding.UTF8.GetString(Convert.FromBase64String(email));
                        }

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            name = Encoding.UTF8.GetString(Convert.FromBase64String(name));
                        }

                        if (!string.IsNullOrWhiteSpace(rut))
                        {
                            rut = Encoding.UTF8.GetString(Convert.FromBase64String(rut));
                        }

                        SiteString = SiteString.Replace("##datatransaction##", idtrans);
                        SiteString = SiteString.Replace("##dataphone##", phone);
                        SiteString = SiteString.Replace("##datamail##", email);
                        SiteString = SiteString.Replace("##dataname##", name);
                        SiteString = SiteString.Replace("##datarut##", rut);

                        return new ContentResult()
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            ContentType = "text/html",
                            Content = SiteString
                        };
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "POST: BuildClaroChat").Error(Exception, "Exception found:");

                return new ContentResult()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ContentType = "text/html",
                    Content = string.Empty
                };
            }

            return new ContentResult()
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                ContentType = "text/html",
                Content = string.Empty
            };
        }
    }
}