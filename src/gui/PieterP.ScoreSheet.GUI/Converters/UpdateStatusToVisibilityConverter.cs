using PieterP.ScoreSheet.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using PieterP.ScoreSheet.ViewModels.Services;

namespace PieterP.ScoreSheet.GUI.Converters
{
    public class UpdateStatusToProgressVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return Visibility.Hidden;

            var status = (UpdateStatus)value;
            if (status == UpdateStatus.DownloadingUpdate)
                return Visibility.Visible;
            return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}