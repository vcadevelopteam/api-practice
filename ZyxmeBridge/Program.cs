using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using ZyxMeBridge.Models.Common;

namespace ZyxMeBridge
{
    public class Program
    {
        public static void Main(string[] Arguments)
        {
            IWebHost WebHost = HostBuilder(Arguments);

            using (IServiceScope ServiceScope = WebHost.Services.CreateScope())
            {
                IServiceProvider ServiceProvider = ServiceScope.ServiceProvider;

                try
                {
                    IOptions<AppSettings> AppSettings = ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
                }
                catch (Exception Exception)
                {
                    Console.WriteLine(Exception.Message);
                }
            }

            WebHost.Run();
        }

        public static IWebHost HostBuilder(string[] Arguments) => WebHost.CreateDefaultBuilder(Arguments).UseContentRoot(Directory.GetCurrentDirectory()).ConfigureAppConfiguration(Configuration => Configuration.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables()).ConfigureKestrel(Configuration => Configuration.Limits.MaxRequestBodySize = long.MaxValue).UseIISIntegration().UseUrls("http://*:56377").UseStartup<Startup>().Build();
    }
}