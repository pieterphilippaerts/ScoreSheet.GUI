using System;
using System.Collections.Generic;
using System.Text;
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

namespace PieterP.ScoreSheet.GUI.Views.Score {
    /// <summary>
    /// Interaction logic for SuspectSemanticsPopup.xaml
    /// </summary>
    public partial class SuspectSemanticsPopup : Popup {
        public SuspectSemanticsPopup() {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e) {
            this.IsOpen = false;
        }
    }
}
