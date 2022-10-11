using Dapper;
using Newtonsoft.Json;
using Npgsql;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ZyxMeBridge.Models.Common;

namespace ZyxMeBridge.Services
{
    public class DapperService
    {
        public static async Task<T> ExecuteStoredProcedureSingle<T>(AppSettings AppSetting, Logger Logger, string StoredProcedure, dynamic Parameters = null) where T : class
        {
            try
            {
                Logger.ForContext("Context", "ExecuteStoredProcedureSingle").Debug("Stored procedure: {StoredProcedure}", StoredProcedure);

                if (Parameters != null)
                {
                    Logger.ForContext("Context", "ExecuteStoredProcedureSingle").Debug("Parameters: {Parameters}", JsonConvert.SerializeObject(Parameters));
                }

                using NpgsqlConnection NpgsqlConnection = new NpgsqlConnection(AppSetting.ConnectionStrings.ConnectionCredentials);

                NpgsqlConnection.Open();

                IEnumerable<T> QueryResult = await NpgsqlConnection.QueryAsync<T>(StoredProcedure, (object)Parameters, null, null, CommandType.Text);

                return await Task.Run(() => Enumerable.FirstOrDefault(QueryResult));
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ExecuteStoredProcedureSingle").Error(Exception, "Exception found:");

                throw Exception;
            }
        }

        public static async Task<dynamic> ExecuteStoredProcedureMultiple<T>(AppSettings AppSetting, Logger Logger, string StoredProcedure, dynamic Parameters = null) where T : class
        {
            try
            {
                Logger.ForContext("Context", "ExecuteStoredProcedureMultiple").Debug("Stored procedure: {StoredProcedure}", StoredProcedure);

                if (Parameters != null)
                {
                    Logger.ForContext("Context", "ExecuteStoredProcedureMultiple").Debug("Parameters: {Parameters}", JsonConvert.SerializeObject(Parameters));
                }

                using NpgsqlConnection NpgsqlConnection = new NpgsqlConnection(AppSetting.ConnectionStrings.ConnectionCredentials);

                NpgsqlConnection.Open();

                return await NpgsqlConnection.QueryAsync<T>(StoredProcedure, (object)Parameters, null, null, CommandType.Text);
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "ExecuteStoredProcedureMultiple").Error(Exception, "Exception found:");

                throw Exception;
            }
        }
    }
}