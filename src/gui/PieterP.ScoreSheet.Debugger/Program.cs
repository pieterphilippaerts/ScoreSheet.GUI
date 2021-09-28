using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.GUI.Services;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Debugger {
    public class Program {
        static void Main(string[] args) {
            ServiceLocator.RegisterInstance<DatabaseManager>(new DatabaseManager("default"));
            ServiceLocator.RegisterInstance<Logger>(new Logger(Path.Combine(ServiceLocator.Resolve<DatabaseManager>().ActiveProfilePath, "log.txt"), false));
            ServiceLocator.RegisterType<IRegionFinder, Model.Information.RegionFinder>();
            ServiceLocator.RegisterType<ITimerService, WpfTimerService>();

            ToonPloegen("LK100");

            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }
        private static async void ToonSterktelijst(string club) {
            var connector = new TabTConnector();
            var members = await connector.GetMembers(club, 1);
            foreach(var member in members) {
                Console.WriteLine($"{member.Position}. {member.Firstname} {member.Lastname} ({member.VttlIndex}), rank={member.RankIndex}, status={member.Status}");
            }
        }
        private static async void ToonPloegen(string club) {
            var connector = new TabTConnector();
            var teams = await connector.GetTeams(club);
            foreach (var team in teams) {
                Console.WriteLine($"Ploeg {team.Team} {team.DivisionName} (cat: {team.DivisionCategory}, matchtype: {team.MatchType})");
            }
        }
    }
}
