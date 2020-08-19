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

namespace PieterP.ScoreSheet.GUI.Views.Score {
    /// <summary>
    /// Interaction logic for TeamInfo.xaml
    /// </summary>
    public partial class TeamInfo : UserControl {
        public TeamInfo() {
            InitializeComponent();
        }

        

        public bool EnableProtectedItems {
            get { return (bool)GetValue(EnableProtectedItemsProperty); }
            set { SetValue(EnableProtectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnableProtectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableProtectedItemsProperty =
            DependencyProperty.Register("EnableProtectedItems", typeof(bool), typeof(TeamInfo), new PropertyMetadata(true));


    }
}
