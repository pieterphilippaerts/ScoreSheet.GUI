using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Localization.Views.Score;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.ScoreSheet.ViewModels.Score;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class MatchInfoToSubtitleConverter : IValueConverter {
        // in afdeling Provinciaal Heren/Dames/Jeugd 2B
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var ms = value as MatchStartInfo;
            if (ms == null)
                return value;
            return Safe.Format(PieterP.ScoreSheet.Localization.Views.Wizards.Resources.OrphanedMatch_InSeries, LevelToString(ms.Level), CategoryToString(ms), ms.Series ?? "");
        }

        private string LevelToString(Level? l) {
            return l switch { 
                Level.Super => Strings.LevelInfo_Super,
                Level.National => Strings.LevelInfo_National,
                Level.Regional => Strings.LevelInfo_Regional,
                Level.Provincial => Strings.LevelInfo_Provincial,
                _ => "?"
            };
        }
        private string CategoryToString(MatchStartInfo ms) {
            if (ms.Youth == true) {
                return Resources.CompetitiveMatchType_Youth;
            } else if (ms.Veterans == true) {
                return Resources.CompetitiveMatchType_Veterans;
            } else if (ms.Cup == true) {
                return Resources.CompetitiveMatchType_Cup;
            } else if (ms.Men == true) {
                return Resources.CompetitiveMatchType_Men;
            } else if (ms.Women == true) {
                return Resources.CompetitiveMatchType_Women;
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
