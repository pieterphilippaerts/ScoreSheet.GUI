using PieterP.ScoreSheet.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class DateToWeekConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            //return value;
            if (value is DateTime) {
                var v = (DateTime)value;
                _lastValue = v;
                var from = v.FindStartOfWeek();
                var to = from.AddDays(6);
                if (from.Month == to.Month) {
                    return $"{from.Day} - {to.Day} {from:MMMM}";
                } else {
                    return $"{from.Day} {from:MMM} - {to.Day} {to:MMM}";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
            if (_lastValue != null)
                return _lastValue;
            return value;
        }
        private DateTime? _lastValue;
    }
}
