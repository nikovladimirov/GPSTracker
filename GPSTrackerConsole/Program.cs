using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using GPSTrackerService;

namespace GPSTracker
{
    class Program
    {
        private static ITrackerService TrackerService;

        static void Main(string[] args)
        {

            //var r1 = "(027044493575BR00170429A5959.1706N03011.6680E000.51736430.000000000000L00000000)(027044493575BR00170429A5959.1699N03011.6684E000.31736550.000000000000L00000000) (027044493575BR00170429A5959.1580N03011.6674E001.317312047.88000000000L00000000)(027044493575BP00000027044493575HSO)";

            //var rows = r1.Split(new[] {')'}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim() + ")");
            //foreach (var row in rows)
            //{
            //    Console.WriteLine(row);
            //}
            //Console.ReadKey();
            //return;


            //new TrackerServerAIO(20101);
            //Console.ReadKey();

            //var factory = new ChannelFactory<ITrackerService>(new BasicHttpBinding(), "http://localhost:20100/TrackerService.svc");
            var factory = new ChannelFactory<ITrackerService>(new BasicHttpBinding(), "http://hydra-server.cloudapp.net:20100/TrackerService.svc");
            TrackerService = factory.CreateChannel();
            while (true)
            {
                Console.WriteLine("1. ReadLine");
                Console.WriteLine("0. Exit");
                var r = Console.ReadKey();
                Console.Clear();
                switch (r.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                //Console.WriteLine(TrackerService.GetData(Console.ReadLine()));
                            }
                            catch
                            {
                                
                            }
                        });

                    }
                        break;

                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        return;
                }
            }
        }


    }
}
