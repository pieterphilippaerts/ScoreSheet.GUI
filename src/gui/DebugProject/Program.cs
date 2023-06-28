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
            var n = PieterP.ScoreSheet.Model.Information.OperatingSystem.Name;

            return;

            try {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, policyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)0x3000 /* Tls13*/;
                ServiceLocator.RegisterType<IRegionFinder, RegionFinder>();
                ServiceLocator.RegisterInstance(new Logger(null, false));

                //ListMatchSystems();
                TryHammering();
            } catch (Exception e ) {
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }
        private static async void ShowRanking(string club) {
            var connector = new TabTConnector();
            var members = await connector.GetMembers(club, 1, await connector.GetActiveSeason());
            foreach (var member in members) {
                Console.WriteLine($"{member.Position}. {member.Firstname} {member.Lastname} ({member.VttlIndex}), rank={member.RankIndex}, status={member.Status}");
            }
        }
        private static async void ShowTeams(string club) {
            var connector = new TabTConnector();
            var teams = await connector.GetTeams(club, await connector.GetActiveSeason());
            foreach (var team in teams) {
                Console.WriteLine($"Ploeg {team.Team} {team.DivisionName} (cat: {team.DivisionCategory}, matchtype: {team.MatchType})");
            }
        }
        public static async void TryHammering() {
                var connector = new TabTConnector();

                int c = 0;
            while (true) {
                try {
                    //var c = await connector.TestAsync();
                    //if (c.Info != null)
                    //Console.WriteLine($"{c.Info.CurrentQuota}/{c.Info.AllowedQuota}");
                    var tasks = new Task[100];
                    for (int i = 0; i < 100; i++) {
                        tasks[i] = connector.GetDivisions(TabTDivisionRegion.National, await connector.GetActiveSeason());
                    }
                    Task.WaitAll(tasks);
                    Console.WriteLine(c);
                    c += 100;
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
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
