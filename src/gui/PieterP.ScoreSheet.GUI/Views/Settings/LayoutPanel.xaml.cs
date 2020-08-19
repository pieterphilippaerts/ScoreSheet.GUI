using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PieterP.ScoreSheet.GUI.Helpers;
using PieterP.ScoreSheet.Model;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Model.NativeMethods;

namespace PieterP.ScoreSheet.GUI.Views.Settings {
    /// <summary>
    /// Interaction logic for LayoutPanel.xaml
    /// </summary>
    public partial class LayoutPanel : UserControl {
        public LayoutPanel() {
            InitializeComponent();
        }

    }
}
