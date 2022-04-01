using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Web;
using GPSTrackerServiceAPI;
using NLog;

namespace GPSTrackerService.res
{
    public static class SqlExecute
    {
        public static void Work(Action<SqlConnector> action)
        {
            SqlConnector sqlConnector;

            using (sqlConnector = new SqlConnector(TrackerService.ConnectionString))
            {
                try
                {
                    action(sqlConnector);
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Fatal(ex.ToString());
                }
                finally
                {
                    sqlConnector.Dispose();
                }
            }
        }

        public static void AddMessageInDB(this SqlConnector sql, GPSMessage message)
        {
            sql.SelectQuery(string.Format("INSERT INTO {0} (id,date,longitude,latitude,speed,orientation) VALUES({1})", "messages", message.ToDB()));
        }
        public static List<GPSMessage> GetMessagesFromDB(this SqlConnector sql, string id, DateTime start, DateTime end)
        {
            var dt = sql.SelectQuery(string.Format("SELECT * FROM {0} WHERE id = '{1}' and date > '{2}' and date < '{3}'", "messages", id, start.ToSQL(), end.ToSQL()));
            var result= dt.Select().Select(x => ConvertToGPSMessages(x.ItemArray)).ToList();
            return result;
        }
        private static GPSMessage ConvertToGPSMessages(object[] itemArray)
        {
            return new GPSMessage(itemArray);
        }
    }
    public class SqlConnector : IDisposable
    {
        private readonly SQLiteConnection _sqlite;

        public SqlConnector(string connectionString)
        {
            _sqlite = new SQLiteConnection(connectionString);
            _sqlite.Open();
        }

        public DataTable SelectQuery(string query)
        {
            SQLiteDataAdapter ad;
            var dt = new DataTable();

            Thread.Sleep(50);
            SQLiteCommand cmd;
            cmd = _sqlite.CreateCommand();
            cmd.CommandText = query;  //set the passed query
            ad = new SQLiteDataAdapter(cmd);
            ad.Fill(dt); //fill the datasource

            return dt;
        }

        public void Dispose()
        {
            _sqlite.Close();
        }
    }
}