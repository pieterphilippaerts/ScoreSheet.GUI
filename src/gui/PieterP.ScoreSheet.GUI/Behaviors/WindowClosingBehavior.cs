using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PieterP.ScoreSheet.GUI.Behaviors {
    // code from http://www.codeproject.com/Articles/73251/Handling-a-Window-s-Closed-and-Closing-events-in-t
    // The behavior exposes three commands:
    //   * Closed - This is executed when the Window.Closed event is fired.
    //   * Closing 
    //     - Execute - This is executed if the Window.Closing event was not cancelled.
    //     - CanExecute - This is called when the Window.Closing event is fired, and gives you a chance to decide if you want to cancel or not.
    //   * CancelClosing - This is executed when the Window.Closing  event is cancelled.

    public class WindowClosingBehavior {
        public static ICommand? GetClosed(DependencyObject? obj) {
            if (obj == null)
                return null;
            return obj.GetValue(ClosedProperty) as ICommand;
        }

        public static void SetClosed(DependencyObject? obj, ICommand value) {
            if (obj != null)
                obj.SetValue(ClosedProperty, value);
        }

        public static readonly DependencyProperty ClosedProperty = DependencyProperty.RegisterAttached("Closed", typeof(ICommand), typeof(WindowClosingBehavior), new UIPropertyMetadata(new PropertyChangedCallback(ClosedChanged)));

        private static void ClosedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            Window? window = target as Window;            
            if (window != null) {
                if (e.NewValue != null) {
                    window.Closed += Window_Closed;
                } else {
                    window.Closed -= Window_Closed;
                }
            }
        }

        public static ICommand GetClosing(DependencyObject obj) {
            return (ICommand)obj.GetValue(ClosingProperty);
        }

        public static void SetClosing(DependencyObject obj, ICommand value) {
            obj.SetValue(ClosingProperty, value);
        }

        public static readonly DependencyProperty ClosingProperty = DependencyProperty.RegisterAttached("Closing", typeof(ICommand), typeof(WindowClosingBehavior), new UIPropertyMetadata(new PropertyChangedCallback(ClosingChanged)));

        private static void ClosingChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) {
            Window window = target as Window;

            if (window != null) {
                if (e.NewValue != null) {
                    window.Closing += Window_Closing;
                } else {
                    window.Closing -= Window_Closing;
                }
            }
        }

        public static ICommand GetCancelClosing(DependencyObject obj) {
            return (ICommand)obj.GetValue(CancelClosingProperty);
        }

        public static void SetCancelClosing(DependencyObject obj, ICommand value) {
            obj.SetValue(CancelClosingProperty, value);
        }

        public static readonly DependencyProperty CancelClosingProperty = DependencyProperty.RegisterAttached("CancelClosing", typeof(ICommand), typeof(WindowClosingBehavior));

        static void Window_Closed(object sender, EventArgs e) {
            ICommand closed = GetClosed(sender as Window);
            if (closed != null) {
                closed.Execute(null);
            }
        }

        static void Window_Closing(object sender, CancelEventArgs e) {
            var window = sender as Window;
            ICommand closing = GetClosing(window);
            if (closing != null) {
                if (closing.CanExecute(null)) {
                    closing.Execute(null);
                } else {
                    ICommand cancelClosing = GetCancelClosing(sender as Window);
                    if (cancelClosing != null) {
                        cancelClosing.Execute(null);
                    }

                    e.Cancel = true;
                }
            }
        }
    }
}
