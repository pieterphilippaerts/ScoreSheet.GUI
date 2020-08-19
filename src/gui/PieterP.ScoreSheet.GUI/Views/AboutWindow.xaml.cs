using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PieterP.ScoreSheet.Localization;
using SR = PieterP.ScoreSheet.Localization.Views.Resources;

namespace PieterP.ScoreSheet.GUI.Views {
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window {
        public AboutWindow() {
            InitializeComponent();
            string year = "2011" + "-" + DateTime.Now.Year.ToString();
            this.CopyrightText.Text = Safe.Format(SR.AboutWindow_Copyright, year);
        }
    }
}
