using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class LogicalMultiConverter : IMultiValueConverter {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length == 0)
                return null;
            var bools = values.Cast<bool>();
            bool result = bools.First();
            foreach (var b in bools.Skip(1)) {
                if (LogicOperation == LogicOperations.And) {
                    result = result && b;
                } else {
                    result = result || b;
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public LogicOperations LogicOperation { get; set; }
    }
    public enum LogicOperations { 
        And,
        Or
    }
}
