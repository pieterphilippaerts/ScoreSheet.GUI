using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PieterP.ScoreSheet.Localization;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Connector {
    public static class TabTExtensions {
        public static CultureInfo ToCulture(this TabTService.SupportedLanguages lang) {
            switch (lang) {
                case TabTService.SupportedLanguages.en:
                    return new CultureInfo("en");
                case TabTService.SupportedLanguages.nl:
                    return new CultureInfo("nl");
                case TabTService.SupportedLanguages.fr:
                    return new CultureInfo("fr");
                default:
                    Logger.Log(LogType.Warning, Safe.Format(Errors.TabT_CultureError, lang.ToString()));
                    return new CultureInfo("en");
            }
        }
#if DEBUG
        public static CultureInfo ToCulture(this DebugService.SupportedLanguages lang) {
            switch (lang) {
                case DebugService.SupportedLanguages.en:
                    return new CultureInfo("en");
                case DebugService.SupportedLanguages.nl:
                    return new CultureInfo("nl");
                case DebugService.SupportedLanguages.fr:
                    return new CultureInfo("fr");
                default:
                    Logger.Log(LogType.Warning, Safe.Format(Errors.TabT_CultureError, lang.ToString()));
                    return new CultureInfo("en");
            }
        }
#endif
    }
}
