using Serilog.Core;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ZyxMeBridge.Models.Common;
using ZyxMeBridge.Processors;

namespace ZyxMeBridge.Services
{
    public class EchoService
    {
        public static async Task<string> SendEcho(AppSettings AppSettings, string Endpoint, string Authorization, string Body)
        {
            Logger Logger = LoggerProcessor.ConfigureLogger(AppSettings, "Echo");

            try
            {
                using HttpClient EchoClient = new HttpClient();

                if (!string.IsNullOrWhiteSpace(Authorization))
                {
                    EchoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Authorization);
                }

                EchoClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Logger.ForContext("Context", "Send Echo").Debug("Http POST body: {Body}", Body);

                HttpResponseMessage EchoResponseMessage = await EchoClient.PostAsync(Endpoint, new StringContent(Body, Encoding.UTF8, "application/json"));

                Logger.ForContext("Context", "Send Echo").Debug("Http POST endpoint: {Endpoint}", Endpoint);

                string EchoResponseContent = await EchoResponseMessage.Content.ReadAsStringAsync();

                Logger.ForContext("Context", "Send Echo").Debug("Http POST response: {EchoResponseContent}", EchoResponseContent);

                return EchoResponseContent;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "Send Echo").Error(Exception, "Exception found:");
            }

            return null;
        }
    }
}