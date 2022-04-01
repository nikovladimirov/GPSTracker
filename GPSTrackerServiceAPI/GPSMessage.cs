using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using GPSTrackerServiceAPI;

namespace GPSTrackerService
{
    public class GPSMessage
    {
        public static int offset = 3;

        public GPSMessage(object[] itemArray)
        {
            Id = itemArray[0].ToString();
            DateTime = Convert.ToDateTime(itemArray[1].ToString());
            Longitude = Convert.ToDouble(itemArray[2]);
            Latitude = Convert.ToDouble(itemArray[3]);
            Speed = Convert.ToInt32(itemArray[4]);
            Orientation = Convert.ToDouble(itemArray[5]);
        }

        public GPSMessage()
        {

        }


        public static GPSMessage ParseRow(string row)
        {
            var m = Regex.Match(row, @"^\((.{12})BR00(\d\d)(\d\d)(\d\d)A(\d\d)([\d\.]{7})N(\d\d\d)([\d\.]{7})E(\d\d\d\.\d)(\d\d)(\d\d)(\d\d)(.{6}).{17}\)$");
            if (m.Success)
            {
                var msg = new GPSMessage();
                msg.Id = m.Groups[1].Value;
                //msg.Date = string.Format("{0}.{1}.{2}", m.Groups[4].Value, m.Groups[3].Value, m.Groups[2].Value);
                msg.Latitude = int.Parse(m.Groups[5].Value) + double.Parse(m.Groups[6].Value, CultureInfo.InvariantCulture) / 60;
                msg.Longitude = int.Parse(m.Groups[7].Value) + double.Parse(m.Groups[8].Value, CultureInfo.InvariantCulture) / 60;
                msg.Speed = (int)double.Parse(m.Groups[9].Value, CultureInfo.InvariantCulture);
                //msg.Time = string.Format("{0}:{1}:{2}", int.Parse(m.Groups[10].Value) + TrackerService.GMToffset, m.Groups[11].Value, m.Groups[12].Value);

                msg.DateTime = new DateTime(int.Parse("20" + m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value), int.Parse(m.Groups[10].Value) + offset,
                    int.Parse(m.Groups[11].Value), int.Parse(m.Groups[12].Value));
                msg.Orientation = double.Parse(m.Groups[13].Value, CultureInfo.InvariantCulture);

                return msg;
            }
            return null;
        }



        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double? LongitudeStart { get; set; }
        public double? LatitudeStart { get; set; }


        [ScriptIgnore]
        public double Orientation { get; set; }
        public int Speed { get; set; }
        public DateTime DateTime { get; set; }
        [ScriptIgnore]
        public string Id { get; set; }

        public string Market { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format("'{0}', '{1}', '{2}', '{3}', '{4}', '{5}'", Id, DateTime.ToSQL(), Longitude.ToSQL(), Latitude.ToSQL(), Speed, Orientation.ToSQL());
        }

        public string ToDB()
        {
            return ToString();
        }
    }
}