using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PieterP.ScoreSheet.GUI.Behaviors {
    public class MouseTrackingBehavior {
        public static ICommand? GetOnMouseEnter(DependencyObject? obj) {
            if (obj == null)
                return null;
            return obj.GetValue(OnMouseEnterProperty) as ICommand;
        }
        public static void SetOnMouseEnter(DependencyObject? obj, ICommand value) {
            if (obj != null)
                obj.SetValue(OnMouseEnterProperty, value);
        }
        public static readonly DependencyProperty OnMouseEnterProperty = DependencyProperty.RegisterAttached("OnMouseEnter", typeof(ICommand), typeof(MouseTrackingBehavior), new UIPropertyMetadata(new PropertyChangedCallback(OnMouseEnterChanged)));
        private static void OnMouseEnterChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            var ui = target as UIElement;
            if (ui != null) {
                if (e.NewValue != null) {
                    ui.MouseEnter += OnMouseEnter;
                } else {
                    ui.MouseEnter -= OnMouseEnter;
                }
            }
        }

        public static ICommand? GetOnMouseLeave(DependencyObject? obj) {
            if (obj == null)
                return null;
            return obj.GetValue(OnMouseLeaveProperty) as ICommand;
        }
        public static void SetOnMouseLeave(DependencyObject obj, ICommand value) {
            obj.SetValue(OnMouseLeaveProperty, value);
        }
        public static readonly DependencyProperty OnMouseLeaveProperty = DependencyProperty.RegisterAttached("OnMouseLeave", typeof(ICommand), typeof(MouseTrackingBehavior), new UIPropertyMetadata(new PropertyChangedCallback(OnMouseLeaveChanged)));
        private static void OnMouseLeaveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            var ui = target as UIElement;
            if (ui != null) {
                if (e.NewValue != null) {
                    ui.MouseLeave += OnMouseLeave;
                } else {
                    ui.MouseLeave -= OnMouseLeave;
                }
            }
        }

        static void OnMouseEnter(object sender, EventArgs e) {
            var enterCommand = GetOnMouseEnter(sender as UIElement);
            if (enterCommand != null && enterCommand.CanExecute(null)) {
                enterCommand.Execute(null);
            }
        }

        static void OnMouseLeave(object sender, EventArgs e) {
            var leaveCommand = GetOnMouseLeave(sender as UIElement);
            if (leaveCommand != null && leaveCommand.CanExecute(null)) {
                leaveCommand.Execute(null);
            }
        }
    }
}
