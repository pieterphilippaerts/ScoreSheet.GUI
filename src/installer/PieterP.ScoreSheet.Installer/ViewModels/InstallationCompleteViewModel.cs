using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PieterP.ScoreSheet.Installer.Models;

namespace PieterP.ScoreSheet.Installer.ViewModels {
    public class InstallationCompleteViewModel {
        public InstallationCompleteViewModel(MainViewModel parent, string error = null) {
            _parent = parent;
            this.Launch = new RelayCommand(OnLaunch);
            this.Close = new RelayCommand(OnClose);
            this.Error = error;
            this.HasError = error != null && error.Length > 0;
        }
        private void OnLaunch() {
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(DatabaseManager.Current.BasePath, "launcher.exe");
            process.Start();
            OnClose();
        }

        private void OnClose() {
            Application.Current.Shutdown();
        }

        public bool HasError { get; private set; }
        public string Error { get; private set; }
        public ICommand Launch { get; private set; }
        public ICommand Close { get; private set; }
        private MainViewModel _parent;
    }
}
