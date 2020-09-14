using PieterP.ScoreSheet.ViewModels.Score;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class PlayerRankingsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var list = value as List<SinglePlayerInfo>;
            if (list == null)
                return "";
            var sb = new StringBuilder();
            foreach (var spi in list) {
                var rank = spi.Ranking.Value;
                if (rank != null) {
                    if (sb.Length > 0)
                        sb.AppendLine();
                    sb.Append(rank);
                }
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
