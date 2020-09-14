using System;
using System.Collections.Generic;
using System.Text;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Information;
using PieterP.ScoreSheet.ViewModels.Information;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using PInvoke;

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class SecondScreenService : IDisposable {
        public SecondScreenService(MainWindowViewModel mainVm) {
            _mainVm = mainVm;
            _timer = ServiceLocator.Resolve<ITimerService>();
            _timer.Tick += (timer) => CheckExtendedScreen();
            DatabaseManager.Current.Settings.EnableSecondScreen.ValueChanged += RefreshTimer;
            DatabaseManager.Current.Settings.SelectedScreen.ValueChanged += CheckExtendedScreen;
            DatabaseManager.Current.Settings.ChooseScreenAutomatically.ValueChanged += CheckExtendedScreen;
            DatabaseManager.Current.Settings.ChooseScreenManually.ValueChanged += CheckExtendedScreen;
            RefreshTimer();
        }
        private void RefreshTimer() {
            if (DatabaseManager.Current.Settings.EnableSecondScreen.Value) {
                _timer.Start(new TimeSpan(0, 0, 5));
            } else {
                _timer.Stop();
            }
            CheckExtendedScreen();
        }

        private void CheckExtendedScreen() {
            var screen = FindExtendedScreen();
            if (_window != null && (!DatabaseManager.Current.Settings.EnableSecondScreen.Value || screen == null)) {
                // the window is open, but the user turned the feature off or the second screen got disconnected
                NotificationManager.Current.Raise(new CloseWindowNotification(_window));
                _window = null;
                _windowHandle = null;
                _windowScreenDevice = null;
                _secondScreenVm?.Dispose();
                _secondScreenVm = null;
            }

            if (_window == null && screen != null && DatabaseManager.Current.Settings.EnableSecondScreen.Value) {
                // the window is not yet open
                _secondScreenVm = new SecondScreenWindowViewModel(_mainVm);
                var not = new ShowWindowNotification(_secondScreenVm);
                NotificationManager.Current.Raise(not);
                _window = not.Window;
                var lookup = ServiceLocator.Resolve<IWindowHandleLookup>();
                _windowHandle = lookup.Lookup(_window).Handle;
                // set the window position
                NativeMethods.SetWindowPos(_windowHandle.Value, IntPtr.Zero, screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height, NativeMethods.SetWindowPosFlags.None);
                _windowScreenDevice = screen.DeviceName;
            }

            if (_window != null && screen != null && _secondScreenVm != null && _windowHandle != null && _windowScreenDevice != screen.DeviceName) {
                NativeMethods.SetWindowPos(_windowHandle.Value, IntPtr.Zero, screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height, NativeMethods.SetWindowPosFlags.None);
                _windowScreenDevice = screen.DeviceName;
            }
        }

        private Screen? FindExtendedScreen() {
            var all = Screen.AllScreens;
            if (all.Count == 1)
                return null;
            var selectedScreen = DatabaseManager.Current.Settings.SelectedScreen.Value;
            if (DatabaseManager.Current.Settings.ChooseScreenManually.Value && selectedScreen != "") {
                foreach (var s in all) {
                    if (s.DeviceName == selectedScreen)
                        return s;
                }
            } else {  // automatically
                var handle = ServiceLocator.Resolve<IMainWindowHandle>();
                var wbs = Screen.FromHandle(handle.Handle);
                if (wbs != null) {
                    foreach (var s in all) {
                        if (s.DeviceName != wbs.DeviceName)
                            return s;
                    }
                }
            }
            return null;
        }

        public void Dispose() {
            _timer.Stop();
            if (_window != null) {
                NotificationManager.Current.Raise(new CloseWindowNotification(_window));
                _window = null;
                _secondScreenVm?.Dispose();
                _secondScreenVm = null;
            }
        }

        private object? _window;
        private IntPtr? _windowHandle;
        private string? _windowScreenDevice;
        private SecondScreenWindowViewModel? _secondScreenVm;
        private MainWindowViewModel _mainVm;
        private ITimerService _timer;
    }
}