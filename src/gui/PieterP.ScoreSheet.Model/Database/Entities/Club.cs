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
    public class Club {
        public string? Name { get; set; }
        public string? LongName { get; set; }
        public Province? Province { get; set; }
        public string? UniqueIndex { get; set; }
        public List<string>? Venues { get; set; }

        [JsonIgnore]
        public string IndexAndName {
            get {
                return $"{ UniqueIndex } - { LongName }";
            }
        }
    }
}
