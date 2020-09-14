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
    public class WindowHandleLookup : IWindowHandleLookup {
        public IWindowHandle Lookup(object window) {
            return new HandleContainer(new WindowInteropHelper(window as Window).Handle);
        }
        private class HandleContainer : IWindowHandle {
            public HandleContainer(IntPtr handle) => this.Handle = handle;
            public IntPtr Handle { get; }
        }
    }
}
