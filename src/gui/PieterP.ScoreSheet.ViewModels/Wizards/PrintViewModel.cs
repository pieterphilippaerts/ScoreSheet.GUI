using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.ScoreSheet.ViewModels.Score.Validations;
using PieterP.ScoreSheet.ViewModels.Templates;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class PrintViewModel : WizardPanelViewModel{
        public PrintViewModel(WizardViewModel parent, CompetitiveMatchViewModel match) : base(parent) {
            _match = match;

            bool finished = match.Score.Result.Value != Winner.Incomplete && match.Score.Result.Value != Winner.Error;

            this.Preview = Cell.Create<object?>(null);
            this.PrintReferee = Cell.Create(!finished, OnSelectionChanged);
            this.PrintMatch = Cell.Create(finished, OnSelectionChanged);
            this.RefereeLayouts = RefereeLayout.All;
            this.Print = new RelayCommand(OnPrint);
            this.RefereeLayouts = RefereeLayout.All;
            this.SelectedRefereeLayout = Cell.Create<RefereeLayout?>(this.RefereeLayouts.Where(r => r.Id == DatabaseManager.Current.Settings.DefaultRefereeLayoutOption.Value).FirstOrDefault(), OnSelectionChanged);
            this.UseHandicap = DatabaseManager.Current.Settings.UseHandicap;
            OnSelectionChanged();
        }

        private void OnSelectionChanged() {
            if (this.PrintMatch.Value) {
                Preview.Value = _match.MatchSystem.GenerateTemplate(_match);
            } else {
                Preview.Value = this.SelectedRefereeLayout.Value?.CreateTemplate(_match, _match.Matches, HandicapTable, _match.Matches.Count == 16);
            }
        }
        private void OnPrint() {
            bool landscape = false;
            IEnumerable<object>? documents;
            IList<string>? validationErrors = null;
            if (this.PrintMatch.Value) {
                var validator = new MatchValidator();
                validationErrors = validator.Validate(_match);
                if (validationErrors != null) { // if there are no errors, upload the match
                    var sb = new StringBuilder();
                    sb.AppendLine(Strings.Print_ValidationErrorsFound);
                    foreach (var ve in validationErrors) {
                        if (ve != null && ve.Length > 0) {
                            sb.AppendLine(" - " + ve);
                        }
                    }
                    sb.AppendLine();
                    sb.Append(Strings.PrintValidationErrorsContinue);
                    var message = new ShowMessageNotification(sb.ToString(), NotificationTypes.Exclamation, NotificationButtons.YesNo);
                    NotificationManager.Current.Raise(message);
                    if (message.Result != true)
                        return;
                }
                var match = _match.MatchSystem.GenerateTemplate(_match);
                documents = new object[] { match };
                landscape = match.IsLandscape;
            } else {
                documents = this.SelectedRefereeLayout.Value?.CreateDocuments(_match, HandicapTable);
            }
            if (documents != null) {
                var exporter = ServiceLocator.Resolve<IExportService>();
                exporter.ToPrinter(documents, landscape);
                if (this.PrintMatch.Value) {
                    AutoUpload(validationErrors);
                }
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
            }
        }
        private HandicapTable? HandicapTable { 
            get {
                if (!this.UseHandicap.Value)
                    return null;
                if (_match.Women.Value) {
                    return HandicapTable.Women;
                } else {
                    return HandicapTable.Men;
                }
            }
        }
        private void AutoUpload(IList<string>? validationErrors) {
            if (DatabaseManager.Current.Settings.EnableAutoUpload.Value && _match.IsOfficial.Value && _match.IsCompetitive) {
                if (validationErrors == null) { // if there are no errors, upload the match
                    var uploader = new MatchUploader(false);
                    uploader.Upload(_match);
                } else {
                    NotificationManager.Current.Raise(new ShowMessageNotification(Strings.Print_AutoUploadSkipped, NotificationTypes.Exclamation, NotificationButtons.OK));
                }
            }
        }

        public Cell<bool> PrintReferee { get; private set; }
        public Cell<bool> PrintMatch { get; private set; }
        public Cell<bool> UseHandicap { get; private set; }
        public IEnumerable<RefereeLayout> RefereeLayouts { get; private set; }
        public Cell<RefereeLayout?> SelectedRefereeLayout { get; private set; }


        public ICommand Print { get; private set; }

        public Cell<object?> Preview { get; private set; }

        public override string Title => Wizard_Print;
        public override string Description => Wizard_PrintDesc;

        private CompetitiveMatchViewModel _match;
    }

}
