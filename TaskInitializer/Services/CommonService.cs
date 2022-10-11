using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TaskInitializer.Services
{
    public class CommonService
    {
        public static async Task<HttpResponseMessage> HttpRequestFormData(string BaseAddress, string Endpoint, string Method, StreamContent StreamContent, Dictionary<string, string> HeaderDictionary, string AuthorizationType = null, string Authorization = null)
        {
            try
            {
                HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Clear();

                HttpRequestAuthorization(HttpClient, AuthorizationType, Authorization);

                HttpClient.Timeout = TimeSpan.FromMinutes(20);

                HttpRequestHeader(HttpClient, HeaderDictionary);

                HttpRequestMessage HttpRequestMessage = new HttpRequestMessage()
                {
                    RequestUri = new Uri(baseUri: new Uri(BaseAddress), relativeUri: Endpoint)
                };

                switch (Method.ToUpper())
                {
                    case "DELETE":
                        HttpRequestMessage.Method = HttpMethod.Delete;
                        break;

                    case "GET":
                        HttpRequestMessage.Method = HttpMethod.Get;
                        break;

                    case "HEAD":
                        HttpRequestMessage.Method = HttpMethod.Head;
                        break;

                    case "POST":
                        HttpRequestMessage.Method = HttpMethod.Post;
                        break;

                    case "PUT":
                        HttpRequestMessage.Method = HttpMethod.Put;
                        break;
                }

                if (StreamContent != null)
                {
                    HttpRequestMessage.Content = StreamContent;
                }

                return await HttpClient.SendAsync(HttpRequestMessage);
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
                HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Clear();

                HttpRequestAuthorization(HttpClient, AuthorizationType, Authorization);

                HttpRequestHeader(HttpClient, HeaderDictionary);

                HttpRequestMessage HttpRequestMessage = new HttpRequestMessage()
                {
                    RequestUri = new Uri(baseUri: new Uri(BaseAddress), relativeUri: Endpoint)
                };

                switch (Method.ToUpper())
                {
                    case "DELETE":
                        HttpRequestMessage.Method = HttpMethod.Delete;
                        break;

                    case "GET":
                        HttpRequestMessage.Method = HttpMethod.Get;
                        break;

                    case "HEAD":
                        HttpRequestMessage.Method = HttpMethod.Head;
                        break;

                    case "POST":
                        HttpRequestMessage.Method = HttpMethod.Post;
                        break;

                    case "PUT":
                        HttpRequestMessage.Method = HttpMethod.Put;
                        break;
                }

                if (FormUrlEncodedContent != null)
                {
                    HttpRequestMessage.Content = FormUrlEncodedContent;
                }

                return await HttpClient.SendAsync(HttpRequestMessage);
            }
            catch (Exception Exception)
            {
                throw Exception;
            }
        }

        public static void HttpRequestHeader(HttpClient HttpClient, Dictionary<string, string> HeaderDictionary)
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

        public static void HttpRequestAuthorization(HttpClient HttpClient, string AuthorizationType, string Authorization)
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