using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Information;
using PieterP.ScoreSheet.ViewModels.Helpers;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels {
    public class AboutWindowViewModel {
        public AboutWindowViewModel() {
            this.WebsiteText = "https://score.pieterp.be/";
            this.GitText = "https://bitbucket.org/pieterpbevttl/";
            this.SupportText = "score@pieterp.be";
            this.Close = new CloseCommand();
            this.Website = new LaunchCommand(this.WebsiteText);
            this.Git = new LaunchCommand(this.GitText);
            this.Support = new LaunchCommand("mailto:" + this.SupportText);
            this.ProgramFolder = new LaunchCommand("explorer.exe" , DatabaseManager.Current.BasePath);
            this.DataFolder = new LaunchCommand("explorer.exe", DatabaseManager.Current.ActiveProfilePath);
            this.BuildDate = Application.CompilationTimestamp.ToString("dddd, d MMMM yyyy", Thread.CurrentThread.CurrentUICulture);
            this.Version = $"{ Application.Version.ToString(3) } ({ Application.BuildType }, { Application.BuildPlatform })";
        }
        public string Version { get; private set; }
        public string WebsiteText { get; private set; }
        public string GitText { get; private set; }
        public string SupportText { get; private set; }
        public string BuildDate { get; private set; }
        public ICommand Git { get; private set; }
        public ICommand Website { get; private set; }
        public ICommand Support { get; private set; }
        public ICommand ProgramFolder { get; private set; }
        public ICommand DataFolder { get; private set; }
        public ICommand Close { get; private set; }

        private class CloseCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
            }
        }
    }
}
