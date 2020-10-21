using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.ViewModels.Commands.Bases;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.ScoreSheet.ViewModels.Wizards;
using PieterP.Shared.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PieterP.ScoreSheet.ViewModels.Commands
{
    /// <summary>
    /// This command handles uploading a selection of matches to the TabT service.
    /// </summary>
    internal class UploadCommand : OpenWizardCommand<SelectMatchesViewModel>
    {
        public UploadCommand(MainWindowViewModel mainWindowViewModel) : base(mainWindowViewModel)
        {
            mainWindowViewModel.ActiveMatches.CollectionChanged += RaiseCanExecuteChanged;
        }

        /// <inheritdoc/>
        public override bool CanExecute(object _) => _mainWindowViewModel.ActiveMatches.Count > 0;

        /// <inheritdoc/>
        protected override SelectMatchesViewModel CreatePanelViewModel()
        {
            return new SelectMatchesViewModel(
                _wizardViewModel,
                _mainWindowViewModel.ActiveMatches,
                Strings.Wizard_Upload,
                Strings.Wizard_UploadDesc,
                Strings.Wizard_UploadMessage,
                ExportTypes.Upload,
                ContinueWithSelectedMatchesAsync,
                m => m.IsOfficial.Value && m.IsCompetitive && m.UploadStatus.Value != UploadStatus.Uploaded,
                m => m.IsOfficial.Value && m.UploadStatus.Value == UploadStatus.Uploaded,
                m => !m.IsCompetitive);
        }

        /// <summary>
        /// Continue by uploading the selected matches.
        /// </summary>
        /// <param name="selectedMatches">The selected matches to export.</param>
        private async void ContinueWithSelectedMatchesAsync(IEnumerable<CompetitiveMatchViewModel> selectedMatches)
        {
            var uploader = new MatchUploader(true);
            (var errorCode, var errors) = await uploader.Upload(selectedMatches.ToArray());
            switch (errorCode)
            {
                case TabTErrorCode.NetworkError:
                case TabTErrorCode.InvalidCredentials:
                case TabTErrorCode.DataError:
                    NotificationManager.Current.Raise(new ShowMessageNotification(FormatError(errorCode, errors), NotificationTypes.Error, NotificationButtons.OK));
                    break;
                case TabTErrorCode.NoError:
                    NotificationManager.Current.Raise(new ShowMessageNotification(Strings.Wizard_UploadSuccessful, NotificationTypes.Informational, NotificationButtons.OK));
                    break;
            }
            NotificationManager.Current.Raise(new CloseDialogNotification(true));
        }

        /// <summary>
        /// Formats a collection of TabT errors.
        /// </summary>
        /// <param name="errorCode">The TabT error code.</param>
        /// <param name="errors">The collection of errors to format.</param>
        /// <returns>A concatenated string of errors.</returns>
        private string FormatError(TabTErrorCode errorCode, IEnumerable<string> errors)
        {
            var sb = new StringBuilder();
            switch (errorCode)
            {
                case TabTErrorCode.DataError:
                    sb.AppendLine(Errors.Wizard_ServerError);
                    break;
                case TabTErrorCode.NetworkError:
                    sb.AppendLine(Errors.Wizard_NetworkError);
                    break;
                case TabTErrorCode.InvalidCredentials:
                    sb.AppendLine(Errors.Wizard_InvalidCredentials);
                    break;
            }

            if (errors != null && errors.Any())
            {
                sb.AppendLine();
                sb.AppendLine(Strings.Wizard_ServerErrors);
                foreach (var e in errors)
                {
                    sb.AppendLine(" - " + e);
                }
            }
            return sb.ToString();
        }
    }
}
