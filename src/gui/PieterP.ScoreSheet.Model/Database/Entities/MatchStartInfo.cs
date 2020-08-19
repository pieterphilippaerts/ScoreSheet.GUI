using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model.Database.Enums;
#if NETSTANDARD
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif

namespace PieterP.ScoreSheet.Model.Database.Entities {
    public class MatchStartInfo {
        public int? MatchSystemId { get; set; }
        public int? PlayerCategory { get; set; } // this cannot be modified by the user
        public Region? Region { get; set; }
        public bool? IsOfficial { get; set; }

        public string? Venue { get; set; }

        public bool? Men { get; set; }
        public bool? Women { get; set; }
        public bool? Interclub { get; set; }
        public bool? Super { get; set; }
        public bool? Cup { get; set; }
        public bool? Youth { get; set; }
        public bool? Veterans { get; set; }

        public string? Date { get; set; }
        public DateTime? WeekStart { get; set; } // de datum van de maandag van de week waarop de match doorgaat; belangrijk voor bye's
        public Level? Level { get; set; } // afdeling
        public string? StartHour { get; set; }
        public string? MatchId { get; set; }
        public string? Series { get; set; } // reeks

        public string? HomeTeam { get; set; }
        public string? HomeClub { get; set; }

        public string? AwayTeam { get; set; }
        public string? AwayClub { get; set; }

        [JsonIgnore]
        public bool IsBye {
            get {
                return IsByeIndex(HomeClub) || IsByeIndex(AwayClub);
            }
        }
        public static bool IsByeIndex(string? clubId) {
            return clubId == "-" || clubId?.ToLower() == "bye" || clubId?.ToLower() == "vrij";
        }
    }
}