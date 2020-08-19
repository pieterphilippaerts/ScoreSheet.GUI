using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.GUI.ViewModels;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class ZoomableConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var zoomable = values[0]  as IZoomable;
            if (zoomable == null)
                return values[0];
            return new ZoomableViewModel(values[0], values[1] as Cell<float>);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
