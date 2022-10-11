using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Database;

namespace TaskInitializer.Data
{
    public class EamContext : DbContext
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

            DbContextOptionsBuilder.UseNpgsql(AppSettings.ConnectionStrings.EamCredentials, Options => Options.EnableRetryOnFailure(errorCodesToAdd: null, maxRetryCount: (int)RetryCount, maxRetryDelay: TimeSpan.FromSeconds((double)RetryDelay)));
        }

        protected override void OnModelCreating(ModelBuilder ModelBuilder)
        {
            ModelBuilder.Entity<SessionInformationGeneral>().HasNoKey();

            base.OnModelCreating(ModelBuilder);
        }

        public DbSet<SessionInformationGeneral> SessionInformationGeneral { get; set; }
    }
}