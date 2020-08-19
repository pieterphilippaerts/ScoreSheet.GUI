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
    /// Interaction logic for Captains.xaml
    /// </summary>
    public partial class Captains : UserControl {
        public Captains() {
            InitializeComponent();
        }



        public Visibility Article632Visibility {
            get { return (Visibility)GetValue(Article632VisibilityProperty); }
            set { SetValue(Article632VisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Article632Visibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Article632VisibilityProperty =
            DependencyProperty.Register("Article632Visibility", typeof(Visibility), typeof(Captains), new PropertyMetadata(Visibility.Visible));


    }
}
