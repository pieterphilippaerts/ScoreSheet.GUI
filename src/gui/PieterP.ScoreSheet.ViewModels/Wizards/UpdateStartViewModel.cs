using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class UpdateStartViewModel : WizardPanelViewModel {
        public UpdateStartViewModel(WizardViewModel parent) : base(parent) {
            this.FromFileHelp = new RelayCommand<object>(p => NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_UpdateFileHelp)));
            this.FromInternet = new RelayCommand<object>(p => Parent.CurrentPanel.Value = new UpdateFromInternetViewModel(Parent));
            this.FromFile = new RelayCommand(UpdateFromFile);
        }

        private void UpdateFromFile() {
            var of = new FileDialogNotification(FileDialogTypes.OpenFile) {
                Filter = string.Format("{0} (*.ssjsu)|*.ssjsu|{1} (*.*)|*.*", Wizard_UpdateFile, Wizard_AllFiles)
            };            
            NotificationManager.Current.Raise(of);
            if (of.SelectedPath != null) {
                if (!File.Exists(of.SelectedPath)) {
                    NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_NoFileSelected, NotificationTypes.Error));
                    return;
                }
                try {
                    DatabaseManager.Current.Import(of.SelectedPath);
                    NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_UpdateSuccessful, NotificationTypes.Informational));
                    NotificationManager.Current.Raise(new  CloseDialogNotification(true));
                } catch (Exception e) {
                    Logger.Log(e);
                    NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_FileReadError, NotificationTypes.Error));
                }
            }
        }

        public override string Title => Wizard_InstallUpdate;
        public override string Description => Wizard_InstallUpdateDesc;

        public ICommand FromInternet { get; private set; }
        public ICommand FromFile { get; private set; }
        public ICommand FromFileHelp { get; private set; }
    }
}
