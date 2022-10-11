using ClosedXML.Excel;
using FluentFTP;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Blogger.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaskInitializer.Data;
using TaskInitializer.Models.Common;
using TaskInitializer.Models.Cores;
using TaskInitializer.Models.Database;
using TaskInitializer.Services;

namespace TaskInitializer.Cores
{
    internal class SchedulerCore
    {
        public static async Task HandleTask(AppSettings AppSettings, Logger Logger, long TaskSchedulerId)
        {
            try
            {
                using DatabaseContext DatabaseContext = new DatabaseContext();

                TaskData TaskData = DatabaseContext.TaskData.First(DatabaseRow => DatabaseRow.TaskSchedulerId == TaskSchedulerId);

                if ((bool)!TaskData.Completed)
                {
                    Logger.ForContext("Context", $"HandleTask: {TaskSchedulerId}").Debug("Initializing task: {TaskType}", TaskData.TaskType.ToUpper());

                    TaskData.DateTimeLastRun = DateTime.UtcNow;

                    if (AppSettings.GeneralSettings != null)
                    {
                        if (AppSettings.GeneralSettings.UseLocalTime)
                        {
                            TaskData.DateTimeLastRun = DateTime.Now;
                        }
                    }

                    TaskData.Completed = true;

                    DatabaseContext.TaskData.Update(TaskData);

                    DatabaseContext.SaveChanges();

                    TaskBody TaskBody = new TaskBody();

                    if (!string.IsNullOrWhiteSpace(TaskData.TaskBody))
                    {
                        TaskBody = JsonConvert.DeserializeObject<TaskBody>(TaskData.TaskBody);
                    }

                    if (TaskBody.TaskOffset == 0)
                    {
                        TaskBody.TaskOffset = -5;
                    }

                    bool ForceCompletion = false;

                    bool JumpInterval = false;

                    bool Success = false;

                    switch (TaskData.TaskType.ToUpper())
                    {
                        case "ALERTMAIL":
                            Success = await ProcessAlertMail(AppSettings, DatabaseContext);
                            break;

                        case "ATTACHMENTLIST":
                            Success = await ProcessAttachmentList(AppSettings, DatabaseContext, TaskBody, (long)TaskData.CorpId, (long)TaskData.OrgId);
                            break;

                        case "CHECKABANDONMENT":
                            Success = await ProcessCheckAbandonment(AppSettings, DatabaseContext);
                            break;

                        case "CLEANSMOOCHSESSION":
                            Success = ProcessCleanSmoochSession(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "CLEANTICKET":
                            Success = await ProcessCleanTicket(AppSettings, TaskBody);
                            break;

                        case "COMMENTCHECK":
                            Success = await ProcessCommentCheck(AppSettings, DatabaseContext);
                            break;

                        case "CONVERSATIONCHECK":
                            Success = await ProcessConversationCheck(AppSettings, TaskBody);
                            break;

                        case "DATABASECHECK":
                            Success = await ProcessDatabaseCheck(AppSettings, TaskBody);
                            break;

                        case "EXECUTECAMPAIGN":
                            switch (await ProcessExecuteCampaign(AppSettings, DatabaseContext, TaskBody, TaskData.TaskSchedulerId))
                            {
                                case "CONTINUE":
                                    Success = true;
                                    break;

                                case "FINISH":
                                    ForceCompletion = true;
                                    break;

                                case "JUMP":
                                    JumpInterval = true;
                                    break;
                            }
                            break;

                        case "EXECUTEQUERY":
                            Success = await ProcessExecuteQuery(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "GLOBALCHECK":
                            Success = await ProcessGlobalCheck(AppSettings, TaskBody);
                            break;

                        case "HOLDINGCHECK":
                            Success = await ProcessHoldingCheck(AppSettings, TaskBody);
                            break;

                        case "LEADAUTOMATIZATIONRULES":
                            Success = await ProcessLeadAutomatizationRules(AppSettings, TaskData.TaskBody, (long)TaskData.CorpId, (long)TaskData.OrgId);
                            break;

                        case "LEADSENDHSM":
                            Success = await ProcessLeadSendHsm(AppSettings, TaskData.TaskBody, (long)TaskData.CorpId, (long)TaskData.OrgId);
                            break;

                        case "REFRESHTOKEN":
                            Success = await ProcessRefreshToken(AppSettings, DatabaseContext);
                            break;

                        case "RELOCATEFILE":
                            Success = await ProcessRelocateFile(AppSettings, TaskBody);
                            break;

                        case "SENDAUTOMATICHSM":
                            Success = await ProcessSendAutomaticHsm(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "SENDAZTECAREPORT":
                            Success = await ProcessSendAztecaReport(AppSettings, TaskBody);
                            break;

                        case "SENDFTPREPORT":
                            Success = await ProcessSendFtpReport(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "SENDHSM":
                            Success = await ProcessSendHsm(AppSettings, TaskData);
                            break;

                        case "SENDMAIL":
                            Success = await ProcessSendMail(AppSettings, TaskBody, DatabaseContext, (long)TaskData.CorpId, (long)TaskData.OrgId);
                            break;

                        case "SENDPASSWORDALERT":
                            Success = await ProcessSendPasswordAlert(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "SENDREPORTQUERY":
                            Success = await ProcessSendReportQuery(AppSettings, TaskBody);
                            break;

                        case "SENDREPORTTEMPLATE":
                            Success = await ProcessSendReportTemplate(AppSettings, TaskBody);
                            break;

                        case "SESSIONCHECK":
                            Success = await ProcessSessionCheck(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "STARTKPI":
                            Success = await ProcessStartKpi(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "STARTPENDINGPAYMENT":
                            Success = await ProcessStartPaymentPending(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "STARTREPORTSCHEDULER":
                            Success = await ProcessStartReportScheduler(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "STATUSCHECK":
                            Success = await ProcessStatusCheck(AppSettings, TaskBody);
                            break;

                        case "UPDATEBILLING":
                            Success = await ProcessUpdateBilling(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "UPDATEBILLINGMONTH":
                            Success = ProcessUpdateBillingMonth(AppSettings, DatabaseContext);
                            break;

                        case "UPDATECALLLOG":
                            Success = await ProcessUpdateCallLog(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "UPDATECHANNEL":
                            Success = await ProcessUpdateChannel(AppSettings, DatabaseContext);
                            break;

                        case "UPDATECOUPON":
                            Success = await ProcessUpdateCoupon(AppSettings, DatabaseContext);
                            break;

                        case "UPDATECOUPONLARAIGO":
                            Success = await ProcessUpdateCouponLaraigo(AppSettings, DatabaseContext);
                            break;

                        case "UPDATECOUPONVARIANT":
                            Success = ProcessUpdateCouponVariant(AppSettings, DatabaseContext);
                            break;

                        case "UPDATESESSION":
                            Success = await ProcessUpdateSession(AppSettings, DatabaseContext);
                            break;

                        case "UPDATESUBSCRIPTION":
                            Success = await ProcessUpdateSubscription(AppSettings, DatabaseContext);
                            break;

                        case "UPDATEUSER":
                            Success = await ProcessUpdateUser(AppSettings, DatabaseContext);
                            break;

                        case "UPDATEVOXIMPLANT":
                            Success = await ProcessUpdateVoximplant(AppSettings, DatabaseContext);
                            break;

                        case "VOXIMPLANTRECHARGE":
                            Success = await ProcessVoximplantRecharge(AppSettings, DatabaseContext);
                            break;

                        case "WITAICREATEAPP":
                            Success = await ProcessWitaiCreateApp(AppSettings, DatabaseContext, TaskBody);
                            break;

                        case "WITAIUPDATETRAINING":
                            Success = await ProcessWitaiUpdateTraining(AppSettings, DatabaseContext, TaskBody);
                            break;
                    }

                    if (TaskData.TaskType != "UPDATEBILLINGMONTH")
                    {
                        if (!ForceCompletion)
                        {
                            if (TaskData.RepeatFlag == true || !Success)
                            {
                                if (!JumpInterval)
                                {
                                    if (TaskData.RepeatFlag == true && Success)
                                    {
                                        DateTime DateNow = DateTime.UtcNow;

                                        if (AppSettings.GeneralSettings != null)
                                        {
                                            if (AppSettings.GeneralSettings.UseLocalTime)
                                            {
                                                DateNow = DateTime.Now;
                                            }
                                        }

                                        while (TaskData.DateTimeStart < DateNow)
                                        {
                                            switch (TaskData.RepeatMode)
                                            {
                                                case 1:
                                                    TaskData.DateTimeStart = ((DateTime)TaskData.DateTimeStart).AddMinutes((double)TaskData.RepeatInterval);
                                                    break;

                                                case 2:
                                                    TaskData.DateTimeStart = ((DateTime)TaskData.DateTimeStart).AddHours((double)TaskData.RepeatInterval);
                                                    break;

                                                case 3:
                                                    TaskData.DateTimeStart = ((DateTime)TaskData.DateTimeStart).AddDays((double)TaskData.RepeatInterval);
                                                    break;
                                            }
                                        }

                                        if (TaskData.DateTimeStart <= TaskData.DateTimeEnd)
                                        {
                                            TaskData.Completed = false;
                                        }
                                    }

                                    if (!Success)
                                    {
                                        TaskData.Completed = false;
                                    }

                                    DatabaseContext.TaskData.Update(TaskData);

                                    DatabaseContext.SaveChanges();
                                }
                                else
                                {
                                    DateTime DateNow = DateTime.UtcNow;

                                    if (AppSettings.GeneralSettings != null)
                                    {
                                        if (AppSettings.GeneralSettings.UseLocalTime)
                                        {
                                            DateNow = DateTime.Now;
                                        }
                                    }

                                    while (TaskData.DateTimeStart < DateNow)
                                    {
                                        DatabaseContext.TaskData.Attach(TaskData);

                                        switch (TaskData.RepeatMode)
                                        {
                                            case 1:
                                                TaskData.DateTimeStart = ((DateTime)TaskData.DateTimeStart).AddMinutes((double)TaskData.RepeatInterval);
                                                break;

                                            case 2:
                                                TaskData.DateTimeStart = ((DateTime)TaskData.DateTimeStart).AddHours((double)TaskData.RepeatInterval);
                                                break;

                                            case 3:
                                                TaskData.DateTimeStart = ((DateTime)TaskData.DateTimeStart).AddDays((double)TaskData.RepeatInterval);
                                                break;
                                        }

                                        DatabaseContext.Entry(TaskData).Property(DatabaseRow => DatabaseRow.DateTimeStart).IsModified = true;

                                        DatabaseContext.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Success)
                        {
                            DateTime DateTimeStart = new DateTime(DateTime.UtcNow.AddMonths(1).Year, DateTime.UtcNow.AddMonths(1).Month, 1);

                            if (AppSettings.GeneralSettings != null)
                            {
                                if (AppSettings.GeneralSettings.UseLocalTime)
                                {
                                    DateTimeStart = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1);
                                }
                            }

                            DateTimeStart = DateTimeStart.AddHours((double)TaskBody.Offset).AddMinutes(1);

                            TaskData.DateTimeStart = DateTimeStart;
                            TaskData.Completed = false;

                            DatabaseContext.TaskData.Update(TaskData);

                            DatabaseContext.SaveChanges();
                        }
                        else
                        {
                            TaskData.Completed = false;

                            DatabaseContext.TaskData.Update(TaskData);

                            DatabaseContext.SaveChanges();
                        }
                    }

                    Logger.ForContext("Context", $"HandleTask: {TaskSchedulerId}").Debug("Finalizing task: {TaskType}", TaskData.TaskType.ToUpper());
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", $"HandleTask: {TaskSchedulerId}").Error(Exception, "Exception found:");

                ExceptionService.HandleException(AppSettings, Logger, Exception);
            }
        }

        public static async Task<bool> ProcessAlertMail(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "AlertMail");

            bool Success = false;

            try
            {
                List<ActiveOrganization> ActiveOrganizationList = await DatabaseContext.ActiveOrganization.FromSqlRaw(StoredProcedure.ActiveOrganizationSelect).ToListAsync();

                if (ActiveOrganizationList != null)
                {
                    if (ActiveOrganizationList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        foreach (var ActiveOrganization in ActiveOrganizationList)
                        {
                            try
                            {
                                List<AlertMessageCron> AlertMessageCronList = await DatabaseContext.AlertMessageCron.FromSqlRaw(StoredProcedure.MailAlertSelect, ActiveOrganization.CorpId, ActiveOrganization.OrgId).ToListAsync();

                                if (AlertMessageCronList != null)
                                {
                                    if (AlertMessageCronList.Count == 0)
                                    {
                                        Success = true;
                                    }
                                    else
                                    {
                                        foreach (var AlertMessageCron in AlertMessageCronList)
                                        {
                                            try
                                            {
                                                List<VariableContext> VariableContextList = new List<VariableContext>();

                                                if (!string.IsNullOrWhiteSpace(AlertMessageCron.VariableContext))
                                                {
                                                    VariableContextList = JsonConvert.DeserializeObject<List<VariableContext>>(AlertMessageCron.VariableContext);
                                                }

                                                string MailSubject = AlertMessageCron.EmailAlertSubject;

                                                string[] SubjectArray = MailSubject.Split(new string[] { "{{", "}}" }, StringSplitOptions.RemoveEmptyEntries);

                                                foreach (var Variable in SubjectArray)
                                                {
                                                    VariableContext VariableContext = VariableContextList.Find(DatabaseRow => DatabaseRow.Name.ToUpper() == Variable);

                                                    if (VariableContext != null)
                                                    {
                                                        MailSubject = MailSubject?.Replace("{{" + Variable + "}}", VariableContext.Value);
                                                    }
                                                }

                                                MailSubject = MailSubject?.Replace("{{asesordesc}}", AlertMessageCron.AsesorDesc);
                                                MailSubject = MailSubject?.Replace("{{asesoremail}}", AlertMessageCron.AsesorEmail);
                                                MailSubject = MailSubject?.Replace("{{asesorid}}", AlertMessageCron.AsesorId.ToString());
                                                MailSubject = MailSubject?.Replace("{{channel}}", AlertMessageCron.Channel);
                                                MailSubject = MailSubject?.Replace("{{channelid}}", AlertMessageCron.ChannelId.ToString());
                                                MailSubject = MailSubject?.Replace("{{channeltype}}", AlertMessageCron.ChannelType);
                                                MailSubject = MailSubject?.Replace("{{conversationid}}", AlertMessageCron.ConversationId.ToString());
                                                MailSubject = MailSubject?.Replace("{{emailalertmessage}}", AlertMessageCron.EmailAlertMessage);
                                                MailSubject = MailSubject?.Replace("{{emailalertsubject}}", AlertMessageCron.EmailAlertSubject);
                                                MailSubject = MailSubject?.Replace("{{emailalerttime}}", AlertMessageCron.EmailAlertTime);
                                                MailSubject = MailSubject?.Replace("{{lasttag}}", AlertMessageCron.LastTag);
                                                MailSubject = MailSubject?.Replace("{{supervisordesc}}", AlertMessageCron.SupervisorDesc);
                                                MailSubject = MailSubject?.Replace("{{supervisoremail}}", AlertMessageCron.SupervisorEmail);
                                                MailSubject = MailSubject?.Replace("{{supervisorid}}", AlertMessageCron.SupervisorId.ToString());
                                                MailSubject = MailSubject?.Replace("{{tags}}", AlertMessageCron.Tags);
                                                MailSubject = MailSubject?.Replace("{{ticketnum}}", AlertMessageCron.TicketNum);
                                                MailSubject = MailSubject?.Replace("{{usergroup}}", AlertMessageCron.UserGroup);
                                                MailSubject = MailSubject?.Replace("{{variablecontext}}", AlertMessageCron.VariableContext);
                                                MailSubject = MailSubject?.Replace("{{withresponse}}", AlertMessageCron.WithResponse.ToString());

                                                string MailBody = AlertMessageCron.EmailAlertMessage;

                                                string[] BodyArray = MailBody.Split(new string[] { "{{", "}}" }, StringSplitOptions.RemoveEmptyEntries);

                                                foreach (var Variable in BodyArray)
                                                {
                                                    VariableContext VariableContext = VariableContextList.Find(DatabaseRow => DatabaseRow.Name.ToUpper() == Variable);

                                                    if (VariableContext != null)
                                                    {
                                                        MailBody = MailBody?.Replace("{{" + Variable + "}}", VariableContext.Value);
                                                    }
                                                }

                                                MailBody = MailBody?.Replace("{{asesordesc}}", AlertMessageCron.AsesorDesc);
                                                MailBody = MailBody?.Replace("{{asesoremail}}", AlertMessageCron.AsesorEmail);
                                                MailBody = MailBody?.Replace("{{asesorid}}", AlertMessageCron.AsesorId.ToString());
                                                MailBody = MailBody?.Replace("{{channel}}", AlertMessageCron.Channel);
                                                MailBody = MailBody?.Replace("{{channelid}}", AlertMessageCron.ChannelId.ToString());
                                                MailBody = MailBody?.Replace("{{channeltype}}", AlertMessageCron.ChannelType);
                                                MailBody = MailBody?.Replace("{{conversationid}}", AlertMessageCron.ConversationId.ToString());
                                                MailBody = MailBody?.Replace("{{emailalertmessage}}", AlertMessageCron.EmailAlertMessage);
                                                MailBody = MailBody?.Replace("{{emailalertsubject}}", AlertMessageCron.EmailAlertSubject);
                                                MailBody = MailBody?.Replace("{{emailalerttime}}", AlertMessageCron.EmailAlertTime);
                                                MailBody = MailBody?.Replace("{{lasttag}}", AlertMessageCron.LastTag);
                                                MailBody = MailBody?.Replace("{{supervisordesc}}", AlertMessageCron.SupervisorDesc);
                                                MailBody = MailBody?.Replace("{{supervisoremail}}", AlertMessageCron.SupervisorEmail);
                                                MailBody = MailBody?.Replace("{{supervisorid}}", AlertMessageCron.SupervisorId.ToString());
                                                MailBody = MailBody?.Replace("{{tags}}", AlertMessageCron.Tags);
                                                MailBody = MailBody?.Replace("{{ticketnum}}", AlertMessageCron.TicketNum);
                                                MailBody = MailBody?.Replace("{{usergroup}}", AlertMessageCron.UserGroup);
                                                MailBody = MailBody?.Replace("{{variablecontext}}", AlertMessageCron.VariableContext);
                                                MailBody = MailBody?.Replace("{{withresponse}}", AlertMessageCron.WithResponse.ToString());

                                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                                                using HttpClient HttpClient = new HttpClient();

                                                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                                SendMailBody SendMailBody = new SendMailBody()
                                                {
                                                    MailAddress = AlertMessageCron.SupervisorEmail,
                                                    MailBody = MailBody,
                                                    MailTitle = MailSubject
                                                };

                                                string StringContent = JsonConvert.SerializeObject(SendMailBody);

                                                Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Debug("Http POST body: {StringContent}", StringContent);

                                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                if (HttpResponseMessage.IsSuccessStatusCode)
                                                {
                                                    BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                                    if (BridgeResponse.Success)
                                                    {
                                                        Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                        DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.MailAlertUpdate, AlertMessageCron.ConversationId);

                                                        Success = true;
                                                    }
                                                    else
                                                    {
                                                        Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                                    }
                                                }
                                                else
                                                {
                                                    Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                    Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                }
                                            }
                                            catch (Exception Exception)
                                            {
                                                Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Error(Exception, "Exception found:");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Success = true;
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessAlertMail: {ActiveOrganization.OrgId}").Error(Exception, "Exception found:");
                            }

                            Success = true;
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessAlertMail").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessAttachmentList(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody, long CorpId, long OrgId)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "AttachmentList");

            bool Success = false;

            try
            {
                List<AttachmentData> AttachmentDataList = await DatabaseContext.AttachmentData.FromSqlRaw(StoredProcedure.SelectSunatInteraction, CorpId, OrgId, TaskBody.Start, TaskBody.End).ToListAsync();

                if (AttachmentDataList != null)
                {
                    if (!Directory.Exists(TaskBody.Path))
                    {
                        Directory.CreateDirectory(TaskBody.Path);
                    }

                    foreach (var AttachmentData in AttachmentDataList)
                    {
                        if (Uri.IsWellFormedUriString(AttachmentData.InteractionText, UriKind.Absolute))
                        {
                            Directory.CreateDirectory(Path.Combine(TaskBody.Path, AttachmentData.TicketNum));

                            try
                            {
                                using WebClient WebClient = new WebClient();

                                WebClient.DownloadFile(AttachmentData.InteractionText, Path.Combine(TaskBody.Path, AttachmentData.TicketNum, Path.GetFileName(AttachmentData.InteractionText)));
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", "ProcessAttachmentList").Error(Exception, "Exception found:");
                            }
                        }
                    }
                }

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessAttachmentList").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessCheckAbandonment(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "CheckAbandonment");

            bool Success = false;

            try
            {
                List<AbandonedTicket> AbandonedTicketList = await DatabaseContext.AbandonedTicket.FromSqlRaw(StoredProcedure.AbandonedTicketSelect).ToListAsync();

                if (AbandonedTicketList != null)
                {
                    if (AbandonedTicketList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        string HttpEndpoint = $"{AppSettings.ZyxMeSettings.ServicesEndpoint}api/handler/ticket/abandoned";

                        using HttpClient HttpClient = new HttpClient();

                        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        foreach (var AbandonedTicket in AbandonedTicketList)
                        {
                            dynamic ObjectContent = new ExpandoObject();

                            ObjectContent.CloseTicket = AbandonedTicket.CloseTicket;
                            ObjectContent.CommunicationChannelId = AbandonedTicket.CommunicationChannel;
                            ObjectContent.ConversationId = AbandonedTicket.ConversationId;
                            ObjectContent.CorpId = AbandonedTicket.CorpId;
                            ObjectContent.InteractionType = AbandonedTicket.InteractionType;
                            ObjectContent.MessageText = AbandonedTicket.MessageText;
                            ObjectContent.OrgId = AbandonedTicket.OrgId;
                            ObjectContent.PersonCommunicationChannel = AbandonedTicket.PersonCommunicationChannel;
                            ObjectContent.PersonId = AbandonedTicket.PersonId;
                            ObjectContent.Type = AbandonedTicket.Type;

                            string StringContent = JsonConvert.SerializeObject(ObjectContent);

                            Logger.ForContext("Context", $"ProcessCheckAbandonment: {AbandonedTicket.CorpId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                            Logger.ForContext("Context", $"ProcessCheckAbandonment: {AbandonedTicket.CorpId}").Debug("Http POST body: {StringContent}", StringContent);

                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                            if (HttpResponseMessage.IsSuccessStatusCode)
                            {
                                ZyxMeResponse ZyxMeResponse = JsonConvert.DeserializeObject<ZyxMeResponse>(HttpResponseContent);

                                if (ZyxMeResponse.Success)
                                {
                                    Logger.ForContext("Context", $"ProcessCheckAbandonment: {AbandonedTicket.CorpId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Success = true;
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessCheckAbandonment: {AbandonedTicket.CorpId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                }
                            }
                            else
                            {
                                Logger.ForContext("Context", $"ProcessCheckAbandonment: {AbandonedTicket.CorpId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                Logger.ForContext("Context", $"ProcessCheckAbandonment: {AbandonedTicket.CorpId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessCheckAbandonment").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static bool ProcessCleanSmoochSession(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "CleanSmoochSession");

            bool Success = false;

            try
            {
                DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.CleanSmoochSessionSelect, TaskBody.Hour);

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessCleanSmoochSession").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessCleanTicket(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "CleanTicket");

            bool Success = false;

            try
            {
                Logger.ForContext("Context", "ProcessCleanTicket").Debug("Cleaning ticket: {CloseMotive}", TaskBody.CloseMotive);

                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.ServicesEndpoint}api/triggers/closeticketmanual";

                using HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                dynamic ObjectContent = new ExpandoObject();

                ObjectContent.closeby = TaskBody.CloseBy;
                ObjectContent.closemotive = TaskBody.CloseMotive;
                ObjectContent.deleteall = TaskBody.DeleteAll;
                ObjectContent.messagetosend = TaskBody.MessageToSend;
                ObjectContent.observation = TaskBody.Observation;
                ObjectContent.querytoclose = $"{StoredProcedure.CleanTicketSelect} {TaskBody.Query}";

                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                Logger.ForContext("Context", "ProcessCleanTicket").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", "ProcessCleanTicket").Debug("Http POST body: {StringContent}", StringContent);

                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    ZyxMeResponse ZyxMeResponse = JsonConvert.DeserializeObject<ZyxMeResponse>(HttpResponseContent);

                    if (ZyxMeResponse.Success)
                    {
                        Logger.ForContext("Context", "ProcessCleanTicket").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Success = true;
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessCleanTicket").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);
                    }
                }
                else
                {
                    Logger.ForContext("Context", "ProcessCleanTicket").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", "ProcessCleanTicket").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessCleanTicket").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessCommentCheck(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "CommentCheck");

            bool Success = false;

            try
            {
                List<ServiceSubscription> ServiceSubscriptionList = await DatabaseContext.ServiceSubscription.Where(DataRow => (DataRow.SubscriptionDateEnd <= DateTime.UtcNow || DataRow.SubscriptionDateEnd == null) && DataRow.Status == "ACTIVO").ToListAsync();

                if (ServiceSubscriptionList != null)
                {
                    if (ServiceSubscriptionList.Count > 0)
                    {
                        foreach (var ServiceSubscription in ServiceSubscriptionList)
                        {
                            try
                            {
                                switch (ServiceSubscription.Type.ToUpper())
                                {
                                    case "GOOGLE-BLOGGER":
                                        if (!string.IsNullOrWhiteSpace(ServiceSubscription.Account))
                                        {
                                            ServiceToken ServiceToken = await DatabaseContext.ServiceToken.FirstOrDefaultAsync(DataRow => DataRow.Account == ServiceSubscription.Account && DataRow.Type == "GOOGLE" && DataRow.Status == "ACTIVO");

                                            if (ServiceToken != null)
                                            {
                                                GoogleCredential UserCredential = GoogleCredential.FromAccessToken(ServiceToken.AccessToken);

                                                BloggerService BloggerServiceClient = new BloggerService(new BaseClientService.Initializer()
                                                {
                                                    HttpClientInitializer = UserCredential
                                                });

                                                PostsResource.ListRequest ListRequest = BloggerServiceClient.Posts.List(ServiceSubscription.Node);

                                                ListRequest.MaxResults = 500;
                                                ListRequest.OrderBy = PostsResource.ListRequest.OrderByEnum.UPDATED;
                                                ListRequest.Status = PostsResource.ListRequest.StatusEnum.LIVE;

                                                Google.Apis.Blogger.v3.Data.PostList PostList = ListRequest.Execute();

                                                if (PostList != null)
                                                {
                                                    if (PostList.Items != null)
                                                    {
                                                        foreach (var Post in PostList.Items)
                                                        {
                                                            Google.Apis.Blogger.v3.CommentsResource.ListRequest CommentListRequest = BloggerServiceClient.Comments.List(ServiceSubscription.Node, Post.Id);

                                                            CommentListRequest.MaxResults = 500;
                                                            CommentListRequest.Status = Google.Apis.Blogger.v3.CommentsResource.ListRequest.StatusEnum.LIVE;

                                                            Google.Apis.Blogger.v3.Data.CommentList CommentList = CommentListRequest.Execute();

                                                            if (CommentList != null)
                                                            {
                                                                if (CommentList.Items != null)
                                                                {
                                                                    foreach (var Comment in CommentList.Items)
                                                                    {
                                                                        if (Post.Author.Id != Comment.Author.Id)
                                                                        {
                                                                            DateTime DateComment = DateTime.Parse(Comment.Published).ToUniversalTime();

                                                                            if (DateComment >= ServiceSubscription.SubscriptionDateStart && DateComment <= ServiceSubscription.SubscriptionDateEnd)
                                                                            {
                                                                                bool SendComment = true;

                                                                                try
                                                                                {
                                                                                    Google.Apis.Blogger.v3.Data.BlogUserInfo BlogUserInfo = BloggerServiceClient.BlogUserInfos.Get(Comment.Author.Id, ServiceSubscription.Node).Execute();

                                                                                    if (BlogUserInfo != null)
                                                                                    {
                                                                                        if (BlogUserInfo.BlogUserInfoValue != null)
                                                                                        {
                                                                                            if (!string.IsNullOrWhiteSpace(BlogUserInfo.BlogUserInfoValue.Role))
                                                                                            {
                                                                                                if (BlogUserInfo.BlogUserInfoValue.Role.ToUpper() == "ADMIN")
                                                                                                {
                                                                                                    SendComment = false;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                catch
                                                                                {
                                                                                    SendComment = true;
                                                                                }

                                                                                if (SendComment)
                                                                                {
                                                                                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.HookEndpoint}api/blogger/webhookasync";

                                                                                    using HttpClient HttpClient = new HttpClient();

                                                                                    dynamic ObjectContent = new ExpandoObject();

                                                                                    ObjectContent.CommentId = Comment.Id;
                                                                                    ObjectContent.Content = Comment.Content;
                                                                                    ObjectContent.PersonAvatar = Comment.Author.Image.Url;
                                                                                    ObjectContent.PersonId = Comment.Author.Id;
                                                                                    ObjectContent.PersonName = Comment.Author.DisplayName;
                                                                                    ObjectContent.PostId = Post.Id;
                                                                                    ObjectContent.SiteId = $"{ServiceSubscription.Account}&%BLOG%&{ServiceSubscription.Node}";

                                                                                    if (Comment.InReplyTo != null)
                                                                                    {
                                                                                        if (!string.IsNullOrWhiteSpace(Comment.InReplyTo.Id))
                                                                                        {
                                                                                            ObjectContent.ParentComment = Comment.InReplyTo.Id;
                                                                                        }
                                                                                    }

                                                                                    string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST body: {StringContent}", StringContent);

                                                                                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                                                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                                                                    {
                                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                while (!string.IsNullOrWhiteSpace(CommentList.NextPageToken))
                                                                {
                                                                    CommentListRequest.PageToken = CommentList.NextPageToken;

                                                                    CommentList = CommentListRequest.Execute();

                                                                    if (CommentList.Items != null)
                                                                    {
                                                                        foreach (var Comment in CommentList.Items)
                                                                        {
                                                                            if (Post.Author.Id != Comment.Author.Id)
                                                                            {
                                                                                DateTime DateComment = DateTime.Parse(Comment.Published).ToUniversalTime();

                                                                                if (DateComment >= ServiceSubscription.SubscriptionDateStart && DateComment <= ServiceSubscription.SubscriptionDateEnd)
                                                                                {
                                                                                    bool SendComment = true;

                                                                                    try
                                                                                    {
                                                                                        Google.Apis.Blogger.v3.Data.BlogUserInfo BlogUserInfo = BloggerServiceClient.BlogUserInfos.Get(Comment.Author.Id, ServiceSubscription.Node).Execute();

                                                                                        if (BlogUserInfo != null)
                                                                                        {
                                                                                            if (BlogUserInfo.BlogUserInfoValue != null)
                                                                                            {
                                                                                                if (!string.IsNullOrWhiteSpace(BlogUserInfo.BlogUserInfoValue.Role))
                                                                                                {
                                                                                                    if (BlogUserInfo.BlogUserInfoValue.Role.ToUpper() == "ADMIN")
                                                                                                    {
                                                                                                        SendComment = false;
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    catch
                                                                                    {
                                                                                        SendComment = true;
                                                                                    }

                                                                                    if (SendComment)
                                                                                    {
                                                                                        string HttpEndpoint = $"{AppSettings.ZyxMeSettings.HookEndpoint}api/blogger/webhookasync";

                                                                                        using HttpClient HttpClient = new HttpClient();

                                                                                        dynamic ObjectContent = new ExpandoObject();

                                                                                        ObjectContent.CommentId = Comment.Id;
                                                                                        ObjectContent.Content = Comment.Content;
                                                                                        ObjectContent.PersonAvatar = Comment.Author.Image.Url;
                                                                                        ObjectContent.PersonId = Comment.Author.Id;
                                                                                        ObjectContent.PersonName = Comment.Author.DisplayName;
                                                                                        ObjectContent.PostId = Post.Id;
                                                                                        ObjectContent.SiteId = $"{ServiceSubscription.Account}&%BLOG%&{ServiceSubscription.Node}";

                                                                                        if (Comment.InReplyTo != null)
                                                                                        {
                                                                                            if (!string.IsNullOrWhiteSpace(Comment.InReplyTo.Id))
                                                                                            {
                                                                                                ObjectContent.ParentComment = Comment.InReplyTo.Id;
                                                                                            }
                                                                                        }

                                                                                        string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST body: {StringContent}", StringContent);

                                                                                        HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                                                        string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                                                        if (HttpResponseMessage.IsSuccessStatusCode)
                                                                                        {
                                                                                            Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                                                            Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                ServiceSubscription.ChangeBy = "scheduler";
                                                ServiceSubscription.ChangeDate = DateTime.UtcNow;
                                                ServiceSubscription.SubscriptionDateStart = ServiceSubscription.SubscriptionDateEnd;
                                                ServiceSubscription.SubscriptionDateEnd = DateTime.UtcNow.AddMinutes((long)ServiceSubscription.Interval);

                                                DatabaseContext.ServiceSubscription.Update(ServiceSubscription);
                                                DatabaseContext.SaveChanges();

                                                Success = true;
                                            }
                                        }
                                        break;

                                    case "GOOGLE-PLAYSTORE":
                                        if (!string.IsNullOrWhiteSpace(ServiceSubscription.Account))
                                        {
                                            ServiceToken ServiceToken = await DatabaseContext.ServiceToken.FirstOrDefaultAsync(DataRow => DataRow.Account == ServiceSubscription.Account && DataRow.Type == "PLAYSTORE" && DataRow.Status == "ACTIVO");

                                            if (ServiceToken != null)
                                            {
                                                string HttpEndpoint = $"https://www.googleapis.com/androidpublisher/v3/applications/{ServiceSubscription.Node}/reviews?access_token={ServiceToken.AccessToken}&maxResults=80";

                                                Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                                                using HttpClient HttpClient = new HttpClient();

                                                HttpResponseMessage HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                if (HttpResponseMessage.IsSuccessStatusCode)
                                                {
                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                                                    GoogleReview GoogleReview = JsonConvert.DeserializeObject<GoogleReview>(HttpResponseContent);

                                                    if (GoogleReview != null)
                                                    {
                                                        if (GoogleReview.Reviews != null)
                                                        {
                                                            foreach (var GoogleReviewReview in GoogleReview.Reviews)
                                                            {
                                                                foreach (var GoogleReviewReviewComment in GoogleReviewReview.Comments)
                                                                {
                                                                    if (GoogleReviewReviewComment.UserComment != null)
                                                                    {
                                                                        if (GoogleReviewReviewComment.UserComment.LastModified != null)
                                                                        {
                                                                            DateTime DateComment = DateTimeOffset.FromUnixTimeSeconds(long.Parse(GoogleReviewReviewComment.UserComment.LastModified.Seconds)).UtcDateTime;

                                                                            if (DateComment >= ServiceSubscription.SubscriptionDateStart && DateComment <= ServiceSubscription.SubscriptionDateEnd)
                                                                            {
                                                                                HttpEndpoint = $"{AppSettings.ZyxMeSettings.HookEndpoint}api/appstore/playstorewebhookasync";

                                                                                string ReviewMessage = GoogleReviewReviewComment.UserComment.Text;
                                                                                string StarRating = string.Empty;

                                                                                for (int Counter = 0; Counter < GoogleReviewReviewComment.UserComment.StarRating; Counter++)
                                                                                {
                                                                                    StarRating = $"{StarRating}⭐";
                                                                                }

                                                                                ReviewMessage = $"Rating: {StarRating}{Environment.NewLine}Review: {ReviewMessage?.Replace("\t", string.Empty)}";

                                                                                dynamic ObjectContent = new ExpandoObject();

                                                                                ObjectContent.CommentId = $"{GoogleReviewReview.ReviewId}#ID#{GoogleReviewReviewComment.UserComment.LastModified.Seconds}";
                                                                                ObjectContent.Content = ReviewMessage;
                                                                                ObjectContent.PersonId = GoogleReviewReview.ReviewId;
                                                                                ObjectContent.PersonName = GoogleReviewReview.AuthorName;
                                                                                ObjectContent.PostId = ServiceSubscription.Node;
                                                                                ObjectContent.SiteId = ServiceSubscription.Account;

                                                                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                                                                Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                                                Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST body: {StringContent}", StringContent);

                                                                                HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                                                HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                                                if (HttpResponseMessage.IsSuccessStatusCode)
                                                                                {
                                                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                                                                }
                                                                                else
                                                                                {
                                                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                    ServiceSubscription.ChangeBy = "scheduler";
                                                    ServiceSubscription.ChangeDate = DateTime.UtcNow;
                                                    ServiceSubscription.SubscriptionDateStart = ServiceSubscription.SubscriptionDateEnd;
                                                    ServiceSubscription.SubscriptionDateEnd = DateTime.UtcNow.AddMinutes((long)ServiceSubscription.Interval);

                                                    DatabaseContext.ServiceSubscription.Update(ServiceSubscription);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                                else
                                                {
                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http GET response: {HttpResponseContent}", HttpResponseContent);

                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http GET: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                }
                                            }
                                        }
                                        break;

                                    case "GOOGLE-YOUTUBE":
                                        if (!string.IsNullOrWhiteSpace(ServiceSubscription.Account))
                                        {
                                            ServiceToken ServiceToken = await DatabaseContext.ServiceToken.FirstOrDefaultAsync(DataRow => DataRow.Account == ServiceSubscription.Account && DataRow.Type == "GOOGLE" && DataRow.Status == "ACTIVO");

                                            if (ServiceToken != null)
                                            {
                                                GoogleCredential UserCredential = GoogleCredential.FromAccessToken(ServiceToken.AccessToken);

                                                YouTubeService YouTubeServiceClient = new YouTubeService(new BaseClientService.Initializer()
                                                {
                                                    HttpClientInitializer = UserCredential
                                                });

                                                CommentThreadsResource.ListRequest ListRequest = YouTubeServiceClient.CommentThreads.List("snippet,replies");

                                                ListRequest.AllThreadsRelatedToChannelId = ServiceSubscription.Node;
                                                ListRequest.MaxResults = 80;
                                                ListRequest.Order = CommentThreadsResource.ListRequest.OrderEnum.Time;

                                                CommentThreadListResponse CommentThreadListResponse = ListRequest.Execute();

                                                if (CommentThreadListResponse != null)
                                                {
                                                    if (CommentThreadListResponse.Items != null)
                                                    {
                                                        foreach (var CommentThread in CommentThreadListResponse.Items)
                                                        {
                                                            string SourceId = string.Empty;

                                                            if (!string.IsNullOrWhiteSpace(CommentThread.Snippet.ChannelId))
                                                            {
                                                                SourceId = $"ORIGINATEDFROMCHANNEL:{CommentThread.Snippet.ChannelId}";
                                                            }

                                                            if (!string.IsNullOrWhiteSpace(CommentThread.Snippet.VideoId))
                                                            {
                                                                SourceId = CommentThread.Snippet.VideoId;
                                                            }

                                                            if (ServiceSubscription.Node != CommentThread.Snippet.TopLevelComment.Snippet.AuthorChannelId.Value)
                                                            {
                                                                DateTime DateComment = DateTime.Parse(CommentThread.Snippet.TopLevelComment.Snippet.PublishedAtRaw).ToUniversalTime();

                                                                if (DateComment >= ServiceSubscription.SubscriptionDateStart && DateComment <= ServiceSubscription.SubscriptionDateEnd)
                                                                {
                                                                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.HookEndpoint}api/youtube/webhookasync";

                                                                    using HttpClient HttpClient = new HttpClient();

                                                                    dynamic ObjectContent = new ExpandoObject();

                                                                    ObjectContent.CommentId = CommentThread.Snippet.TopLevelComment.Id;
                                                                    ObjectContent.Content = CommentThread.Snippet.TopLevelComment.Snippet.TextDisplay;
                                                                    ObjectContent.PersonAvatar = CommentThread.Snippet.TopLevelComment.Snippet.AuthorProfileImageUrl;
                                                                    ObjectContent.PersonId = CommentThread.Snippet.TopLevelComment.Snippet.AuthorChannelId.Value;
                                                                    ObjectContent.PersonName = CommentThread.Snippet.TopLevelComment.Snippet.AuthorDisplayName;
                                                                    ObjectContent.PostId = SourceId;
                                                                    ObjectContent.SiteId = $"{ServiceSubscription.Account}&%YOUT%&{ServiceSubscription.Node}";

                                                                    string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                                    Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST body: {StringContent}", StringContent);

                                                                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                                                    {
                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                                                    }
                                                                    else
                                                                    {
                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                                        Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                                    }
                                                                }
                                                            }

                                                            if (CommentThread.Replies != null)
                                                            {
                                                                foreach (var Comment in CommentThread.Replies.Comments)
                                                                {
                                                                    if (ServiceSubscription.Node != Comment.Snippet.AuthorChannelId.Value)
                                                                    {
                                                                        DateTime DateComment = DateTime.Parse(Comment.Snippet.PublishedAtRaw).ToUniversalTime();

                                                                        if (DateComment >= ServiceSubscription.SubscriptionDateStart && DateComment <= ServiceSubscription.SubscriptionDateEnd)
                                                                        {
                                                                            string HttpEndpoint = $"{AppSettings.ZyxMeSettings.HookEndpoint}api/youtube/webhookasync";

                                                                            using HttpClient HttpClient = new HttpClient();

                                                                            dynamic ObjectContent = new ExpandoObject();

                                                                            ObjectContent.CommentId = Comment.Id;
                                                                            ObjectContent.Content = Comment.Snippet.TextDisplay;
                                                                            ObjectContent.PersonAvatar = Comment.Snippet.AuthorProfileImageUrl;
                                                                            ObjectContent.PersonId = Comment.Snippet.AuthorChannelId.Value;
                                                                            ObjectContent.PersonName = Comment.Snippet.AuthorDisplayName;
                                                                            ObjectContent.PostId = SourceId;
                                                                            ObjectContent.ReplyId = CommentThread.Snippet.TopLevelComment.Id;
                                                                            ObjectContent.SiteId = $"{ServiceSubscription.Account}&%YOUT%&{ServiceSubscription.Node}";

                                                                            string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                                                            Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                                            Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST body: {StringContent}", StringContent);

                                                                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                                                            {
                                                                                Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                                                            }
                                                                            else
                                                                            {
                                                                                Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                                                Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                ServiceSubscription.ChangeBy = "scheduler";
                                                ServiceSubscription.ChangeDate = DateTime.UtcNow;
                                                ServiceSubscription.SubscriptionDateStart = ServiceSubscription.SubscriptionDateEnd;
                                                ServiceSubscription.SubscriptionDateEnd = DateTime.UtcNow.AddMinutes((long)ServiceSubscription.Interval);

                                                DatabaseContext.ServiceSubscription.Update(ServiceSubscription);
                                                DatabaseContext.SaveChanges();

                                                Success = true;
                                            }
                                        }
                                        break;
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessCommentCheck: {ServiceSubscription.ServiceSubscriptionId}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                    else
                    {
                        Success = true;
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessCommentCheck").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessConversationCheck(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "ConversationCheck");

            bool Success = false;

            try
            {
                string MessageSubject = $"Laraigo Conversation Check | Date: [{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1):yyyy-MM-dd}] | Environment: [{AppSettings.ZyxMeSettings.ApiServicesEndpoint.Split("/")[2].ToUpper()}]";
                string MessageBody = string.Empty;

                using DatabaseContext DatabaseContext = new DatabaseContext();

                DateTime DateNow = DateTime.UtcNow;

                if (AppSettings.GeneralSettings != null)
                {
                    if (AppSettings.GeneralSettings.UseLocalTime)
                    {
                        DateNow = DateTime.Now;
                    }
                }

                DateNow = DateNow.AddDays(-1);

                DatabaseContext.Database.SetCommandTimeout(7200);

                List<ConversationData> ConversationDataList = await DatabaseContext.ConversationData.FromSqlRaw(StoredProcedure.ConversationOverviewSelect, DateNow.Date, TaskBody.Offset).ToListAsync();

                Logger.ForContext("Context", "ProcessConversationCheck").Debug("Conversation list: {@ConversationDataList}", ConversationDataList);

                if (ConversationDataList != null)
                {
                    if (ConversationDataList.Count > 0)
                    {
                        MessageBody = $"{MessageBody}<table border = \"1\"><tr><th><b>Cantidad de Conversaciones</b></th><th><b>Corporación</b></th><th><b>Organización</b></th><th><b>Canal</b></th><th><b>Tipo</b></th><th><b>Identificador</b></th><th><b>Fecha</b></th></tr>";

                        foreach (var ConversationData in ConversationDataList)
                        {
                            MessageBody = $"{MessageBody}<tr><td><center>{ConversationData.ConversationNumber}</center></td><td><center>{ConversationData.Corporation}</center></td><td><center>{ConversationData.Organization}</center></td><td><center>{ConversationData.CommunicationChannelDescription}</center></td><td><center>{ConversationData.Type}</center></td><td><center>{ConversationData.CommunicationChannelType}</center></td><td><center>{ConversationData.StartDate:yyyy-MM-dd}</center></td></tr>";
                        }

                        MessageBody = $"{MessageBody}</table>";
                    }
                }

                if (!string.IsNullOrWhiteSpace(MessageBody))
                {
                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                    using HttpClient HttpClient = new HttpClient();

                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    SendMailBody ObjectContent = new SendMailBody()
                    {
                        MailAddress = TaskBody.Receiver,
                        MailBlindAddress = TaskBody.BlindReceiver,
                        MailBody = MessageBody,
                        MailCopyAddress = TaskBody.CopyReceiver,
                        MailTitle = MessageSubject
                    };

                    string StringContent = JsonConvert.SerializeObject(ObjectContent);

                    Logger.ForContext("Context", "ProcessConversationCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                    Logger.ForContext("Context", "ProcessConversationCheck").Debug("Http POST body: {StringContent}", StringContent);

                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                        if (BridgeResponse.Success)
                        {
                            Logger.ForContext("Context", "ProcessConversationCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            Success = true;
                        }
                        else
                        {
                            Logger.ForContext("Context", "ProcessConversationCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            MailService.SendMail(AppSettings, Logger, ObjectContent);
                        }
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessConversationCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Logger.ForContext("Context", "ProcessConversationCheck").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                        MailService.SendMail(AppSettings, Logger, ObjectContent);
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessConversationCheck").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessDatabaseCheck(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "DatabaseCheck");

            bool Success = false;

            try
            {
                EamContext EamContext = new EamContext();

                List<SessionInformationGeneral> SessionInformationGeneralList = await EamContext.SessionInformationGeneral.FromSqlRaw(StoredProcedure.SessionCheckGeneral, TaskBody.Database).ToListAsync();

                long ActiveNumber = 0;
                long IdleNumber = 0;

                string QuerySelected = string.Empty;
                string QueryProblem = string.Empty;

                TimeSpan TimeSpanSelected = TimeSpan.Zero;
                TimeSpan TimeSpanProblem = TimeSpan.Zero;

                if (SessionInformationGeneralList != null)
                {
                    if (SessionInformationGeneralList.Count > 0)
                    {
                        ActiveNumber = SessionInformationGeneralList.Where(DataRow => DataRow.State == "active").Count();
                        IdleNumber = SessionInformationGeneralList.Where(DataRow => DataRow.State == "idle").Count();
                    }

                    foreach (var SessionInformationGeneral in SessionInformationGeneralList)
                    {
                        if (SessionInformationGeneral.State == "active")
                        {
                            if (TaskBody.Offset != 0)
                            {
                                if ((TimeSpan)SessionInformationGeneral.QueryTime >= TimeSpan.FromSeconds((double)TaskBody.Offset))
                                {
                                    QueryProblem = SessionInformationGeneral.Query;
                                    TimeSpanProblem = (TimeSpan)SessionInformationGeneral.QueryTime;

                                    if (TaskBody.TaskOffset != 0)
                                    {
                                        if (TimeSpanProblem >= TimeSpan.FromSeconds((double)TaskBody.TaskOffset))
                                        {
                                            try
                                            {
                                                EamContext.Database.ExecuteSqlRaw("SELECT pg_terminate_backend({0});", SessionInformationGeneral.Pid);
                                            }
                                            catch (Exception Exception)
                                            {
                                                Logger.ForContext("Context", $"ProcessDatabaseCheck: {SessionInformationGeneral.Pid}").Error(Exception, "Exception found:");
                                            }
                                        }
                                    }
                                }
                            }

                            if (SessionInformationGeneral.Query.Contains(TaskBody.Query))
                            {
                                if (TaskBody.Offset != 0)
                                {
                                    if ((TimeSpan)SessionInformationGeneral.QueryTime >= TimeSpan.FromSeconds((double)TaskBody.Offset))
                                    {
                                        QuerySelected = SessionInformationGeneral.Query;
                                        TimeSpanSelected = (TimeSpan)SessionInformationGeneral.QueryTime;

                                        if (TaskBody.TaskOffset != 0)
                                        {
                                            if (TimeSpanSelected >= TimeSpan.FromSeconds((double)TaskBody.TaskOffset))
                                            {
                                                try
                                                {
                                                    EamContext.Database.ExecuteSqlRaw("SELECT pg_terminate_backend({0});", SessionInformationGeneral.Pid);
                                                }
                                                catch (Exception Exception)
                                                {
                                                    Logger.ForContext("Context", $"ProcessDatabaseCheck: {SessionInformationGeneral.Pid}").Error(Exception, "Exception found:");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    QuerySelected = SessionInformationGeneral.Query;
                                    TimeSpanSelected = (TimeSpan)SessionInformationGeneral.QueryTime;

                                    if (TaskBody.TaskOffset != 0)
                                    {
                                        if (TimeSpanSelected >= TimeSpan.FromSeconds((double)TaskBody.TaskOffset))
                                        {
                                            try
                                            {
                                                EamContext.Database.ExecuteSqlRaw("SELECT pg_terminate_backend({0});", SessionInformationGeneral.Pid);
                                            }
                                            catch (Exception Exception)
                                            {
                                                Logger.ForContext("Context", $"ProcessDatabaseCheck: {SessionInformationGeneral.Pid}").Error(Exception, "Exception found:");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (ActiveNumber > TaskBody.HoldingLimit || !string.IsNullOrWhiteSpace(QuerySelected) || !string.IsNullOrWhiteSpace(QueryProblem))
                {
                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                    using HttpClient HttpClient = new HttpClient();

                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    TaskBody.Subject = TaskBody.Subject?.Replace("{{activenumber}}", ActiveNumber.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{date}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy"));
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{datetime}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy HH:mm:ss"));
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{holdinglimit}}", TaskBody.HoldingLimit.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{idlenumber}}", IdleNumber.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{information}}", JsonConvert.SerializeObject(SessionInformationGeneralList));
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{queryproblem}}", QueryProblem.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{queryproblemtimespan}}", TimeSpanProblem.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{queryselected}}", QuerySelected.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{queryselectedtimespan}}", TimeSpanSelected.ToString());

                    TaskBody.Body = TaskBody.Body?.Replace("{{activenumber}}", ActiveNumber.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{date}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy"));
                    TaskBody.Body = TaskBody.Body?.Replace("{{datetime}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy HH:mm:ss"));
                    TaskBody.Body = TaskBody.Body?.Replace("{{holdinglimit}}", TaskBody.HoldingLimit.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{idlenumber}}", IdleNumber.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{information}}", JsonConvert.SerializeObject(SessionInformationGeneralList));
                    TaskBody.Body = TaskBody.Body?.Replace("{{queryproblem}}", QueryProblem.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{queryproblemtimespan}}", TimeSpanProblem.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{queryselected}}", QuerySelected.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{queryselectedtimespan}}", TimeSpanSelected.ToString());

                    SendMailBody SendMailBody = new SendMailBody()
                    {
                        MailAddress = TaskBody.Receiver,
                        MailBlindAddress = TaskBody.BlindReceiver,
                        MailBody = TaskBody.Body,
                        MailCopyAddress = TaskBody.CopyReceiver,
                        MailTitle = TaskBody.Subject
                    };

                    string StringContent = JsonConvert.SerializeObject(SendMailBody);

                    Logger.ForContext("Context", "ProcessDatabaseCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                    Logger.ForContext("Context", "ProcessDatabaseCheck").Debug("Http POST body: {StringContent}", StringContent);

                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        Logger.ForContext("Context", "ProcessDatabaseCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                        if (BridgeResponse.Success)
                        {
                            Success = true;
                        }
                        else
                        {
                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                        }
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessDatabaseCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Logger.ForContext("Context", "ProcessDatabaseCheck").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessDatabaseCheck").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<string> ProcessExecuteCampaign(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody, long TaskSchedulerId)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "ExecuteCampaign");

            string Success = "ERROR";

            try
            {
                List<CampaignData> CampaignDataList = await DatabaseContext.CampaignData.FromSqlRaw(StoredProcedure.CampaignCheckSelect, TaskBody.CampaignId, TaskBody.BatchIndex, TaskSchedulerId).ToListAsync();

                if (CampaignDataList != null)
                {
                    foreach (var CampaignData in CampaignDataList)
                    {
                        long ActivateCampaign = 0;

                        if (CampaignData.Last)
                        {
                            ActivateCampaign = 1;
                        }

                        string HttpEndpoint = $"{AppSettings.ZyxMeSettings.ServicesEndpoint}api/campaign/execute/{TaskBody.CampaignId}/{ActivateCampaign}/{TaskBody.BatchIndex}";

                        using HttpClient HttpClient = new HttpClient();

                        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        Logger.ForContext("Context", $"ProcessExecuteCampaign: {TaskBody.CampaignId}").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                        HttpResponseMessage HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                        string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                        if (HttpResponseMessage.IsSuccessStatusCode)
                        {
                            Logger.ForContext("Context", $"ProcessExecuteCampaign: {TaskBody.CampaignId}").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                            if (!string.IsNullOrWhiteSpace(TaskBody.ExecutionType))
                            {
                                Success = "JUMP";
                            }
                            else
                            {
                                if (!CampaignData.Last)
                                {
                                    Success = "CONTINUE";
                                }
                                else
                                {
                                    Success = "FINISH";
                                }
                            }
                        }
                        else
                        {
                            Logger.ForContext("Context", $"ProcessExecuteCampaign: {TaskBody.CampaignId}").Warning("Http GET response: {HttpResponseContent}", HttpResponseContent);

                            Logger.ForContext("Context", $"ProcessExecuteCampaign: {TaskBody.CampaignId}").Warning("Unsuccessful http GET: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", $"ProcessExecuteCampaign: {TaskBody.CampaignId}").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessExecuteQuery(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "ExecuteQuery");

            bool Success = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(TaskBody.Query) && !string.IsNullOrWhiteSpace(TaskBody.ExecutionType))
                {
                    DatabaseContext.Database.SetCommandTimeout(7200);

                    switch (TaskBody.ExecutionType.ToUpper())
                    {
                        case "SINGLE":

                            string SingleQueryString = TaskBody.Query;

                            SingleQueryString = SingleQueryString?.Replace("{{datenow}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd"));
                            SingleQueryString = SingleQueryString?.Replace("{{datetimenow}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd HH:mm:ss"));
                            SingleQueryString = SingleQueryString?.Replace("{{datetimeyesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                            SingleQueryString = SingleQueryString?.Replace("{{dateyesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd"));

                            DatabaseContext.Database.ExecuteSqlRaw(SingleQueryString);

                            Success = true;
                            break;

                        case "ALL":
                            List<ActiveOrganization> ActiveOrganizationList = await DatabaseContext.ActiveOrganization.FromSqlRaw(StoredProcedure.ActiveOrganizationSelect).ToListAsync();

                            if (ActiveOrganizationList != null)
                            {
                                foreach (var Organization in ActiveOrganizationList)
                                {
                                    string AllQueryString = TaskBody.Query;

                                    AllQueryString = AllQueryString?.Replace("{{corpid}}", Organization.CorpId.ToString());
                                    AllQueryString = AllQueryString?.Replace("{{datenow}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd"));
                                    AllQueryString = AllQueryString?.Replace("{{datetimenow}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd HH:mm:ss"));
                                    AllQueryString = AllQueryString?.Replace("{{datetimeyesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                                    AllQueryString = AllQueryString?.Replace("{{dateyesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd"));
                                    AllQueryString = AllQueryString?.Replace("{{orgid}}", Organization.OrgId.ToString());

                                    DatabaseContext.Database.ExecuteSqlRaw(AllQueryString);

                                    Success = true;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessExecuteQuery").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessGlobalCheck(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "GlobalCheck");

            bool Success = false;

            try
            {
                string MessageSubject = $"Laraigo Global Check | Date: [{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset)}] | Environment: [GLOBAL]";
                string MessageBody = TaskBody.Body;

                bool Error = false;

                if (TaskBody.Check != null)
                {
                    string HttpEndpoint = string.Empty;

                    using HttpClient HttpClient = new HttpClient();

                    foreach (var Check in TaskBody.Check)
                    {
                        MessageBody = $"{MessageBody}<br/><br/><b>================================================================</b>";

                        try
                        {
                            HttpClient.DefaultRequestHeaders.Clear();

                            if (Check.Header != null)
                            {
                                foreach (var Header in Check.Header)
                                {
                                    HttpClient.DefaultRequestHeaders.Add(Header.Key, Header.Value);
                                }
                            }

                            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            HttpEndpoint = Check.Endpoint;

                            MessageBody = $"{MessageBody}<br/><b>Endpoint:</b> {HttpEndpoint}";

                            HttpResponseMessage HttpResponseMessage = new HttpResponseMessage();

                            switch (Check.Method.ToUpper())
                            {
                                case "GET":
                                    Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);
                                    break;

                                case "POST":
                                    Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                    Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Debug("Http POST body: {Body}", Check.Body);

                                    HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(Check.Body, Encoding.UTF8, "application/json"));

                                    MessageBody = $"{MessageBody}<br/><b>Request Body:</b> {Check.Body}";
                                    break;
                            }

                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                            MessageBody = $"{MessageBody}<br/><b>Request Result:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}";
                            MessageBody = $"{MessageBody}<br/><b>Response Content:</b> {HttpResponseContent}";

                            if (HttpResponseMessage.IsSuccessStatusCode)
                            {
                                switch (Check.Method.ToUpper())
                                {
                                    case "GET":
                                        Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);
                                        break;

                                    case "POST":
                                        Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                        break;
                                }

                                if (HttpResponseContent.Contains(Check.ExpectedResult))
                                {
                                    MessageBody = $"{MessageBody}<br/><b>Status:</b> <b style=\"color:green\">OKAY</b>";
                                }
                                else
                                {
                                    MessageBody = $"{MessageBody}<br/><b>Status:</b> <b style=\"color:red\">ERROR</b>";

                                    Error = true;
                                }
                            }
                            else
                            {
                                switch (Check.Method.ToUpper())
                                {
                                    case "GET":
                                        Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Warning("Http GET response: {HttpResponseContent}", HttpResponseContent);
                                        break;

                                    case "POST":
                                        Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                        break;
                                }

                                MessageBody = $"{MessageBody}<br/><b>Status:</b> <b style=\"color:red\">ERROR</b>";

                                switch (Check.Method.ToUpper())
                                {
                                    case "GET":
                                        Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Warning("Unsuccessful http GET: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                        break;

                                    case "POST":
                                        Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                        break;
                                }

                                Error = true;
                            }
                        }
                        catch (Exception Exception)
                        {
                            MessageBody = $"{MessageBody}<br/><b>Status:</b> <b style=\"color:red\">ERROR</b> - {Exception.Message}";

                            Logger.ForContext("Context", $"ProcessGlobalCheck: {TaskBody.Check.IndexOf(Check)}").Error(Exception, "Exception found:");

                            Error = true;
                        }

                        MessageBody = $"{MessageBody}<br/><b>================================================================</b>";
                    }
                }
                else
                {
                    Success = true;
                }

                if (Error)
                {
                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                    using HttpClient HttpClient = new HttpClient();

                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    MessageBody = $"{MessageBody}{TaskBody.Firm}";

                    SendMailBody SendMailBody = new SendMailBody()
                    {
                        MailAddress = TaskBody.Receiver,
                        MailBlindAddress = TaskBody.BlindReceiver,
                        MailBody = MessageBody,
                        MailCopyAddress = TaskBody.CopyReceiver,
                        MailTitle = MessageSubject
                    };

                    string StringContent = JsonConvert.SerializeObject(SendMailBody);

                    Logger.ForContext("Context", "ProcessGlobalCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                    Logger.ForContext("Context", "ProcessGlobalCheck").Debug("Http POST body: {StringContent}", StringContent);

                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                        if (BridgeResponse.Success)
                        {
                            Logger.ForContext("Context", "ProcessGlobalCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            Success = true;
                        }
                        else
                        {
                            Logger.ForContext("Context", "ProcessGlobalCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                        }
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessGlobalCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Logger.ForContext("Context", "ProcessGlobalCheck").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessGlobalCheck").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessHoldingCheck(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "HoldingCheck");

            bool Success = false;

            try
            {
                using DatabaseContext DatabaseContext = new DatabaseContext();

                string MessageSubject = $"Laraigo Holding Check | Date: [{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset)}] | Environment: [{AppSettings.ZyxMeSettings.ApiServicesEndpoint.Split("/")[2].ToUpper()}]";
                string MessageBody = "Se encontraron las siguientes excepciones:<br/><br/>";

                bool Error = false;

                List<OrganizationData> OrganizationDataList = await DatabaseContext.OrganizationData.FromSqlRaw(StoredProcedure.CheckHoldingOrganizationSelect).ToListAsync();

                if (OrganizationDataList != null)
                {
                    if (OrganizationDataList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        foreach (var OrganizationData in OrganizationDataList)
                        {
                            List<HoldingData> HoldingDataList = await DatabaseContext.HoldingData.FromSqlRaw(StoredProcedure.HoldingCountSelect, OrganizationData.CorpId, OrganizationData.OrgId).ToListAsync();

                            if (HoldingDataList != null)
                            {
                                if (HoldingDataList.Count == 0)
                                {
                                    Success = true;
                                }
                                else
                                {
                                    foreach (var HoldingData in HoldingDataList)
                                    {
                                        if (HoldingData.Activos > TaskBody.HoldingLimit)
                                        {
                                            MessageBody = $"{MessageBody}<b>Corporación:</b> {OrganizationData.CorpDescription}<br/><b>Organizacion:</b> {OrganizationData.OrgDescription}<br/><b>Holding Actual:</b> {HoldingData.Activos}<br/><b>Límite:</b> {TaskBody.HoldingLimit}<br/><br/>";

                                            Error = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Success = true;
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }

                if (Error)
                {
                    MessageBody = $"{MessageBody}En el ambiente: <br/><br/><b>API SERVICES:</b> {AppSettings.ZyxMeSettings.ApiServicesEndpoint}<br/><b>APP:</b> {AppSettings.ZyxMeSettings.AppEndpoint}<br/><b>BRIDGE:</b> {AppSettings.ZyxMeSettings.BridgeEndpoint}<br/><b>HOOK:</b> {AppSettings.ZyxMeSettings.HookEndpoint}<br/><b>SERVICES:</b> {AppSettings.ZyxMeSettings.ServicesEndpoint}";

                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                    using HttpClient HttpClient = new HttpClient();

                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    SendMailBody SendMailBody = new SendMailBody()
                    {
                        MailBlindAddress = TaskBody.BlindReceiver,
                        MailCopyAddress = TaskBody.CopyReceiver,
                        MailAddress = TaskBody.Receiver,
                        MailTitle = MessageSubject,
                        MailBody = MessageBody
                    };

                    string StringContent = JsonConvert.SerializeObject(SendMailBody);

                    Logger.ForContext("Context", "ProcessHoldingCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                    Logger.ForContext("Context", "ProcessHoldingCheck").Debug("Http POST body: {StringContent}", StringContent);

                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                        if (BridgeResponse.Success)
                        {
                            Logger.ForContext("Context", "ProcessHoldingCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            Success = true;
                        }
                        else
                        {
                            Logger.ForContext("Context", "ProcessHoldingCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                        }
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessHoldingCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Logger.ForContext("Context", "ProcessHoldingCheck").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessHoldingCheck").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessLeadAutomatizationRules(AppSettings AppSettings, string TaskBody, long CorpId, long OrgId)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "LeadAutomatizationRules");

            bool Success = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(TaskBody))
                {
                    LoadAutomatization LoadAutomatization = JsonConvert.DeserializeObject<LoadAutomatization>(TaskBody);

                    if (LoadAutomatization != null)
                    {
                        dynamic ObjectContent = new ExpandoObject();

                        ObjectContent.data = new ExpandoObject();

                        ObjectContent.data.communicationchannelid = LoadAutomatization.CommunicationChannelId;
                        ObjectContent.data.communicationchanneltype = LoadAutomatization.CommunicationChannelType;
                        ObjectContent.data.corpid = CorpId;
                        ObjectContent.data.hsmtemplateid = LoadAutomatization.HsmTemplateId;
                        ObjectContent.data.hsmtemplatename = LoadAutomatization.HsmTemplateName;
                        ObjectContent.data.listmembers = new List<dynamic>();
                        ObjectContent.data.orgid = OrgId;
                        ObjectContent.data.platformtype = LoadAutomatization.PlatformType;
                        ObjectContent.data.shippingreason = LoadAutomatization.ShippingReason;
                        ObjectContent.data.type = LoadAutomatization.Type;
                        ObjectContent.data.userid = 1;
                        ObjectContent.data.username = "admin";

                        if (LoadAutomatization.PersonJson != null)
                        {
                            LoadAutomatizationPersonJson LoadAutomatizationPersonJson = JsonConvert.DeserializeObject<LoadAutomatizationPersonJson>(JsonConvert.SerializeObject(LoadAutomatization.PersonJson));

                            dynamic ListMember = new ExpandoObject();

                            ListMember.firstname = LoadAutomatizationPersonJson.FirstName;
                            ListMember.lastname = LoadAutomatizationPersonJson.LastName;
                            ListMember.parameters = new List<dynamic>();
                            ListMember.personid = LoadAutomatizationPersonJson.PersonId;
                            ListMember.phone = LoadAutomatization.Phone.Replace("+", string.Empty).Replace(" ", string.Empty);

                            if (LoadAutomatization.Parameters != null)
                            {
                                Dictionary<string, string> DictionaryPerson = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(LoadAutomatization.PersonJson));

                                foreach (var Parameter in LoadAutomatization.Parameters)
                                {
                                    dynamic ParameterData = new ExpandoObject();

                                    ParameterData.name = Parameter.Name;
                                    ParameterData.type = Parameter.Type;

                                    if (Parameter.Variable.ToUpper() == "CUSTOM")
                                    {
                                        ParameterData.text = !string.IsNullOrWhiteSpace(Parameter.Text) ? Parameter.Text : $"{{{{{Parameter.Name}}}}}";
                                    }
                                    else
                                    {
                                        ParameterData.text = !string.IsNullOrWhiteSpace(DictionaryPerson.GetValueOrDefault(Parameter.Variable)) ? DictionaryPerson.GetValueOrDefault(Parameter.Variable) : $"{{{{{Parameter.Name}}}}}";
                                    }

                                    ListMember.parameters.Add(ParameterData);
                                }
                            }

                            ObjectContent.data.listmembers.Add(ListMember);
                        }

                        string StringContent = JsonConvert.SerializeObject(ObjectContent);

                        string HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/ticket/send/hsm/allow";

                        using HttpClient HttpClient = new HttpClient();

                        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        Logger.ForContext("Context", "ProcessLeadAutomatizationRules").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                        Logger.ForContext("Context", "ProcessLeadAutomatizationRules").Debug("Http POST body: {StringContent}", StringContent);

                        HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                        string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                        if (HttpResponseMessage.IsSuccessStatusCode)
                        {
                            ZyxMeResponse ZyxMeResponse = JsonConvert.DeserializeObject<ZyxMeResponse>(HttpResponseContent);

                            if (ZyxMeResponse.Success)
                            {
                                Logger.ForContext("Context", "ProcessLeadAutomatizationRules").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                            }
                            else
                            {
                                Logger.ForContext("Context", "ProcessLeadAutomatizationRules").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);
                            }

                            Success = true;
                        }
                        else
                        {
                            Logger.ForContext("Context", "ProcessLeadAutomatizationRules").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            Logger.ForContext("Context", "ProcessLeadAutomatizationRules").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessLeadAutomatizationRules").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessLeadSendHsm(AppSettings AppSettings, string StringContent, long CorpId, long OrgId)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "LeadSendHsm");

            bool Success = false;

            try
            {
                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/ticket/send/hsm/allow";

                dynamic ObjectContent = new ExpandoObject();

                ObjectContent.data = JsonConvert.DeserializeObject<dynamic>(StringContent);

                ObjectContent.data.corpid = CorpId;
                ObjectContent.data.orgid = OrgId;
                ObjectContent.data.userid = 1;
                ObjectContent.data.username = "admin";

                StringContent = JsonConvert.SerializeObject(ObjectContent);

                using HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Logger.ForContext("Context", "ProcessLeadSendHsm").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", "ProcessLeadSendHsm").Debug("Http POST body: {StringContent}", StringContent);

                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    ZyxMeResponse ZyxMeResponse = JsonConvert.DeserializeObject<ZyxMeResponse>(HttpResponseContent);

                    if (ZyxMeResponse.Success)
                    {
                        Logger.ForContext("Context", "ProcessLeadSendHsm").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessLeadSendHsm").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);
                    }

                    Success = true;
                }
                else
                {
                    Logger.ForContext("Context", "ProcessLeadSendHsm").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", "ProcessLeadSendHsm").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessLeadSendHsm").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessRefreshToken(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "RefreshToken");

            bool Success = false;

            try
            {
                List<ServiceToken> ServiceTokenList = await DatabaseContext.ServiceToken.Where(DataRow => ((DateTime)DataRow.ChangeDate).AddMinutes((long)DataRow.Interval) <= DateTime.UtcNow && DataRow.Status == "ACTIVO").ToListAsync();

                if (ServiceTokenList != null)
                {
                    if (ServiceTokenList.Count > 0)
                    {
                        foreach (var ServiceToken in ServiceTokenList)
                        {
                            try
                            {
                                switch (ServiceToken.Type.ToUpper())
                                {
                                    case "CLARO":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.ExtraData))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = $"{ExtraData.endpoint}security/claro/oauth/token";

                                            Dictionary<string, string> HttpContent = new Dictionary<string, string>() { ["grant_type"] = "password", ["username"] = ExtraData.username, ["password"] = ExtraData.password };

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {@HttpContent}", HttpContent);

                                            using HttpClient HttpClient = new HttpClient();

                                            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ExtraData.basicUsername}:{ExtraData.basicPassword}")));

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new FormUrlEncodedContent(HttpContent));

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                dynamic TokenResponse = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                                if (!string.IsNullOrWhiteSpace((string)TokenResponse.access_token))
                                                {
                                                    ServiceToken.AccessToken = TokenResponse.access_token;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;

                                    case "EVOLTA":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.ExtraData))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = $"{ExtraData.endpoint}oauth2/token";

                                            Dictionary<string, string> HttpContent = new Dictionary<string, string>() { ["grant_type"] = "password", ["username"] = ExtraData.username, ["password"] = ExtraData.password };

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {@HttpContent}", HttpContent);

                                            using HttpClient HttpClient = new HttpClient();

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new FormUrlEncodedContent(HttpContent));

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                dynamic TokenResponse = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                                if (!string.IsNullOrWhiteSpace((string)TokenResponse.access_token))
                                                {
                                                    ServiceToken.AccessToken = TokenResponse.access_token;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;

                                    case "GOOGLE":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.Account))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = "https://accounts.google.com/o/oauth2/token";

                                            List<string> FormContent = new List<string>() { $"grant_type={Uri.EscapeDataString("refresh_token")}", $"refresh_token={Uri.EscapeDataString(ServiceToken.RefreshToken)}", $"client_id={Uri.EscapeDataString((string)ExtraData.clientId)}", $"client_secret={Uri.EscapeDataString((string)ExtraData.clientSecret)}" };

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {FormContent}", JsonConvert.SerializeObject(FormContent));

                                            using HttpRequestMessage HttpRequestMessage = new HttpRequestMessage(new HttpMethod("POST"), HttpEndpoint);

                                            HttpRequestMessage.Content = new StringContent(string.Join("&", FormContent));

                                            HttpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                                            using HttpClient HttpClient = new HttpClient();

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.SendAsync(HttpRequestMessage);

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                dynamic GoogleToken = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                                if (!string.IsNullOrWhiteSpace((string)GoogleToken.access_token))
                                                {
                                                    ServiceToken.AccessToken = GoogleToken.access_token;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;

                                    case "HUBSPOT":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.ExtraData))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = $"{ExtraData.endpoint}oauth/v1/token";

                                            List<string> FormContent = new List<string>() { $"grant_type={Uri.EscapeDataString("refresh_token")}", $"code={Uri.EscapeDataString((string)ExtraData.code)}", $"redirect_uri={Uri.EscapeDataString((string)ExtraData.redirectUri)}", $"client_id={Uri.EscapeDataString((string)ExtraData.clientId)}", $"client_secret={Uri.EscapeDataString((string)ExtraData.clientSecret)}", $"refresh_token={Uri.EscapeDataString(ServiceToken.RefreshToken)}" };

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {FormContent}", JsonConvert.SerializeObject(FormContent));

                                            using HttpRequestMessage HttpRequestMessage = new HttpRequestMessage(new HttpMethod("POST"), HttpEndpoint);

                                            HttpRequestMessage.Content = new StringContent(string.Join("&", FormContent));

                                            HttpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                                            using HttpClient HttpClient = new HttpClient();

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.SendAsync(HttpRequestMessage);

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                dynamic HubSpotToken = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                                if (!string.IsNullOrWhiteSpace((string)HubSpotToken.access_token))
                                                {
                                                    ServiceToken.AccessToken = HubSpotToken.access_token;
                                                    ServiceToken.RefreshToken = HubSpotToken.refresh_token;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;

                                    case "LINKEDIN":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.Account))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = ExtraData.endpoint;

                                            List<string> FormContent = new List<string>() { $"grant_type={Uri.EscapeDataString("refresh_token")}", $"refresh_token={Uri.EscapeDataString(ServiceToken.RefreshToken)}", $"client_id={Uri.EscapeDataString((string)ExtraData.clientId)}", $"client_secret={Uri.EscapeDataString((string)ExtraData.clientSecret)}" };

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {FormContent}", JsonConvert.SerializeObject(FormContent));

                                            using HttpRequestMessage HttpRequestMessage = new HttpRequestMessage(new HttpMethod("POST"), HttpEndpoint);

                                            HttpRequestMessage.Content = new StringContent(string.Join("&", FormContent));

                                            HttpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                                            HttpRequestMessage.Content.Headers.Add("X-Restli-Protocol-Version", "2.0.0");
                                            HttpRequestMessage.Content.Headers.Add("Authorization", $"Bearer {ServiceToken.AccessToken}");

                                            using HttpClient HttpClient = new HttpClient();

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.SendAsync(HttpRequestMessage);

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                LinkedInToken LinkedInToken = JsonConvert.DeserializeObject<LinkedInToken>(HttpResponseContent);

                                                if (!string.IsNullOrWhiteSpace(LinkedInToken.AccessToken))
                                                {
                                                    ServiceToken.AccessToken = LinkedInToken.AccessToken;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;
                                                    ServiceToken.RefreshToken = LinkedInToken.RefreshToken;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;

                                    case "OUTLOOK":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.Account))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = $"{ExtraData.endpoint}token";

                                            using HttpClient HttpClient = new HttpClient();

                                            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                            List<KeyValuePair<string, string>> HttpContent = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("client_secret", ExtraData.clientSecret), new KeyValuePair<string, string>("refresh_token", ServiceToken.RefreshToken), new KeyValuePair<string, string>("redirect_uri", ExtraData.redirectUri), new KeyValuePair<string, string>("grant_type", ExtraData.grantType), new KeyValuePair<string, string>("client_id", ExtraData.clientId), new KeyValuePair<string, string>("scope", ExtraData.scope) };

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {@HttpContent}", HttpContent);

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new FormUrlEncodedContent(HttpContent));

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                OutlookToken OutlookToken = JsonConvert.DeserializeObject<OutlookToken>(HttpResponseContent);

                                                if (OutlookToken.TokenType.ToUpper() == "BEARER")
                                                {
                                                    ServiceToken.AccessToken = OutlookToken.AccessToken;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;
                                                    ServiceToken.RefreshToken = OutlookToken.RefreshToken;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;

                                    case "PLAYSTORE":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.Account))
                                        {
                                            string CredentialPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"Root{Path.DirectorySeparatorChar}credentials{Path.DirectorySeparatorChar}playstore{Path.DirectorySeparatorChar}{ServiceToken.Account}.json");

                                            if (File.Exists(CredentialPath))
                                            {
                                                string GeneratedToken = await CryptoService.GeneratePlayStoreToken(CredentialPath);

                                                if (!string.IsNullOrWhiteSpace(GeneratedToken))
                                                {
                                                    string HttpEndpoint = "https://oauth2.googleapis.com/token";

                                                    List<string> FormContent = new List<string>() { $"grant_type={Uri.EscapeDataString("urn:ietf:params:oauth:grant-type:jwt-bearer")}", $"assertion={Uri.EscapeDataString(GeneratedToken)}" };

                                                    Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                    Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {FormContent}", JsonConvert.SerializeObject(FormContent));

                                                    using HttpRequestMessage HttpRequestMessage = new HttpRequestMessage(new HttpMethod("POST"), HttpEndpoint);

                                                    HttpRequestMessage.Content = new StringContent(string.Join("&", FormContent));

                                                    HttpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                                                    using HttpClient HttpClient = new HttpClient();

                                                    HttpResponseMessage HttpResponseMessage = await HttpClient.SendAsync(HttpRequestMessage);

                                                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                                    {
                                                        Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                        GoogleToken GoogleToken = JsonConvert.DeserializeObject<GoogleToken>(HttpResponseContent);

                                                        if (!string.IsNullOrWhiteSpace(GoogleToken.AccessToken))
                                                        {
                                                            ServiceToken.AccessToken = GoogleToken.AccessToken;
                                                            ServiceToken.ChangeBy = "scheduler";
                                                            ServiceToken.ChangeDate = DateTime.UtcNow;

                                                            DatabaseContext.ServiceToken.Update(ServiceToken);
                                                            DatabaseContext.SaveChanges();

                                                            Success = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                        Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                    }
                                                }
                                            }
                                        }
                                        break;

                                    case "PODERJUDICIAL":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.ExtraData))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = $"{ExtraData.endpoint}initSession";

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                                            using HttpClient HttpClient = new HttpClient();

                                            HttpClient.Timeout = TimeSpan.FromMinutes(60);

                                            HttpRequestMessage HttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, HttpEndpoint);

                                            HttpRequestMessage.Headers.Add("App-Token", (string)ExtraData.appToken);
                                            HttpRequestMessage.Headers.Add("Authorization", $"user_token {(string)ExtraData.userToken}");

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.SendAsync(HttpRequestMessage);

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                dynamic TokenResponse = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                                if (!string.IsNullOrWhiteSpace((string)TokenResponse.session_token))
                                                {
                                                    ServiceToken.AccessToken = TokenResponse.session_token;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;

                                    case "VOXIMPLANT":
                                        if (!string.IsNullOrWhiteSpace(ServiceToken.ExtraData))
                                        {
                                            dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceToken.ExtraData);

                                            string HttpEndpoint = $"https://kit-im-{ExtraData.region}.voximplant.com/api/v3/botService/refreshToken";

                                            FormUrlEncodedContent FormContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("refresh_token", ServiceToken.RefreshToken) });

                                            using HttpClient HttpClient = new HttpClient();

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                            Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST body: {TaskBody}", JsonConvert.SerializeObject(FormContent));

                                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, FormContent);

                                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                            if (HttpResponseMessage.IsSuccessStatusCode)
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                VoximplantAccessToken VoximplantAccessToken = JsonConvert.DeserializeObject<VoximplantAccessToken>(HttpResponseContent);

                                                if (VoximplantAccessToken.Success)
                                                {
                                                    ServiceToken.AccessToken = VoximplantAccessToken.Result.AccessToken;
                                                    ServiceToken.ChangeBy = "scheduler";
                                                    ServiceToken.ChangeDate = DateTime.UtcNow;
                                                    ServiceToken.RefreshToken = VoximplantAccessToken.Result.RefreshToken;

                                                    DatabaseContext.ServiceToken.Update(ServiceToken);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                            }
                                            else
                                            {
                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                            }
                                        }
                                        break;
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessRefreshToken: {ServiceToken.ServiceTokenId}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                    else
                    {
                        Success = true;
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessRefreshToken").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessRelocateFile(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "RelocateFile");

            bool Success = false;

            try
            {
                if (TaskBody.Relocate != null)
                {
                    if (Directory.Exists(TaskBody.Relocate.DirectoryDestination))
                    {
                        DateTime DateTimeNow = DateTime.UtcNow;

                        foreach (var DirectorySource in TaskBody.Relocate.DirectorySourceList)
                        {
                            try
                            {
                                if (Directory.Exists(DirectorySource))
                                {
                                    List<FileInfo> FileInfoList = new List<FileInfo>();

                                    if (TaskBody.Relocate.RecursiveSearch)
                                    {
                                        Queue<string> DirectoryRouteQueue = new Queue<string>();

                                        DirectoryRouteQueue.Enqueue(DirectorySource);

                                        while (DirectoryRouteQueue.Count > 0)
                                        {
                                            string DirectoryRoute = DirectoryRouteQueue.Dequeue();

                                            try
                                            {
                                                DirectoryInfo DirectoryInfo = new DirectoryInfo(DirectoryRoute);

                                                FileInfo[] FileInfoArray = DirectoryInfo.GetFiles(TaskBody.Relocate.FileExtension);

                                                foreach (var FileInfo in FileInfoArray)
                                                {
                                                    if (FileInfo.CreationTime.Date <= DateTimeNow.AddDays(-TaskBody.Relocate.Range).Date && FileInfo.LastWriteTime.Date <= DateTimeNow.AddDays(-TaskBody.Relocate.Range).Date)
                                                    {
                                                        FileInfoList.Add(FileInfo);
                                                    }
                                                }

                                                foreach (var DirectoryInfoFound in DirectoryInfo.GetDirectories())
                                                {
                                                    DirectoryRouteQueue.Enqueue(DirectoryInfoFound.FullName);
                                                }
                                            }
                                            catch (Exception Exception)
                                            {
                                                Logger.ForContext("Context", $"ProcessRelocateFile: {DirectoryRoute}").Error(Exception, "Exception found:");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            DirectoryInfo DirectoryInfo = new DirectoryInfo(DirectorySource);

                                            foreach (var FileInfo in DirectoryInfo.GetFiles($"*.{TaskBody.Relocate.FileExtension}"))
                                            {
                                                if (FileInfo.LastWriteTime.Date <= DateTimeNow.AddDays(-TaskBody.Relocate.Range).Date)
                                                {
                                                    FileInfoList.Add(FileInfo);
                                                }
                                            }
                                        }
                                        catch (Exception Exception)
                                        {
                                            Logger.ForContext("Context", $"ProcessRelocateFile: {DirectorySource}").Error(Exception, "Exception found:");
                                        }
                                    }

                                    foreach (var FileInfo in FileInfoList)
                                    {
                                        try
                                        {
                                            string DestinationPath = $"{TaskBody.Relocate.DirectoryDestination}{Path.DirectorySeparatorChar}";

                                            FileInfo.Refresh();

                                            switch (FileInfo.Directory.Name.ToUpper())
                                            {
                                                case "LOG":
                                                case "LOGS":
                                                case "WWWROOT":
                                                    DestinationPath = $"{DestinationPath}{FileInfo.Directory.Parent.Name.ToUpper()}{Path.DirectorySeparatorChar}";
                                                    break;

                                                default:
                                                    DestinationPath = $"{DestinationPath}{FileInfo.Directory.Name.ToUpper()}{Path.DirectorySeparatorChar}";
                                                    break;
                                            }

                                            if (!Directory.Exists(DestinationPath))
                                            {
                                                Directory.CreateDirectory(DestinationPath);
                                            }

                                            if (File.Exists($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}"))
                                            {
                                                File.Delete($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}");
                                            }

                                            if (TaskBody.Relocate.Copy)
                                            {
                                                FileInfo.CopyTo($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}");
                                            }
                                            else
                                            {
                                                FileInfo.MoveTo($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}");
                                            }

                                            if (TaskBody.Relocate.Compress)
                                            {
                                                if (File.Exists($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}.zip"))
                                                {
                                                    File.Delete($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}.zip");
                                                }

                                                using ZipArchive ZipArchive = ZipFile.Open($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}.zip", ZipArchiveMode.Update);

                                                ZipArchive.CreateEntryFromFile($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}", FileInfo.Name, CompressionLevel.Optimal);

                                                ZipArchive.Dispose();

                                                File.Delete($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}");

                                                if (TaskBody.Relocate.Upload)
                                                {
                                                    await StorageService.StorageUpload(AppSettings, Logger, File.ReadAllBytes($"{DestinationPath}{Path.DirectorySeparatorChar}{FileInfo.Name}.zip"), $"{FileInfo.Name}.zip");
                                                }
                                            }
                                        }
                                        catch (Exception Exception)
                                        {
                                            Logger.ForContext("Context", $"ProcessRelocateFile: {FileInfo.FullName}").Error(Exception, "Exception found:");
                                        }
                                    }
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", "ProcessRelocateFile").Error(Exception, "Exception found:");
                            }
                        }

                        Success = true;
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessRelocateFile").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendAutomaticHsm(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendAutomaticHsm");

            bool Success = false;

            try
            {
                if (TaskBody.Ftp != null)
                {
                    FtpClient FtpClient = new FtpClient(TaskBody.Ftp.Host)
                    {
                        Credentials = new NetworkCredential(TaskBody.Ftp.Username, TaskBody.Ftp.Password)
                    };

                    FtpClient.Connect();

                    if (true)
                    {
                        string BearerToken = string.Empty;

                        string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processauthentication/generatebearertoken";

                        dynamic ObjectContent = new ExpandoObject();

                        ObjectContent.username = TaskBody.Bridge.Username;
                        ObjectContent.password = TaskBody.Bridge.Password;

                        string StringContent = JsonConvert.SerializeObject(ObjectContent);

                        Logger.ForContext("Context", "ProcessSendAutomaticHsm").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                        Logger.ForContext("Context", "ProcessSendAutomaticHsm").Debug("Http POST body: {StringContent}", StringContent);

                        using HttpClient HttpClient = new HttpClient();

                        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                        string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                        if (HttpResponseMessage.IsSuccessStatusCode)
                        {
                            Logger.ForContext("Context", "ProcessSendAutomaticHsm").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            dynamic Output = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                            BearerToken = Output.bearerToken;
                        }
                        else
                        {
                            Logger.ForContext("Context", "ProcessSendAutomaticHsm").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            Logger.ForContext("Context", "ProcessSendAutomaticHsm").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                        }

                        if (!string.IsNullOrWhiteSpace(BearerToken))
                        {
                            using OdooContext OdooContext = new OdooContext();

                            foreach (var Item in FtpClient.GetListing("/pdfs/"))
                            {
                                if (Item.Type == FtpObjectType.File)
                                {
                                    if (Path.GetExtension(Item.Name).ToUpper().Contains("PDF"))
                                    {
                                        string FileName = Path.GetFileNameWithoutExtension(Item.Name);

                                        if (!string.IsNullOrWhiteSpace(FileName))
                                        {
                                            List<OdooOrder> OdooOrderList = await OdooContext.OdooOrder.FromSqlRaw(StoredProcedure.SelectAutomaticCampaign, FileName).ToListAsync();

                                            bool HsmSuccess = false;

                                            if (OdooOrderList != null)
                                            {
                                                foreach (var OdooOrder in OdooOrderList)
                                                {
                                                    List<LaraigoConversation> LaraigoConversationList = await DatabaseContext.LaraigoConversation.FromSqlRaw(StoredProcedure.SelectLaraigoConversation, OdooOrder.CorpId, OdooOrder.OrgId, OdooOrder.ConversationId).ToListAsync();

                                                    if (LaraigoConversationList != null)
                                                    {
                                                        foreach (var LaraigoConversation in LaraigoConversationList)
                                                        {
                                                            List<LaraigoPerson> LaraigoPersonList = await DatabaseContext.LaraigoPerson.FromSqlRaw(StoredProcedure.SelectLaraigoPerson, OdooOrder.CorpId, OdooOrder.OrgId, LaraigoConversation.PersonId).ToListAsync();

                                                            if (LaraigoPersonList != null)
                                                            {
                                                                foreach (var LaraigoPerson in LaraigoPersonList)
                                                                {
                                                                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processlaraigo/uselaraigomessaging";

                                                                    ObjectContent = new ExpandoObject();

                                                                    ObjectContent.communicationChannelId = LaraigoConversation.CommunicationChannelId;
                                                                    ObjectContent.corpId = OdooOrder.CorpId;
                                                                    ObjectContent.memberList = new List<dynamic>();
                                                                    ObjectContent.orgId = OdooOrder.OrgId;
                                                                    ObjectContent.templateId = TaskBody.TemplateId;
                                                                    ObjectContent.context = FileName;

                                                                    dynamic MemberContent = new ExpandoObject();

                                                                    MemberContent.header = new ExpandoObject();
                                                                    MemberContent.externalId = 1;
                                                                    MemberContent.firstName = LaraigoPerson.FirstName;
                                                                    MemberContent.lastName = LaraigoPerson.LastName;
                                                                    MemberContent.parameterList = new List<dynamic>();
                                                                    MemberContent.phone = LaraigoPerson.Phone;

                                                                    MemberContent.header.filename = Item.Name;
                                                                    MemberContent.header.type = "document";

                                                                    MemoryStream MemoryStream = new MemoryStream();

                                                                    FtpClient.DownloadStream(MemoryStream, Item.FullName);

                                                                    MemberContent.header.data = Convert.ToBase64String(MemoryStream.ToArray());

                                                                    dynamic ParameterContent = new ExpandoObject();

                                                                    ParameterContent.name = "ticket";
                                                                    ParameterContent.text = FileName;

                                                                    MemberContent.parameterList.Add(ParameterContent);

                                                                    ObjectContent.memberList.Add(MemberContent);

                                                                    StringContent = JsonConvert.SerializeObject(ObjectContent);

                                                                    Logger.ForContext("Context", $"ProcessSendAutomaticHsm: {LaraigoPerson.Phone}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                                    Logger.ForContext("Context", $"ProcessSendAutomaticHsm: {LaraigoPerson.Phone}").Debug("Http POST body: {StringContent}", StringContent);

                                                                    using HttpClient HttpClientCampaign = new HttpClient();

                                                                    HttpClientCampaign.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);

                                                                    HttpClientCampaign.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                                                    HttpResponseMessage = await HttpClientCampaign.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                                                    {
                                                                        Logger.ForContext("Context", $"ProcessSendAutomaticHsm: {LaraigoPerson.Phone}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                                        dynamic Output = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                                                        if ((bool)Output.success)
                                                                        {
                                                                            HsmSuccess = true;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        Logger.ForContext("Context", $"ProcessSendAutomaticHsm: {LaraigoPerson.Phone}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                                        Logger.ForContext("Context", $"ProcessSendAutomaticHsm: {LaraigoPerson.Phone}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (HsmSuccess)
                                            {
                                                try
                                                {
                                                    FtpClient.DeleteFile(Item.FullName);
                                                }
                                                catch (Exception Exception)
                                                {
                                                    Logger.ForContext("Context", $"ProcessSendAutomaticHsm - {Item.FullName}").Error(Exception, "Exception found:");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    FtpClient.Disconnect();

                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendAutomaticHsm").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendAztecaReport(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendAztecaReport");

            bool Success = false;

            try
            {
                if (TaskBody.DniList != null)
                {
                    if (TaskBody.DniList.Count > 0)
                    {
                        string TextBody = $"\"DNI\",\"Perfil\",\"Monto\",\"Tasa\",\"Requisitos\",\"Paperless\"{Environment.NewLine}";

                        foreach (var Dni in TaskBody.DniList)
                        {
                            try
                            {
                                string AztecaToken = await GetAztecaToken(AppSettings, Logger);

                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processbancoazteca/getevaluationoffersclientnew";

                                dynamic ObjectContent = new ExpandoObject();

                                ObjectContent.Token = AztecaToken;
                                ObjectContent.Dni = Dni;

                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                using HttpClient HttpClient = new HttpClient();

                                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST body: {StringContent}", StringContent);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    AztecaOferta AztecaOferta = JsonConvert.DeserializeObject<AztecaOferta>(HttpResponseContent);

                                    string DniPerfil = string.Empty;
                                    string Paperless = string.Empty;

                                    bool IsPaperless = false;

                                    switch (AztecaOferta.IdCampania)
                                    {
                                        case 0:
                                            DniPerfil = "RECHAZADO";
                                            break;

                                        case 29:
                                            DniPerfil = "CRÉDITO VELOZ MENSUAL";
                                            IsPaperless = true;
                                            break;

                                        case 49:
                                            DniPerfil = "TRADICIONAL BANCARIZADOS";
                                            IsPaperless = true;
                                            break;

                                        case 50:
                                            DniPerfil = "TRADICIONAL NO BANCARIZADOS";
                                            IsPaperless = true;
                                            break;
                                    }

                                    if (IsPaperless)
                                    {
                                        HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processbancoazteca/getregistereddni";

                                        Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                        Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST body: {StringContent}", StringContent);

                                        HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                        HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                        if (HttpResponseMessage.IsSuccessStatusCode)
                                        {
                                            Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                            AztecaPaperless AztecaPaperless = JsonConvert.DeserializeObject<AztecaPaperless>(HttpResponseContent);

                                            Paperless = "SI";
                                        }
                                        else
                                        {
                                            Logger.ForContext("Context", "ProcessSendAztecaReport").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                            Logger.ForContext("Context", "ProcessSendAztecaReport").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                        }
                                    }

                                    TextBody = $"{TextBody}\"{Dni}\",\"{DniPerfil}\",\"{AztecaOferta.OfferMaxima}\",\"{AztecaOferta.Rate}\",\"{AztecaOferta.MsjRequisitos}\",\"{Paperless}\"{Environment.NewLine}";
                                }
                                else
                                {
                                    Logger.ForContext("Context", "ProcessSendAztecaReport").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", "ProcessSendAztecaReport").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", "ProcessSendAztecaReport").Error(Exception, "Exception found:");
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(TextBody))
                        {
                            using HttpClient HttpClient = new HttpClient();

                            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            string Filename = $"ReporteAzteca{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset):yyyy-MM-dd_HH:mm:ss}.csv";

                            byte[] ByteArray = Encoding.UTF8.GetBytes(TextBody);

                            string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                            SendMailBody SendMailBody = new SendMailBody()
                            {
                                MailBlindAddress = TaskBody.BlindReceiver,
                                MailCopyAddress = TaskBody.CopyReceiver,
                                MailAddress = TaskBody.Receiver,
                                MailAttachmentData = ByteArray,
                                MailAttachmentName = Filename,
                                MailTitle = TaskBody.Subject,
                                MailBody = TaskBody.Body,
                            };

                            string StringContent = JsonConvert.SerializeObject(SendMailBody);

                            SendMailBody.MailAttachmentData = null;

                            string StringContentLog = JsonConvert.SerializeObject(SendMailBody);

                            Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                            Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST body: {StringContent}", StringContentLog);

                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                            if (HttpResponseMessage.IsSuccessStatusCode)
                            {
                                Logger.ForContext("Context", "ProcessSendAztecaReport").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                if (BridgeResponse.Success)
                                {
                                    Success = true;
                                }
                                else
                                {
                                    MailService.SendMail(AppSettings, Logger, SendMailBody);
                                }
                            }
                            else
                            {
                                Logger.ForContext("Context", "ProcessSendAztecaReport").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                Logger.ForContext("Context", "ProcessSendAztecaReport").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                                MailService.SendMail(AppSettings, Logger, SendMailBody);
                            }
                        }
                    }
                    else
                    {
                        Success = true;
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendAztecaReport").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendFtpReport(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendFtpReport");

            bool Success = false;

            try
            {
                if (TaskBody.Ftp != null)
                {
                    if (TaskBody.ReportList != null)
                    {
                        DatabaseContext.Database.SetCommandTimeout(7200);

                        foreach (var Report in TaskBody.ReportList)
                        {
                            switch (Report.Type.ToUpper())
                            {
                                case "CONVERSATION":
                                    if (!string.IsNullOrWhiteSpace(Report.Type))
                                    {
                                        string Query = Report.Query;

                                        Query = Query?.Replace("{{yesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd"));
                                        Query = Query?.Replace("{{today}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd"));

                                        string Name = Report.Name;

                                        Name = Name?.Replace("{{yesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd"));
                                        Name = Name?.Replace("{{today}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd"));

                                        using MemoryStream MemoryStream = new MemoryStream();

                                        using TextWriter TextWriter = new StreamWriter(MemoryStream);

                                        List<ReportCron> ReportCronList = await DatabaseContext.ReportCron.FromSqlRaw(Query).ToListAsync();

                                        TextWriter.WriteLine("\"numeroticket\", \"anio\", \"mes\", \"semana\", \"dia\", \"hora\", \"canal\", \"cliente\", \"cerradopor\", \"tipocierre\", \"fechainicio\", \"horainicio\", \"fechafin\", \"horafin\", \"fechaderivación\", \"horaderivación\", \"fechaprimerainteraccion\", \"horaprimerainteraccion\", \"tmo\", \"tmg\", \"tiemposuspension\", \"tiempopromediorespuestaasesor\", \"firstname\", \"lastname\", \"phone\", \"email\", \"displayname\", \"holdingwaitingtime\", \"tmoasesor\", \"canalpersonatipo\", \"canalpersonareferencia\", \"fechahandoff\", \"fechaprimeraconversacion\", \"fechaultimaconversacion\", \"asesorinicial\", \"asesorfinal\", \"tiempoprimerarespuesta\", \"tiempopromediorespuesta\", \"tiempoprimerarespuestaasesor\", \"tiempopromediorespuestaasesor-2\", \"tiempopromediorespuestapersona\", \"duraciontotal\", \"duracionreal\", \"duracionpausa\", \"estadoconversacion\", \"tipocierre-2\", \"tipification\", \"attentiongroup\", \"tipo de documento\", \"dni\", \"estadoevaluacion\", \"balancetimes\", \"tiempoprimeraasignacion\", \"labels\", \"abandoned\", \"horaticket\", \"asesor\", \"origin\", \"flujo\", \"nombre completo\", \"tipo de documento\", \"número de documento\", \"teléfono alternativo\", \"correo\", \"provincia\", \"distrito\", \"dirección\", \"términos y condiciones\", \"operación\", \"operador actual\", \"plan poseido\", \"plan a renovar\", \"celular a portar/renovar\", \"productos deseados\", \"tipo pago\", \"opcion full claro\", \"opcion negocio\", \"plan seleccionado\", \"cliente movil claro\", \"servicio\", \"plan internet\", \"plan tv\", \"tipo plan\", \"satisfaccion cliente\", \"necesidad cliente\", \"contacto otros operadores\", \"oferta del otro operador\", \"tiempo beneficio\", \"marca celular\", \"caracteristicas equipo\", \"problema señal\", \"lugar inconveniente\", \"frecuencia inconveniente\", \"comentario cliente\"");

                                        foreach (var ReportCron in ReportCronList)
                                        {
                                            if (TaskBody.RemoveAccent)
                                            {
                                                if (!string.IsNullOrWhiteSpace(ReportCron.DisplayName))
                                                {
                                                    ReportCron.DisplayName = NormalizeString(ReportCron.DisplayName);

                                                    if (string.IsNullOrWhiteSpace(ReportCron.DisplayName))
                                                    {
                                                        ReportCron.DisplayName = ReportCron.Phone;
                                                    }
                                                }

                                                if (!string.IsNullOrWhiteSpace(ReportCron.FirstName))
                                                {
                                                    ReportCron.FirstName = NormalizeString(ReportCron.FirstName);

                                                    if (string.IsNullOrWhiteSpace(ReportCron.DisplayName))
                                                    {
                                                        ReportCron.FirstName = ReportCron.Phone;
                                                    }
                                                }

                                                if (!string.IsNullOrWhiteSpace(ReportCron.LastName))
                                                {
                                                    ReportCron.LastName = NormalizeString(ReportCron.LastName);

                                                    if (string.IsNullOrWhiteSpace(ReportCron.DisplayName))
                                                    {
                                                        ReportCron.LastName = ReportCron.Phone;
                                                    }
                                                }

                                                if (!string.IsNullOrWhiteSpace(ReportCron.Cliente))
                                                {
                                                    ReportCron.Cliente = NormalizeString(ReportCron.Cliente);
                                                }
                                            }

                                            string VariableLine = $"\"{ReportCron.NumeroTicket?.Replace("\"", string.Empty)}\", \"{ReportCron.Anio}\", \"{ReportCron.Mes}\", \"{ReportCron.Semana}\", \"{ReportCron.Dia}\", \"{ReportCron.Hora}\", \"{ReportCron.Canal?.Replace("\"", string.Empty)}\", \"{ReportCron.Cliente?.Replace("\"", string.Empty)}\", \"{ReportCron.CerradoPor?.Replace("\"", string.Empty)}\", \"{ReportCron.TipoCierre?.Replace("\"", string.Empty)}\", \"{ReportCron.FechaInicio?.Replace("\"", string.Empty)}\", \"{ReportCron.HoraInicio?.Replace("\"", string.Empty)}\", \"{ReportCron.FechaFin?.Replace("\"", string.Empty)}\", \"{ReportCron.HoraFin?.Replace("\"", string.Empty)}\", \"{ReportCron.FechaDerivacion?.Replace("\"", string.Empty)}\", \"{ReportCron.HoraDerivacion?.Replace("\"", string.Empty)}\", \"{ReportCron.FechaPrimeraInteraccion?.Replace("\"", string.Empty)}\", \"{ReportCron.HoraPrimeraInteraccion?.Replace("\"", string.Empty)}\", \"{ReportCron.Tmo?.Replace("\"", string.Empty)}\", \"{ReportCron.Tmg?.Replace("\"", string.Empty)}\", \"{ReportCron.TiempoSuspension?.Replace("\"", string.Empty)}\", \"{ReportCron.TiempoPromedioRespuestaAsesor?.Replace("\"", string.Empty)}\", \"{ReportCron.FirstName?.Replace("\"", string.Empty)}\", \"{ReportCron.LastName?.Replace("\"", string.Empty)}\", \"{ReportCron.Phone?.Replace("\"", string.Empty)}\", \"{ReportCron.Email?.Replace("\"", string.Empty)}\", \"{ReportCron.DisplayName?.Replace("\"", string.Empty)}\", \"{ReportCron.HoldingWaitingTime?.Replace("\"", string.Empty)}\", \"{ReportCron.TmoAsesor?.Replace("\"", string.Empty)}\", \"{ReportCron.CanalPersonaTipo?.Replace("\"", string.Empty)}\", \"{ReportCron.CanalPersonaReferencia?.Replace("\"", string.Empty)}\", \"{ReportCron.FechaHandoff?.Replace("\"", string.Empty)}\", \"{ReportCron.FechaPrimeraConversacion?.Replace("\"", string.Empty)}\", \"{ReportCron.FechaUltimaConversacion?.Replace("\"", string.Empty)}\", \"{ReportCron.AsesorInicial?.Replace("\"", string.Empty)}\", \"{ReportCron.AsesorFinal?.Replace("\"", string.Empty)}\", \"{ReportCron.TiempoPrimeraRespuesta?.Replace("\"", string.Empty)}\", \"{ReportCron.TiempoPromedioRespuesta?.Replace("\"", string.Empty)}\", \"{ReportCron.TiempoPrimeraRespuestAasesor?.Replace("\"", string.Empty)}\", \"{ReportCron.TiempoPromedioRespuestaAsesor2?.Replace("\"", string.Empty)}\", \"{ReportCron.TiempoPromedioRespuestaPersona?.Replace("\"", string.Empty)}\", \"{ReportCron.DuracionTotal?.Replace("\"", string.Empty)}\", \"{ReportCron.DuracionReal?.Replace("\"", string.Empty)}\", \"{ReportCron.DuracionPausa?.Replace("\"", string.Empty)}\", \"{ReportCron.EstadoConversacion?.Replace("\"", string.Empty)}\", \"{ReportCron.TipoCierre2?.Replace("\"", string.Empty)}\", \"{ReportCron.Tipificacion?.Replace("\"", string.Empty)}\", \"{ReportCron.AttentionGroup?.Replace("\"", string.Empty)}\", \"{ReportCron.TipoDocumento?.Replace("\"", string.Empty)}\", \"{ReportCron.Dni?.Replace("\"", string.Empty)}\", \"{ReportCron.EstadoEvaluacion?.Replace("\"", string.Empty)}\", \"{ReportCron.BalanceTimes}\", \"{ReportCron.TiempoPrimerAasignacion?.Replace("\"", string.Empty)}\", \"{ReportCron.Labels?.Replace("\"", string.Empty)}\", \"{ReportCron.Abandoned?.Replace("\"", string.Empty)}\", \"{ReportCron.HoraTicket?.Replace("\"", string.Empty)}\", \"{ReportCron.Asesor?.Replace("\"", string.Empty)}\", \"{ReportCron.Origin?.Replace("\"", string.Empty)}\"";

                                            if (Report.VariableList != null)
                                            {
                                                JObject VariableObject = null;

                                                if (!string.IsNullOrWhiteSpace(ReportCron.VariableContext))
                                                {
                                                    try
                                                    {
                                                        VariableObject = JObject.Parse(ReportCron.VariableContext);
                                                    }
                                                    catch (Exception Exception)
                                                    {
                                                        Logger.ForContext("Context", "ProcessSendFtpReport - JObject").Error(Exception, "Exception found:");
                                                    }
                                                }

                                                if (VariableObject != null)
                                                {
                                                    foreach (var Variable in Report.VariableList)
                                                    {
                                                        string VariableValue = string.Empty;

                                                        try
                                                        {
                                                            try
                                                            {
                                                                VariableValue = VariableObject[Variable]?.ToString();
                                                            }
                                                            catch (Exception Exception)
                                                            {
                                                                Logger.ForContext("Context", "ProcessSendFtpReport - VariableValue Legacy").Error(Exception, "Exception found:");
                                                            }

                                                            if (string.IsNullOrWhiteSpace(VariableValue))
                                                            {
                                                                VariableValue = VariableObject[Variable]?["Value"]?.ToString();
                                                            }
                                                        }
                                                        catch (Exception Exception)
                                                        {
                                                            Logger.ForContext("Context", "ProcessSendFtpReport - VariableValue").Error(Exception, "Exception found:");
                                                        }

                                                        VariableLine = $"{VariableLine}, \"{NormalizeString(VariableValue)}\"";
                                                    }
                                                }
                                            }

                                            TextWriter.WriteLine(VariableLine.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\r\n", string.Empty));
                                        }

                                        TextWriter.Flush();

                                        using MemoryStream OutStream = new MemoryStream();

                                        if (TaskBody.ZipFile)
                                        {
                                            using ZipArchive ZipArchive = new ZipArchive(OutStream, ZipArchiveMode.Create, true);

                                            ZipArchiveEntry ZipArchiveEntry = ZipArchive.CreateEntry(Name.Split("/").Last(), CompressionLevel.Optimal);

                                            using Stream Stream = ZipArchiveEntry.Open();

                                            MemoryStream.Position = 0;

                                            MemoryStream.Seek(0, SeekOrigin.Begin);

                                            MemoryStream.CopyTo(Stream);
                                        }

                                        MemoryStream.Position = 0;
                                        OutStream.Position = 0;

                                        if (true)
                                        {
                                            using SftpClient SftpClient = new SftpClient(TaskBody.Ftp.Host, (int)TaskBody.Ftp.Port, TaskBody.Ftp.Username, TaskBody.Ftp.Password);

                                            SftpClient.Connect();

                                            if (SftpClient.IsConnected)
                                            {
                                                SftpClient.BufferSize = TaskBody.ZipFile ? (uint)MemoryStream.Length : (uint)MemoryStream.Length;

                                                SftpClient.UploadFile(TaskBody.ZipFile ? OutStream : MemoryStream, TaskBody.ZipFile ? $"{Name.Split(".").First()}.zip" : Name);

                                                Success = true;
                                            }
                                        }
                                        else
                                        {
                                            FileStream FileStream = File.Create(TaskBody.ZipFile ? $"{Name.Split(".").First()}.zip" : Name);

                                            if (TaskBody.ZipFile)
                                            {
                                                OutStream.Seek(0, SeekOrigin.Begin);
                                                OutStream.CopyTo(FileStream);
                                            }
                                            else
                                            {
                                                MemoryStream.Seek(0, SeekOrigin.Begin);
                                                MemoryStream.CopyTo(FileStream);
                                            }

                                            FileStream.Close();

                                            Success = true;
                                        }
                                    }
                                    break;

                                case "INTERACTION":
                                    if (!string.IsNullOrWhiteSpace(Report.Type))
                                    {
                                        string Query = Report.Query;

                                        Query = Query?.Replace("{{yesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd"));
                                        Query = Query?.Replace("{{today}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd"));

                                        string Name = Report.Name;

                                        Name = Name?.Replace("{{yesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd"));
                                        Name = Name?.Replace("{{today}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd"));

                                        using MemoryStream MemoryStream = new MemoryStream();

                                        using TextWriter TextWriter = new StreamWriter(MemoryStream);

                                        List<InteractionCron> InteractionCronList = await DatabaseContext.InteractionCron.FromSqlRaw(Query).ToListAsync();

                                        TextWriter.WriteLine("\"N° TICKET\", \"LÍNEA\", \"FECHA LÍNEA\", \"HORA LÍNEA\", \"REMITENTE\", \"TIPO INTERACCION\", \"TEXTO\"");

                                        foreach (var InteractionCron in InteractionCronList)
                                        {
                                            if (TaskBody.RemoveAccent)
                                            {
                                                if (!string.IsNullOrWhiteSpace(InteractionCron.InteractionUser))
                                                {
                                                    InteractionCron.InteractionUser = NormalizeString(InteractionCron.InteractionUser);
                                                }

                                                if (!string.IsNullOrWhiteSpace(InteractionCron.InteractionText))
                                                {
                                                    InteractionCron.InteractionText = NormalizeString(InteractionCron.InteractionText);
                                                }
                                            }

                                            string VariableLine = $"\"{InteractionCron.TicketNum?.Replace("\"", string.Empty)}\", \"{InteractionCron.InteractionId}\", \"{InteractionCron.InteractionDate?.Replace("\"", string.Empty)}\", \"{InteractionCron.InteractionTime?.Replace("\"", string.Empty)}\", \"{InteractionCron.InteractionUser?.Replace("\"", string.Empty)}\", \"{InteractionCron.InteractionType?.Replace("\"", string.Empty)}\", \"{InteractionCron.InteractionText?.Replace("\"", string.Empty)}\"";

                                            TextWriter.WriteLine(VariableLine.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\r\n", string.Empty));
                                        }

                                        TextWriter.Flush();

                                        using MemoryStream OutStream = new MemoryStream();

                                        if (TaskBody.ZipFile)
                                        {
                                            using ZipArchive ZipArchive = new ZipArchive(OutStream, ZipArchiveMode.Create, true);

                                            ZipArchiveEntry ZipArchiveEntry = ZipArchive.CreateEntry(Name.Split("/").Last(), CompressionLevel.Optimal);

                                            using Stream Stream = ZipArchiveEntry.Open();

                                            MemoryStream.Position = 0;

                                            MemoryStream.Seek(0, SeekOrigin.Begin);

                                            MemoryStream.CopyTo(Stream);
                                        }

                                        MemoryStream.Position = 0;
                                        OutStream.Position = 0;

                                        if (true)
                                        {
                                            using SftpClient SftpClient = new SftpClient(TaskBody.Ftp.Host, (int)TaskBody.Ftp.Port, TaskBody.Ftp.Username, TaskBody.Ftp.Password);

                                            SftpClient.Connect();

                                            if (SftpClient.IsConnected)
                                            {
                                                SftpClient.BufferSize = TaskBody.ZipFile ? (uint)MemoryStream.Length : (uint)MemoryStream.Length;

                                                SftpClient.UploadFile(TaskBody.ZipFile ? OutStream : MemoryStream, TaskBody.ZipFile ? $"{Name.Split(".").First()}.zip" : Name);

                                                Success = true;
                                            }
                                        }
                                        else
                                        {
                                            FileStream FileStream = File.Create(TaskBody.ZipFile ? $"{Name.Split(".").First()}.zip" : Name);

                                            if (TaskBody.ZipFile)
                                            {
                                                OutStream.Seek(0, SeekOrigin.Begin);
                                                OutStream.CopyTo(FileStream);
                                            }
                                            else
                                            {
                                                MemoryStream.Seek(0, SeekOrigin.Begin);
                                                MemoryStream.CopyTo(FileStream);
                                            }

                                            FileStream.Close();

                                            Success = true;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendFtpReport").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendHsm(AppSettings AppSettings, TaskData TaskData)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendHsm");

            bool Success = false;

            try
            {
                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.ServicesEndpoint}api/handler/sendhsm";

                using HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Logger.ForContext("Context", "ProcessSendHsm").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", "ProcessSendHsm").Debug("Http POST body: {TaskBody}", TaskData.TaskBody);

                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(JsonConvert.SerializeObject(TaskData.TaskBody), Encoding.UTF8, "application/json"));

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    Logger.ForContext("Context", "ProcessSendHsm").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Success = true;
                }
                else
                {
                    Logger.ForContext("Context", "ProcessSendHsm").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", "ProcessSendHsm").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendHsm").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessStartKpi(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "StartKpi");

            bool Success = false;

            try
            {
                List<KpiData> KpiDataList = await DatabaseContext.KpiData.FromSqlRaw(StoredProcedure.SelectKpiRun, true).ToListAsync();

                if (KpiDataList != null)
                {
                    foreach (var KpiData in KpiDataList)
                    {
                        try
                        {
                            DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.SelectKpiCalc, KpiData.CorpId, KpiData.OrgId, KpiData.KpiId, TaskBody.Username, TaskBody.Task);
                        }
                        catch (Exception Exception)
                        {
                            Logger.ForContext("Context", $"ProcessStartKpi: {KpiData.KpiId}").Error(Exception, "Exception found:");
                        }
                    }
                }

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessStartKpi").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessStartPaymentPending(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "StartPaymentPending");

            bool Success = false;

            try
            {
                List<PaymentPending> PaymentPendingList = await DatabaseContext.PaymentPending.Where(DataRow => DataRow.Status == "ACTIVO" && DataRow.Attempt < TaskBody.AttemptLimit && DataRow.RunDate <= DateTime.UtcNow).ToListAsync();

                if (PaymentPendingList != null)
                {
                    if (PaymentPendingList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        foreach (var PaymentPending in PaymentPendingList)
                        {
                            PaymentPending.Attempt++;
                            PaymentPending.ChangeBy = "SCHEDULER";
                            PaymentPending.ChangeDate = DateTime.UtcNow;
                            PaymentPending.RunDate = ((DateTime)PaymentPending.RunDate).AddHours(TaskBody.HourInterval);

                            DatabaseContext.PaymentPending.Update(PaymentPending);
                            DatabaseContext.SaveChanges();

                            try
                            {
                                dynamic ObjectContent = new ExpandoObject();

                                ObjectContent.corpid = PaymentPending.CorpId;
                                ObjectContent.invoiceid = PaymentPending.InvoiceId;
                                ObjectContent.orgid = PaymentPending.OrgId;

                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                PaymentPending.LastBody = JsonConvert.SerializeObject(StringContent);

                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/payment/automaticpayment";

                                using HttpClient HttpClient = new HttpClient();

                                Logger.ForContext("Context", $"ProcessStartPaymentPending: {PaymentPending.PaymentPendingId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                Logger.ForContext("Context", $"ProcessStartPaymentPending: {PaymentPending.PaymentPendingId}").Debug("Http POST body: {StringContent}", StringContent);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                PaymentPending.LastResponse = HttpResponseContent;

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    Logger.ForContext("Context", $"ProcessStartPaymentPending: {PaymentPending.PaymentPendingId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    PaymentPending.Status = "PAGADO";
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessStartPaymentPending: {PaymentPending.PaymentPendingId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", $"ProcessStartPaymentPending: {PaymentPending.PaymentPendingId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessStartPaymentPending: {PaymentPending.PaymentPendingId}").Error(Exception, "Exception found:");

                                PaymentPending.LastResponse = JsonConvert.SerializeObject(Exception);
                            }

                            DatabaseContext.PaymentPending.Update(PaymentPending);
                            DatabaseContext.SaveChanges();

                            Success = true;
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessStartPaymentPending").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessStartReportScheduler(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "StartReportScheduler");

            bool Success = false;

            try
            {
                List<ReportSchedulerData> ReportSchedulerDataList = await DatabaseContext.ReportSchedulerData.FromSqlRaw(StoredProcedure.SelectReportSchedulerRun, true).ToListAsync();

                if (ReportSchedulerDataList != null)
                {
                    DatabaseContext.Database.SetCommandTimeout(7200);

                    foreach (var ReportSchedulerData in ReportSchedulerDataList)
                    {
                        try
                        {
                            ReportSchedulerDataJson ReportSchedulerDataJson = JsonConvert.DeserializeObject<ReportSchedulerDataJson>(ReportSchedulerData.DataJson);

                            MailAttachment MailAttachment = null;

                            if (!string.IsNullOrWhiteSpace(ReportSchedulerDataJson.Query))
                            {
                                dynamic ReportResult = await DapperService.ExecuteStoredProcedureMultiple<dynamic>(AppSettings, Logger, ReportSchedulerDataJson.Query);

                                if (ReportSchedulerData.ReportName.ToUpper() == "FILTERREPORT_PRODUCTIVITY")
                                {
                                    dynamic ReportRaw = new List<dynamic>();

                                    foreach (var Report in ReportResult)
                                    {
                                        dynamic NewReport = new ExpandoObject();

                                        NewReport.@break = null;
                                        NewReport.asignedtickets = Report.asignedtickets;
                                        NewReport.avgfirstreplytime = Report.avgfirstreplytime;
                                        NewReport.avgtotalasesorduration = Report.avgtotalasesorduration;
                                        NewReport.backoffice = null;
                                        NewReport.bathroom = null;
                                        NewReport.closedtickets = Report.closedtickets;
                                        NewReport.coaching = null;
                                        NewReport.fullname = Report.fullname;
                                        NewReport.groups = Report.groups;
                                        NewReport.infirmary = null;
                                        NewReport.maxfirstreplytime = Report.maxfirstreplytime;
                                        NewReport.maxtotalasesorduration = Report.maxtotalasesorduration;
                                        NewReport.maxtotalduration = Report.maxtotalduration;
                                        NewReport.minfirstreplytime = Report.minfirstreplytime;
                                        NewReport.mintotalasesorduration = Report.mintotalasesorduration;
                                        NewReport.mintotalduration = Report.mintotalduration;
                                        NewReport.suspendedtickets = Report.suspendedtickets;
                                        NewReport.totaltickets = Report.totaltickets;
                                        NewReport.userconnectedduration = Report.userconnectedduration;
                                        NewReport.userstatus = Report.userstatus;
                                        NewReport.usr = Report.usr;

                                        if (!string.IsNullOrWhiteSpace((string)Report.desconectedtimejson))
                                        {
                                            dynamic DesconectedTime = JsonConvert.DeserializeObject<dynamic>((string)Report.desconectedtimejson);

                                            NewReport.@break = DesconectedTime.BREAK;
                                            NewReport.backoffice = DesconectedTime.BACKOFFICE;
                                            NewReport.bathroom = DesconectedTime.BAÑO;
                                            NewReport.coaching = DesconectedTime.COACHING;
                                            NewReport.infirmary = DesconectedTime.ENFERMERÍA;
                                        }

                                        ReportRaw.Add(NewReport);
                                    }

                                    ReportResult = ReportRaw;
                                }

                                string ReportJson = JsonConvert.SerializeObject(ReportResult);

                                if (AppSettings.SpanishDictionary != null)
                                {
                                    foreach (var Dictionary in AppSettings.SpanishDictionary)
                                    {
                                        if (!string.IsNullOrWhiteSpace(Dictionary.Alias) && !string.IsNullOrWhiteSpace(Dictionary.Key))
                                        {
                                            ReportJson = ReportJson?.Replace($"\"{Dictionary.Key}\"", $"\"{Dictionary.Alias}\"");
                                        }
                                    }
                                }

                                DataTable DataTable = (DataTable)JsonConvert.DeserializeObject(ReportJson, typeof(DataTable));

                                using XLWorkbook XLWorkbook = new XLWorkbook();

                                IXLWorksheet XLWorksheet = XLWorkbook.Worksheets.Add(string.Concat(ReportSchedulerData.Title.Split(Path.GetInvalidFileNameChars()))[..Math.Min(30, string.Concat(ReportSchedulerData.Title.Split(Path.GetInvalidFileNameChars())).Length)]);

                                XLWorksheet.FirstCell().InsertTable(DataTable, false);

                                XLWorksheet.Rows().Height = 15;
                                XLWorksheet.RangeUsed().SetAutoFilter();
                                XLWorksheet.Style.Alignment.WrapText = false;

                                using MemoryStream MemoryStream = new MemoryStream();

                                XLWorkbook.SaveAs(MemoryStream);

                                MailAttachment = new MailAttachment()
                                {
                                    Name = $"{string.Concat(ReportSchedulerData.Title.Split(Path.GetInvalidFileNameChars()))}-{DateTime.UtcNow:yyyy-MM-dd HH-mm-ss}.xlsx",
                                    Type = "DATA",
                                    Value = Convert.ToBase64String(MemoryStream.ToArray())
                                };
                            }
                            else
                            {
                                ReportSchedulerBody ReportSchedulerBody = new ReportSchedulerBody()
                                {
                                    Parameters = new ReportSchedulerParameters()
                                    {
                                        FormatToExport = "excel",
                                        Offset = long.Parse(ReportSchedulerData.TimeZoneOffset),
                                        ReportName = ReportSchedulerDataJson.Description
                                    },
                                    User = new ReportSchedulerUser()
                                    {
                                        Corpid = (long)ReportSchedulerData.CorpId,
                                        Orgid = (long)ReportSchedulerData.OrgId,
                                        Userid = (long)ReportSchedulerData.UserId,
                                        Usr = ReportSchedulerData.Usr
                                    }
                                };

                                if (!string.IsNullOrWhiteSpace(ReportSchedulerDataJson.ColumnJson))
                                {
                                    ReportSchedulerBody.Columns = JsonConvert.DeserializeObject<List<ReportSchedulerColumnJson>>(ReportSchedulerDataJson.ColumnJson);
                                }

                                if (!string.IsNullOrWhiteSpace(ReportSchedulerDataJson.SummaryJson))
                                {
                                    ReportSchedulerBody.Summaries = JsonConvert.DeserializeObject<dynamic>(ReportSchedulerDataJson.SummaryJson);
                                }

                                if (!string.IsNullOrWhiteSpace(ReportSchedulerDataJson.FilterJson))
                                {
                                    ReportSchedulerBody.Filters = JsonConvert.DeserializeObject<List<ReportSchedulerFilterJson>>(ReportSchedulerDataJson.FilterJson);
                                }

                                if (ReportSchedulerBody.Columns != null)
                                {
                                    ReportSchedulerBody.Parameters.HeaderClient = new List<ReportSchedulerParametersHeader>();

                                    foreach (var Column in ReportSchedulerBody.Columns)
                                    {
                                        ReportSchedulerParametersHeader ReportSchedulerParametersHeader = new ReportSchedulerParametersHeader()
                                        {
                                            Alias = Column.Alias,
                                            Key = Column.Columnname?.Replace(".", string.Empty)
                                        };

                                        ReportSchedulerBody.Parameters.HeaderClient.Add(ReportSchedulerParametersHeader);
                                    }
                                }

                                ReportSchedulerBody.Filters ??= new List<ReportSchedulerFilterJson>();

                                if (ReportSchedulerBody.Filters != null)
                                {
                                    if (ReportSchedulerBody.Filters.FirstOrDefault(DataRow => DataRow.Columnname == "conversation.createdate") == null)
                                    {
                                        ReportSchedulerBody.Filters.Add(new ReportSchedulerFilterJson()
                                        {
                                            Columnname = "conversation.createdate",
                                            Description = "conversation.createdate",
                                            End = ReportSchedulerData.EndDate,
                                            Join_alias = string.Empty,
                                            Join_on = string.Empty,
                                            Join_table = string.Empty,
                                            Start = ReportSchedulerData.StartDate,
                                            Type = "timestamp without time zone",
                                            TypeFilter = string.Empty,
                                            Value = string.Empty
                                        });
                                    }

                                    foreach (var Filter in ReportSchedulerBody.Filters)
                                    {
                                        if (Filter.Columnname == "conversation.createdate")
                                        {
                                            Filter.Start = ReportSchedulerData.StartDate;
                                            Filter.End = ReportSchedulerData.EndDate;
                                        }

                                        try
                                        {
                                            Dictionary<string, string> FilterList = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReportSchedulerData.FilterJson);

                                            if (!string.IsNullOrWhiteSpace(FilterList.GetValueOrDefault(Filter.Columnname)))
                                            {
                                                if (Filter.Type.ToUpper().Contains("TIMESTAMP"))
                                                {
                                                    Filter.Start = FilterList.GetValueOrDefault(Filter.Columnname);
                                                    Filter.End = FilterList.GetValueOrDefault(Filter.Columnname);
                                                    Filter.Value = string.Empty;
                                                }
                                                else
                                                {
                                                    Filter.Start = string.Empty;
                                                    Filter.End = string.Empty;
                                                    Filter.Value = FilterList.GetValueOrDefault(Filter.Columnname);
                                                }
                                            }
                                        }
                                        catch (Exception Exception)
                                        {
                                            Console.WriteLine(JsonConvert.SerializeObject(Exception));
                                        }
                                    }

                                    List<ReportSchedulerFilterJson> FilterBackup = new List<ReportSchedulerFilterJson>();

                                    foreach (var Filter in ReportSchedulerBody.Filters)
                                    {
                                        if (Filter.Type.ToUpper().Contains("TIMESTAMP"))
                                        {
                                            if (!string.IsNullOrWhiteSpace(Filter.Start) && !string.IsNullOrWhiteSpace(Filter.End))
                                            {
                                                FilterBackup.Add(Filter);
                                            }
                                        }
                                        else
                                        {
                                            FilterBackup.Add(Filter);
                                        }
                                    }

                                    ReportSchedulerBody.Filters = FilterBackup;
                                }

                                string StringContent = JsonConvert.SerializeObject(ReportSchedulerBody);

                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/reportdesigner/exporttask";

                                using HttpClient HttpClient = new HttpClient();

                                HttpClient.Timeout = TimeSpan.FromSeconds(7200);

                                Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Debug("Http POST body: {StringContent}", StringContent);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    dynamic ReportResponse = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                    string ReportUrl = ReportResponse.url;

                                    MailAttachment = new MailAttachment()
                                    {
                                        Type = "URL",
                                        Value = ReportUrl
                                    };
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                                    MailAttachment = new MailAttachment()
                                    {
                                        Name = "FAIL.csv",
                                        Type = "DATA",
                                        Value = string.Empty
                                    };
                                }
                            }

                            if (MailAttachment != null)
                            {
                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                                using HttpClient HttpClient = new HttpClient();

                                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                SendMailBody SendMailBody = new SendMailBody()
                                {
                                    Attachments = new List<MailAttachment>()
                                    {
                                        MailAttachment
                                    },
                                    MailAddress = ReportSchedulerData.MailTo,
                                    MailBody = ReportSchedulerData.MailBody,
                                    MailCopyAddress = ReportSchedulerData.MailCc,
                                    MailTitle = ReportSchedulerData.MailSubject
                                };

                                string StringContent = JsonConvert.SerializeObject(SendMailBody);

                                if (SendMailBody.Attachments != null)
                                {
                                    foreach (var Attachment in SendMailBody.Attachments)
                                    {
                                        if (Attachment.Type == "DATA")
                                        {
                                            Attachment.Value = null;
                                        }
                                    }
                                }

                                string StringContentLog = JsonConvert.SerializeObject(SendMailBody);

                                Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Debug("Http POST body: {StringContent}", StringContentLog);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                    if (BridgeResponse.Success)
                                    {
                                        Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                        Success = true;
                                    }
                                    else
                                    {
                                        Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                    }
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                }
                            }
                        }
                        catch (Exception Exception)
                        {
                            Logger.ForContext("Context", $"ProcessStartReportScheduler: {ReportSchedulerData.ReportSchedulerId}").Error(Exception, "Exception found:");
                        }

                        DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.SelectReportSchedulerCalc, ReportSchedulerData.CorpId, ReportSchedulerData.OrgId, ReportSchedulerData.ReportSchedulerId, TaskBody.Username, TaskBody.Task);
                    }
                }

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessStartReportScheduler").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessStatusCheck(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "StatusCheck");

            bool Success = false;

            try
            {
                string MailSubject = $"Laraigo Status Check | Date: [{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset)}] | Environment: [{AppSettings.ZyxMeSettings.ApiServicesEndpoint.Split("/")[2].ToUpper()}]";

                string HttpEndpoint = string.Empty;

                string MessageBody = string.Empty;

                bool Error = false;

                using HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage HttpResponseMessage = new HttpResponseMessage();

                string HttpResponseContent = string.Empty;

                try
                {
                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.ApiServicesEndpoint}api/values";

                    MessageBody = $"{MessageBody}Revisión de ambientes:<br/><br/><b>LARAIGO API SERVICES</b><br/><br/><b>- General Endpoint:</b> {HttpEndpoint}<br/>";

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                    }
                    else
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                        Error = true;
                    }

                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.ApiServicesEndpoint}api/values/isconnectedbd";

                    MessageBody = $"{MessageBody}<b>- Database Endpoint:</b> {HttpEndpoint}<br/>";

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                    }
                    else
                    {
                        MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                        Error = true;
                    }
                }
                catch (Exception Exception)
                {
                    Logger.ForContext("Context", "ProcessStatusCheck").Error(Exception, "Exception found:");

                    MessageBody = $"{MessageBody}<br/><br/><b>- General Status:</b> <b style=\"color:red\">EXCEPTION</b><br/><b>- Exception Found:</b> {Exception.Message}<br/><b>- Exception Detail:</b> {JsonConvert.SerializeObject(Exception)}<br/><br/>";

                    Error = true;
                }

                try
                {
                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/values";

                    MessageBody = $"{MessageBody}<b>LARAIGO BRIDGE</b><br/><br/><b>- General Endpoint:</b> {HttpEndpoint}<br/>";

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                    }
                    else
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                        Error = true;
                    }

                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/values/isconnectedbd";

                    bool BridgeError = false;

                    MessageBody = $"{MessageBody}<b>- Database Endpoint:</b> {HttpEndpoint}<br/>";

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        if (HttpResponseContent.ToUpper().Contains("WORKING WITH DATABASE"))
                        {
                            MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                        }
                        else
                        {
                            BridgeError = true;
                        }
                    }
                    else
                    {
                        BridgeError = true;
                    }

                    if (BridgeError)
                    {
                        MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                        Error = true;
                    }
                }
                catch (Exception Exception)
                {
                    Logger.ForContext("Context", "ProcessStatusCheck").Error(Exception, "Exception found:");

                    MessageBody = $"{MessageBody}<br/><br/><b>- General Status:</b> <b style=\"color:red\">EXCEPTION</b><br/><b>- Exception Found:</b> {Exception.Message}<br/><b>- Exception Detail:</b> {JsonConvert.SerializeObject(Exception)}<br/><br/>";

                    Error = true;
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(AppSettings.ZyxMeSettings.ChatflowApiEndpoint))
                    {
                        HttpEndpoint = $"{AppSettings.ZyxMeSettings.ChatflowApiEndpoint}api/query/getfree";

                        MessageBody = $"{MessageBody}<b>LARAIGO CHATFLOW API</b><br/><br/><b>- Database Endpoint:</b> {HttpEndpoint}<br/>";

                        bool ChatflowError = false;

                        Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                        HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent("{\"method\":\"UFN_CORP_LST\",\"parameters\": {}}", Encoding.UTF8, "application/json"));

                        HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                        Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        if (HttpResponseMessage.IsSuccessStatusCode)
                        {
                            if (HttpResponseContent.ToUpper().Contains("SUCCESS"))
                            {
                                MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                            }
                            else
                            {
                                ChatflowError = true;
                            }
                        }
                        else
                        {
                            ChatflowError = true;
                        }

                        if (ChatflowError)
                        {
                            MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                            Error = true;
                        }
                    }
                }
                catch (Exception Exception)
                {
                    Logger.ForContext("Context", "ProcessStatusCheck").Error(Exception, "Exception found:");

                    MessageBody = $"{MessageBody}<br/><br/><b>- General Status:</b> <b style=\"color:red\">EXCEPTION</b><br/><b>- Exception Found:</b> {Exception.Message}<br/><b>- Exception Detail:</b> {JsonConvert.SerializeObject(Exception)}<br/><br/>";

                    Error = true;
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(AppSettings.ZyxMeSettings.LaraigoEndpoint))
                    {
                        HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/check";

                        MessageBody = $"{MessageBody}<b>LARAIGO API</b><br/><br/><b>- Database Endpoint:</b> {HttpEndpoint}<br/>";

                        bool ChatflowError = false;

                        Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                        HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(string.Empty, Encoding.UTF8, "application/json"));

                        HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                        Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        if (HttpResponseMessage.IsSuccessStatusCode)
                        {
                            if (HttpResponseContent.ToUpper().Contains("SUCCESS"))
                            {
                                MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                            }
                            else
                            {
                                ChatflowError = true;
                            }
                        }
                        else
                        {
                            ChatflowError = true;
                        }

                        if (ChatflowError)
                        {
                            MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                            Error = true;
                        }
                    }
                }
                catch (Exception Exception)
                {
                    Logger.ForContext("Context", "ProcessStatusCheck").Error(Exception, "Exception found:");

                    MessageBody = $"{MessageBody}<br/><br/><b>- General Status:</b> <b style=\"color:red\">EXCEPTION</b><br/><b>- Exception Found:</b> {Exception.Message}<br/><b>- Exception Detail:</b> {JsonConvert.SerializeObject(Exception)}<br/><br/>";

                    Error = true;
                }

                try
                {
                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.HookEndpoint}api/values";

                    MessageBody = $"{MessageBody}<b>LARAIGO HOOK</b><br/><br/><b>- General Endpoint:</b> {HttpEndpoint}<br/>";

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                    }
                    else
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                        Error = true;
                    }
                }
                catch (Exception Exception)
                {
                    Logger.ForContext("Context", "ProcessStatusCheck").Error(Exception, "Exception found:");

                    MessageBody = $"{MessageBody}<br/><br/><b>- General Status:</b> <b style=\"color:red\">EXCEPTION</b><br/><b>- Exception Found:</b> {Exception.Message}<br/><b>- Exception Detail:</b> {JsonConvert.SerializeObject(Exception.StackTrace)}<br/><br/>";

                    Error = true;
                }

                try
                {
                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.ServicesEndpoint}api/values";

                    MessageBody = $"{MessageBody}<b>LARAIGO SERVICES</b></h4><br/><br/><b>- General Endpoint:</b> {HttpEndpoint}<br/>";

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:green\">OKAY</b><br/><br/>";
                    }
                    else
                    {
                        MessageBody = $"{MessageBody}<b>- General Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseContent)}<br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/>";

                        Error = true;
                    }

                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.ServicesEndpoint}api/values/isconnectedbd";

                    bool ServicesError = false;

                    MessageBody = $"{MessageBody}<b>- Database Endpoint:</b> {HttpEndpoint}<br/>";

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                    HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        if (HttpResponseContent.ToUpper().Contains("WORKING WITH DATABASE"))
                        {
                            MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:green\">OKAY";
                        }
                        else
                        {
                            ServicesError = true;
                        }
                    }
                    else
                    {
                        ServicesError = true;
                    }

                    if (ServicesError)
                    {
                        MessageBody = $"{MessageBody}<b>- Database Status:</b> <b style=\"color:red\">ERROR</b><br/><br/><b>-------------------------------------------------------------------------------------------</b><br/><br/><b>- Error Found:</b> {HttpResponseMessage.ReasonPhrase}<br/><b>- Error Content:</b> {HttpResponseContent}<br/><b>- Error Detail:</b> {JsonConvert.SerializeObject(HttpResponseMessage)}<br/><br/><b>-------------------------------------------------------------------------------------------</b>";

                        Error = true;
                    }
                }
                catch (Exception Exception)
                {
                    Logger.ForContext("Context", "ProcessStatusCheck").Error(Exception, "Exception found:");

                    MessageBody = $"{MessageBody}<br/><br/><b>- General Status:</b> <b style=\"color:red\">EXCEPTION</b><br/><b>- Exception Found:</b> {Exception.Message}<br/><b>- Exception Detail:</b> {JsonConvert.SerializeObject(Exception)}";

                    Error = true;
                }

                if (Error)
                {
                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                    SendMailBody SendMailBody = new SendMailBody()
                    {
                        MailAddress = TaskBody.Receiver,
                        MailBlindAddress = TaskBody.BlindReceiver,
                        MailBody = MessageBody,
                        MailCopyAddress = TaskBody.CopyReceiver,
                        MailTitle = MailSubject
                    };

                    string StringContent = JsonConvert.SerializeObject(SendMailBody);

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                    Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http POST body: {StringContent}", StringContent);

                    HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        Logger.ForContext("Context", "ProcessStatusCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                        if (BridgeResponse.Success)
                        {
                            Success = true;
                        }
                        else
                        {
                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                        }
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessStatusCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Logger.ForContext("Context", "ProcessStatusCheck").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessStatusCheck").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateBilling(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateBilling");

            bool Success = false;

            try
            {
                List<ActiveBilling> ActiveBillingList = await DatabaseContext.ActiveBilling.FromSqlRaw(StoredProcedure.ActiveBillingSelect).ToListAsync();

                if (ActiveBillingList != null)
                {
                    if (ActiveBillingList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        List<ReceiptData> ReceiptDataList = new List<ReceiptData>();

                        long BillingMonth = DateTime.UtcNow.AddDays(-1).Month;

                        long BillingYear = DateTime.UtcNow.AddDays(-1).Year;

                        foreach (var ActiveBilling in ActiveBillingList)
                        {
                            try
                            {
                                List<BillingPeriod> BillingPeriodList = await DatabaseContext.BillingPeriod.FromSqlRaw(StoredProcedure.UpdateBillingSelect, ActiveBilling.CorpId, ActiveBilling.OrgId, BillingYear, BillingMonth, false).ToListAsync();

                                if (BillingPeriodList != null)
                                {
                                    Success = true;

                                    foreach (var BillingPeriod in BillingPeriodList)
                                    {
                                        if (BillingPeriod != null)
                                        {
                                            if (BillingPeriod.Updated != null)
                                            {
                                                if ((bool)BillingPeriod.Updated)
                                                {
                                                    await UpdateVoximplantPeriod(AppSettings, Logger, ActiveBilling.CorpId, ActiveBilling.OrgId, BillingYear, BillingMonth);
                                                }
                                            }

                                            if (BillingPeriod.Bill != null)
                                            {
                                                if ((bool)BillingPeriod.Bill)
                                                {
                                                    if (ActiveBilling.BillByOrg != null)
                                                    {
                                                        if ((bool)ActiveBilling.BillByOrg)
                                                        {
                                                            ReceiptData ReceiptData = ReceiptDataList.FirstOrDefault(DataRow => DataRow.CorpId == ActiveBilling.CorpId && DataRow.OrgId == ActiveBilling.OrgId && DataRow.ReceiptType == "ORG");

                                                            if (ReceiptData == null)
                                                            {
                                                                ReceiptData = new ReceiptData()
                                                                {
                                                                    CorpId = ActiveBilling.CorpId,
                                                                    Month = BillingMonth,
                                                                    OrgId = ActiveBilling.OrgId,
                                                                    ReceiptType = "ORG",
                                                                    Year = BillingYear
                                                                };

                                                                ReceiptDataList.Add(ReceiptData);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ReceiptData ReceiptData = ReceiptDataList.FirstOrDefault(DataRow => DataRow.CorpId == ActiveBilling.CorpId && DataRow.ReceiptType == "CORP");

                                                            if (ReceiptData == null)
                                                            {
                                                                ReceiptData = new ReceiptData()
                                                                {
                                                                    CorpId = ActiveBilling.CorpId,
                                                                    Month = BillingMonth,
                                                                    OrgId = 0,
                                                                    ReceiptType = "CORP",
                                                                    Year = BillingYear
                                                                };

                                                                ReceiptDataList.Add(ReceiptData);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessUpdateBilling: {ActiveBilling.OrgId}").Error(Exception, "Exception found:");
                            }

                            if (TaskBody.UpdateHsm)
                            {
                                try
                                {
                                    DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.UpdateHsmSelect, ActiveBilling.CorpId, ActiveBilling.OrgId, BillingYear, BillingMonth, false);

                                    Success = true;
                                }
                                catch (Exception Exception)
                                {
                                    Logger.ForContext("Context", $"ProcessUpdateBilling: {ActiveBilling.OrgId}").Error(Exception, "Exception found:");
                                }
                            }
                        }

                        if (ReceiptDataList.Count > 0)
                        {
                            DateTime DateTimeUtc = DateTime.UtcNow;

                            double Exchange = await GetExchange(Logger, DateTimeUtc.ToString("yyyy-MM-dd"));

                            while (Exchange == 0)
                            {
                                DateTimeUtc = DateTimeUtc.AddDays(-1);

                                Exchange = await GetExchange(Logger, DateTimeUtc.ToString("yyyy-MM-dd"));
                            }

                            foreach (var ReceiptData in ReceiptDataList)
                            {
                                List<InvoiceData> InvoiceDataList = new List<InvoiceData>();

                                switch (ReceiptData.ReceiptType.ToUpper())
                                {
                                    case "CORP":
                                        InvoiceDataList = await DatabaseContext.InvoiceData.FromSqlRaw(StoredProcedure.SelectInvoiceCorp, ReceiptData.CorpId, ReceiptData.Year, ReceiptData.Month, Exchange).ToListAsync();
                                        break;

                                    case "ORG":
                                        InvoiceDataList = await DatabaseContext.InvoiceData.FromSqlRaw(StoredProcedure.SelectInvoiceOrg, ReceiptData.CorpId, ReceiptData.OrgId, ReceiptData.Year, ReceiptData.Month, Exchange).ToListAsync();
                                        break;
                                }

                                if (InvoiceDataList != null)
                                {
                                    foreach (var InvoiceData in InvoiceDataList)
                                    {
                                        if (InvoiceData.AutomaticPayment == true)
                                        {
                                            PaymentPending PaymentPending = new PaymentPending()
                                            {
                                                Attempt = 0,
                                                ChangeBy = "SCHEDULER",
                                                ChangeDate = DateTime.UtcNow,
                                                CorpId = ReceiptData.CorpId,
                                                CreateBy = "SCHEDULER",
                                                CreateDate = DateTime.UtcNow,
                                                InvoiceId = InvoiceData.InvoiceId,
                                                LastBody = string.Empty,
                                                LastResponse = string.Empty,
                                                OrgId = ReceiptData.OrgId,
                                                RunDate = DateTime.UtcNow,
                                                Status = "ACTIVO",
                                                Type = string.Empty
                                            };

                                            DatabaseContext.PaymentPending.Add(PaymentPending);
                                            DatabaseContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateBilling").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static bool ProcessUpdateBillingMonth(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateBillingMonth");

            bool Success = false;

            try
            {
                DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.UpdateBillingMonthSelect, DateTime.UtcNow.Year, DateTime.UtcNow.Month);

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateBillingMonth").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateVoximplant(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateVoximplant");

            bool Success = false;

            try
            {
                List<ActiveBilling> ActiveBillingList = await DatabaseContext.ActiveBilling.FromSqlRaw(StoredProcedure.ActiveVoximplantSelect).ToListAsync();

                if (ActiveBillingList != null)
                {
                    if (ActiveBillingList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        long BillingMonth = DateTime.UtcNow.AddDays(-1).Month;

                        long BillingYear = DateTime.UtcNow.AddDays(-1).Year;

                        foreach (var ActiveBilling in ActiveBillingList)
                        {
                            if (await UpdateVoximplantPeriod(AppSettings, Logger, ActiveBilling.CorpId, ActiveBilling.OrgId, BillingYear, BillingMonth))
                            {
                                Success = true;
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateVoximplant").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateCoupon(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateCoupon");

            bool Success = false;

            try
            {
                List<CouponShop> CouponShopList = await DatabaseContext.CouponShop.Where(DataRow => DataRow.Status == "ACTIVO").ToListAsync();

                if (CouponShopList != null)
                {
                    foreach (var CouponShop in CouponShopList)
                    {
                        if (CouponShop.MaxStock != 0)
                        {
                            if (((DateTime)CouponShop.ChangeDate).AddMinutes((double)CouponShop.SupplyTimer) < DateTime.UtcNow)
                            {
                                CouponShop.ChangeBy = "scheduler";
                                CouponShop.ChangeDate = DateTime.UtcNow;
                                CouponShop.CurrentStock = 0;

                                DatabaseContext.CouponShop.Update(CouponShop);
                                DatabaseContext.SaveChanges();
                            }
                        }
                    }
                }

                List<CouponPromotion> CouponPromotionList = await DatabaseContext.CouponPromotion.Where(DataRow => DataRow.Status == "ACTIVO").ToListAsync();

                if (CouponPromotionList != null)
                {
                    foreach (var CouponPromotion in CouponPromotionList)
                    {
                        if (CouponPromotion.MaxStock != 0)
                        {
                            if (((DateTime)CouponPromotion.ChangeDate).AddMinutes((double)CouponPromotion.SupplyTimer) < DateTime.UtcNow)
                            {
                                CouponPromotion.ChangeBy = "scheduler";
                                CouponPromotion.ChangeDate = DateTime.UtcNow;
                                CouponPromotion.CurrentStock = 0;

                                DatabaseContext.CouponPromotion.Update(CouponPromotion);
                                DatabaseContext.SaveChanges();
                            }
                        }
                    }
                }

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateCoupon").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateCouponLaraigo(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateCouponLaraigo");

            bool Success = false;

            try
            {
                List<CouponLaraigoShop> CouponLaraigoShopList = await DatabaseContext.CouponLaraigoShop.Where(DataRow => DataRow.Status == "ACTIVO").ToListAsync();

                if (CouponLaraigoShopList != null)
                {
                    foreach (var CouponLaraigoShop in CouponLaraigoShopList)
                    {
                        if (long.Parse(CouponLaraigoShop.MaxStock) != 0)
                        {
                            if (((DateTime)CouponLaraigoShop.ChangeDate).AddMinutes(double.Parse(CouponLaraigoShop.SupplyTimer)) < DateTime.UtcNow)
                            {
                                CouponLaraigoShop.ChangeDate = DateTime.UtcNow;
                                CouponLaraigoShop.CurrentStock = "0";

                                DatabaseContext.CouponLaraigoShop.Update(CouponLaraigoShop);
                                DatabaseContext.SaveChanges();
                            }
                        }
                    }
                }

                List<CouponLaraigoPromotion> CouponLaraigoPromotionList = await DatabaseContext.CouponLaraigoPromotion.Where(DataRow => DataRow.Status == "ACTIVO").ToListAsync();

                if (CouponLaraigoPromotionList != null)
                {
                    foreach (var CouponLaraigoPromotion in CouponLaraigoPromotionList)
                    {
                        if (long.Parse(CouponLaraigoPromotion.MaxStock) != 0)
                        {
                            if (((DateTime)CouponLaraigoPromotion.ChangeDate).AddMinutes(double.Parse(CouponLaraigoPromotion.SupplyTimer)) < DateTime.UtcNow)
                            {
                                CouponLaraigoPromotion.ChangeDate = DateTime.UtcNow;
                                CouponLaraigoPromotion.CurrentStock = "0";

                                DatabaseContext.CouponLaraigoPromotion.Update(CouponLaraigoPromotion);
                                DatabaseContext.SaveChanges();
                            }
                        }
                    }
                }

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateCouponLaraigo").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static bool ProcessUpdateCouponVariant(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateCouponVariant");

            bool Success = false;

            try
            {
                List<CouponPromotion> CouponPromotionList = DatabaseContext.CouponPromotion.Where(DataRow => DataRow.Status == "ACTIVO").ToList();

                if (CouponPromotionList != null)
                {
                    List<CouponTicket> CouponTicketList = DatabaseContext.CouponTicket.Where(DataRow => DataRow.Status == "ACTIVO").ToList();

                    if (CouponTicketList != null)
                    {
                        foreach (var CouponTicket in CouponTicketList)
                        {
                            CouponPromotion CouponPromotion = CouponPromotionList.FirstOrDefault(DataRow => DataRow.Name == CouponTicket.Promotion);

                            if (CouponPromotion != null)
                            {
                                if (((DateTime)CouponTicket.CreateDate).AddMinutes((double)CouponPromotion.SupplyTimer) < DateTime.UtcNow)
                                {
                                    CouponTicket.ChangeDate = DateTime.UtcNow;
                                    CouponTicket.Status = "EXPIRADO";

                                    DatabaseContext.CouponTicket.Update(CouponTicket);
                                    DatabaseContext.SaveChanges();
                                }
                            }
                        }
                    }
                }

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateCouponVariant").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> UpdateVoximplantPeriod(AppSettings AppSettings, Logger Logger, long CorpId, long OrgId, long Year, long Month)
        {
            bool Success = false;

            try
            {
                dynamic ObjectContent = new ExpandoObject();

                ObjectContent.corpid = CorpId;
                ObjectContent.orgid = OrgId;
                ObjectContent.year = Year;
                ObjectContent.month = Month;

                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/voximplant/updatevoximplantperiod";

                using HttpClient HttpClient = new HttpClient();

                Logger.ForContext("Context", $"UpdateVoximplantPeriod: {OrgId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", $"UpdateVoximplantPeriod: {OrgId}").Debug("Http POST body: {StringContent}", StringContent);

                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    Logger.ForContext("Context", $"UpdateVoximplantPeriod: {OrgId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Success = true;
                }
                else
                {
                    Logger.ForContext("Context", $"UpdateVoximplantPeriod: {OrgId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", $"UpdateVoximplantPeriod: {OrgId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "UpdateVoximplantPeriod").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<double> GetExchange(Logger Logger, string DateTime)
        {
            double Exchange = 0;

            try
            {
                string HttpEndpoint = $"https://api.apis.net.pe/v1/tipo-cambio-sunat?fecha={DateTime}";

                using HttpClient HttpClient = new HttpClient();

                Logger.ForContext("Context", "GetExchange").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                HttpResponseMessage HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    Logger.ForContext("Context", "GetExchange").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    dynamic ExchangeResponse = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                    Exchange = (double)ExchangeResponse.venta;
                }
                else
                {
                    Logger.ForContext("Context", "GetExchange").Warning("Http GET response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", "GetExchange").Warning("Unsuccessful http GET: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "GetExchange").Error(Exception, "Exception found:");
            }

            return Exchange;
        }

        public static async Task<GetInvoiceOutput> SendInvoice(AppSettings AppSettings, Logger Logger, Invoice Invoice, List<InvoiceDetail> InvoiceDetailList, InvoiceCorrelative InvoiceCorrelative)
        {
            GetInvoiceOutput Output = new GetInvoiceOutput();

            try
            {
                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processmifact/sendinvoice";

                dynamic ObjectContent = new ExpandoObject();

                ObjectContent.CodigoAnexoEmisor = Invoice.AnnexCode;
                ObjectContent.CodigoFormatoImpresion = Invoice.PrintingFormat;
                ObjectContent.CodigoMoneda = Invoice.Currency;
                ObjectContent.CodigoRucReceptor = Invoice.ReceiverDocType;
                ObjectContent.CodigoOperacionSunat = Invoice.SunatOpeCode;
                ObjectContent.CodigoUbigeoEmisor = Invoice.IssuerUbigeo;
                ObjectContent.EnviarSunat = Invoice.SendToSunat;
                ObjectContent.MailEnvio = Invoice.ReceiverMail;
                ObjectContent.MontoTotal = Invoice.TotalAmount;
                ObjectContent.MontoTotalGravado = Invoice.Subtotal;
                ObjectContent.MontoTotalIgv = Invoice.Taxes;
                ObjectContent.NombreComercialEmisor = Invoice.IssuerTradeName;
                ObjectContent.RazonSocialEmisor = Invoice.IssuerBusinessName;
                ObjectContent.RazonSocialReceptor = Invoice.ReceiverBusinessName;
                ObjectContent.CorrelativoDocumento = InvoiceCorrelative.Correlative.ToString("00000000.##");
                ObjectContent.RucEmisor = Invoice.IssuerRuc;
                ObjectContent.NumeroDocumentoReceptor = Invoice.ReceiverDocNum;
                ObjectContent.NumeroSerieDocumento = Invoice.Serie;
                ObjectContent.RetornaPdf = Invoice.ReturnPdf;
                ObjectContent.RetornaXmlSunat = Invoice.ReturnXmlSunat;
                ObjectContent.RetornaXml = Invoice.ReturnXml;
                ObjectContent.TipoCambio = Invoice.ExchangeRate;
                ObjectContent.DireccionFiscalEmisor = Invoice.IssuerFiscalAddress;
                ObjectContent.DireccionFiscalReceptor = Invoice.ReceiverFiscalAddress;
                ObjectContent.VersionXml = Invoice.XmlVersion;
                ObjectContent.VersionUbl = Invoice.UblVersion;
                ObjectContent.TipoDocumento = Invoice.InvoiceType;
                ObjectContent.TipoRucEmisor = Invoice.EmitterType;
                ObjectContent.Endpoint = Invoice.SunatUrl;
                ObjectContent.Username = Invoice.SunatUsername;
                ObjectContent.Token = Invoice.Token;

                if (Invoice.InvoiceDate != null)
                {
                    ObjectContent.FechaEmision = ((DateTime)Invoice.InvoiceDate).ToString("yyyy-MM-dd");
                }

                if (InvoiceDetailList != null)
                {
                    ObjectContent.ProductList = new List<dynamic>();

                    foreach (var InvoiceDetail in InvoiceDetailList)
                    {
                        dynamic ProductData = new ExpandoObject();

                        ProductData.CantidadProducto = InvoiceDetail.Quantity;
                        ProductData.CodigoProducto = InvoiceDetail.ProductCode;
                        ProductData.AfectadoIgv = InvoiceDetail.HasIgv;
                        ProductData.TipoVenta = InvoiceDetail.SaleType;
                        ProductData.TributoIgv = InvoiceDetail.IgvTribute;
                        ProductData.UnidadMedida = InvoiceDetail.MeasureUnit;
                        ProductData.IgvTotal = InvoiceDetail.TotalIgv;
                        ProductData.MontoTotal = InvoiceDetail.TotalAmount;
                        ProductData.TasaIgv = InvoiceDetail.IgvRate;
                        ProductData.PrecioProducto = InvoiceDetail.ProductPrice;
                        ProductData.DescripcionProducto = InvoiceDetail.ProductDescription;
                        ProductData.PrecioNetoProducto = InvoiceDetail.ProductnetPrice;
                        ProductData.ValorNetoProducto = InvoiceDetail.ProductNetWorth;

                        ObjectContent.ProductList.Add(ProductData);
                    }
                }

                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                Logger.ForContext("Context", "SendInvoice").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", "SendInvoice").Debug("Http POST body: {StringContent}", StringContent);

                using HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    Logger.ForContext("Context", "SendInvoice").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Output = JsonConvert.DeserializeObject<GetInvoiceOutput>(HttpResponseContent);
                }
                else
                {
                    Logger.ForContext("Context", "SendInvoice").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", "SendInvoice").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                    Output = JsonConvert.DeserializeObject<GetInvoiceOutput>(HttpResponseContent);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "SendInvoice").Error(Exception, "Exception found:");
            }

            return Output;
        }

        public static async Task<bool> ProcessVoximplantRecharge(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "VoximplantRecharge");

            bool Success = false;

            try
            {
                List<VoximplantOrganization> VoximplantOrganizationList = await DatabaseContext.VoximplantOrganization.FromSqlRaw(StoredProcedure.SelectVoximplantRecharge).ToListAsync();

                if (VoximplantOrganizationList != null)
                {
                    foreach (var VoximplantOrganization in VoximplantOrganizationList)
                    {
                        dynamic ObjectContent = new ExpandoObject();

                        ObjectContent.corpid = VoximplantOrganization.CorpId;
                        ObjectContent.daterange = VoximplantOrganization.VoximplantRechargeRange;
                        ObjectContent.orgid = VoximplantOrganization.OrgId;
                        ObjectContent.timezoneoffset = VoximplantOrganization.TimezoneOffset;

                        string StringContent = JsonConvert.SerializeObject(ObjectContent);

                        string HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/voximplant/directgetmaximumconsumption";

                        using HttpClient HttpClient = new HttpClient();

                        Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                        Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST body: {StringContent}", StringContent);

                        HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                        string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                        if (HttpResponseMessage.IsSuccessStatusCode)
                        {
                            Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            dynamic MaximumConsumption = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                            ObjectContent = new ExpandoObject();

                            ObjectContent.corpid = VoximplantOrganization.CorpId;
                            ObjectContent.orgid = VoximplantOrganization.OrgId;

                            StringContent = JsonConvert.SerializeObject(ObjectContent);

                            HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/voximplant/directgetaccountbalance";

                            Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                            Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST body: {StringContent}", StringContent);

                            HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                            HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                            if (HttpResponseMessage.IsSuccessStatusCode)
                            {
                                Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                dynamic CurrentBalance = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                double MaximumAmount = (double)MaximumConsumption.data.maximumconsumption;

                                MaximumAmount = (MaximumAmount * ((double)VoximplantOrganization.VoximplantRechargePercentage + 1)) + (double)VoximplantOrganization.VoximplantRechargeFixed;

                                if ((double)CurrentBalance.data.balancechild < MaximumAmount)
                                {
                                    ObjectContent = new ExpandoObject();

                                    ObjectContent.corpid = VoximplantOrganization.CorpId;
                                    ObjectContent.description = $"AUTOMATIC RECHARGE TO {VoximplantOrganization.OrgId}: {MaximumAmount - (double)CurrentBalance.data.balancechild}";
                                    ObjectContent.motive = "AUTOMATIC";
                                    ObjectContent.orgid = VoximplantOrganization.OrgId;
                                    ObjectContent.transferamount = MaximumAmount - (double)CurrentBalance.data.balancechild;
                                    ObjectContent.type = "AUTOMATIC";
                                    ObjectContent.usr = "SCHEDULER";

                                    StringContent = JsonConvert.SerializeObject(ObjectContent);

                                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/voximplant/directtransferaccountbalance";

                                    Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                    Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST body: {StringContent}", StringContent);

                                    HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                    {
                                        Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);
                                    }
                                    else
                                    {
                                        Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                        Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                    }
                                }
                            }
                            else
                            {
                                Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                            }
                        }
                        else
                        {
                            Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                            Logger.ForContext("Context", $"ProcessVoximplantRecharge: {VoximplantOrganization.OrgId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                        }

                        DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.SelectVoximplantUpdate, VoximplantOrganization.CorpId, VoximplantOrganization.OrgId);
                    }
                }

                Success = true;
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessVoximplantRecharge").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendMail(AppSettings AppSettings, TaskBody TaskBody, DatabaseContext DatabaseContext, long CorpId, long OrgId)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendMail");

            bool Success = true;

            try
            {
                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                switch (TaskBody.MessageType.ToUpper())
                {
                    case "OWNERBODY":
                        using (HttpClient HttpClient = new HttpClient())
                        {
                            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            SendMailBody SendMailBody = new SendMailBody()
                            {
                                Attachments = TaskBody.Attachments,
                                MailAddress = TaskBody.Receiver,
                                MailBlindAddress = TaskBody.BlindReceiver,
                                MailBody = TaskBody.Body,
                                MailCopyAddress = TaskBody.CopyReceiver,
                                MailCredentials = TaskBody.Credentials,
                                MailTitle = TaskBody.Subject
                            };

                            string StringContent = JsonConvert.SerializeObject(SendMailBody);

                            if (SendMailBody.Attachments != null)
                            {
                                foreach (var Attachment in SendMailBody.Attachments)
                                {
                                    if (Attachment.Type == "DATA")
                                    {
                                        Attachment.Value = null;
                                    }
                                }
                            }

                            string StringContentLog = JsonConvert.SerializeObject(SendMailBody);

                            Logger.ForContext("Context", "ProcessSendMail").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                            Logger.ForContext("Context", "ProcessSendMail").Debug("Http POST body: {StringContent}", StringContentLog);

                            HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                            string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                            string Username = AppSettings.MailSettings.Address;
                            string ShippingReason = string.Empty;

                            if (!string.IsNullOrWhiteSpace(TaskBody.Credentials))
                            {
                                dynamic CredentialData = JsonConvert.DeserializeObject<dynamic>(TaskBody.Credentials);

                                if (CredentialData.username != null)
                                {
                                    Username = CredentialData.username;
                                }
                            }

                            if (TaskBody.Config != null)
                            {
                                TaskBody.Config.CommunicationChannelSite = Username;
                                ShippingReason = TaskBody.Config.ShippingReason;
                            }

                            if (HttpResponseMessage.IsSuccessStatusCode)
                            {
                                Logger.ForContext("Context", "ProcessSendMail").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                if (TaskBody.Config != null)
                                {
                                    DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.InsertHsmHistory, CorpId, OrgId, BridgeResponse.Success, BridgeResponse.OperationMessage, JsonConvert.SerializeObject(TaskBody.Config), ShippingReason, TaskBody.Config.MessageTemplateId);
                                }
                            }
                            else
                            {
                                Logger.ForContext("Context", "ProcessSendMail").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                Logger.ForContext("Context", "ProcessSendMail").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                                if (TaskBody.Config != null)
                                {
                                    DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.InsertHsmHistory, CorpId, OrgId, false, HttpResponseMessage.ReasonPhrase, JsonConvert.SerializeObject(TaskBody.Config), ShippingReason);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendMail").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendPasswordAlert(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendPasswordAlert");

            bool Success = false;

            try
            {
                List<SecurityValidation> SecurityValidationList = await DatabaseContext.SecurityValidation.FromSqlRaw(StoredProcedure.SecurityValidationSelect).ToListAsync();

                if (SecurityValidationList != null)
                {
                    if (SecurityValidationList.Count > 0)
                    {
                        foreach (var SecurityValidation in SecurityValidationList)
                        {
                            try
                            {
                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                                using HttpClient HttpClient = new HttpClient();

                                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                string MailSubject = TaskBody.Subject;

                                MailSubject = MailSubject?.Replace("{{action}}", SecurityValidation.Action);
                                MailSubject = MailSubject?.Replace("{{corpid}}", SecurityValidation.CorpId.ToString());
                                MailSubject = MailSubject?.Replace("{{email}}", SecurityValidation.Email);
                                MailSubject = MailSubject?.Replace("{{fullname}}", SecurityValidation.FullName);
                                MailSubject = MailSubject?.Replace("{{lastactivitydate}}", SecurityValidation.LastActivityDate.ToString());
                                MailSubject = MailSubject?.Replace("{{maxinactivedays}}", SecurityValidation.MaxInactiveDays.ToString());
                                MailSubject = MailSubject?.Replace("{{now}}", SecurityValidation.Now.ToString());
                                MailSubject = MailSubject?.Replace("{{passwordchangedate}}", SecurityValidation.PasswordChangeDate.ToString());
                                MailSubject = MailSubject?.Replace("{{periodvaliditypwd}}", SecurityValidation.PeriodValidityPwd.ToString());
                                MailSubject = MailSubject?.Replace("{{pwdvaliddays}}", SecurityValidation.PwdValidDays.ToString());
                                MailSubject = MailSubject?.Replace("{{status}}", SecurityValidation.Status);
                                MailSubject = MailSubject?.Replace("{{userid}}", SecurityValidation.UserId.ToString());
                                MailSubject = MailSubject?.Replace("{{usr}}", SecurityValidation.Usr);

                                string MailBody = TaskBody.Body;

                                switch (SecurityValidation.Action.ToUpper())
                                {
                                    case "INACTIVITY":
                                    case "PWDEXPIRED":
                                        MailBody = TaskBody.ExpireBody;
                                        break;

                                    case "INACTIVITYALERT":
                                    case "PWDALERT":
                                        MailBody = TaskBody.AlertBody;
                                        break;
                                }

                                MailBody = MailBody?.Replace("{{action}}", SecurityValidation.Action);
                                MailBody = MailBody?.Replace("{{corpid}}", SecurityValidation.CorpId.ToString());
                                MailBody = MailBody?.Replace("{{email}}", SecurityValidation.Email);
                                MailBody = MailBody?.Replace("{{fullname}}", SecurityValidation.FullName);
                                MailBody = MailBody?.Replace("{{lastactivitydate}}", SecurityValidation.LastActivityDate.ToString());
                                MailBody = MailBody?.Replace("{{maxinactivedays}}", SecurityValidation.MaxInactiveDays.ToString());
                                MailBody = MailBody?.Replace("{{now}}", SecurityValidation.Now.ToString());
                                MailBody = MailBody?.Replace("{{passwordchangedate}}", SecurityValidation.PasswordChangeDate.ToString());
                                MailBody = MailBody?.Replace("{{periodvaliditypwd}}", SecurityValidation.PeriodValidityPwd.ToString());
                                MailBody = MailBody?.Replace("{{pwdvaliddays}}", SecurityValidation.PwdValidDays.ToString());
                                MailBody = MailBody?.Replace("{{status}}", SecurityValidation.Status);
                                MailBody = MailBody?.Replace("{{userid}}", SecurityValidation.UserId.ToString());
                                MailBody = MailBody?.Replace("{{usr}}", SecurityValidation.Usr);

                                SendMailBody SendMailBody = new SendMailBody()
                                {
                                    MailAddress = SecurityValidation.Email,
                                    MailBody = MailBody,
                                    MailTitle = MailSubject,
                                };

                                string StringContent = JsonConvert.SerializeObject(SendMailBody);

                                Logger.ForContext("Context", $"ProcessSendPasswordAlert: {SecurityValidation.Usr}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                Logger.ForContext("Context", $"ProcessSendPasswordAlert: {SecurityValidation.Usr}").Debug("Http POST body: {StringContent}", StringContent);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    Logger.ForContext("Context", $"ProcessSendPasswordAlert: {SecurityValidation.Usr}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                    if (BridgeResponse.Success)
                                    {
                                        Success = true;
                                    }
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessSendPasswordAlert: {SecurityValidation.Usr}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", $"ProcessSendPasswordAlert: {SecurityValidation.Usr}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessSendPasswordAlert: {SecurityValidation.Usr}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                    else
                    {
                        Success = true;
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendPasswordAlert").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendReportQuery(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendReportQuery");

            bool Success = false;

            try
            {
                Success = true;

                if (!string.IsNullOrWhiteSpace(TaskBody.ExecutionType))
                {
                    using OdooContext OdooContext = new OdooContext();

                    OdooContext.Database.SetCommandTimeout(7200);

                    using HttpClient HttpClient = new HttpClient();

                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    switch (TaskBody.ExecutionType.ToUpper())
                    {
                        case "ODOOPRODUCT":
                            List<OdooProduct> OdooProductList = await OdooContext.OdooProduct.FromSqlRaw(StoredProcedure.SelectOdooProduct).ToListAsync();

                            if (OdooProductList != null)
                            {
                                using MemoryStream MemoryStream = new MemoryStream();

                                using TextWriter TextWriter = new StreamWriter(MemoryStream);

                                TextWriter.WriteLine("\"Solicitud\",\"Orden\",\"OrdenDescripcion\",\"Estado\",\"Placa\",\"FechaSolicitud\",\"FechaInicioReal\",\"FechaFinReal\",\"Distribuidor\",\"IdCliente\",\"Cliente\",\"Producto\",\"Cantidad\",\"Problema\",\"Causa\",\"Solución\"");

                                foreach (var OdooProduct in OdooProductList)
                                {
                                    TextWriter.WriteLine($"\"{OdooProduct.Solicitud}\",\"{OdooProduct.Orden}\",\"{OdooProduct.OrdenDescripcion}\",\"{OdooProduct.Estado}\",\"{OdooProduct.Placa}\",\"{OdooProduct.FechaSolicitud}\",\"{OdooProduct.FechaInicioReal}\",\"{OdooProduct.FechaFinReal}\",\"{OdooProduct.Distribuidor}\",\"{OdooProduct.IdCliente}\",\"{OdooProduct.Cliente}\",\"{OdooProduct.Producto}\",\"{OdooProduct.Cantidad}\",\"{OdooProduct.Problema}\",\"{OdooProduct.Causa}\",\"{OdooProduct.Solución}\"");
                                }

                                TextWriter.Flush();

                                MemoryStream.Position = 0;

                                MemoryStream.Seek(0, SeekOrigin.Begin);

                                if (MemoryStream != null)
                                {
                                    string Filename = $"{TaskBody.FilePath}-{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset):yyyy-MM-dd_HH:mm:ss}.csv";

                                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                                    SendMailBody SendMailBody = new SendMailBody()
                                    {
                                        MailBlindAddress = TaskBody.BlindReceiver,
                                        MailCopyAddress = TaskBody.CopyReceiver,
                                        MailAddress = TaskBody.Receiver,
                                        MailAttachmentData = MemoryStream.ToArray(),
                                        MailAttachmentName = Filename,
                                        MailTitle = TaskBody.Subject,
                                        MailBody = TaskBody.Body,
                                    };

                                    string StringContent = JsonConvert.SerializeObject(SendMailBody);

                                    SendMailBody.MailAttachmentData = null;

                                    string StringContentLog = JsonConvert.SerializeObject(SendMailBody);

                                    Logger.ForContext("Context", "ProcessSendReportQuery").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                    Logger.ForContext("Context", "ProcessSendReportQuery").Debug("Http POST body: {StringContent}", StringContentLog);

                                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                    {
                                        Logger.ForContext("Context", "ProcessSendReportQuery").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                        if (BridgeResponse.Success)
                                        {
                                            Success = true;
                                        }
                                        else
                                        {
                                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                                        }
                                    }
                                    else
                                    {
                                        Logger.ForContext("Context", "ProcessSendReportQuery").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                        Logger.ForContext("Context", "ProcessSendReportQuery").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                                    }
                                }
                            }
                            break;

                        case "ODOOEQUIPMENT":
                            List<OdooEquipment> OdooEquipmentList = await OdooContext.OdooEquipment.FromSqlRaw(StoredProcedure.SelectOdooEquipment).ToListAsync();

                            if (OdooEquipmentList != null)
                            {
                                using MemoryStream MemoryStream = new MemoryStream();

                                using TextWriter TextWriter = new StreamWriter(MemoryStream);

                                TextWriter.WriteLine("\"Solicitud\",\"Prioridad\",\"Orden\",\"TipoProblema\",\"Estado\",\"Placa\",\"FechaSolicitud\",\"FechaInicioReal\",\"FechaFinReal\",\"Distribuidor\",\"IdCliente\",\"Cliente\",\"Ruc\",\"Dni\"");

                                foreach (var OdooEquipment in OdooEquipmentList)
                                {
                                    TextWriter.WriteLine($"\"{OdooEquipment.Solicitud}\",\"{OdooEquipment.Prioridad}\",\"{OdooEquipment.Orden}\",\"{OdooEquipment.TipoProblema}\",\"{OdooEquipment.Estado}\",\"{OdooEquipment.Placa}\",\"{OdooEquipment.FechaSolicitud}\",\"{OdooEquipment.FechaInicioReal}\",\"{OdooEquipment.FechaFinReal}\",\"{OdooEquipment.Distribuidor}\",\"{OdooEquipment.IdCliente}\",\"{OdooEquipment.Cliente}\",\"{OdooEquipment.Ruc}\",\"{OdooEquipment.Dni}\"");
                                }

                                TextWriter.Flush();

                                MemoryStream.Position = 0;

                                MemoryStream.Seek(0, SeekOrigin.Begin);

                                if (MemoryStream != null)
                                {
                                    string Filename = $"{TaskBody.FilePath}-{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset):yyyy-MM-dd_HH:mm:ss}.csv";

                                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                                    SendMailBody SendMailBody = new SendMailBody()
                                    {
                                        MailAddress = TaskBody.Receiver,
                                        MailAttachmentData = MemoryStream.ToArray(),
                                        MailAttachmentName = Filename,
                                        MailBlindAddress = TaskBody.BlindReceiver,
                                        MailBody = TaskBody.Body,
                                        MailCopyAddress = TaskBody.CopyReceiver,
                                        MailTitle = TaskBody.Subject
                                    };

                                    string StringContent = JsonConvert.SerializeObject(SendMailBody);

                                    SendMailBody.MailAttachmentData = null;

                                    string StringContentLog = JsonConvert.SerializeObject(SendMailBody);

                                    Logger.ForContext("Context", "ProcessSendReportQuery").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                    Logger.ForContext("Context", "ProcessSendReportQuery").Debug("Http POST body: {StringContent}", StringContentLog);

                                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                    {
                                        Logger.ForContext("Context", "ProcessSendReportQuery").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                        if (BridgeResponse.Success)
                                        {
                                            Success = true;
                                        }
                                        else
                                        {
                                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                                        }
                                    }
                                    else
                                    {
                                        Logger.ForContext("Context", "ProcessSendReportQuery").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                        Logger.ForContext("Context", "ProcessSendReportQuery").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                                    }
                                }
                            }
                            break;

                        case "ODOOPROVIDER":
                            List<ProviderMail> ProviderMailList = await OdooContext.ProviderMail.FromSqlRaw(StoredProcedure.SelectProviderList).ToListAsync();

                            if (ProviderMailList != null)
                            {
                                foreach (var ProviderMail in ProviderMailList)
                                {
                                    try
                                    {
                                        List<ProviderReport> ProviderReportList = await OdooContext.ProviderReport.FromSqlRaw(StoredProcedure.SelectProviderReport, (long)ProviderMail.Id).ToListAsync();

                                        if (ProviderReportList != null)
                                        {
                                            string ReportJson = JsonConvert.SerializeObject(ProviderReportList);

                                            DataTable DataTable = (DataTable)JsonConvert.DeserializeObject(ReportJson, typeof(DataTable));

                                            using XLWorkbook XLWorkbook = new XLWorkbook();

                                            IXLWorksheet XLWorksheet = XLWorkbook.Worksheets.Add();

                                            XLWorksheet.FirstCell().InsertTable(DataTable, false);

                                            if (ProviderReportList.Count > 0)
                                            {
                                                XLWorksheet.Rows().Height = 15;
                                                XLWorksheet.RangeUsed().SetAutoFilter();
                                                XLWorksheet.Style.Alignment.WrapText = false;
                                            }

                                            using MemoryStream MemoryStream = new MemoryStream();

                                            XLWorkbook.SaveAs(MemoryStream);

                                            MailAttachment MailAttachment = new MailAttachment()
                                            {
                                                Name = $"{TaskBody.FilePath}-{ProviderMail.Id}-{DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset):yyyy-MM-dd_HH:mm:ss}.xlsx",
                                                Type = "DATA",
                                                Value = Convert.ToBase64String(MemoryStream.ToArray())
                                            };

                                            if (MailAttachment != null)
                                            {
                                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                                                SendMailBody SendMailBody = new SendMailBody()
                                                {
                                                    MailAddress = $"{TaskBody.Receiver}{(!string.IsNullOrWhiteSpace(ProviderMail.EmailNotification) ? $",{ProviderMail.EmailNotification}" : string.Empty)}",
                                                    MailBlindAddress = TaskBody.BlindReceiver,
                                                    MailBody = TaskBody.Body,
                                                    MailCopyAddress = TaskBody.CopyReceiver,
                                                    MailTitle = $"{TaskBody.Subject} [{ProviderMail.Name}]",
                                                    Attachments = new List<MailAttachment>()
                                                    {
                                                        MailAttachment
                                                    },
                                                };

                                                string StringContent = JsonConvert.SerializeObject(SendMailBody);

                                                string StringContentLog = JsonConvert.SerializeObject(SendMailBody);

                                                Logger.ForContext("Context", $"ProcessSendReportQuery: {ProviderMail.Id}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                Logger.ForContext("Context", $"ProcessSendReportQuery: {ProviderMail.Id}").Debug("Http POST body: {StringContent}", StringContentLog);

                                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                if (HttpResponseMessage.IsSuccessStatusCode)
                                                {
                                                    Logger.ForContext("Context", $"ProcessSendReportQuery: {ProviderMail.Id}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                    BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                                                    if (BridgeResponse.Success)
                                                    {
                                                        Success = true;
                                                    }
                                                    else
                                                    {
                                                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                                                    }
                                                }
                                                else
                                                {
                                                    Logger.ForContext("Context", $"ProcessSendReportQuery: {ProviderMail.Id}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                    Logger.ForContext("Context", $"ProcessSendReportQuery: {ProviderMail.Id}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                                                    MailService.SendMail(AppSettings, Logger, SendMailBody);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception Exception)
                                    {
                                        Logger.ForContext("Context", $"ProcessSendReportQuery: {ProviderMail.Id}").Error(Exception, "Exception found:");
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendReportQuery").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSendReportTemplate(AppSettings AppSettings, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SendReportTemplate");

            bool Success = false;

            try
            {
                string StringContent = TaskBody.ReportFilter;

                StringContent = StringContent?.Replace("{{date}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd"));
                StringContent = StringContent?.Replace("{{datetime}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("yyyy-MM-dd HH:mm:ss"));
                StringContent = StringContent?.Replace("{{yesterday}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).AddDays(-1).ToString("yyyy-MM-dd"));

                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.LaraigoEndpoint}api/reportdesigner/exporttask";

                using HttpClient HttpClient = new HttpClient();

                Logger.ForContext("Context", "ProcessSendReportTemplate").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                Logger.ForContext("Context", "ProcessSendReportTemplate").Debug("Http POST body: {StringContent}", StringContent);

                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    Logger.ForContext("Context", "ProcessSendReportTemplate").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    dynamic ReportResponse = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                    string ReportUrl = ReportResponse.url;

                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                    SendMailBody SendMailBody = new SendMailBody()
                    {
                        Attachments = new List<MailAttachment>()
                        {
                            new MailAttachment()
                            {
                                Type = "URL",
                                Value = ReportUrl
                            }
                        },
                        MailAddress = TaskBody.Receiver,
                        MailBlindAddress = TaskBody.BlindReceiver,
                        MailBody = TaskBody.Body,
                        MailCopyAddress = TaskBody.CopyReceiver,
                        MailTitle = TaskBody.Subject
                    };

                    StringContent = JsonConvert.SerializeObject(SendMailBody);

                    Logger.ForContext("Context", "ProcessSendReportTemplate").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                    Logger.ForContext("Context", "ProcessSendReportTemplate").Debug("Http POST body: {StringContent}", StringContent);

                    HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        Logger.ForContext("Context", "ProcessSendReportTemplate").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                        if (BridgeResponse.Success)
                        {
                            Success = true;
                        }
                        else
                        {
                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                        }
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessSendReportTemplate").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Logger.ForContext("Context", "ProcessSendReportTemplate").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                    }
                }
                else
                {
                    Logger.ForContext("Context", "ProcessSendReportTemplate").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", "ProcessSendReportTemplate").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSendReportTemplate").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessSessionCheck(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "SessionCheck");

            bool Success = false;

            try
            {
                List<SessionInformation> SessionInformationList = await DatabaseContext.SessionInformation.FromSqlRaw(StoredProcedure.SessionCheckSelect).ToListAsync();

                long ActiveNumber = 0;
                long IdleNumber = 0;

                if (SessionInformationList != null)
                {
                    if (SessionInformationList.Count > 0)
                    {
                        ActiveNumber = SessionInformationList.Where(DataRow => DataRow.State == "active").Count();
                        IdleNumber = SessionInformationList.Where(DataRow => DataRow.State == "idle").Count();
                    }
                }

                SessionLog SessionLog = new SessionLog()
                {
                    ActiveNumber = ActiveNumber,
                    ColumnData = JsonConvert.SerializeObject(SessionInformationList),
                    DateTime = DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset),
                    IdleNumber = IdleNumber
                };

                DatabaseContext.SessionLog.Add(SessionLog);
                DatabaseContext.SaveChanges();

                Success = true;

                if (ActiveNumber > TaskBody.HoldingLimit)
                {
                    string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processscheduler/sendmail";

                    using HttpClient HttpClient = new HttpClient();

                    HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    TaskBody.Subject = TaskBody.Subject?.Replace("{{activenumber}}", ActiveNumber.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{date}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy"));
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{datetime}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy HH:mm:ss"));
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{holdinglimit}}", TaskBody.HoldingLimit.ToString());
                    TaskBody.Subject = TaskBody.Subject?.Replace("{{idlenumber}}", IdleNumber.ToString());

                    TaskBody.Body = TaskBody.Body?.Replace("{{activenumber}}", ActiveNumber.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{date}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy"));
                    TaskBody.Body = TaskBody.Body?.Replace("{{datetime}}", DateTime.UtcNow.AddHours((double)TaskBody.TaskOffset).ToString("dd-MM-yyyy HH:mm:ss"));
                    TaskBody.Body = TaskBody.Body?.Replace("{{holdinglimit}}", TaskBody.HoldingLimit.ToString());
                    TaskBody.Body = TaskBody.Body?.Replace("{{idlenumber}}", IdleNumber.ToString());

                    SendMailBody SendMailBody = new SendMailBody()
                    {
                        MailAddress = TaskBody.Receiver,
                        MailBlindAddress = TaskBody.BlindReceiver,
                        MailBody = TaskBody.Body,
                        MailCopyAddress = TaskBody.CopyReceiver,
                        MailTitle = TaskBody.Subject
                    };

                    string StringContent = JsonConvert.SerializeObject(SendMailBody);

                    Logger.ForContext("Context", "ProcessSessionCheck").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                    Logger.ForContext("Context", "ProcessSessionCheck").Debug("Http POST body: {StringContent}", StringContent);

                    HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                    string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                    if (HttpResponseMessage.IsSuccessStatusCode)
                    {
                        Logger.ForContext("Context", "ProcessSessionCheck").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        BridgeResponse BridgeResponse = JsonConvert.DeserializeObject<BridgeResponse>(HttpResponseContent);

                        if (!BridgeResponse.Success)
                        {
                            MailService.SendMail(AppSettings, Logger, SendMailBody);
                        }
                    }
                    else
                    {
                        Logger.ForContext("Context", "ProcessSessionCheck").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                        Logger.ForContext("Context", "ProcessSessionCheck").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);

                        MailService.SendMail(AppSettings, Logger, SendMailBody);
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessSessionCheck").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateCallLog(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateCallLog");

            bool Success = false;

            try
            {
                if (TaskBody.Ftp != null)
                {
                    FtpClient FtpClient = new FtpClient(TaskBody.Ftp.Host)
                    {
                        Credentials = new NetworkCredential(TaskBody.Ftp.Username, TaskBody.Ftp.Password)
                    };

                    FtpClient.Connect();

                    if (FtpClient.DirectoryExists("/var/spool/asterisk/monitor/"))
                    {
                        foreach (var Item in FtpClient.GetListing("/var/spool/asterisk/monitor/"))
                        {
                            if (Item.Type == FtpObjectType.Directory)
                            {
                                foreach (var SubItem in FtpClient.GetListing(Item.FullName))
                                {
                                    if (SubItem.Type == FtpObjectType.Directory)
                                    {
                                        foreach (var SubSubItem in FtpClient.GetListing(SubItem.FullName))
                                        {
                                            if (SubSubItem.Type == FtpObjectType.Directory)
                                            {
                                                foreach (var SubSubSubItem in FtpClient.GetListing(SubSubItem.FullName))
                                                {
                                                    if (SubSubSubItem.Type == FtpObjectType.File)
                                                    {
                                                        await ProcessFile(AppSettings, Logger, DatabaseContext, FtpClient, SubSubSubItem);
                                                    }
                                                }
                                            }

                                            if (SubSubItem.Type == FtpObjectType.File)
                                            {
                                                await ProcessFile(AppSettings, Logger, DatabaseContext, FtpClient, SubSubItem);
                                            }
                                        }
                                    }

                                    if (SubItem.Type == FtpObjectType.File)
                                    {
                                        await ProcessFile(AppSettings, Logger, DatabaseContext, FtpClient, SubItem);
                                    }
                                }
                            }

                            if (Item.Type == FtpObjectType.File)
                            {
                                await ProcessFile(AppSettings, Logger, DatabaseContext, FtpClient, Item);
                            }
                        }
                    }

                    FtpClient.Disconnect();

                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateCallLog").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<string> GetAztecaToken(AppSettings AppSettings, Logger Logger)
        {
            string Token = string.Empty;

            try
            {
                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processbancoazteca/getbancoaztecatokennew";

                using HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Logger.ForContext("Context", "GetAztecaToken").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(string.Empty, Encoding.UTF8, "application/json"));

                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                if (HttpResponseMessage.IsSuccessStatusCode)
                {
                    Logger.ForContext("Context", "GetAztecaToken").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    dynamic AztecaToken = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                    return (string)AztecaToken.token;
                }
                else
                {
                    Logger.ForContext("Context", "GetAztecaToken").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                    Logger.ForContext("Context", "GetAztecaToken").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "GetAztecaToken").Error(Exception, "Exception found:");
            }

            return Token;
        }

        public static string NormalizeString(string Text)
        {
            StringBuilder StringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Text))
            {
                foreach (var Character in Text)
                {
                    if ((Character >= '0' && Character <= '9') || (Character >= 'A' && Character <= 'Z') || (Character >= 'a' && Character <= 'z') || Character == '.' || Character == '_' || Character == '-' || Character == '@' || Character == '!' || Character == '¡' || Character == '?' || Character == '¿' || Character == '(' || Character == ')' || Character == '{' || Character == '}' || Character == '[' || Character == ']' || Character == ';' || Character == '$' || Character == '%' || Character == '&' || Character == '#' || Character == '/' || Character == ':' || Character == 'á' || Character == 'é' || Character == 'í' || Character == 'ó' || Character == 'ú' || Character == 'Á' || Character == 'É' || Character == 'Í' || Character == 'Ó' || Character == 'Ú' || Character == ' ' || Character == 'ñ' || Character == 'Ñ')
                    {
                        StringBuilder.Append(Character);
                    }
                    else
                    {
                        StringBuilder.Append(" ");
                    }
                }
            }

            return StringBuilder.ToString();
        }

        public static async Task ProcessFile(AppSettings AppSettings, Logger Logger, DatabaseContext DatabaseContext, FtpClient FtpClient, FtpListItem FtpListItem)
        {
            try
            {
                CallLog CallLog = await DatabaseContext.CallLog.FirstOrDefaultAsync(DataRow => DataRow.Name == FtpListItem.Name);

                bool UpdateCall = false;

                if (CallLog != null)
                {
                    if (CallLog.ChangeDate < FtpClient.GetModifiedTime(FtpListItem.FullName))
                    {
                        UpdateCall = true;
                    }
                }

                if (UpdateCall)
                {
                    CallLog.Size = FtpClient.GetFileSize(FtpListItem.FullName).ToString();
                    CallLog.ChangeDate = FtpClient.GetModifiedTime(FtpListItem.FullName);

                    if (FtpClient.DownloadBytes(out byte[] ByteArray, FtpListItem.FullName))
                    {
                        if (ByteArray.Length > 28)
                        {
                            long ByteRate = BitConverter.ToInt32(new[] { ByteArray[28], ByteArray[29], ByteArray[30], ByteArray[31] }, 0);

                            CallLog.Duration = ((ByteArray.Length - 8) / ByteRate).ToString();
                        }

                        if (!string.IsNullOrWhiteSpace(CallLog.Duration))
                        {
                            if (CallLog.Duration != "0")
                            {
                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processzyxme/uploadfile";

                                using HttpClient HttpClient = new HttpClient();

                                dynamic ObjectContent = new ExpandoObject();

                                ObjectContent.FileName = FtpListItem.Name;
                                ObjectContent.FileData = ByteArray;

                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    UploadFile UploadFile = JsonConvert.DeserializeObject<UploadFile>(HttpResponseContent);

                                    if (UploadFile.Success)
                                    {
                                        CallLog.StorageLink = UploadFile.Url;
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(CallLog.StorageLink))
                                {
                                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processzyxme/watsonspeechtotext";

                                    ObjectContent = new ExpandoObject();

                                    ObjectContent.url = CallLog.StorageLink;
                                    ObjectContent.tipo = "speechToText";
                                    ObjectContent.respuesta = string.Empty;
                                    ObjectContent.mensaje = string.Empty;

                                    StringContent = JsonConvert.SerializeObject(ObjectContent);

                                    HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                    {
                                        dynamic WatsonSTT = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                        CallLog.ConversationText = WatsonSTT.mensaje;
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(CallLog.StorageLink))
                                {
                                    DatabaseContext.CallLog.Update(CallLog);
                                    DatabaseContext.SaveChanges();
                                }
                            }
                        }
                    }
                }

                if (CallLog == null)
                {
                    CallLog = new CallLog()
                    {
                        Size = FtpClient.GetFileSize(FtpListItem.FullName).ToString(),
                        ChangeDate = FtpClient.GetModifiedTime(FtpListItem.FullName),
                        CreateDate = FtpClient.GetModifiedTime(FtpListItem.FullName),
                        Receptor = FtpListItem.Name.Split("-")[1],
                        Emitter = FtpListItem.Name.Split("-")[2],
                        Name = FtpListItem.Name,
                        ChangeBy = "scheduler",
                        CreateBy = "scheduler"
                    };

                    if (FtpClient.DownloadBytes(out byte[] ByteArray, FtpListItem.FullName))
                    {
                        if (ByteArray.Length > 28)
                        {
                            long ByteRate = BitConverter.ToInt32(new[] { ByteArray[28], ByteArray[29], ByteArray[30], ByteArray[31] }, 0);

                            CallLog.Duration = ((ByteArray.Length - 8) / ByteRate).ToString();
                        }

                        if (!string.IsNullOrWhiteSpace(CallLog.Duration))
                        {
                            if (CallLog.Duration != "0")
                            {
                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processzyxme/uploadfile";

                                using HttpClient HttpClient = new HttpClient();

                                dynamic ObjectContent = new ExpandoObject();

                                ObjectContent.FileName = FtpListItem.Name;
                                ObjectContent.FileData = ByteArray;

                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    UploadFile UploadFile = JsonConvert.DeserializeObject<UploadFile>(HttpResponseContent);

                                    if (UploadFile.Success)
                                    {
                                        CallLog.StorageLink = UploadFile.Url;
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(CallLog.StorageLink))
                                {
                                    HttpEndpoint = $"{AppSettings.ZyxMeSettings.BridgeEndpoint}api/processzyxme/watsonspeechtotext";

                                    ObjectContent = new ExpandoObject();

                                    ObjectContent.url = CallLog.StorageLink;
                                    ObjectContent.tipo = "speechToText";
                                    ObjectContent.respuesta = string.Empty;
                                    ObjectContent.mensaje = string.Empty;

                                    StringContent = JsonConvert.SerializeObject(ObjectContent);

                                    HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                    {
                                        dynamic WatsonSTT = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                        CallLog.ConversationText = WatsonSTT.mensaje;
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(CallLog.StorageLink))
                                {
                                    DatabaseContext.CallLog.Add(CallLog);
                                    DatabaseContext.SaveChanges();

                                    List<CallConversation> CallConversationList = await DatabaseContext.CallConversation.FromSqlRaw(StoredProcedure.CallConversationSelect, CallLog.Emitter).ToListAsync();

                                    if (CallConversationList != null)
                                    {
                                        foreach (var CallConversation in CallConversationList)
                                        {
                                            List<CallInteraction> CallInteractionList = await DatabaseContext.CallInteraction.FromSqlRaw(StoredProcedure.CallInteractionSelect, CallConversation.ConversationId, CallConversation.CorpId, CallConversation.OrgId).ToListAsync();

                                            if (CallInteractionList != null)
                                            {
                                                foreach (var CallInteraction in CallInteractionList)
                                                {
                                                    DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.CallInteractionUpdate, CallLog.StorageLink, CallInteraction.CorpId, CallInteraction.OrgId, CallInteraction.PersonId, CallInteraction.ConversationId, CallInteraction.InteractionId);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessFile").Error(Exception, "Exception found:");
            }
        }

        public static async Task<bool> ProcessUpdateChannel(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateChannel");

            bool Success = false;

            try
            {
                List<ActiveChannel> ActiveChannelList = await DatabaseContext.ActiveChannel.FromSqlRaw(StoredProcedure.SelectActiveChannel).ToListAsync();

                if (ActiveChannelList != null)
                {
                    foreach (var ActiveChannel in ActiveChannelList)
                    {
                        if (ActiveChannelList.Count == 0)
                        {
                            Success = true;
                        }
                        else
                        {
                            try
                            {
                                bool IsActive = true;

                                if (string.IsNullOrWhiteSpace(ActiveChannel.Schedule) || ActiveChannel.Schedule == "[]")
                                {
                                    string TimeInterval = "+ '0";

                                    if (ActiveChannel.TimeZoneOffset != null)
                                    {
                                        TimeInterval = TimeInterval?.Replace("0", Convert.ToInt32(Math.Abs((double)ActiveChannel.TimeZoneOffset)).ToString());

                                        if (ActiveChannel.TimeZoneOffset < 0)
                                        {
                                            TimeInterval = TimeInterval?.Replace("+", "-");
                                        }
                                    }

                                    string ScheduleSelect = StoredProcedure.ChannelScheduleSelect?.Replace("##TIMEZONEOFFSET##", TimeInterval);

                                    List<ChannelSchedule> ChannelScheduleList = await DatabaseContext.ChannelSchedule.FromSqlRaw(ScheduleSelect, ActiveChannel.CorpId, ActiveChannel.OrgId, ActiveChannel.CommunicationChannelId, ActiveChannel.CorpId, ActiveChannel.OrgId, ActiveChannel.CommunicationChannelId).ToListAsync();

                                    if (ChannelScheduleList != null)
                                    {
                                        foreach (var ChannelSchedule in ChannelScheduleList)
                                        {
                                            IsActive = ChannelSchedule.Atencion;
                                        }
                                    }
                                }
                                else
                                {
                                    List<Schedule> ChannelScheduleList = JsonConvert.DeserializeObject<List<Schedule>>(ActiveChannel.Schedule);

                                    DateTime DateTimeSchedule = DateTime.UtcNow.AddHours((double)ActiveChannel.TimeZoneOffset);

                                    Schedule Schedule = ChannelScheduleList.Find(DatabaseRow => DatabaseRow.Days.Contains(((int)DateTimeSchedule.DayOfWeek).ToString()));

                                    if (Schedule != null)
                                    {
                                        TimeSpan TimeSpanStart = new TimeSpan(int.Parse(Schedule.Start.Split(":").First()), int.Parse(Schedule.Start.Split(":").Last()), 0);

                                        TimeSpan TimeSpanEnd = new TimeSpan(int.Parse(Schedule.End.Split(":").First()), int.Parse(Schedule.End.Split(":").Last()), 0);

                                        TimeSpan TimeSpanNow = DateTimeSchedule.TimeOfDay;

                                        if ((TimeSpanNow < TimeSpanStart) || (TimeSpanNow > TimeSpanEnd))
                                        {
                                            IsActive = false;
                                        }
                                    }
                                    else
                                    {
                                        IsActive = false;
                                    }
                                }

                                if (ActiveChannel.ChannelActive != IsActive)
                                {
                                    DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.UpdateActiveChannel, IsActive, ActiveChannel.CorpId, ActiveChannel.OrgId, ActiveChannel.CommunicationChannelId);
                                }

                                Success = true;
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessUpdateChannel: {ActiveChannel.CommunicationChannelId}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateChannel").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateSubscription(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateSubscription");

            bool Success = false;

            try
            {
                List<ServiceSubscription> ServiceSubscriptionList = await DatabaseContext.ServiceSubscription.Where(DataRow => (DataRow.SubscriptionDateEnd <= DateTime.UtcNow || DataRow.SubscriptionDateEnd == null) && DataRow.Status == "ACTIVO").ToListAsync();

                if (ServiceSubscriptionList != null)
                {
                    if (ServiceSubscriptionList.Count > 0)
                    {
                        foreach (var ServiceSubscription in ServiceSubscriptionList)
                        {
                            try
                            {
                                switch (ServiceSubscription.Type.ToUpper())
                                {
                                    case "GOOGLE-GMAIL":
                                        if (!string.IsNullOrWhiteSpace(ServiceSubscription.Account))
                                        {
                                            ServiceToken ServiceToken = await DatabaseContext.ServiceToken.FirstOrDefaultAsync(DataRow => DataRow.Account == ServiceSubscription.Account && DataRow.Type == "GOOGLE" && DataRow.Status == "ACTIVO");

                                            if (ServiceToken != null)
                                            {
                                                dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceSubscription.ExtraData);

                                                GoogleCredential UserCredential = GoogleCredential.FromAccessToken(ServiceToken.AccessToken);

                                                GmailService GmailServiceClient = new GmailService(new BaseClientService.Initializer()
                                                {
                                                    HttpClientInitializer = UserCredential
                                                });

                                                WatchRequest WatchRequest = new WatchRequest()
                                                {
                                                    LabelFilterAction = "include",
                                                    LabelIds = new List<string> { "INBOX" },
                                                    TopicName = ExtraData.topicName
                                                };

                                                WatchResponse WatchResponse = GmailServiceClient.Users.Watch(WatchRequest, ServiceSubscription.Account).Execute();

                                                ServiceSubscription.ChangeBy = "scheduler";
                                                ServiceSubscription.ChangeDate = DateTime.UtcNow;
                                                ServiceSubscription.SubscriptionDateEnd = DateTime.UtcNow.AddMinutes((long)ServiceSubscription.Interval);
                                                ServiceSubscription.SubscriptionDateStart = DateTime.UtcNow;

                                                DatabaseContext.ServiceSubscription.Update(ServiceSubscription);
                                                DatabaseContext.SaveChanges();

                                                Success = true;
                                            }
                                        }
                                        break;

                                    case "OUTLOOK":
                                        if (!string.IsNullOrWhiteSpace(ServiceSubscription.Account))
                                        {
                                            ServiceToken ServiceToken = await DatabaseContext.ServiceToken.FirstOrDefaultAsync(DataRow => DataRow.Account == ServiceSubscription.Account && DataRow.Type == "OUTLOOK" && DataRow.Status == "ACTIVO");

                                            if (ServiceToken != null)
                                            {
                                                dynamic ExtraData = JsonConvert.DeserializeObject<dynamic>(ServiceSubscription.ExtraData);

                                                string HttpEndpoint = $"{ExtraData.subscriptionEndpoint}subscriptions";

                                                using HttpClient HttpClient = new HttpClient();

                                                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken.AccessToken);

                                                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                                Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                                                HttpResponseMessage HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                if (HttpResponseMessage.IsSuccessStatusCode)
                                                {
                                                    Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                                                    dynamic SubscriptionResponse = JsonConvert.DeserializeObject<dynamic>(HttpResponseContent);

                                                    if (SubscriptionResponse.value != null)
                                                    {
                                                        foreach (var Subscription in SubscriptionResponse.value)
                                                        {
                                                            if (!string.IsNullOrWhiteSpace((string)Subscription.id) && !string.IsNullOrWhiteSpace((string)Subscription.notificationUrl))
                                                            {
                                                                if ((string)Subscription.notificationUrl == ServiceSubscription.Webhook)
                                                                {
                                                                    HttpEndpoint = $"{ExtraData.subscriptionEndpoint}subscriptions/{(string)Subscription.id}";

                                                                    Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http DELETE endpoint: {HttpEndpoint}", HttpEndpoint);

                                                                    HttpResponseMessage = await HttpClient.DeleteAsync(HttpEndpoint);

                                                                    HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                                    if (HttpResponseMessage.IsSuccessStatusCode)
                                                                    {
                                                                        Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http DELETE response: {HttpResponseContent}", HttpResponseContent);
                                                                    }
                                                                    else
                                                                    {
                                                                        Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http DELETE response: {HttpResponseContent}", HttpResponseContent);

                                                                        Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http DELETE: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http GET response: {HttpResponseContent}", HttpResponseContent);

                                                    Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http GET: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                }

                                                HttpEndpoint = $"{ExtraData.subscriptionEndpoint}subscriptions";

                                                dynamic ObjectContent = new ExpandoObject();

                                                ObjectContent.changeType = ExtraData.changeType;
                                                ObjectContent.clientState = ServiceSubscription.Account;
                                                ObjectContent.expirationDateTime = DateTime.UtcNow.AddMinutes(ExtraData.expiration).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                                                ObjectContent.notificationUrl = ServiceSubscription.Webhook;
                                                ObjectContent.resource = ExtraData.resource;

                                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                                Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                                Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST body: {StringContent}", StringContent);

                                                HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                                HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                                if (HttpResponseMessage.IsSuccessStatusCode)
                                                {
                                                    Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                    Success = true;

                                                    ServiceSubscription.ChangeBy = "scheduler";
                                                    ServiceSubscription.ChangeDate = DateTime.UtcNow;
                                                    ServiceSubscription.SubscriptionDateEnd = DateTime.UtcNow.AddMinutes((long)ServiceSubscription.Interval);
                                                    ServiceSubscription.SubscriptionDateStart = DateTime.UtcNow;

                                                    DatabaseContext.ServiceSubscription.Update(ServiceSubscription);
                                                    DatabaseContext.SaveChanges();

                                                    Success = true;
                                                }
                                                else
                                                {
                                                    Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                                    Logger.ForContext("Context", $"UpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessUpdateSubscription: {ServiceSubscription.ServiceSubscriptionId}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                    else
                    {
                        Success = true;
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateSubscription").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateSession(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateSession");

            bool Success = false;

            try
            {
                List<SessionData> SessionDataList = await DatabaseContext.SessionData.FromSqlRaw(StoredProcedure.SessionExpiredSelect).ToListAsync();

                if (SessionDataList != null)
                {
                    if (SessionDataList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        DatabaseContext.Database.SetCommandTimeout(7200);

                        foreach (var SessionData in SessionDataList)
                        {
                            try
                            {
                                string HttpEndpoint = $"{AppSettings.ZyxMeSettings.AppEndpoint}inbox/connuserhub";

                                using HttpClient HttpClient = new HttpClient();

                                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                dynamic ObjectContent = new ExpandoObject();

                                ObjectContent.corpid = SessionData.CorpId;
                                ObjectContent.userid = SessionData.UserId;
                                ObjectContent.orgid = SessionData.OrgId;
                                ObjectContent.isconnected = false;

                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                Logger.ForContext("Context", $"ProcessUpdateSession: {SessionData.UserId}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                Logger.ForContext("Context", $"ProcessUpdateSession: {SessionData.UserId}").Debug("Http POST body: {StringContent}", StringContent);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    Logger.ForContext("Context", $"ProcessUpdateSession: {SessionData.UserId}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Success = true;
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessUpdateSession: {SessionData.UserId}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", $"ProcessUpdateSession: {SessionData.UserId}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessUpdateSession: {SessionData.UserId}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateSession").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessUpdateUser(AppSettings AppSettings, DatabaseContext DatabaseContext)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "UpdateUser");

            bool Success = false;

            try
            {
                List<ActiveOrganization> ActiveOrganizationList = await DatabaseContext.ActiveOrganization.FromSqlRaw(StoredProcedure.ActiveOrganizationSelect).ToListAsync();

                if (ActiveOrganizationList != null)
                {
                    if (ActiveOrganizationList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        DatabaseContext.Database.SetCommandTimeout(7200);

                        foreach (var ActiveOrganization in ActiveOrganizationList)
                        {
                            try
                            {
                                DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.UserProductivitySelect, ActiveOrganization.CorpId, ActiveOrganization.OrgId, DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"));
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessUpdateUser: {ActiveOrganization.OrgId}").Error(Exception, "Exception found:");
                            }

                            Success = true;
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessUpdateUser").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessWitaiCreateApp(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "WitaiCreateApp");

            bool Success = false;

            try
            {
                List<WitaiCron> WitaiCronList = await DatabaseContext.WitaiCron.FromSqlRaw(StoredProcedure.SelectWitaiCron).ToListAsync();

                if (WitaiCronList != null)
                {
                    if (WitaiCronList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        foreach (var WitaiCron in WitaiCronList)
                        {
                            try
                            {
                                string HttpEndpoint = $"{TaskBody.Endpoint}/apps?v={TaskBody.Version}";

                                dynamic ObjectContent = new ExpandoObject();

                                ObjectContent.name = $"{TaskBody.Environment}_{WitaiCron.Name}";
                                ObjectContent.lang = !string.IsNullOrWhiteSpace(WitaiCron.Lang) ? WitaiCron.Lang : "es";
                                ObjectContent.timezone = !string.IsNullOrWhiteSpace(WitaiCron.Timezone) ? WitaiCron.Timezone : "America/Lima";
                                ObjectContent.@private = true;

                                string StringContent = JsonConvert.SerializeObject(ObjectContent);

                                Logger.ForContext("Context", $"ProcessWitaiCreateApp: {WitaiCron.Id}").Debug("Http POST endpoint: {HttpEndpoint}", HttpEndpoint);

                                Logger.ForContext("Context", $"ProcessWitaiCreateApp: {WitaiCron.Id}").Debug("Http POST body: {StringContent}", StringContent);

                                using HttpClient HttpClient = new HttpClient();

                                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TaskBody.Token);

                                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                HttpResponseMessage HttpResponseMessage = await HttpClient.PostAsync(HttpEndpoint, new StringContent(StringContent, Encoding.UTF8, "application/json"));

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    Logger.ForContext("Context", $"ProcessWitaiCreateApp: {WitaiCron.Id}").Debug("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    WitaiResponse WitaiResponse = JsonConvert.DeserializeObject<WitaiResponse>(HttpResponseContent);

                                    if (WitaiResponse != null)
                                    {
                                        DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.SelectWitaiConfig, WitaiCron.CorpId, WitaiCron.OrgId, WitaiCron.Id, WitaiResponse.AppId, WitaiResponse.AccessToken);

                                        Success = true;
                                    }
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessWitaiCreateApp: {WitaiCron.Id}").Warning("Http POST response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", $"ProcessWitaiCreateApp: {WitaiCron.Id}").Warning("Unsuccessful http POST: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessWitaiCreateApp: {WitaiCron.Id}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessWitaiCreateApp").Error(Exception, "Exception found:");
            }

            return Success;
        }

        public static async Task<bool> ProcessWitaiUpdateTraining(AppSettings AppSettings, DatabaseContext DatabaseContext, TaskBody TaskBody)
        {
            Logger Logger = LoggerCore.ConfigureLogger(AppSettings, "WitaiUpdateTraining");

            bool Success = false;

            try
            {
                List<WitaiSchedule> WitaiScheduleList = await DatabaseContext.WitaiSchedule.FromSqlRaw(StoredProcedure.SelectWitaiSchedule).ToListAsync();

                if (WitaiScheduleList != null)
                {
                    if (WitaiScheduleList.Count == 0)
                    {
                        Success = true;
                    }
                    else
                    {
                        foreach (var WitaiSchedule in WitaiScheduleList)
                        {
                            try
                            {
                                string HttpEndpoint = $"{TaskBody.Endpoint}/apps/{WitaiSchedule.AppId}?v={TaskBody.Version}";

                                Logger.ForContext("Context", $"ProcessWitaiUpdateTraining: {WitaiSchedule.Id}").Debug("Http GET endpoint: {HttpEndpoint}", HttpEndpoint);

                                using HttpClient HttpClient = new HttpClient();

                                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", WitaiSchedule.Token);

                                HttpResponseMessage HttpResponseMessage = await HttpClient.GetAsync(HttpEndpoint);

                                string HttpResponseContent = await HttpResponseMessage.Content.ReadAsStringAsync();

                                if (HttpResponseMessage.IsSuccessStatusCode)
                                {
                                    Logger.ForContext("Context", $"ProcessWitaiUpdateTraining: {WitaiSchedule.Id}").Debug("Http GET response: {HttpResponseContent}", HttpResponseContent);

                                    WitaiTraining WitaiTraining = JsonConvert.DeserializeObject<WitaiTraining>(HttpResponseContent);

                                    if (WitaiTraining != null)
                                    {
                                        DatabaseContext.Database.ExecuteSqlRaw(StoredProcedure.SelectWitaiWorker, WitaiSchedule.CorpId, WitaiSchedule.OrgId, WitaiSchedule.Id, WitaiTraining.TrainingStatus);

                                        Success = true;
                                    }
                                }
                                else
                                {
                                    Logger.ForContext("Context", $"ProcessWitaiUpdateTraining: {WitaiSchedule.Id}").Warning("Http GET response: {HttpResponseContent}", HttpResponseContent);

                                    Logger.ForContext("Context", $"ProcessWitaiUpdateTraining: {WitaiSchedule.Id}").Warning("Unsuccessful http GET: {ReasonPhrase}", HttpResponseMessage.ReasonPhrase);
                                }
                            }
                            catch (Exception Exception)
                            {
                                Logger.ForContext("Context", $"ProcessWitaiUpdateTraining: {WitaiSchedule.Id}").Error(Exception, "Exception found:");
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ProcessWitaiUpdateTraining").Error(Exception, "Exception found:");
            }

            return Success;
        }
    }
}