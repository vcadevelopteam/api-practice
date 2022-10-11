using Newtonsoft.Json.Linq;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TaskInitializer.Models.Common;

namespace TaskInitializer.Services
{
    public class StorageService
    {
        public static async Task<Dictionary<string, object>> StorageToken(Logger Logger, string ApiKey)
        {
            Dictionary<string, object> ResultDictionary;

            try
            {
                string HttpEndpoint = "https://iam.cloud.ibm.com/oidc/token";

                Dictionary<string, string> ParameterDictionary = new Dictionary<string, string>() { ["grant_type"] = "urn:ibm:params:oauth:grant-type:apikey", ["response_type"] = "cloud_iam", ["apikey"] = ApiKey };

                Logger.ForContext("Context", "StorageToken").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", "StorageToken").Debug("Http POST body: {@ParameterDictionary}", ParameterDictionary);

                HttpResponseMessage HttpResponseMessage = await CommonService.HttpRequestFormUrlEncoded(HttpEndpoint, string.Empty, "POST", new FormUrlEncodedContent(ParameterDictionary), null);

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                Logger.ForContext("Context", "StorageToken").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                JObject JObject = JObject.Parse(HttpResponseContent);

                ResultDictionary = CommonService.JObjectToDictionary(JObject);

                ResultDictionary["statuscode"] = $"{(int)HttpResponseMessage.StatusCode} - {HttpResponseMessage.StatusCode}";
            }
            catch (Exception Exception)
            {
                throw Exception;
            }

            return ResultDictionary;
        }

        public static async Task<Dictionary<string, object>> StorageUpload(AppSettings AppSettings, Logger Logger, byte[] ByteArray, string FileName)
        {
            Dictionary<string, object> ResultDictionary = new Dictionary<string, object>();

            try
            {
                Dictionary<string, object> TokenDictionary = await StorageToken(Logger, AppSettings.StorageSettings.ApiKey);

                Dictionary<string, string> HeaderDictionary = new Dictionary<string, string>() { { "ibm-service-instance-id", AppSettings.StorageSettings.ResourceId } };

                if (TokenDictionary["statuscode"].ToString().StartsWith("4"))
                {
                    throw new Exception(TokenDictionary["errorMessage"].ToString());
                }

                string HttpEndpoint = $"https://{AppSettings.StorageSettings.Endpoint}{AppSettings.StorageSettings.Bucket}/{FileName}";

                StreamContent StreamContent = new StreamContent(new MemoryStream(ByteArray));

                Logger.ForContext("Context", "StorageUpload").Debug("Http PUT endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", "StorageUpload").Debug("Http PUT body: {FileName}", FileName);

                HttpResponseMessage HttpResponseMessage = await CommonService.HttpRequestFormData(string.Empty, HttpEndpoint, "PUT", StreamContent, HeaderDictionary, "Bearer", TokenDictionary["access_token"].ToString());

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                Logger.ForContext("Context", "StorageUpload").Warning("Http PUT response: {HttpResponseContent}", HttpResponseContent);

                ResultDictionary["statuscode"] = $"{(int)HttpResponseMessage.StatusCode} - {HttpResponseMessage.StatusCode}";

                ResultDictionary["result"] = HttpResponseContent;
            }
            catch (Exception Exception)
            {
                throw Exception;
            }

            return ResultDictionary;
        }
    }
}