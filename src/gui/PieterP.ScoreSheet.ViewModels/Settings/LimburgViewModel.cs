using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Helpers;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class LimburgViewModel {
        public LimburgViewModel() {
            this.Host= DatabaseManager.Current.Settings.SmtpHost;
            this.Port = DatabaseManager.Current.Settings.SmtpPort;
            this.UseStartTls = DatabaseManager.Current.Settings.SmtpUseStartTls;
            this.Username = DatabaseManager.Current.Settings.SmtpUsername;
            this.Password = DatabaseManager.Current.Settings.SmtpPassword;
            this.MailFrom = DatabaseManager.Current.Settings.FreeTimeMailFrom;
            this.MailTo = DatabaseManager.Current.Settings.FreeTimeMailTo;
            this.ClubResponsibleInCC = DatabaseManager.Current.Settings.ClubResponsibleInCC;
            this.SmtpHelp = new LaunchCommand("https://score.pieterp.be/Help/Smtp");
            this.Test = new RelayCommand(OnTest);
        }
        private async void OnTest() {
            var client = new MailClient();
            ShowMessageNotification message;
            if (await client.Test()) {
                message = new ShowMessageNotification(Strings.Limburg_EmailSent, NotificationTypes.Informational);
            } else {
                message = new ShowMessageNotification(Strings.Limburg_EmailError, NotificationTypes.Exclamation);
            }
            NotificationManager.Current.Raise(message);
        }
        public Cell<string> Host { get; private set; }
        public Cell<int> Port { get; private set; }
        public Cell<bool> UseStartTls { get; private set; }
        public Cell<bool> ClubResponsibleInCC { get; private set; }
        public Cell<string> Username { get; private set; }
        public Cell<string> Password { get; private set; }
        public Cell<string> MailFrom { get; private set; }
        public Cell<string> MailTo { get; private set; }
        public ICommand Test { get; private set; }
        public ICommand SmtpHelp { get; private set; }
    }
}
