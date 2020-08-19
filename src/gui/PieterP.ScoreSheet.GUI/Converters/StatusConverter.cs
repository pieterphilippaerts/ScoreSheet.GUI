using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Score;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class ActiveForegroundConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if ((bool)value) {
                return Brushes.White;
            } else {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class StatusTooltipConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((MatchStatus)value) {
                case MatchStatus.Saved:
                    return Strings.Status_Saved;
                case MatchStatus.Uploaded:
                    return Strings.Status_Uploaded;
                case MatchStatus.Dirty:
                    return Strings.Status_Dirty;
                case MatchStatus.UploadError:
                    return Strings.Status_UploadError;
                case MatchStatus.Emailed:
                    return Strings.Status_Emailed;
                default:
                    return null;
            }
        }     
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class StatusFillConverter : IMultiValueConverter {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            switch ((MatchStatus)values[0]) {
                case MatchStatus.Dirty:
                    if ((bool)values[1])
                        return Brushes.Gold;
                    else 
                        return Brushes.Black;
                //case MatchStatus.Saved:
                //case MatchStatus.Uploaded:
                //    return Brushes.DarkGreen;
                //case MatchStatus.UploadError:
                //    return Brushes.DarkRed;
                default:
                    return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class StatusBackgroundConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var status = (MatchStatus)values[0];
            var isMouseOver = (bool)values[1];
            var isActive = (bool)values[2];
            var colors = new Color[2];
            switch (status) {
                case MatchStatus.Saved:
                case MatchStatus.Uploaded:
                case MatchStatus.Emailed:
                    if (isActive) {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#357B67");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#025A41");
                    } else if (isMouseOver) {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#96DFCA");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#68BFA6");
                    } else {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#C8EBD0");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#A6ECC1");
                    }
                    break;
                case MatchStatus.UploadError:
                    if (isActive) {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#FF0000");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#7F0000");
                    } else if (isMouseOver) {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#FFB2BD");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#FF7287");
                    } else {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#FFE8EB");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#FFD1D7");
                    }
                    break;
                case MatchStatus.Dirty:
                    //if (isActive) {
                    //    colors[0] = (Color)ColorConverter.ConvertFromString("#C4A000");
                    //    colors[1] = (Color)ColorConverter.ConvertFromString("#967500");
                    //} else if (isMouseOver) {
                    //    colors[0] = (Color)ColorConverter.ConvertFromString("#FFE696");
                    //    colors[1] = (Color)ColorConverter.ConvertFromString("#FFDA56");
                    //} else {
                    //    colors[0] = (Color)ColorConverter.ConvertFromString("#FFF1CC");
                    //    colors[1] = (Color)ColorConverter.ConvertFromString("#FFE084");
                    //}
                    //break;
                case MatchStatus.None:
                default:
                    if (isActive) {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#357B67");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#025A41");
                    } else if (isMouseOver) {
                        colors[0] = (Color)ColorConverter.ConvertFromString("#96DFCA");
                        colors[1] = (Color)ColorConverter.ConvertFromString("#68BFA6");
                    } else {
                        colors[0] = Colors.White;
                        colors[1] = (Color)ColorConverter.ConvertFromString("#D5D5D5");
                    }
                    break;
            }
            var stops = new GradientStopCollection(2);
            stops.Add(new GradientStop() { Offset = 0, Color = colors[0] });
            stops.Add(new GradientStop() { Offset = 1, Color = colors[1] });
            return new LinearGradientBrush() {
                StartPoint = new System.Windows.Point(1, 0),
                EndPoint = new System.Windows.Point(0.5, 1),
                GradientStops = stops
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
