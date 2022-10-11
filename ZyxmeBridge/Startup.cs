using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using ZyxMeBridge.Models.Common;

namespace ZyxMeBridge
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
        }

        public void ConfigureServices(IServiceCollection ServiceCollection)
        {
            string SymmetricKey = Configuration.GetSection("AuthenticationSettings").GetSection("Configuration")["SymmetricKey"];

            string CommerceMedicalString = Configuration.GetConnectionString("CommerceMedicalCredentials");

            string CommerceCrmString = Configuration.GetConnectionString("CommerceCrmCredentials");

            string ConnectionString = Configuration.GetConnectionString("ConnectionCredentials");

            string AnalyticsString = Configuration.GetConnectionString("AnalyticsCredentials");

            string CommerceString = Configuration.GetConnectionString("CommerceCredentials");

            string RetryCount = Configuration.GetSection("DatabaseSettings")["RetryCount"];

            string RetryDelay = Configuration.GetSection("DatabaseSettings")["RetryDelay"];

            string EntelString = Configuration.GetConnectionString("EntelCredentials");

            if (string.IsNullOrWhiteSpace(CommerceMedicalString))
            {
                CommerceMedicalString = ConnectionString;
            }

            if (string.IsNullOrWhiteSpace(CommerceCrmString))
            {
                CommerceCrmString = ConnectionString;
            }

            if (string.IsNullOrWhiteSpace(AnalyticsString))
            {
                AnalyticsString = ConnectionString;
            }

            if (string.IsNullOrWhiteSpace(CommerceString))
            {
                CommerceString = ConnectionString;
            }

            if (string.IsNullOrWhiteSpace(EntelString))
            {
                EntelString = ConnectionString;
            }

            if (string.IsNullOrWhiteSpace(SymmetricKey))
            {
                SymmetricKey = "ZyxMeBridge2020";
            }

            if (string.IsNullOrWhiteSpace(RetryCount))
            {
                RetryCount = "0";
            }

            if (string.IsNullOrWhiteSpace(RetryDelay))
            {
                RetryDelay = "0";
            }

            ServiceCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(Options => Options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false, ValidateIssuer = false, ValidateLifetime = true, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SymmetricKey)), ClockSkew = TimeSpan.Zero });

            ServiceCollection.AddControllers().AddNewtonsoftJson(Options => { });

            ServiceCollection.Configure<AppSettings>(Configuration);

            ServiceCollection.AddCors();

            ServiceCollection.Configure<IISServerOptions>(Options =>
            {
                Options.MaxRequestBodySize = long.MaxValue;
            });

            ServiceCollection.Configure<KestrelServerOptions>(Options =>
            {
                Options.Limits.MaxRequestBodySize = long.MaxValue;
            });
        }

        public void Configure(IApplicationBuilder ApplicationBuilder)
        {
            ApplicationBuilder.UseCors(Options => Options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            ApplicationBuilder.UseStaticFiles();

            ApplicationBuilder.UseRouting();

            ApplicationBuilder.UseCors();

            ApplicationBuilder.UseAuthentication();

            ApplicationBuilder.UseAuthorization();

            ApplicationBuilder.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            ApplicationBuilder.UseEndpoints(Endpoint =>
            {
                Endpoint.MapControllers();
            });
        }
    }
}