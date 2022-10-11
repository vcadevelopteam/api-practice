using Newtonsoft.Json.Linq;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ZyxMeBridge.Models.Common;

namespace ZyxMeBridge.Services
{
    public class StorageService
    {
        public static async Task<Dictionary<string, object>> StorageToken(Logger Logger, string ApiKey)
        {
            Dictionary<string, object> ResultDictionary;

            try
            {
                Dictionary<string, string> Parameters = new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ibm:params:oauth:grant-type:apikey",
                    ["response_type"] = "cloud_iam",
                    ["apikey"] = ApiKey
                };

                Logger.ForContext("Context", "Storage Token").Debug("Http POST body: {@Parameters}", Parameters);

                HttpResponseMessage ResponseMessage = await CommonService.HttpRequestFormUrlEncoded("https://iam.cloud.ibm.com/oidc/token", string.Empty, "POST", new FormUrlEncodedContent(Parameters), null);

                Logger.ForContext("Context", "Storage Token").Debug("Http POST endpoint: {ZyxMeEndpoint}", "https://iam.cloud.ibm.com/oidc/token");

                string ZyxMeResponseContent = await ResponseMessage.Content.ReadAsStringAsync();

                Logger.ForContext("Context", "Storage Token").Debug("Http POST response: {ZyxMeResponseContent}", ZyxMeResponseContent);

                JObject JObject = JObject.Parse(ZyxMeResponseContent);

                ResultDictionary = CommonService.JObjectToDictionary(JObject);

                ResultDictionary["statuscode"] = $"{(int)ResponseMessage.StatusCode}{ResponseMessage.StatusCode}";
            }
            catch (Exception Exception)
            {
                throw Exception;
            }

            return ResultDictionary;
        }

        public static async Task<Dictionary<string, object>> StorageOptions(Logger Logger, Dictionary<string, object> RequestBody)
        {
            Dictionary<string, object> ResultDictionary;

            try
            {
                Dictionary<string, object> Token = await StorageToken(Logger, RequestBody["apikey"].ToString());

                if (Token["statuscode"].ToString().StartsWith("4"))
                {
                    throw new Exception(Token["errorMessage"].ToString());
                }

                string Endpoint = RequestBody["url"].ToString();

                Dictionary<string, string> HeaderDictionary = new Dictionary<string, string>
                {
                    { "ibm-service-instance-id", RequestBody["resourceid"].ToString() }
                };

                HttpResponseMessage ResponseMessage = new HttpResponseMessage();

                switch (RequestBody["operation"].ToString().ToUpper())
                {
                    case "GETBUCKETS":
                        ResponseMessage = await CommonService.HttpRequestJObject(Endpoint, string.Empty, "GET", string.Empty, HeaderDictionary, "Bearer", Token["access_token"].ToString());
                        break;

                    case "GETBUCKET":
                        ResponseMessage = await CommonService.HttpRequestJObject(Endpoint, (string)RequestBody["bucket"], "GET", string.Empty, HeaderDictionary, "Bearer", Token["access_token"].ToString());
                        break;
                }

                ResultDictionary = CommonService.JObjectToDictionary(JObject.Parse(await ResponseMessage.Content.ReadAsStringAsync()));

                ResultDictionary["statuscode"] = $"{ResponseMessage.StatusCode}{(int)ResponseMessage.StatusCode}";
            }
            catch (Exception Exception)
            {
                throw Exception;
            }

            return ResultDictionary;
        }

        public static async Task<Dictionary<string, object>> StorageUpload(AppSettings AppSettings, Logger Logger, byte[] DecodedAttachmentArray, string FileName)
        {
            Dictionary<string, object> ResultDictionary = new Dictionary<string, object>();

            try
            {
                Dictionary<string, object> Token = await StorageToken(Logger, AppSettings.StorageSettings.ApiKey);

                if (Token["statuscode"].ToString().StartsWith("4"))
                {
                    throw new Exception(Token["errorMessage"].ToString());
                }

                Dictionary<string, string> HeaderDictionary = new Dictionary<string, string>
                {
                    { "ibm-service-instance-id", AppSettings.StorageSettings.ResourceId }
                };

                HttpResponseMessage ResponseMessage = new HttpResponseMessage();

                StreamContent StreamContent = new StreamContent(new MemoryStream(DecodedAttachmentArray));

                Logger.ForContext("Context", "Storage Upload").Debug("Http PUT body: {FileName}", FileName);

                ResponseMessage = await CommonService.HttpRequestFormData($"https://{AppSettings.StorageSettings.Endpoint}", $"{AppSettings.StorageSettings.Bucket}/{FileName}", "PUT", StreamContent, HeaderDictionary, "Bearer", Token["access_token"].ToString());

                Logger.ForContext("Context", "Storage Upload").Debug("Http PUT endpoint: {ZyxMeEndpoint}", $"https://{AppSettings.StorageSettings.Endpoint}");

                ResultDictionary["statuscode"] = $"{(int)ResponseMessage.StatusCode}{ResponseMessage.StatusCode}";

                Logger.ForContext("Context", "Storage Upload").Debug("Http PUT response: {StatusCode}", ResultDictionary["statuscode"]);

                ResultDictionary["result"] = await ResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception Exception)
            {
                throw Exception;
            }

            return ResultDictionary;
        }

        public static async Task<byte[]> StorageDownload(Logger Logger, Dictionary<string, object> RequestBody)
        {
            try
            {
                Dictionary<string, object> Token = await StorageToken(Logger, RequestBody["apikey"].ToString());

                string Endpoint = RequestBody["url"].ToString();

                Dictionary<string, string> HeaderDictionary = new Dictionary<string, string>
                {
                    { "ibm-service-instance-id", RequestBody["resourceid"].ToString() }
                };

                HttpResponseMessage ResponseMessage = new HttpResponseMessage();

                ResponseMessage = await CommonService.HttpRequestJObject(Endpoint, RequestBody["bucket"] + "/" + RequestBody["objectkey"], "GET", string.Empty, HeaderDictionary, "Bearer", Token["access_token"].ToString());

                return await ResponseMessage.Content.ReadAsByteArrayAsync();
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }
    }
}