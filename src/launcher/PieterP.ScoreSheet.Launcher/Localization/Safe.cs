using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PieterP.ScoreSheet.Launcher.Localization {
    public static class Safe {
        public static string Format(string input, params object[] p) {
            try {
                return string.Format(input, p);
            } catch {
                return input;
            }
        }
    }
}
