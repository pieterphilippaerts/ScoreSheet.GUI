using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class WebServiceViewModel {
        public WebServiceViewModel() {
            this.EnableJsonService = DatabaseManager.Current.Settings.EnableJsonService;
            this.AvailableHosts = new WebServiceHost[] {
                new WebServiceHost() { Name = "localhost", Host = "127.0.0.1" },
                new WebServiceHost() { Name = Strings.WebService_AllIPs, Host = "*" }
            };
            var selectedHost = this.AvailableHosts.Where(h => h.Host.ToString() == DatabaseManager.Current.Settings.JsonServiceHost.Value).FirstOrDefault();
            if (selectedHost == null)
                selectedHost = this.AvailableHosts.First();
            this.SelectedHost = Cell.Create(selectedHost, () => DatabaseManager.Current.Settings.JsonServiceHost.Value = this.SelectedHost.Value.Host.ToString());
            this.Port = Cell.Create(DatabaseManager.Current.Settings.JsonServicePort.Value.ToString(), () => {
                if (int.TryParse(this.Port.Value, out int p) && p > 0 && p < 65536)
                    DatabaseManager.Current.Settings.JsonServicePort.Value = p;
                else
                    DatabaseManager.Current.Settings.JsonServicePort.Value = 6221;
            });
            this.IsServiceActive = ServiceLocator.Resolve<JsonService>().IsActive;
            this.OpenUrl = new RelayCommand(OnOpenUrl);
            this.OpenMoreInfo = new RelayCommand(OnOpenMoreInfo);
            this.UnblockAddress = new RelayCommand(OnUnblockAddress);
        }
        private void OnOpenUrl() {
            var service = ServiceLocator.Resolve<JsonService>();
            var psi = new ProcessStartInfo {
                FileName = $"http://localhost:{ DatabaseManager.Current.Settings.JsonServicePort.Value }/",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        private void OnOpenMoreInfo() {
            var service = ServiceLocator.Resolve<JsonService>();
            var psi = new ProcessStartInfo {
                FileName = "https://score.pieterp.be/Help/WebService",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        private void OnUnblockAddress() {
            string url = $"http://{DatabaseManager.Current.Settings.JsonServiceHost.Value}:{ DatabaseManager.Current.Settings.JsonServicePort.Value }/";
            var not = new ShowMessageNotification(Safe.Format(Strings.WebService_UnblockAddressMessage, $"{ Environment.UserDomainName }\\{ Environment.UserName }", url), NotificationTypes.Question, NotificationButtons.YesNo);
            NotificationManager.Current.Raise(not);
            if (not.Result == true) {
                string args = string.Format(@"http add urlacl url={0} user={1}\{2}", url, Environment.UserDomainName, Environment.UserName);
                var psi = new ProcessStartInfo("netsh", args);
                psi.Verb = "runas";
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = true;
                Process.Start(psi).WaitForExit();
            }
        }

        public Cell<bool> EnableJsonService { get; private set; }
        public IEnumerable<WebServiceHost> AvailableHosts { get; private set; }
        public Cell<WebServiceHost> SelectedHost { get; private set; }
        public Cell<string> Port { get; private set; }
        public Cell<bool> IsServiceActive { get; private set; }
        public ICommand OpenMoreInfo { get; private set; }
        public ICommand OpenUrl { get; private set; }
        public ICommand UnblockAddress { get; private set; }


        //public static void AddAddress(string address) {
        //    AddAddress(address, Environment.UserDomainName, Environment.UserName);
        //}
        //public static void AddAddress(string address, string domain, string user) {
        //    string args = string.Format(@"http add urlacl url={0} user={1}\{2}", address, domain, user);

        //    ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
        //    psi.Verb = "runas";
        //    psi.CreateNoWindow = true;
        //    psi.WindowStyle = ProcessWindowStyle.Hidden;
        //    psi.UseShellExecute = true;

        //    Process.Start(psi).WaitForExit();
        //}
    }
    public class WebServiceHost { 
        public string Name { get; set; }
        public string Host { get; set; }
    }
}
