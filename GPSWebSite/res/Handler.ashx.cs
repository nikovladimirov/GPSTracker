using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using GPSTrackerServiceAPI;

namespace GPSTrackerService
{
    /// <summary>
    /// Summary description for Handler
    /// </summary>
    public class Handler : IHttpHandler
    {
        public const int GMToffset = 3;

        private static int stopMinutes = int.Parse(ConfigurationManager.AppSettings["StopIntervalMin"]);
        public const int OffColorAfterMinutes = 20;
        public static string DefaultColor;
        public void ProcessRequest(HttpContext context)
        {
            var factory = new ChannelFactory<ITrackerService>(
                new BasicHttpBinding() { MaxReceivedMessageSize = 2147483647, MaxBufferSize = 2147483647 },
                ConfigurationManager.AppSettings["service"]);
            var service = factory.CreateChannel();

            var id = "027044493575";
            DefaultColor = ConfigurationManager.AppSettings["DefaultColorLine"];

            List<GPSMessage> list = null;

            var tmpMaxCount = context.Request.Form["count"];
            int maxCount;
            if (!int.TryParse(tmpMaxCount, out maxCount))
                maxCount = 100;

            var tmplastHours = context.Request.Form["lastHours"];
            int lastHours;
            if (!int.TryParse(tmplastHours, out lastHours))
                lastHours = 24;

            var speedColor = bool.Parse(context.Request.Form["speedColor"]);
            var sourceDB = bool.Parse(context.Request.Form["fromDB"]);
            if (!sourceDB)
            {
                list = service.GetLastData(id, lastHours, maxCount);
                if (list == null || list.Count == 0)
                    list = service.GetLastData(id, 240, 50);
            }
            else
            {
                var to = DateTime.Parse(context.Request.Form["historyTo"]).AddDays(1);
                var from = DateTime.Parse(context.Request.Form["historyFrom"]);
                list = service.GetIntervalData(id, from, to, maxCount);
            }

            var array = PrepareArray(list, speedColor);

            var serializer = new JavaScriptSerializer();

            var output = string.Format("var data = {0};", serializer.Serialize(array));

            var last = list.LastOrDefault();
            if (last != null)
                output += string.Format("var lastPoint = [[\"{0}\",\"{1}\"], \"{2}\", \"{3}\"];", last.Latitude.ToStringDegree(), last.Longitude.ToStringDegree(), last.Description,
                    (last.DateTime.AddMinutes(OffColorAfterMinutes) > DateTime.Now ? "#00FF00" : "#FF0000"));
            else
            {
                output += "var lastPoint=null;";
            }


            context.Response.ContentType = "text/plain";
            context.Response.Write(output);
            context.Response.End();
            context.Response.Cache.SetNoServerCaching();
        }

        public class GPSData
        {
            private int start = 1;
            private int stop = 1;

            public GPSData()
            {
                Arrows = new List<GPSMessage>();
                Stops = new List<GPSMessage>();
                Starts = new List<GPSMessage>();
            }

            public List<GPSMessage> Arrows { get; set; }
            public List<GPSMessage> Stops { get; set; }
            public List<GPSMessage> Starts { get; set; }

            public void AddArrow(List<GPSMessage> data, bool speedColor)
            {
                for (int i = 1; i < data.Count; i++)
                {
                    data[i].LatitudeStart = data[i - 1].Latitude;
                    data[i].LongitudeStart = data[i - 1].Longitude;
                    data[i].Description = string.Format("{0}<br>Скорость: {1}км/ч", data[i].DateTime.ToString("HH:mm:ss dd.MM.yy"), data[i].Speed);

                    if (speedColor)
                        data[i].Color = GetColorFromSpeed(data[i].Speed);
                    else
                        data[i].Color = DefaultColor;

                    Arrows.Add(data[i]);
                }
            }

            private const string Maroon = "#800000";
            private const string Red = "#ff3300";
            private const string Orange = "#ff9900";
            private const string Orange2 = "#ff6600";
            private const string DarkGreen = "#669900";
            private const string Green = "#33cc33";
            private const string Blue = "#0066ff";
            private const string Gray = "#75e5e5";

            private string GetColorFromSpeed(int speed)
            {
                if (speed > 130)
                    return Maroon;
                if (speed > 110)
                    return Red;
                //if (speed > 90)
                //    return Orange2;
                if (speed > 80)
                    return Orange;
                if (speed > 60)
                    return DarkGreen;
                if (speed > 20)
                    return Green;

                return Blue;
            }

            public void AddStop(GPSMessage msg)
            {
                msg.Market = stop++.ToString();
                msg.Description = msg.DateTime.ToString("dd.MM.yy HH:mm:ss");
                Stops.Add(msg);
            }

            public void AddStart(GPSMessage msg)
            {
                msg.Market = start++.ToString();
                msg.Description = msg.DateTime.ToString("dd.MM.yy HH:mm:ss");
                Starts.Add(msg);
            }
        }


        private const int ArrowLength = Int32.MaxValue;
        private GPSData PrepareArray(List<GPSMessage> list, bool speedColor)
        {
            GPSMessage last = null;

            var result = new GPSData();

            var arrow = new List<GPSMessage>();
            int arrowCount = 0;
            if (list.Count > 0)
                result.AddStart(list[0]);
            for (int i = 0; i < list.Count; i++)
            {

                if (arrow.Count == ArrowLength || last != null && (list[i].DateTime - last.DateTime).TotalMinutes > stopMinutes)
                {
                    result.AddArrow(arrow, speedColor);
                    arrow = new List<GPSMessage>();
                    arrowCount = 0;
                    result.AddStop(list[i - 1]);
                    result.AddStart(list[i]);
                }

                last = list[i];

                arrow.Add(list[i]);
                arrowCount++;
            }

            if (arrow.Count > 0)
            {
                result.AddStop(arrow[arrow.Count - 1]);
                result.AddArrow(arrow, speedColor);
            }

            return result;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }


}