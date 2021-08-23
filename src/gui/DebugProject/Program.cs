using PieterP.ScoreSheet.Connector;
using PieterP.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DebugProject {
    class Program {
        private class RegionFinder : IRegionFinder {
            public bool IsVttl => true;
        }
        static void Main(string[] args) {
            try {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, policyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)0x3000 /* Tls13*/;
                ServiceLocator.RegisterType<IRegionFinder, RegionFinder>();
                ServiceLocator.RegisterInstance(new Logger(null, false));

                ListMatchSystems();
            } catch (Exception e ) {
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }
        public static async void ListMatchSystems() {
            try {
                var connector = new TabTConnector();
                var results = await connector.GetMatchSystemsAsync();
                Console.WriteLine("MATCH SYSTEMS:\r\n~~~~~~~~~~~~~~");
                foreach (var system in results) {
                    Console.WriteLine($"{system.Name} ({system.UniqueIndex}");
                    Console.WriteLine(" - Singles: " + system.SingleMatchCount);
                    Console.WriteLine(" - Doubles: " + system.DoubleMatchCount);
                    Console.WriteLine(" - Sets: " + system.SetCount);
                    Console.WriteLine(" - Points per set: " + system.PointCount);
                    Console.WriteLine();
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}
