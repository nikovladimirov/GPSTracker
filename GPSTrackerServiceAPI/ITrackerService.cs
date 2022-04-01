using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace GPSTrackerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITrackerService" in both code and config file together.
    [ServiceContract]
    public interface ITrackerService
    {
        [OperationContract]
        List<GPSMessage> GetLastData(string id,int lastHours, int maxCount);

        [OperationContract]
        List<GPSMessage> GetIntervalData(string id, DateTime start, DateTime end, int maxCount);
    }

}
