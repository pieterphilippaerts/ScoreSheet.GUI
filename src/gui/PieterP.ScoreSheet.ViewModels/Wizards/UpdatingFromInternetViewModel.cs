using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Interfaces;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using Unity.Injection;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class UpdatingFromInternetViewModel : WizardPanelViewModel {
        public UpdatingFromInternetViewModel(WizardViewModel parent, Club club) : base(parent, new CancelCommand()) {
            _club = club;
            ((CancelCommand)this.Cancel).Parent = this;
            this.Messages = new ObservableCollection<object>();
            this.ButtonText = Cell.Create(Wizard_Cancel);
            this.IsUpdating = Cell.Create(true);
            this.IsCanceled = false;
            BeginUpdate();
        }

        public async void BeginUpdate() {
            DatabaseManager.Current.Settings.HomeClub.Value = _club.LongName;
            DatabaseManager.Current.Settings.HomeClubId.Value = _club.UniqueIndex;

            // this is an ideal time, because we are probably connected to the internet
            ServiceLocator.Resolve<INetworkAvailabilityService>().TriggerManually();

            if (!await DatabaseManager.Current.UpdateMatches(_club, OnProgress) && !IsCanceled) {
                // uhoh.. error
                NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_UpdateError, NotificationTypes.Error));
            }
            this.IsUpdating.Value = false;

            // Check whether there are matches without a date
            var orphans = DatabaseManager.Current.MatchStartInfo.GetOrphanMatches();
            if (orphans.Any()) {
                //Parent.CurrentPanel.Value = new OrphanedMatchesViewModel(Parent, Enumerable.Range(0, 8).Select(c => orphans.First()));
                Parent.CurrentPanel.Value = new OrphanedMatchesViewModel(Parent, orphans);
            } else {
                ButtonText.Value = Wizard_Close;
            }
        }
        private void OnProgress(string progress, bool isError) {
            Messages.Add(new ProgressItem(progress, isError));
        }

        public Cell<string> ButtonText { get; private set; }

        public ObservableCollection<object> Messages { get; private set; }

        public Cell<bool> IsUpdating { get; private set; }
        public bool IsCanceled { get; private set; }

        public override string Title => Wizard_Update;
        public override string Description => Wizard_UpdateDesc;

        private Club _club;
        private class ProgressItem {
            public ProgressItem(string message, bool isError) {
                this.Message = message;
                this.IsError = isError;
            }
            public string Message { get; private set; }
            public bool IsError { get; private set; }
        }
        private class CancelCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                if (Parent != null) {
                    if (Parent.IsUpdating.Value) {
                        Parent.IsCanceled = true;
                        DatabaseManager.Current.CancelUpdate();
                    }
                }
                NotificationManager.Current.Raise(new CloseDialogNotification(false));
            }

            public UpdatingFromInternetViewModel? Parent { get; set; }
        }
    }
}