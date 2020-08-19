using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class PlayersToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var r = value as IList<PlayerInfo>;
            if (r == null)
                return "";
            return string.Join("/", r.Select(c => EntryToString(c)));
        }
        private string EntryToString(PlayerInfo info) {
            switch (info) {
                case SinglePlayerInfo spi:
                    return spi.Position.ToString();
                case DoublePlayerInfo dpi:
                    return "D";
                default:
                    Logger.Log(LogType.Exception, Errors.PlayerstoString_DataError);
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
