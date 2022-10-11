using Microsoft.Extensions.Configuration;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TaskInitializer.Cores;
using TaskInitializer.Data;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Database;
using TaskInitializer.Services;

namespace TaskInitializer
{
    internal class Program
    {
        public static void Main()
        {
            IConfigurationBuilder ConfigurationBuilder = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables();

            AppSettings AppSettings = ConfigurationBuilder.Build().Get<AppSettings>();

            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "Console");

            Logger.ForContext("Context", "Main").Information("Initializing console (ZyxMe Scheduler 2.6.14)");

            try
            {
                bool AdvancedMode = true;

                if (AppSettings.GeneralSettings != null)
                {
                    AdvancedMode = !AppSettings.GeneralSettings.BasicMode;
                }

                if (AdvancedMode)
                {
                    using DatabaseContext DatabaseContext = new DatabaseContext();

                    DateTime DateNow = DateTime.UtcNow;

                    if (AppSettings.GeneralSettings != null)
                    {
                        if (AppSettings.GeneralSettings.UseLocalTime)
                        {
                            DateNow = DateTime.Now;
                        }
                    }

                    List<Task> TaskList = new List<Task>();

                    long TaskLimit = 8;

                    List<TaskData> TaskDataList = DatabaseContext.TaskData.Where(DatabaseRow => DatabaseRow.Completed == false && DatabaseRow.DateTimeStart <= DateNow).ToList();

                    if (TaskDataList != null)
                    {
                        using SemaphoreSlim SemaphoreSlim = new SemaphoreSlim((int)TaskLimit);

                        foreach (var TaskData in TaskDataList)
                        {
                            SemaphoreSlim.Wait();

                            Task Task = Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    SchedulerCore.HandleTask(AppSettings, Logger, TaskData.TaskSchedulerId).Wait();
                                }
                                finally
                                {
                                    SemaphoreSlim.Release();
                                }
                            });

                            TaskList.Add(Task);
                        }

                        Task.WaitAll(TaskList.ToArray());
                    }
                }
                else
                {
                    if (AppSettings.ManualSettings != null)
                    {
                        if (AppSettings.ManualSettings.TaskList != null)
                        {
                            List<Task> TaskList = new List<Task>();

                            long TaskLimit = 20;

                            using SemaphoreSlim SemaphoreSlim = new SemaphoreSlim((int)TaskLimit);

                            foreach (var ManualSettingsTask in AppSettings.ManualSettings.TaskList)
                            {
                                SemaphoreSlim.Wait();

                                Task Task = Task.Factory.StartNew(() =>
                                {
                                    try
                                    {
                                        BasicCore.HandleTask(AppSettings, Logger, ManualSettingsTask).Wait();
                                    }
                                    finally
                                    {
                                        SemaphoreSlim.Release();
                                    }
                                });

                                TaskList.Add(Task);
                            }

                            Task.WaitAll(TaskList.ToArray());
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "Main").Error(Exception, "Exception found:");

                ExceptionService.HandleException(AppSettings, Logger, Exception);
            }

            Logger.ForContext("Context", "Main").Information("Finalizing console");
        }
    }
}