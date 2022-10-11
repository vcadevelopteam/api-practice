using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Database;

namespace TaskInitializer.Data
{
    public class DatabaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder DbContextOptionsBuilder)
        {
            IConfigurationBuilder ConfigurationBuilder = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).AddJsonFile("appsettings.json");

            AppSettings AppSettings = ConfigurationBuilder.Build().Get<AppSettings>();

            decimal RetryDelay = 0;

            long RetryCount = 0;

            if (AppSettings.DatabaseSettings != null)
            {
                RetryCount = AppSettings.DatabaseSettings.RetryCount;

                RetryDelay = AppSettings.DatabaseSettings.RetryDelay;
            }

            DbContextOptionsBuilder.UseNpgsql(AppSettings.ConnectionStrings.ConnectionCredentials, Options => Options.EnableRetryOnFailure(errorCodesToAdd: null, maxRetryCount: (int)RetryCount, maxRetryDelay: TimeSpan.FromSeconds((double)RetryDelay)));
        }

        protected override void OnModelCreating(ModelBuilder ModelBuilder)
        {
            ModelBuilder.Entity<CouponLaraigoShop>().HasKey(Key => new { Key.CorpId, Key.OrgId, Key.Name });

            ModelBuilder.Entity<CouponLaraigoPromotion>().HasKey(Key => new { Key.CorpId, Key.CouponShop, Key.Name });

            ModelBuilder.Entity<WitaiCron>().HasNoKey();

            ModelBuilder.Entity<WitaiConfig>().HasNoKey();

            ModelBuilder.Entity<WitaiSchedule>().HasNoKey();

            ModelBuilder.Entity<AttachmentData>().HasNoKey();

            ModelBuilder.Entity<VoximplantOrganization>().HasNoKey();

            ModelBuilder.Entity<LaraigoPerson>().HasNoKey();

            ModelBuilder.Entity<LaraigoConversation>().HasNoKey();

            ModelBuilder.Entity<ReportSchedulerData>().HasNoKey();

            ModelBuilder.Entity<KpiData>().HasNoKey();

            ModelBuilder.Entity<SessionInformation>().HasNoKey();

            ModelBuilder.Entity<InvoiceCorrelative>().HasNoKey();

            ModelBuilder.Entity<ActiveBilling>().HasNoKey();

            ModelBuilder.Entity<BillingPeriod>().HasNoKey();

            ModelBuilder.Entity<InvoiceData>().HasNoKey();

            ModelBuilder.Entity<ActiveOrganization>().HasNoKey();

            ModelBuilder.Entity<SecurityValidation>().HasNoKey();

            ModelBuilder.Entity<AlertMessageCron>().HasNoKey();

            ModelBuilder.Entity<CallConversation>().HasNoKey();

            ModelBuilder.Entity<ConversationData>().HasNoKey();

            ModelBuilder.Entity<OrganizationData>().HasNoKey();

            ModelBuilder.Entity<AbandonedTicket>().HasNoKey();

            ModelBuilder.Entity<CallInteraction>().HasNoKey();

            ModelBuilder.Entity<ChannelSchedule>().HasNoKey();

            ModelBuilder.Entity<ReportTemplate>().HasNoKey();

            ModelBuilder.Entity<ActiveChannel>().HasNoKey();

            ModelBuilder.Entity<CampaignData>().HasNoKey();

            ModelBuilder.Entity<HoldingData>().HasNoKey();

            ModelBuilder.Entity<SessionData>().HasNoKey();

            ModelBuilder.Entity<ReportCron>().HasNoKey();

            ModelBuilder.Entity<InteractionCron>().HasNoKey();

            base.OnModelCreating(ModelBuilder);
        }

        public DbSet<WitaiCron> WitaiCron { get; set; }

        public DbSet<WitaiCron> WitaiConfig { get; set; }

        public DbSet<WitaiSchedule> WitaiSchedule { get; set; }

        public DbSet<AttachmentData> AttachmentData { get; set; }

        public DbSet<VoximplantOrganization> VoximplantOrganization { get; set; }

        public DbSet<LaraigoPerson> LaraigoPerson { get; set; }

        public DbSet<LaraigoConversation> LaraigoConversation { get; set; }

        public DbSet<KpiData> KpiData { get; set; }

        public DbSet<ReportSchedulerData> ReportSchedulerData { get; set; }

        public DbSet<InvoiceCorrelative> InvoiceCorrelative { get; set; }

        public DbSet<InvoiceData> InvoiceData { get; set; }

        public DbSet<ActiveBilling> ActiveBilling { get; set; }

        public DbSet<BillingPeriod> BillingPeriod { get; set; }

        public DbSet<CommunicationChannel> CommunicationChannel { get; set; }

        public DbSet<SessionInformation> SessionInformation { get; set; }

        public DbSet<ActiveOrganization> ActiveOrganization { get; set; }

        public DbSet<SecurityValidation> SecurityValidation { get; set; }

        public DbSet<AlertMessageCron> AlertMessageCron { get; set; }

        public DbSet<CallConversation> CallConversation { get; set; }

        public DbSet<ConversationData> ConversationData { get; set; }

        public DbSet<OrganizationData> OrganizationData { get; set; }

        public DbSet<AbandonedTicket> AbandonedTicket { get; set; }

        public DbSet<CallInteraction> CallInteraction { get; set; }

        public DbSet<ChannelSchedule> ChannelSchedule { get; set; }

        public DbSet<ReportTemplate> ReportTemplate { get; set; }

        public DbSet<ActiveChannel> ActiveChannel { get; set; }

        public DbSet<AppStoreToken> AppStoreToken { get; set; }

        public DbSet<CampaignData> CampaignData { get; set; }

        public DbSet<ServiceToken> ServiceToken { get; set; }

        public DbSet<ServiceSubscription> ServiceSubscription { get; set; }

        public DbSet<HoldingData> HoldingData { get; set; }

        public DbSet<SessionData> SessionData { get; set; }

        public DbSet<ReportCron> ReportCron { get; set; }

        public DbSet<InteractionCron> InteractionCron { get; set; }

        public DbSet<SessionLog> SessionLog { get; set; }

        public DbSet<PaymentPending> PaymentPending { get; set; }

        public DbSet<TaskData> TaskData { get; set; }

        public DbSet<CallLog> CallLog { get; set; }

        public DbSet<Invoice> Invoice { get; set; }

        public DbSet<InvoiceDetail> InvoiceDetail { get; set; }

        public DbSet<CouponPromotion> CouponPromotion { get; set; }

        public DbSet<CouponShop> CouponShop { get; set; }

        public DbSet<CouponTicket> CouponTicket { get; set; }

        public DbSet<CouponLaraigoPromotion> CouponLaraigoPromotion { get; set; }

        public DbSet<CouponLaraigoShop> CouponLaraigoShop { get; set; }
    }
}