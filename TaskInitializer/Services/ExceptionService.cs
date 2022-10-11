using Newtonsoft.Json;
using Serilog.Core;
using System;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Cores;

namespace TaskInitializer.Services
{
    public class ExceptionService
    {
        public static void HandleException(AppSettings AppSettings, Logger Logger, Exception ExceptionMessage)
        {
            try
            {
                if (AppSettings.ExceptionSettings != null)
                {
                    if (AppSettings.ExceptionSettings.EnableException)
                    {
                        Logger.ForContext("Context", "HandleException").Warning("Handling exception: {Message}", ExceptionMessage.Message);

                        SendMailBody SendMailBody = new SendMailBody()
                        {
                            MailBody = $"Excepción encontrada al ejecutar ZyxMe Scheduler:<br/><br/><b>Message:</b> {ExceptionMessage.Message}<br/><b>StackTrace:</b> {ExceptionMessage.StackTrace}<br/><b>Inner Exception:</b> {JsonConvert.SerializeObject(ExceptionMessage.InnerException)}<br/><b>Source:</b> {ExceptionMessage.Source}<br/><br/>En el ambiente: <br/><br/><b>API SERVICES:</b> {AppSettings.ZyxMeSettings.ApiServicesEndpoint}<br/><b>APP:</b> {AppSettings.ZyxMeSettings.AppEndpoint}<br/><b>BRIDGE:</b> {AppSettings.ZyxMeSettings.BridgeEndpoint}<br/><b>HOOK:</b> {AppSettings.ZyxMeSettings.HookEndpoint}<br/><b>SERVICES:</b> {AppSettings.ZyxMeSettings.ServicesEndpoint}",
                            MailTitle = $"Scheduler Error Check | Date: [{DateTime.UtcNow.AddHours(-5)}]",
                            MailAddress = AppSettings.ExceptionSettings.AddressList
                        };

                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "HandleException").Error(Exception, "Exception found:");
            }
        }
    }
}