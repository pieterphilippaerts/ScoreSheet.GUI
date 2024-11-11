using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PieterP.ScoreSheet.GUI.Views.Wizards {
    /// <summary>
    /// Interaction logic for OrphanedMatch.xaml
    /// </summary>
    public partial class OrphanedMatch : UserControl {
        public OrphanedMatch() {
            InitializeComponent();
        }
        private void OnMouseLeftButtonUp(object sender, RoutedEventArgs e) {
            TheDate.IsDropDownOpen = true;
        }
        private void DatePickerTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            // Find the parent DatePicker
            var textBox = sender as TextBox;
            if (textBox != null) {
                var datePicker = FindParent<DatePicker>(textBox);
                if (datePicker != null) {
                    // Open the popup
                    var popup = (Popup)datePicker.Template.FindName("PART_Popup", datePicker);
                    if (popup != null) {
                        popup.IsOpen = true;
                    }
                }
            }
        }

        // Helper method to find the parent of a specific type
        private T FindParent<T>(DependencyObject child) where T : DependencyObject {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null) {
                if (parent is T parentAsT) {
                    return parentAsT;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}