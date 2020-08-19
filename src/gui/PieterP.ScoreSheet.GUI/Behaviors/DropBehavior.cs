using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PieterP.ScoreSheet.GUI.Behaviors {
    public class DropBehavior {
        public static ICommand? GetDrop(DependencyObject? obj) {
            if (obj == null)
                return null;
            return obj.GetValue(DropProperty) as ICommand;
        }
        public static void SetDrop(DependencyObject? obj, ICommand value) {
            if (obj != null)
                obj.SetValue(DropProperty, value);
        }
        public static readonly DependencyProperty DropProperty = DependencyProperty.RegisterAttached("Drop", typeof(ICommand), typeof(DropBehavior), new UIPropertyMetadata(new PropertyChangedCallback(DropChanged)));
        private static void DropChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            FrameworkElement? element = target as FrameworkElement;
            if (element != null) {
                if (e.NewValue != null) {
                    element.Drop += OnDrop;
                } else {
                    element.Drop -= OnDrop;
                }
            }
        }        
        
        public static string GetDataType(DependencyObject? obj) {
            return obj?.GetValue(DataTypeProperty) as string ?? "";
        }
        public static void SetDataType(DependencyObject? obj, string value) {
            if (obj != null)
                obj.SetValue(DataTypeProperty, value);
        }
        public static readonly DependencyProperty DataTypeProperty = DependencyProperty.RegisterAttached("DataType", typeof(string), typeof(DropBehavior), new UIPropertyMetadata(new PropertyChangedCallback(DataTypeChanged)));
        private static void DataTypeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            FrameworkElement? element = target as FrameworkElement;
            if (element != null) {
                if (e.NewValue != null) {
                    element.Drop += OnDrop;
                } else {
                    element.Drop -= OnDrop;
                }
            }
        }

        static void OnDrop(object sender, DragEventArgs e) {
            var drop = GetDrop(sender as DependencyObject);
            if (drop != null) {
                drop.Execute(e.Data.GetData(GetDataType(sender as DependencyObject)));
            }
        }
    }
}
