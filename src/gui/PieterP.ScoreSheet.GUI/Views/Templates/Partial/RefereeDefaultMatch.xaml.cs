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

namespace PieterP.ScoreSheet.GUI.Views.Templates.Partial {
    /// <summary>
    /// Interaction logic for RefereeDefaultMatch.xaml
    /// </summary>
    public partial class RefereeDefaultMatch : UserControl {
        public RefereeDefaultMatch() {
            InitializeComponent();
        }



        public Visibility ShowScissors {
            get { return (Visibility)GetValue(ShowScissorsProperty); }
            set { SetValue(ShowScissorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowScissors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowScissorsProperty =
            DependencyProperty.Register("ShowScissors", typeof(Visibility), typeof(RefereeDefaultMatch), new PropertyMetadata(Visibility.Collapsed));


    }
}
