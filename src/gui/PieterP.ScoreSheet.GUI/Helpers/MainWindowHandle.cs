using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using PieterP.ScoreSheet.GUI.Services;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.GUI.Helpers {
    public class MainWindowHandle : IMainWindowHandle {
        public IntPtr Handle {
            get {
                return new WindowInteropHelper(App.Current.MainWindow).Handle;
            }
        }
    }
    public class ActiveWindowHandle : IActiveWindowHandle {
        public IntPtr Handle {
            get {
                return new WindowInteropHelper(ServiceLocator.Resolve<WindowService>().GetActiveDialog()).Handle;
            }
        }
    }
}
