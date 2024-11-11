using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class MatchInfoToTitleConverter : IValueConverter {
        // Vrij 1 - Meerdaal Leuven D (PVLBH11/016)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var ms = value as MatchStartInfo;
            if (ms == null)
                return value;
            return Safe.Format("{0} — {1} ({2})", ms.HomeTeam ?? "?", ms.AwayTeam ?? "?", ms.MatchId ?? "?");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
