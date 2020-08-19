using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace PieterP.ScoreSheet.ViewModels.Helpers {
    public class LaunchCommand : ICommand {
        public LaunchCommand(string filename, string? args = null) {
            _filename = filename;
            _args = args;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) {
            var psi = new ProcessStartInfo {
                FileName = _filename,
                Arguments = _args,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private readonly string _filename;
        private readonly string? _args;
    }

}
