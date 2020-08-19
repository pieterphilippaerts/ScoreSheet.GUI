using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Wizards;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class ExportButtonConverter : IMultiValueConverter {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] is int && values[1] is ExportTypes) {
                //bool upload = (bool)parameter;
                var exportType = (ExportTypes)values[1];
                int count = (int)values[0];
                switch (exportType) {
                    case ExportTypes.Upload:
                        return Safe.Format(Strings.ExportButton_Upload, count);
                    case ExportTypes.Email:
                        return Safe.Format(Strings.ExportButton_Email, count);
                    default:
                        return Safe.Format(Strings.ExportButton_Export, count);
                }
            }
            
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
