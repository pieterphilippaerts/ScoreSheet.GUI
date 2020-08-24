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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PieterP.ScoreSheet.GUI.Views.Wizards {
    /// <summary>
    /// Interaction logic for SelectedPlayerPanel.xaml
    /// </summary>
    public partial class SelectedPlayerPanel : UserControl {
        public SelectedPlayerPanel() {
            InitializeComponent();
        }
        

        public bool HideWO {
            get { return (bool)GetValue(HideWOProperty); }
            set { SetValue(HideWOProperty, value); }
        }
        // Using a DependencyProperty as the backing store for HideWO.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideWOProperty =
            DependencyProperty.Register("HideWO", typeof(bool), typeof(SelectedPlayerPanel), new PropertyMetadata(false));


        public bool HideCaptain
        {
            get { return (bool)GetValue(HideCaptainProperty); }
            set { SetValue(HideCaptainProperty, value); }
        }
        // Using a DependencyProperty as the backing store for HideWO.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideCaptainProperty =
            DependencyProperty.Register("HideCaptain", typeof(bool), typeof(SelectedPlayerPanel), new PropertyMetadata(false));


        public bool IsRelevant {
            get { return (bool)GetValue(IsRelevantProperty); }
            set { SetValue(IsRelevantProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRelevant.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRelevantProperty =
            DependencyProperty.Register("IsRelevant", typeof(bool), typeof(SelectedPlayerPanel), new PropertyMetadata(true));


    }
}
