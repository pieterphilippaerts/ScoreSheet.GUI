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
    /// Interaction logic for MatchInfo.xaml
    /// </summary>
    public partial class MatchInfo : UserControl {
        public MatchInfo() {
            InitializeComponent();
        }



        public Visibility LevelVisibility {
            get { return (Visibility)GetValue(LevelVisibilityProperty); }
            set { SetValue(LevelVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeriesVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LevelVisibilityProperty =
            DependencyProperty.Register("LevelVisibility", typeof(Visibility), typeof(MatchInfo), new PropertyMetadata(Visibility.Visible));


    }
}
