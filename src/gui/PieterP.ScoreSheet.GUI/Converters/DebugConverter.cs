using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace PieterP.ScoreSheet.GUI.Converters {
    // usage: <TextBlock Text="{Binding SomePath, Converter={src:DebugConverter}}" />
    //      no Resource/StaticResource needed!
    public class DebugConverter : MarkupExtension, IValueConverter {
        private static DebugConverter? _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (_converter == null) {
                _converter = new DebugConverter();
            }
            return _converter;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value; // set breakpoint here to debug your binding
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
}
