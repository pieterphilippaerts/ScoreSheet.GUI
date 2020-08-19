using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class Match : MatchStartInfo {
        public string? UniqueId { get; set; } // this cannot be modified by the user
        public string? EndHour { get; set; }

        public List<PlayerInfo>? HomePlayers { get; set; }
        public List<string>? HomeDoubles { get; set; }
        public List<int?>? HomeSubstitutes { get; set; }
        public bool HomeTeamForfeit { get; set; }

        public List<PlayerInfo>? AwayPlayers { get; set; }
        public List<string>? AwayDoubles { get; set; }
        public List<int?>? AwaySubstitutes { get; set; }
        public bool AwayTeamForfeit { get; set; }

        public List<MatchResult>? Results { get; set; }

        public PersonInfo? ChiefReferee { get; set; }
        public PersonInfo? HomeCaptain { get; set; }
        public PersonInfo? AwayCaptain { get; set; }
        public PersonInfo? RoomCommissioner { get; set; }

        public bool? Article632 { get; set; }

        public string? Comments { get; set; }
        public int? HandicapTableId { get; set; }

        public static Match? FromFile(string file) {
            if (!File.Exists(file))
                return null;
            try {
                string json = File.ReadAllText(file, Encoding.UTF8);
                return DataSerializer.Deserialize<Match>(json);
            } catch (Exception e) {
                Logger.Log(e);
                return null;
            }
        }
        public static bool ToFile(Match match, string file) {
            try {
                var json = DataSerializer.Serialize<Match>(match);
                File.WriteAllText(file, json, Encoding.UTF8);
                return true;
            } catch (Exception e) {
                Logger.Log(e);
                return false;
            }
        }
    }
}
