using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Model.NativeMethods;
using System.Globalization;
using System.Windows.Controls.Primitives;
using PieterP.ScoreSheet.Localization;
using System.Windows.Input;

namespace PieterP.ScoreSheet.GUI.Services {
    public class WindowService {
        public WindowService() {
            _currentDialogs = new LinkedList<Window>();
        }
        /// <summary>
        /// Shows a non-modal window associated with a given viewmodel object. The association between the view and viewmodel
        /// is defined in the Resources tag of the App.xaml file as a DataTemplate.
        /// </summary>
        /// <param name="viewModel">The viewmodel object to use when looking up the corresponding view.</param>
        /// <returns>An object that represents the window. This object can be used as a parameter to the
        /// CloseWindow function.</returns>
        public object ShowWindow(object viewModel) {
            var window = FindWindow(viewModel);
            window.DataContext = viewModel;
            window.Show();
            return window;
        }
        /// <summary>
        /// Shows a modal window associated with a given viewmodel object. The association between the view and viewmodel
        /// is defined in the Resources tag of the App.xaml file as a DataTemplate.
        /// </summary>
        /// <param name="viewModel">The viewmodel object to use when looking up the corresponding view.</param>
        /// <returns>The value of the DialogResult property.</returns>
        public bool ShowDialog(object viewModel) {
            var d = FindWindow(viewModel);
            d.DataContext = viewModel;
            var owner = _currentDialogs.Last?.Value ?? App.Current.MainWindow;
            if (owner != d) // this can happen if an exception occurs before the MainWindow is shown
                d.Owner = owner;
            _currentDialogs.AddLast(d);
            var ret = d.ShowDialog();
            _currentDialogs.RemoveLast();
            if (ret == null)
                return false;
            return ret.Value;
        }
        public void OpenPopup(object viewModel, object owner, bool openUnderOwner) {
            var d = FindPopup(viewModel);
            d.DataContext = viewModel;
            if (owner == null) {
                d.PlacementTarget = Keyboard.FocusedElement as UIElement;
            } else {
                d.PlacementTarget = owner as UIElement;
            }
            d.Placement = openUnderOwner ? PlacementMode.Bottom : PlacementMode.Top;
            d.StaysOpen = false;
            d.PopupAnimation = PopupAnimation.Fade;
            d.AllowsTransparency = true;
            d.IsOpen = true;
        }
        /// <summary>
        /// Closes the active dialog window.
        /// </summary>
        /// <param name="result">The value the DialogResult property of the window should be set to.</param>
        public void CloseDialog(bool result) {
            if (!_currentDialogs.Any())
                throw new NotSupportedException(Errors.WindowService_NoDialogError);
            var win = _currentDialogs.Last!.Value;
            if (win != null && win.DialogResult == null)
                win.DialogResult = result;
        }
        /// <summary>
        /// Closes a given window.
        /// </summary>
        /// <param name="window">A window object, as returned by the ShowWindow function.</param>
        public void CloseWindow(object window) {
            var w = window as Window;
            w.Close();
        }

        public string? ShowFileDialog(FileDialogTypes type, string? initialDir, string? filter, string? filename, string? title) {
            FileDialog fd;
            if (type == FileDialogTypes.OpenFile) {
                fd = new OpenFileDialog();
            } else {
                fd = new SaveFileDialog();
            }
            fd.Filter = filter ?? string.Empty;
            fd.InitialDirectory = initialDir ?? string.Empty;
            fd.FileName = filename;
            fd.Title = title ?? "ScoreSheet - PieterP.be";
            fd.AddExtension = true;
            if (fd.ShowDialog() == true)
                return fd.FileName;
            return null;
        }
        public string? ShowColorDialog(string? initialColor, IList<string> customColors) {
            var cc = new CHOOSECOLOR();
            cc.lpCustColors = Marshal.AllocHGlobal(64);
            try {
                cc.lStructSize = Marshal.SizeOf(cc);
                if (initialColor == null) {
                    cc.rgbResult = 0;
                } else {
                    cc.rgbResult = ColorToUint(initialColor);
                }
                cc.Flags = CC_FULLOPEN | CC_RGBINIT;
                byte[] colors = new byte[64];
                int count = Math.Min(customColors.Count, 16);
                for (int i = 0; i < count; i++) {
                    StoreColor(customColors[i], colors, i * 4);
                }
                Marshal.Copy(colors, 0, cc.lpCustColors, 64);
                cc.hwndOwner = ServiceLocator.Resolve<IActiveWindowHandle>().Handle;
                if (ChooseColor(ref cc)) {
                    var ret = UintToColor(cc.rgbResult);
                    return ret;
                } else {
                    return null;
                }
            } finally {
                Marshal.FreeHGlobal(cc.lpCustColors);
            }

            string UintToColor(uint result) {
                byte[] color = BitConverter.GetBytes(result);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(color);
                return $"#{ color[0].ToString("X2") }{ color[1].ToString("X2") }{ color[2].ToString("X2") }";
            }
            void StoreColor(string color, byte[] array, int offset) {
                var cb = GetComponents(color);
                if (cb == null)
                    return;
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(cb);
                array[offset] = cb[0];
                array[offset + 1] = cb[1];
                array[offset + 2] = cb[2];
                array[offset + 3] = cb[3];
            }
            uint ColorToUint(string color) {
                var cb = GetComponents(color);
                if (cb == null)
                    return 0;
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(cb);
                return BitConverter.ToUInt32(cb, 0);
            }
            byte[]? GetComponents(string color) {
                if (!color.StartsWith("#") || (color.Length != 7 && color.Length != 9))
                    return null;
                int offset = color.Length == 7 ? 1 : 3;
                if (int.TryParse(color.Substring(offset, 2), NumberStyles.HexNumber, null, out int r)
                    && int.TryParse(color.Substring(offset + 2, 2), NumberStyles.HexNumber, null, out int g)
                    && int.TryParse(color.Substring(offset + 4, 2), NumberStyles.HexNumber, null, out int b)) {
                    
                    var ret = new byte[4];
                    ret[0] = (byte)r;
                    ret[1] = (byte)g;
                    ret[2] = (byte)b;
                    return ret;
                }
                return null;
            }
            // string: #AARRGGBB
            // uint (little): 0x00BBGGRR
            // uint (big): 0xRR 0xGG 0xBB 0x00
        }

        [DllImport("user32.dll", EntryPoint = "MessageBoxA", CharSet = CharSet.Ansi)]
        public static extern int Win32MessageBox(IntPtr hWnd, string text, string caption, int options);

        public bool? ShowMessage(string message, NotificationTypes type, NotificationButtons buttons) {
            var owner = GetActiveDialog();
            MessageBoxResult result;
            if (owner == null) {
                result = (MessageBoxResult)Win32MessageBox(IntPtr.Zero, message, "ScoreSheet - PieterP.be", (int)buttons | (int)type);
                //result = MessageBox.Show(message, "ScoreSheet - PieterP.be", (MessageBoxButton)buttons, (MessageBoxImage)type); //  <- this doesn't work if the main window isn't loaded
            } else {
                result = MessageBox.Show(owner, message, "ScoreSheet - PieterP.be", (MessageBoxButton)buttons, (MessageBoxImage)type);
            }
            switch (result) {
                case MessageBoxResult.OK:
                case MessageBoxResult.Yes:
                    return true;
                case MessageBoxResult.No:
                    return false;
                case MessageBoxResult.Cancel:
                default: 
                    return null;
            }
        }

        public Window GetActiveDialog() {
            if (_currentDialogs.Any())
                return _currentDialogs.Last!.Value;
            else
                return App.Current.MainWindow;
        }

        private Window FindWindow(object vm) {
            return FindFrameworkElement(vm) as Window ?? throw new ArgumentException(Safe.Format(Errors.WindowService_WindowError, vm.GetType().Name));
        }
        private Popup FindPopup(object vm) {
            return FindFrameworkElement(vm) as Popup ?? throw new ArgumentException(Safe.Format(Errors.WindowService_PopupError, vm.GetType().Name));
        }
        private FrameworkElement? FindFrameworkElement(object vm) {
            var t = vm.GetType();
            foreach (var value in App.Current.Resources.Values) {
                var dtemp = value as DataTemplate;
                if (dtemp != null && t.Equals(dtemp.DataType)) {
                    return dtemp.LoadContent() as FrameworkElement;
                }
            }
            return null;
        }

        private LinkedList<Window> _currentDialogs;
    }
}
