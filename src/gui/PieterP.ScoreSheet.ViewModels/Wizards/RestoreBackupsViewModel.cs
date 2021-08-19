using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared.Services;
using PieterP.ScoreSheet.Localization;
using System;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Wizards
{
    /// <summary>
    /// Represents the view model for restoring matches found in the backup.
    /// </summary>
    public class RestoreBackupsViewModel : WizardPanelViewModel
    {
        #region Commands

        /// <summary>
        /// Command to be invoked when the selected matches should be restored.
        /// </summary>
        public RelayCommand<object> Restore { get; private set; }

        #endregion

        public RestoreBackupsViewModel(WizardViewModel parent, Action<Match> restoreMatchAction) : base(parent)
        {
            Restore = new RelayCommand(OnRestore);
            RestoreMatchAction = restoreMatchAction;

            MatchesToRestore.CollectionChanged += (_, __) => UpdateRestoreTextBasedOnMatchesToRestoreCount();

            LoadMatchesFromBackup();
        }

        #region Properties 

        /// <inheritdoc/>
        public override string Title => Strings.Wizard_Restore;

        /// <inheritdoc/>
        public override string Description => Strings.Wizard_RestoreDesc;

        /// <summary>
        /// The text to display on the restore action.
        /// </summary>
        public Cell<string> RestoreText { get; private set; } = Cell.Create(string.Empty);

        /// <summary>
        /// The collection of match instances found in the backup.
        /// </summary>
        public ObservableCollection<Match> FoundMatches { get; } = new ObservableCollection<Match>();

        /// <summary>
        /// The set of matches that are selected to be restored.
        /// </summary>
        public ObservableCollection<Match> MatchesToRestore { get; private set; } = new ObservableCollection<Match>();

        /// <summary>
        /// The set of matches that should be destroyed.
        /// </summary>
        public IEnumerable<Match> MatchesToDestroy => FoundMatches.Where(m => !MatchesToRestore.Contains(m)).ToList();

        /// <summary>
        /// The action to be invoked to restore a match.
        /// </summary>
        private Action<Match> RestoreMatchAction { get; set; }

        #endregion

        #region Functions

        /// <summary>
        /// Triggers the restore process.
        /// </summary>
        private void OnRestore()
        {
            RestoreMatches();
            DestroyUnwantedMatches();
            NotificationManager.Current.Raise(new CloseDialogNotification(true));
        }

        /// <summary>
        /// Loads the matches that are found in the match backup.
        /// </summary>
        private void LoadMatchesFromBackup()
        {
            foreach (var matchId in DatabaseManager.Current.MatchBackup.MatchIds)
            {
                var match = DatabaseManager.Current.MatchBackup[matchId];
                if (match == null)
                {
                    // Remove the backup because it's probably illegal JSON.
                    DatabaseManager.Current.MatchBackup[matchId] = null;
                }
                else
                {
                    if (match.UniqueId != matchId)
                        match.UniqueId = matchId; // weird.. invalid json? modified file?

                    FoundMatches.Add(match);
                }
            }
        }

        /// <summary>
        /// Ensures that a list of matches is set to be restored.
        /// </summary>
        /// <param name="matches">The matches to restore.</param>
        public void EnsureMatchesAreSetToRestore(IList<Match> matches)
        {
            if (matches == null)
                return;

            foreach (var match in matches)
                if (!MatchesToRestore.Contains(match))
                    MatchesToRestore.Add(match);
        }

        /// <summary>
        /// Ensures that a list of matches is set to be destroyed.
        /// </summary>
        /// <param name="matches">The matches to restore.</param>
        public void EnsureMatchesAreSetToDestroy(IList<Match> matches)
        {
            if (matches == null)
                return;

            foreach (var match in matches)
                if (MatchesToRestore.Contains(match))
                    MatchesToRestore.Remove(match);
        }

        /// <summary>
        /// Restores the matches by invoking the action to be executed upon restore for each match set to be restored.
        /// </summary>
        private void RestoreMatches()
        {
            // The following is a technical error that a user should never see!
            // It is intended for the developers to notify them that their work is not finished yet.
            if (RestoreMatchAction == null)
                throw new InvalidOperationException("An action must be specified for the matches to restore.");

            foreach (var matchToRestore in MatchesToRestore)
                RestoreMatchAction.Invoke(matchToRestore);
        }

        /// <summary>
        /// Permanently removes the matches the user doesn't want to restore.
        /// </summary>
        private void DestroyUnwantedMatches()
        {
            foreach (var m in MatchesToDestroy)
                DatabaseManager.Current.MatchBackup[m.UniqueId] = null;
        }

        /// <summary>
        /// Updates the restore text on the button based on the count of matches to restore.
        /// </summary>
        private void UpdateRestoreTextBasedOnMatchesToRestoreCount()
        {
            switch (MatchesToRestore.Count)
            {
                case int zero when zero == 0:
                    RestoreText.Value = Strings.RestoreBackup_RestoreNothing;
                    break;
                case int one when one == 1:
                    RestoreText.Value = Strings.RestoreBackup_RestoreOne;
                    break;
                case int multiple when multiple > 1:
                    RestoreText.Value = Safe.Format(Strings.RestoreBackup_RestoreMultiple, multiple);
                    break;
            }
        }

        #endregion
    }
}