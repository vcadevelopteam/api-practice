using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ZyxMeBridge.Services
{
    public class CommonService
    {
        private static HttpClient HttpClient = new HttpClient();

        public static async Task<HttpResponseMessage> HttpRequestJObject(string BaseAddress, string Endpoint, string Method, string Content, Dictionary<string, string> HeaderDictionary, string AuthorizationType = null, string Authorization = null)
        {
            try
            {
                HttpClient = new HttpClient
                {
                    BaseAddress = new Uri(BaseAddress)
                };

                HttpClient.DefaultRequestHeaders.Accept.Clear();

                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpRequestHeader(HeaderDictionary);

                HttpRequestAuthorization(AuthorizationType, Authorization);

                return Method.ToUpper() switch
                {
                    "GET" => await HttpClient.GetAsync(Endpoint),
                    "POST" => await HttpClient.PostAsync(Endpoint, new StringContent(Content, Encoding.UTF8, "application/json")),
                    "PUT" => await HttpClient.PutAsync(Endpoint, new StringContent(Content, Encoding.UTF8, "application/json")),
                    _ => await HttpClient.GetAsync(Endpoint),
                };
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }

        public static async Task<HttpResponseMessage> HttpRequestFormData(string BaseAddress, string Endpoint, string Method, StreamContent StreamContent, Dictionary<string, string> HeaderDictionary, string AuthorizationType = null, string Authorization = null)
        {
            try
            {
                HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Clear();

                HttpRequestHeader(HeaderDictionary);

                HttpClient.Timeout = TimeSpan.FromMinutes(20);

                HttpRequestAuthorization(AuthorizationType, Authorization);

                HttpRequestMessage RequestMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri(baseUri: new Uri(BaseAddress), relativeUri: Endpoint)
                };

                switch (Method.ToUpper())
                {
                    case "GET":
                        RequestMessage.Method = HttpMethod.Get;
                        break;

                    case "POST":
                        RequestMessage.Method = HttpMethod.Post;
                        break;

                    case "PUT":
                        RequestMessage.Method = HttpMethod.Put;
                        break;

                    case "DELETE":
                        RequestMessage.Method = HttpMethod.Delete;
                        break;

                    case "HEAD":
                        RequestMessage.Method = HttpMethod.Head;
                        break;
                }

                if (StreamContent != null)
                {
                    RequestMessage.Content = StreamContent;
                }

                return await HttpClient.SendAsync(RequestMessage);
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }

        public static async Task<HttpResponseMessage> HttpRequestFormUrlEncoded(string BaseAddress, string Endpoint, string Method, FormUrlEncodedContent FormUrlEncodedContent, Dictionary<string, string> HeaderDictionary, string AuthorizationType = null, string Authorization = null)
        {
            try
            {
                HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Clear();

                HttpRequestHeader(HeaderDictionary);

                HttpRequestAuthorization(AuthorizationType, Authorization);

                HttpRequestMessage RequestMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri(baseUri: new Uri(BaseAddress), relativeUri: Endpoint)
                };

                switch (Method.ToUpper())
                {
                    case "GET":
                        RequestMessage.Method = HttpMethod.Get;
                        break;

                    case "POST":
                        RequestMessage.Method = HttpMethod.Post;
                        break;

                    case "PUT":
                        RequestMessage.Method = HttpMethod.Put;
                        break;

                    case "DELETE":
                        RequestMessage.Method = HttpMethod.Delete;
                        break;

                    case "HEAD":
                        RequestMessage.Method = HttpMethod.Head;
                        break;
                }

                if (FormUrlEncodedContent != null)
                {
                    RequestMessage.Content = FormUrlEncodedContent;
                }

                return await HttpClient.SendAsync(RequestMessage);
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }

        public static void HttpRequestHeader(Dictionary<string, string> HeaderDictionary)
        {
            try
            {
                if (HeaderDictionary != null)
                {
                    foreach (var Header in HeaderDictionary)
                    {
                        HttpClient.DefaultRequestHeaders.Add(Header.Key, Header.Value);
                    }
                }
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }

        public static void HttpRequestAuthorization(string AuthorizationType, string Authorization)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(AuthorizationType))
                {
                    switch (AuthorizationType.ToUpper())
                    {
                        case "BASIC":
                            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Authorization);
                            break;

                        case "BEARER":
                            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Authorization);
                            break;
                    }
                }
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }

        public static JObject XmlToJObject(string XmlString)
        {
            XmlDocument XmlDocument = new XmlDocument();

            XmlDocument.LoadXml(XmlString);

            string JsonString = JsonConvert.SerializeXmlNode(XmlDocument);

            JObject JObject = JObject.Parse(JsonString);

            return JObject;
        }

        public static Dictionary<string, object> JObjectToDictionary(dynamic DynamicData)
        {
            try
            {
                return new Dictionary<string, object>(DynamicData.ToObject<IDictionary<string, object>>(), StringComparer.InvariantCultureIgnoreCase);
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }
    }
}