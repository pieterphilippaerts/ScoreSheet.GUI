using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.Shared;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class PlayersToNamesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var r = value as IList<PlayerInfo>;
            if (r == null)
                return "";
            return PlayersToString(r);
        }
        private string PlayersToString(IList<PlayerInfo> players) {
            var sb = new StringBuilder();
            foreach (var p in players) {
                var spi = p as SinglePlayerInfo;
                if (spi != null) {
                    if (sb.Length > 0)
                        sb.Append('/');

                    bool isHome = spi.ParentTeam == spi.ParentTeam.ParentMatch.HomeTeam;

                    if (string.IsNullOrWhiteSpace(spi.Name.Value)) {
                        sb.Append($"{ (isHome ? Strings.PlayersToNames_HomePlayer : Strings.PlayersToNames_AwayPlayer) } { spi.Position }");
                    } else {
                        sb.Append($"{ spi.Name.Value } ({ spi.Ranking.Value })");
                    }
                } else {
                    var dpi = p as DoublePlayerInfo;
                    if (dpi != null) {
                        if (sb.Length > 0)
                            sb.Append('/');
                        sb.Append(Strings.PlayersToNames_Double);
                    }
                }
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}