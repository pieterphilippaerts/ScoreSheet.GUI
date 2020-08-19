using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Services.Json {
    public class TranslatedString {
        public TranslatedString() { }
        public TranslatedString(string nl, string fr, string en) {
            this.Nl = nl;
            this.Fr = fr;
            this.En = en;
        }
        public string Nl { get; set; }
        public string Fr { get; set; }
        public string En { get; set; }
    }
}