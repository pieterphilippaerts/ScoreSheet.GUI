using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PieterP.ScoreSheet.GUI.Helpers {
    public class HidableColumnDefinition : ColumnDefinition {
        // Variables
        public static DependencyProperty VisibleProperty;

        // Properties
        public Boolean Visible {
            get { return (Boolean)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        // Constructors
        static HidableColumnDefinition() {
            VisibleProperty = DependencyProperty.Register("Visible",
                typeof(Boolean),
                typeof(HidableColumnDefinition),
                new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));

            ColumnDefinition.WidthProperty.OverrideMetadata(typeof(HidableColumnDefinition),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null,
                    new CoerceValueCallback(CoerceWidth)));

            ColumnDefinition.MinWidthProperty.OverrideMetadata(typeof(HidableColumnDefinition),
                new FrameworkPropertyMetadata((Double)0, null,
                    new CoerceValueCallback(CoerceMinWidth)));
        }

        // Get/Set
        public static void SetVisible(DependencyObject obj, Boolean nVisible) {
            obj.SetValue(VisibleProperty, nVisible);
        }
        public static Boolean GetVisible(DependencyObject obj) {
            return (Boolean)obj.GetValue(VisibleProperty);
        }

        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            obj.CoerceValue(ColumnDefinition.WidthProperty);
            obj.CoerceValue(ColumnDefinition.MinWidthProperty);
        }
        static Object CoerceWidth(DependencyObject obj, Object nValue) {
            return (((HidableColumnDefinition)obj).Visible) ? nValue : new GridLength(0);
        }
        static Object CoerceMinWidth(DependencyObject obj, Object nValue) {
            return (((HidableColumnDefinition)obj).Visible) ? nValue : (Double)0;
        }
    }
}
