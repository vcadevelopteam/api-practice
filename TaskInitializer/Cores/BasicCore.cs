using Serilog.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Cores;
using TaskInitializer.Services;

namespace TaskInitializer.Cores
{
    public class BasicCore
    {
        public static async Task HandleTask(AppSettings AppSettings, Logger Logger, ManualSettingsTask ManualSettingsTask)
        {
            try
            {
                Logger.ForContext("Context", $"HandleTask: {ManualSettingsTask.TaskType}").Debug("Initializing task");

                switch (ManualSettingsTask.TaskType.ToUpper())
                {
                    case "LOGMONITOR":
                        await ExecuteLogMonitor(AppSettings, ManualSettingsTask.TaskBody);
                        break;
                }

                Logger.ForContext("Context", $"HandleTask: {ManualSettingsTask.TaskType}").Debug("Finalizing task");
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", $"HandleTask: {ManualSettingsTask.TaskType}").Error(Exception, "Exception found:");

                ExceptionService.HandleException(AppSettings, Logger, Exception);
            }
        }

        public static async Task ExecuteLogMonitor(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "LogMonitor");

            try
            {
                Logger.ForContext("Context", "ExecuteLogMonitor").Debug("Checking file: {FilePath}", TaskBody.FilePath);

                if (File.Exists(TaskBody.FilePath))
                {
                    using FileStream FileStream = File.Open(TaskBody.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    FileStream.Seek(0, SeekOrigin.End);

                    using StreamReader StreamReader = new StreamReader(FileStream);

                    for (; ; )
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));

                        string FileLine = StreamReader.ReadToEnd();

                        if (!string.IsNullOrWhiteSpace(FileLine))
                        {
                            if (TaskBody.LogFilter != null)
                            {
                                bool SendAlert = false;

                                foreach (var Filter in TaskBody.LogFilter)
                                {
                                    if (FileLine.Contains(Filter))
                                    {
                                        SendAlert = true;
                                    }
                                }

                                if (SendAlert)
                                {
                                    SendMailBody SendMailBody = new SendMailBody()
                                    {
                                        MailBlindAddress = TaskBody.BlindReceiver,
                                        MailCopyAddress = TaskBody.CopyReceiver,
                                        MailAddress = TaskBody.Receiver,
                                        MailTitle = TaskBody.MailSubject.Replace("{{date}}", DateTime.Now.ToString()),
                                        MailBody = TaskBody.MailBody.Replace("{{line}}", FileLine)
                                    };

                                    MailService.SendMail(AppSettings, Logger, SendMailBody);
                                }
                            }
                        }
                        else
                        {
                            FileStream.Seek(0, SeekOrigin.End);
                        }
                    }
                }
                else
                {
                    Logger.ForContext("Context", "ExecuteLogMonitor").Debug("File not found: {FilePath}", TaskBody.FilePath);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ExecuteLogMonitor").Error(Exception, "Exception found:");
            }
        }
    }
}