using PieterP.ScoreSheet.GUI.ViewModels;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.Shared.Cells;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters
{
    public class UpdateProgressToStringConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            try {

                var total = (int)values[0];
                var progress = (int)values[1];

                if (total <= 0)
                    return Safe.Format(Strings.UpdateProgressToString_Text, BytesToString(progress));
                else
                    return Safe.Format(Strings.UpdateProgressToString_Text, $"{BytesToString(progress)}/{BytesToString(total)} ({progress / (double)total:P1})");
            } catch {
                return "";
            }
        }

        private string BytesToString(int bytes) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024) {
                order++;
                len = len / 1024;
            }
            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{len:0.#}{sizes[order]}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
