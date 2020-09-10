using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using LevelEnum = PieterP.ScoreSheet.Model.Database.Enums.Level;

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class LiveUpdateService : IDisposable {
        public LiveUpdateService(MainWindowViewModel mainVm) {
            _mainVm = mainVm;
            _isUpdating = new Dictionary<CompetitiveMatchViewModel, bool>();
            _canUpload = Cell.Derived(DatabaseManager.Current.Settings.EnableLiveUpdates, DatabaseManager.Current.Settings.TabTUsername, DatabaseManager.Current.Settings.TabTPassword, (e, u, p) => e && u != "" && p != "");
            foreach (var am in _mainVm.ActiveMatches) {
                if (am.IsCompetitive) {
                    am.Score.HomeMatchesWon.ValueChanged += () => Export(am);
                    am.Score.AwayMatchesWon.ValueChanged += () => Export(am);
                }
            }
            _mainVm.ActiveMatches.CollectionChanged += ActiveMatches_CollectionChanged;
        }

        private void ActiveMatches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (CompetitiveMatchViewModel am in e.NewItems) {
                    if (am.IsCompetitive) {
                        am.Score.HomeMatchesWon.ValueChanged += () => Export(am);
                        am.Score.AwayMatchesWon.ValueChanged += () => Export(am);
                    }
                }
            }
        }

        public async void Export(CompetitiveMatchViewModel match) {
            if (_canUpload.Value) {
                if (DatabaseManager.Current.Settings.EnableLiveUpdatesForSuperOnly.Value && match.Level.Value.Id != LevelEnum.Super)
                    return;
                if (match.Score.Result.Value != Winner.Error && match.Score.Result.Value != Winner.Incomplete)
                    return; // match is complete; do not upload complete matches to avoid confusion (people think the match has already been uploaded, while it is in fact only a partial upload) 
                // upload live result
                lock (match) {
                    if (!_isUpdating.ContainsKey(match)) {
                        _isUpdating[match] = true;
                    } else {
                        if (_isUpdating[match])
                            return;
                        _isUpdating[match] = true;
                    }
                }
                try {
                    var exporter = new CsvExporter();
                    string csv = exporter.Export(match, true);
                    var connector = ServiceLocator.Resolve<IConnector>();
                    connector.SetDefaultCredentials(DatabaseManager.Current.Settings.TabTUsername.Value, DatabaseManager.Current.Settings.TabTPassword.Value);
                    (var errorcode, var serverErrors) = await connector.UploadAsync(csv);
                    if (errorcode != TabTErrorCode.NoError) {
                        var errors = new StringBuilder();
                        errors.AppendLine(Errors.LiveUpdate_UploadError);
                        errors.AppendLine(Strings.LiveUpdate_Uploaded);
                        errors.AppendLine(csv);
                        if (serverErrors.Count() > 0) {
                            errors.AppendLine(Strings.LiveUpdate_ServerErrors);
                            foreach (var e in serverErrors) {
                                if (e != null)
                                    errors.AppendLine(e);
                            }
                        }
                        // log it for debugging purposes
                        Logger.Log(LogType.Exception, errors.ToString());
                    }
                } catch (Exception e) {
                    Logger.Log(e);
                } finally {
                    lock (match) {
                        _isUpdating[match] = false;
                    }
                }
            }
        }

        public void Dispose() {
            //
        }

        private MainWindowViewModel _mainVm;
        private Cell<bool> _canUpload;
        private Dictionary<CompetitiveMatchViewModel, bool> _isUpdating;
    }
}
