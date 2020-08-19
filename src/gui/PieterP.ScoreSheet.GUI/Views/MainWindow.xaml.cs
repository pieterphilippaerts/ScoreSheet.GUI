using PieterP.ScoreSheet.GUI.Views.Templates;
using PieterP.ScoreSheet.Model.Information;
using PieterP.ScoreSheet.ViewModels;
using PieterP.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;

namespace PieterP.ScoreSheet.GUI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            //IsFullScreen="{Binding IsFullScreen.Value}"
            var binding = new Binding("IsFullScreen.Value");
            this.SetBinding(MainWindow.IsFullScreenProperty, binding);
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                var binding = ZoomCombo.GetBindingExpression(ComboBox.TextProperty);
                binding.UpdateSource();
            }
        }


        public bool IsFullScreen {
            get { 
                return (bool)GetValue(IsFullScreenProperty); 
            }
            set { 
                SetValue(IsFullScreenProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsFullscreen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, OnIsFullScreenChanged));

        public static void OnIsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var value = (bool)e.NewValue;
            var win = d as MainWindow;
            if (win == null)
                return;
            Screen.LockDesktopUpdate();
            try {
                if (value) {
                    var interop = new WindowInteropHelper(win);
                    var s = Screen.FromHandle(interop.Handle);
                    if (s != null) {
                        win.Left = 0;
                        win.Top = 0;
                        win.Width = s.WorkingArea.Width;
                        win.Height = s.WorkingArea.Height;
                    }
                    win.WindowState = WindowState.Normal;
                    win.WindowStyle = WindowStyle.None;
                } else {
                    win.WindowStyle = WindowStyle.SingleBorderWindow;
                }
                win.WindowState = WindowState.Maximized;
            } finally {
                Screen.UnlockDesktopUpdate();
            }
        }
    }
}
