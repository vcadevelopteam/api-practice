using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Database;

namespace TaskInitializer.Data
{
    public class OdooContext : DbContext
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

            DbContextOptionsBuilder.UseNpgsql(AppSettings.ConnectionStrings.OdooCredentials, Options => Options.EnableRetryOnFailure(errorCodesToAdd: null, maxRetryCount: (int)RetryCount, maxRetryDelay: TimeSpan.FromSeconds((double)RetryDelay)));
        }

        protected override void OnModelCreating(ModelBuilder ModelBuilder)
        {
            ModelBuilder.Entity<ProviderReport>().HasNoKey();

            ModelBuilder.Entity<ProviderMail>().HasNoKey();

            ModelBuilder.Entity<OdooOrder>().HasNoKey();

            ModelBuilder.Entity<OdooProduct>().HasNoKey();

            ModelBuilder.Entity<OdooEquipment>().HasNoKey();

            base.OnModelCreating(ModelBuilder);
        }

        public DbSet<ProviderReport> ProviderReport { get; set; }

        public DbSet<ProviderMail> ProviderMail { get; set; }

        public DbSet<OdooOrder> OdooOrder { get; set; }

        public DbSet<OdooProduct> OdooProduct { get; set; }

        public DbSet<OdooEquipment> OdooEquipment { get; set; }
    }
}