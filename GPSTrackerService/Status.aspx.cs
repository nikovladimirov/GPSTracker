using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GPSTrackerService
{
    public partial class Status : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(TrackerService.Instance != null && TrackerService.Instance.TrackerServer != null)
                Response.Write(string.Format("Threads: {0}<br><br>Data:<br>{1}", TrackerService.Instance.TrackerServer.Connections.Count, string.Join("<br>", TrackerService.Instance.TrackerServer.LastData)));
        }
    }
}