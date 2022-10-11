using System.Collections.Generic;

namespace ZyxMeBridge.Models.Common
{
    public class AppSettings
    {
        public BancoAztecaApiOfertasSettings BancoAztecaApiOfertasSettings { get; set; }

        public AuthenticationSettings AuthenticationSettings { get; set; }

        public BancoAztecaNewSettings BancoAztecaNewSettings { get; set; }

        public PoderJudicialSettings PoderJudicialSettings { get; set; }

        public PagoEfectivoSettings PagoEfectivoSettings { get; set; }

        public BancoAztecaSettings BancoAztecaSettings { get; set; }

        public TecnocableSettings TecnocableSettings { get; set; }

        public VoximplantSettings VoximplantSettings { get; set; }

        public ConnectionStrings ConnectionStrings { get; set; }

        public LuzDelSurSettings LuzDelSurSettings { get; set; }

        public LuzDelSurSettings LuzDelSurTestingSettings { get; set; }

        public DatabaseSettings DatabaseSettings { get; set; }

        public DocumentSettings DocumentSettings { get; set; }

        public LaraigoSettings LaraigoSettings { get; set; }

        public LithiumSettings LithiumSettings { get; set; }

        public StorageSettings StorageSettings { get; set; }

        public GrupoFeSettings GrupoFeSettings { get; set; }

        public CouponSettings CouponSettings { get; set; }

        public GoogleSettings GoogleSettings { get; set; }

        public MifactSettings MifactSettings { get; set; }

        public NiubizSettings NiubizSettings { get; set; }

        public SmoochSettings SmoochSettings { get; set; }

        public SolgasSettings SolgasSettings { get; set; }

        public WatsonSettings WatsonSettings { get; set; }

        public ClaroSettings ClaroSettings { get; set; }

        public EntelSettings EntelSettings { get; set; }

        public ZyxMeSettings ZyxMeSettings { get; set; }

        public HubSpotSettings HubSpotSettings { get; set; }

        public ZendeskSettings ZendeskSettings { get; set; }

        public EvoltaSettings EvoltaSettings { get; set; }

        public ChatSettings ChatSettings { get; set; }

        public EchoSettings EchoSettings { get; set; }

        public MailSettings MailSettings { get; set; }

        public LogSettings LogSettings { get; set; }
    }

    public class BancoAztecaApiOfertasSettings
    {
        public string Endpoint { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class AuthenticationSettings
    {
        public AuthenticationSettingsConfiguration Configuration { get; set; }

        public AuthenticationSettingsWhitelist Whitelist { get; set; }

        public List<AuthenticationSettingsUser> UserList { get; set; }
    }

    public class AuthenticationSettingsConfiguration
    {
        public bool EnableAuthentication { get; set; }

        public double ExpirationTime { get; set; }

        public string SymmetricKey { get; set; }
    }

    public class AuthenticationSettingsWhitelist
    {
        public bool Enabled { get; set; }
    }

    public class AuthenticationSettingsUser
    {
        public string Password { get; set; }
        public string Username { get; set; }
        public string Company { get; set; }
    }

    public class BancoAztecaNewSettings
    {
        public string ApplicationId { get; set; }
        public string EndpointAuth { get; set; }
        public string Endpoint { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string ApiKey { get; set; }
    }

    public class PoderJudicialSettings
    {
        public string AppToken { get; set; }
        public string Endpoint { get; set; }
    }

    public class PagoEfectivoSettings
    {
        public string Endpoint { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class BancoAztecaSettings
    {
        public string Endpoint { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class TecnocableSettings
    {
        public string Endpoint { get; set; }
    }

    public class VoximplantSettings
    {
        public string AccessToken { get; set; }
        public string Domain { get; set; }
        public string Endpoint { get; set; }
    }

    public class ConnectionStrings
    {
        public string ConnectionCredentials { get; set; }
    }

    public class LuzDelSurSettings
    {
        public string Endpoint { get; set; }
        public string Host { get; set; }
    }

    public class DatabaseSettings
    {
        public decimal RetryDelay { get; set; }

        public long RetryCount { get; set; }
    }

    public class DocumentSettings
    {
        public string Template { get; set; }
    }

    public class LaraigoSettings
    {
        public LaraigoSettingsGeneralService GeneralService { get; set; }

        public LaraigoSettingsFacebookService FacebookService { get; set; }

        public LaraigoSettingsSmoochService SmoochService { get; set; }

        public LaraigoSettingsTelegramService TelegramService { get; set; }

        public LaraigoSettingsTwitterService TwitterService { get; set; }

        public LaraigoSettingsWhatsAppService WhatsAppService { get; set; }

        public LaraigoSettingsGupshupService GupshupService { get; set; }
    }

    public class LaraigoSettingsGeneralService
    {
        public string LaraigoEndpoint { get; set; }
        public string WebhookEndpoint { get; set; }
    }

    public class LaraigoSettingsFacebookService
    {
        public List<LaraigoSettingsFacebookServiceAppData> AppDataList { get; set; }

        public string Endpoint { get; set; }
    }

    public class LaraigoSettingsFacebookServiceAppData
    {
        public string AppSecret { get; set; }
        public string AppId { get; set; }
    }

    public class LaraigoSettingsSmoochService
    {
        public string ApiKeyService { get; set; }
        public string ApiKeyId { get; set; }
        public string Endpoint { get; set; }
    }

    public class LaraigoSettingsTelegramService
    {
        public string Endpoint { get; set; }
    }

    public class LaraigoSettingsTwitterService
    {
        public string Endpoint { get; set; }
    }

    public class LaraigoSettingsWhatsAppService
    {
        public string Endpoint { get; set; }
    }

    public class LaraigoSettingsGupshupService
    {
        public string Endpoint { get; set; }
    }

    public class LithiumSettings
    {
        public string CompanyKey { get; set; }
        public string NetworkKey { get; set; }
        public string Endpoint { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class StorageSettings
    {
        public string ResourceId { get; set; }
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string Bucket { get; set; }
    }

    public class GoogleSettings
    {
        public string DefaultMachineLearningProjectId { get; set; }
        public string DefaultMachineLearningLocation { get; set; }
        public string DefaultMachineLearningModelId { get; set; }
    }

    public class GrupoFeSettings
    {
        public string Authorization { get; set; }
        public string Endpoint { get; set; }
    }

    public class CouponSettings
    {
        public long CorpId { get; set; }
        public long OrgId { get; set; }

        public string Promotion { get; set; }
        public string Phone { get; set; }
    }

    public class MifactSettings
    {
        public string Endpoint { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }

    public class NiubizSettings
    {
        public string EndpointAuthentication { get; set; }
        public string EndpointOrder { get; set; }
        public string MerchantId { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class SmoochSettings
    {
        public string Endpoint { get; set; }
    }

    public class SolgasSettings
    {
        public string Endpoint { get; set; }
    }

    public class WatsonSettings
    {
        public string DefaultAssistantWorkspaceId { get; set; }
        public string DefaultClassifierEndpoint { get; set; }
        public string DefaultClassifierModelId { get; set; }
        public string DefaultClassifierApiKey { get; set; }
        public string DefaultAssistantApiKey { get; set; }
        public string DefaultWatsonEndpoint { get; set; }
        public string DefaultWatsonApiKey { get; set; }
    }

    public class ClaroSettings
    {
        public string BasicPassword { get; set; }
        public string BasicUsername { get; set; }
        public string Endpoint { get; set; }
        public string EndpointOverride { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class EntelSettings
    {
        public string ClientSecret { get; set; }
        public string EndpointNew { get; set; }
        public string EndpointOld { get; set; }
        public string AutoLogin { get; set; }
        public string ClientId { get; set; }
    }

    public class ZyxMeSettings
    {
        public string ApiServicesEndpoint { get; set; }
        public string ChatflowApiEndpoint { get; set; }
        public string ServicesEndpoint { get; set; }
        public string HookEndpoint { get; set; }
        public string AppEndpoint { get; set; }
        public string LaraigoEndpoint { get; set; }
    }

    public class HubSpotSettings
    {
        public string Endpoint { get; set; }
    }

    public class EvoltaSettings
    {
        public string Endpoint { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    public class ZendeskSettings
    {
        public string AccessToken { get; set; }
        public string Subdomain { get; set; }
    }

    public class ChatSettings
    {
        public string ClaroEndpoint { get; set; }
    }

    public class EchoSettings
    {
        public bool EchoActivated { get; set; }

        public string Endpoint { get; set; }
    }

    public class MailSettings
    {
        public string Password { get; set; }
        public string Address { get; set; }
    }

    public class LogSettings
    {
        public bool RollOnFileSizeLimit { get; set; }
        public bool Buffered { get; set; }
        public bool Shared { get; set; }

        public long RollingInterval { get; set; }
        public long MinimumLevel { get; set; }

        public string Extension { get; set; }
        public string Location { get; set; }
        public string Template { get; set; }
        public string Prefix { get; set; }
    }
}