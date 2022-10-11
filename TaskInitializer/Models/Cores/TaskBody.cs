using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskInitializer.Models.Cores
{
    public class TaskBody
    {
        [JsonProperty("attachments")]
        public List<MailAttachment> Attachments { get; set; }

        [JsonProperty("reportList")]
        public List<TaskBodyReport> ReportList { get; set; }

        [JsonProperty("check")]
        public List<TaskBodyCheck> Check { get; set; }

        [JsonProperty("relocate")]
        public TaskBodyRelocate Relocate { get; set; }

        [JsonProperty("outlook")]
        public TaskBodyOutlook Outlook { get; set; }

        [JsonProperty("logFilter")]
        public List<string> LogFilter { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("mailSubject")]
        public string MailSubject { get; set; }

        [JsonProperty("mailBody")]
        public string MailBody { get; set; }

        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("database")]
        public string Database { get; set; }

        [JsonProperty("reportParameters")]
        public string ReportParameters { get; set; }

        [JsonProperty("blindReceiver")]
        public string BlindReceiver { get; set; }

        [JsonProperty("executionType")]
        public string ExecutionType { get; set; }

        [JsonProperty("messageToSend")]
        public string MessageToSend { get; set; }

        [JsonProperty("copyReceiver")]
        public string CopyReceiver { get; set; }

        [JsonProperty("reportFilter")]
        public string ReportFilter { get; set; }

        [JsonProperty("closeMotive")]
        public string CloseMotive { get; set; }

        [JsonProperty("credentials")]
        public string Credentials { get; set; }

        [JsonProperty("messageType")]
        public string MessageType { get; set; }

        [JsonProperty("observation")]
        public string Observation { get; set; }

        [JsonProperty("expireBody")]
        public string ExpireBody { get; set; }

        [JsonProperty("alertBody")]
        public string AlertBody { get; set; }

        [JsonProperty("receiver")]
        public string Receiver { get; set; }

        [JsonProperty("closeBy")]
        public string CloseBy { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("regex")]
        public string Regex { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("firm")]
        public string Firm { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("End")]
        public string End { get; set; }

        [JsonProperty("Path")]
        public string Path { get; set; }

        [JsonProperty("dniList")]
        public List<string> DniList { get; set; }

        [JsonProperty("gmail")]
        public TaskBodyGmail Gmail { get; set; }

        [JsonProperty("config")]
        public MailConfig Config { get; set; }

        [JsonProperty("holdingLimit")]
        public long HoldingLimit { get; set; }

        [JsonProperty("batchIndex")]
        public long BatchIndex { get; set; }

        [JsonProperty("campaignId")]
        public long CampaignId { get; set; }

        [JsonProperty("templateId")]
        public long TemplateId { get; set; }

        [JsonProperty("attemptLimit")]
        public long AttemptLimit { get; set; }

        [JsonProperty("hourInterval")]
        public long HourInterval { get; set; }

        [JsonProperty("ftp")]
        public TaskBodyFtp Ftp { get; set; }

        [JsonProperty("bridge")]
        public TaskBodyBridge Bridge { get; set; }

        [JsonProperty("zipFile")]
        public bool ZipFile { get; set; }

        [JsonProperty("removeAccent")]
        public bool RemoveAccent { get; set; }

        [JsonProperty("deleteAll")]
        public bool DeleteAll { get; set; }

        [JsonProperty("updateHsm")]
        public bool UpdateHsm { get; set; }

        [JsonProperty("task")]
        public bool Task { get; set; }

        [JsonProperty("taskOffset")]
        public decimal TaskOffset { get; set; }

        [JsonProperty("offset")]
        public decimal Offset { get; set; }

        [JsonProperty("hour")]
        public decimal Hour { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("environment")]
        public string Environment { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class TaskBodyCredentials
    {
        [JsonProperty("default_Credentials")]
        public bool DefaultCredentials { get; set; }

        [JsonProperty("enableSsl")]
        public bool EnableSsl { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }
    }

    public class TaskBodyReport
    {
        [JsonProperty("variableList")]
        public List<string> VariableList { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class TaskBodyCheck
    {
        [JsonProperty("header")]
        public Dictionary<string, string> Header { get; set; }

        [JsonProperty("expectedResult")]
        public string ExpectedResult { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }

    public class TaskBodyRelocate
    {
        [JsonProperty("directorySourceList")]
        public List<string> DirectorySourceList { get; set; }

        [JsonProperty("directoryDestination")]
        public string DirectoryDestination { get; set; }

        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }

        [JsonProperty("recursiveSearch")]
        public bool RecursiveSearch { get; set; }

        [JsonProperty("compress")]
        public bool Compress { get; set; }

        [JsonProperty("upload")]
        public bool Upload { get; set; }

        [JsonProperty("copy")]
        public bool Copy { get; set; }

        [JsonProperty("range")]
        public long Range { get; set; }
    }

    public class TaskBodyOutlook
    {
        [JsonProperty("changeType")]
        public string ChangeType { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("expiration")]
        public long Expiration { get; set; }
    }

    public class TaskBodyGmail
    {
        [JsonProperty("applicationName")]
        public string ApplicationName { get; set; }

        [JsonProperty("topicName")]
        public string TopicName { get; set; }

        [JsonProperty("delay")]
        public long Delay { get; set; }
    }

    public class TaskBodyFtp
    {
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public long Port { get; set; }
    }

    public class TaskBodyBridge
    {
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}