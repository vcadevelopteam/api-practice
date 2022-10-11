using Newtonsoft.Json;
using System.Collections.Generic;
using TaskInitializer.Models.Cores;

namespace TaskInitializer.Models.Common
{
    public class AppSettings
    {
        [JsonProperty("poderJudicialSettings")]
        public PoderJudicialSettings PoderJudicialSettings { get; set; }

        [JsonProperty("connectionStrings")]
        public ConnectionStrings ConnectionStrings { get; set; }

        [JsonProperty("exceptionSettings")]
        public ExceptionSettings ExceptionSettings { get; set; }

        [JsonProperty("databaseSettings")]
        public DatabaseSettings DatabaseSettings { get; set; }

        [JsonProperty("manualSettings")]
        public ManualSettings ManualSettings { get; set; }

        [JsonProperty("generalSettings")]
        public GeneralSettings GeneralSettings { get; set; }

        [JsonProperty("outlookSettings")]
        public OutlookSettings OutlookSettings { get; set; }

        [JsonProperty("storageSettings")]
        public StorageSettings StorageSettings { get; set; }

        [JsonProperty("claroSettings")]
        public ClaroSettings ClaroSettings { get; set; }

        [JsonProperty("zyxMeSettings")]
        public ZyxMeSettings ZyxMeSettings { get; set; }

        [JsonProperty("mailSettings")]
        public MailSettings MailSettings { get; set; }

        [JsonProperty("logSettings")]
        public LogSettings LogSettings { get; set; }

        [JsonProperty("spanishDictionary")]
        public List<SpanishDictionary> SpanishDictionary { get; set; }
    }

    public class SpanishDictionary
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }

    public class PoderJudicialSettings
    {
        [JsonProperty("userToken")]
        public string UserToken { get; set; }

        [JsonProperty("appToken")]
        public string AppToken { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }

    public class ConnectionStrings
    {
        [JsonProperty("connectionCredentials")]
        public string ConnectionCredentials { get; set; }

        [JsonProperty("odooCredentials")]
        public string OdooCredentials { get; set; }

        [JsonProperty("eamCredentials")]
        public string EamCredentials { get; set; }
    }

    public class ExceptionSettings
    {
        [JsonProperty("enableException")]
        public bool EnableException { get; set; }

        [JsonProperty("addressList")]
        public string AddressList { get; set; }
    }

    public class DatabaseSettings
    {
        [JsonProperty("retryDelay")]
        public decimal RetryDelay { get; set; }

        [JsonProperty("retryCount")]
        public long RetryCount { get; set; }
    }

    public class ManualSettings
    {
        [JsonProperty("taskList")]
        public List<ManualSettingsTask> TaskList { get; set; }
    }

    public class ManualSettingsTask
    {
        [JsonProperty("taskType")]
        public string TaskType { get; set; }

        [JsonProperty("taskBody")]
        public TaskBody TaskBody { get; set; }
    }

    public class GeneralSettings
    {
        [JsonProperty("useLocalTime")]
        public bool UseLocalTime { get; set; }

        [JsonProperty("basicMode")]
        public bool BasicMode { get; set; }
    }

    public class OutlookSettings
    {
        [JsonProperty("subscriptionEndpoint")]
        public string SubscriptionEndpoint { get; set; }

        [JsonProperty("tokenEndpoint")]
        public string TokenEndpoint { get; set; }
    }

    public class StorageSettings
    {
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("bucket")]
        public string Bucket { get; set; }
    }

    public class ClaroSettings
    {
        [JsonProperty("basicPassword")]
        public string BasicPassword { get; set; }

        [JsonProperty("basicUsername")]
        public string BasicUsername { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

    public class ZyxMeSettings
    {
        [JsonProperty("apiServicesEndpoint")]
        public string ApiServicesEndpoint { get; set; }

        [JsonProperty("chatflowApiEndpoint")]
        public string ChatflowApiEndpoint { get; set; }

        [JsonProperty("servicesEndpoint")]
        public string ServicesEndpoint { get; set; }

        [JsonProperty("laraigoEndpoint")]
        public string LaraigoEndpoint { get; set; }

        [JsonProperty("bridgeEndpoint")]
        public string BridgeEndpoint { get; set; }

        [JsonProperty("hookEndpoint")]
        public string HookEndpoint { get; set; }

        [JsonProperty("appEndpoint")]
        public string AppEndpoint { get; set; }
    }

    public class MailSettings
    {
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }

    public class LogSettings
    {
        [JsonProperty("rollOnFileSizeLimit")]
        public bool RollOnFileSizeLimit { get; set; }

        [JsonProperty("buffered")]
        public bool Buffered { get; set; }

        [JsonProperty("shared")]
        public bool Shared { get; set; }

        [JsonProperty("rollingInterval")]
        public long RollingInterval { get; set; }

        [JsonProperty("minimumLevel")]
        public long MinimumLevel { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }
    }
}