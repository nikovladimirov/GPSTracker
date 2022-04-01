using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Web.Script.Services;
using System.Web.Services;
using GPSTrackerService.res;
using GPSTrackerServiceAPI;
using NLog;

namespace GPSTrackerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TrackerService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select TrackerService.svc or TrackerService.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TrackerService : ITrackerService
    {
        public static TrackerService Instance { get; private set; }
        public static string ConnectionString { get; private set; }

        public static int GMToffset = 3;
        public static int OffColorAfter = 2;
        public TrackerService()
        {
            Instance = this;
            LogManager.GetCurrentClassLogger().Debug("start service");

            CheckDBFile();

            GMToffset = int.Parse(ConfigurationManager.AppSettings["GMToffset"]);
            var port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            var threadLifeSec = int.Parse(ConfigurationManager.AppSettings["ThreadLifeSec"]);
            var maxQueue = int.Parse(ConfigurationManager.AppSettings["MaxQueue"]);
            TrackerServer = new TrackerServer(port, threadLifeSec, maxQueue);

            LogManager.GetCurrentClassLogger().Debug("service started");
        }

        private void CheckDBFile()
        {
            var dir = ConfigurationManager.AppSettings["DBPath"];
            var file = string.Format(@"{0}\{1}", dir, ConfigurationManager.AppSettings["DBFile"]);
            ConnectionString = string.Format("Data Source={0}", file);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(file))
                File.WriteAllBytes(file, Resource.server);
        }

        public TrackerServer TrackerServer { get; set; }

        public List<GPSMessage> GetLastData(string id, int lastHours, int maxCount)
        {
            var result = GetLastMessages(id).Where(x => x.DateTime.AddHours(lastHours) > DateTime.Now).ToList();

            //result.Take(result.Count - 1).ToArray().Where(x => x.Speed < 1).ForEach(x => result.Remove(x));
            result = result.GetRange(Math.Max(result.Count - maxCount, 0), Math.Min(result.Count, maxCount));

            return result;
        }

        public List<GPSMessage> GetIntervalData(string id, DateTime start, DateTime end, int maxCount)
        {
            List<GPSMessage> result = null;
            SqlExecute.Work(x => result = x.GetMessagesFromDB(id,start, end));
            if(result == null)
                return null;
            while (result.Count > maxCount)
            {
                for (int i = result.Count - 2; i > 0; i -= 2)
                {
                    result.RemoveAt(i);
                }
            }

            return result;
        }
        
        private List<GPSMessage> GetLastMessages(string id)
        {
            var rows = TrackerServer.LastData.Where(x => string.Equals(x.Id, id)).ToList();
            return rows;
        }
    }
}
