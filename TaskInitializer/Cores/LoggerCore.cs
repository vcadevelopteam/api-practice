using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TaskInitializer.Models.Common;

namespace TaskInitializer.Cores
{
    public class LoggerCore
    {
        public static Logger ConfigureLogger(AppSettings AppSettings, string LoggerName)
        {
            try
            {
                string CharacterList = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                string Extension = "log";

                if (!string.IsNullOrWhiteSpace(AppSettings.LogSettings.Extension))
                {
                    Extension = AppSettings.LogSettings.Extension;
                }

                LoggerName = Regex.Replace(LoggerName.Replace(" ", "-"), "[\\/:*?\"<>|]", string.Empty);

                string LoggerId = new string(Enumerable.Repeat(CharacterList, 20).Select(DatabaseRow => DatabaseRow[new Random().Next(DatabaseRow.Length)]).ToArray());

                return new LoggerConfiguration().Enrich.WithProperty("LoggerId", LoggerId).MinimumLevel.ControlledBy(new LoggingLevelSwitch((LogEventLevel)AppSettings.LogSettings.MinimumLevel)).WriteTo.File(buffered: AppSettings.LogSettings.Buffered, rollingInterval: (RollingInterval)AppSettings.LogSettings.RollingInterval, rollOnFileSizeLimit: AppSettings.LogSettings.RollOnFileSizeLimit, outputTemplate: AppSettings.LogSettings.Template, path: $"{AppSettings.LogSettings.Location}{AppSettings.LogSettings.Prefix}{LoggerName}.{Extension}", shared: AppSettings.LogSettings.Shared).WriteTo.Console(outputTemplate: AppSettings.LogSettings.Template).CreateLogger();
            }
            catch (Exception Exception)
            {
                Console.WriteLine(JsonConvert.SerializeObject(Exception));

                return null;
            }
        }
    }
}