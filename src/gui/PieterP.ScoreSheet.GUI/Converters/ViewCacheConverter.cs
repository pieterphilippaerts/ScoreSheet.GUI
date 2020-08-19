using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using static PieterP.ScoreSheet.ViewModels.MainWindowViewModel;

namespace PieterP.ScoreSheet.GUI.Converters {
    // the ScoreSheet views are expensive to create, so we cache them
    public class ViewCacheConverter : IValueConverter {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return null;

            lock (_cache) {
                if (_cache.TryGetValue(value, out var cached))
                    return cached;

                var fe = Load(value.GetType());
                if (fe != null) {
                    fe.DataContext = value;
                    _cache[value] = fe;
                    return fe;
                }

                return value;
            }
        }

        private static FrameworkElement? Load(Type t) {
            foreach (var v in App.Current.Resources.Values) {
                var dtemp = v as DataTemplate;
                if (dtemp != null && t.Equals(dtemp.DataType)) {
                    return dtemp.LoadContent() as FrameworkElement;
                }
            }
            return null;
        }
       
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static Dictionary<object, DependencyObject> _cache = new Dictionary<object, DependencyObject>();
        private static FrameworkElement? _element;
    }
}
