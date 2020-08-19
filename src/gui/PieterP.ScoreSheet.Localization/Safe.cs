using System;
using System.Collections.Generic;
using System.Text;

namespace PieterP.ScoreSheet.Localization {
    public static class Safe {
        public static string Format(string input, params object[] args) {
            try {
                return string.Format(input, args);
            } catch {
                return input;
            }
        }
    }
}
