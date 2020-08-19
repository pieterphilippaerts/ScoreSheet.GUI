using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.ScoreSheet.Model.Database.Updater;
using PieterP.ScoreSheet.Model.Information;
using PieterP.ScoreSheet.ViewModels.Helpers;
using PieterP.ScoreSheet.ViewModels.Information;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.ScoreSheet.ViewModels.Settings;
using PieterP.ScoreSheet.ViewModels.Wizards;
using PieterP.Shared.Cells;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels {
    public class MainWindowViewModel {
        public MainWindowViewModel(bool debug) {
            _matchContainer = new MatchContainerViewModel();
            this.ActiveMatches = new ObservableCollection<CompetitiveMatchViewModel>();
            this.Screens = new ObservableCollection<ScreenInfo>();
            this.CurrentScreen = Cell.Create<object>(null);
            this.ZoomLevel = DatabaseManager.Current.Settings.ZoomLevel;
            this.ProtectMatchInfo = DatabaseManager.Current.Settings.ProtectMatchInfo;
            this.LatestStatus = ServiceLocator.Resolve<Logger>().LatestMessage;
            this.IsFullScreen = Cell.Create(DatabaseManager.Current.Settings.StartFullScreen.Value);
            _hideNavigation = Cell.Create(false);
            this.IsNavigationVisible = Cell.Derived(this.IsFullScreen, DatabaseManager.Current.Settings.HideNavigation, _hideNavigation,
                (isFullScreen, autoHideNavigation, hideNavigation) => !(isFullScreen && autoHideNavigation && hideNavigation));
            this.About = new AboutCommand();
            this.Update = new UpdateCommand();
            this.AppUpdate = new AppUpdateCommand();
            this.SaveUpdateFile = new SaveUpdateFileCommand();
            this.NewMatchday = new NewMatchdayCommand(this);
            this.NewCustomMatch = new NewCustomMatchCommand(this);
            this.NewDivisionDay = new NewDivisionDayCommand(this);
            this.ProtectMatchInfoClick = new RelayCommand<object>(o => this.ProtectMatchInfo.Value = !this.ProtectMatchInfo.Value);
            this.Upload = new UploadCommand(this);
#if LIMBURG_FREETIME_SUPPORT
            this.Email = new EmailCommand(this);
#endif
            var title = DatabaseManager.Current.Settings.HomeClub.Value;
            this.Open = new OpenCommand(this);
            this.Close = new CloseCommand(this);
            this.CloseAll = new CloseAllCommand(this);
            this.Save = new SaveCommand(this);
            this.SaveAs = new SaveAsCommand(this);
            this.Quit = new QuitCommand(this);
            this.DoQuit = new DoQuitCommand(this);
            this.Export = new ExportCommand(this);
            this.FullScreen = new RelayCommand(() => { this.IsFullScreen.Value = !this.IsFullScreen.Value; });
            this.ShowNavigation = new RelayCommand(() => _hideNavigation.Value = false);
            this.HideNavigation = new RelayCommand(() => _hideNavigation.Value = true);
            this.Print = new PrintCommand(this);
            this.OpenSettings = new OpenSettingsCommand(this);
            this.TranslationError = new LaunchCommand("https://score.pieterp.be/Help/Translation");
            this.SelectedLanguage = DatabaseManager.Current.Settings.ActiveCulture;
            this.ShowLogBook = new RelayCommand<object>(p => {
                NotificationManager.Current.Raise(new ShowPopupNotification() { Context = new LogBookViewModel(debug), Owner = p });
            });
            this.SelectLanguage = new RelayCommand<object>(p => {
                NotificationManager.Current.Raise(new ShowPopupNotification() { Context = new LanguagesPopupViewModel(), Owner = p });
            });
            this.UpdateText = Cell.Create<string?>(null);
            DatabaseManager.Current.Settings.LatestInstalledVersion.ValueChanged += ReevaluateUpdateText;
            ReevaluateUpdateText();

            this.AppTitle = Cell.Derived(DatabaseManager.Current.Settings.HomeClub, hc => {
                if (hc == null)
                    return "ScoreSheet - PieterP.be";
                else
                    return hc + " - ScoreSheet - PieterP.be";
            });

            // initialize services
            _services = new List<IDisposable>();
            var mts = new MatchTrackerService();
            this.AwayMatches = mts.AwayMatches;
            _services.Add(mts);
            _services.Add(new SecondScreenService(this));
            _services.Add(new LiveUpdateService(this));
            _services.Add(new JsonService(this));
            _services.Add(new PhoneHomeService(this));
            _services.Add(new AppUpdateService());
            _services.Add(new MemberListUpdater());
            this.ApplicationExiting += OnExiting;

            if (DatabaseManager.Current.Settings.TurnOnCapsLock.Value) {
                Keyboard.SetKey(NativeMethods.VK_CAPITAL, true);
            }

            // initialize first window
            var orchestrator = new InformationOrchestratorViewModel(DatabaseManager.Current.Settings.OverviewVisualization, this.ActiveMatches, this.AwayMatches);
            _overviewScreen = new ScreenInfo(this, Main_Overview, true, orchestrator, ScreenClick);
            this.Screens.Add(_overviewScreen);
            this.CurrentScreen.Value = orchestrator;
        }

        private void ReevaluateUpdateText() {
            var installed = DatabaseManager.Current.Settings.LatestInstalledVersion.Value;
            var current = Application.Version;

            if (current < installed) {
                this.UpdateText.Value = Strings.UpdateText_Updated;
            } else if (DateTime.Now > Application.ExpiryTime) {
                this.UpdateText.Value = Strings.UpdateText_Outdated;
            }
        }

        public T? GetService<T>() where T : class {
            foreach (var s in _services) {
                if (s is T)
                    return s as T;
            }
            return null;
        }
        private void OnExiting() {
            foreach (var s in _services) {
                s.Dispose();
            }
        }

        public void DoStartupChecks() {
            // this method gets called when the main window is loaded
            CheckForCrash();
            CheckFirstStart();
            CheckDatabaseEmpty();
            ShowWelcome();
        }
        private void CheckFirstStart() {
            if (DatabaseManager.Current.Settings.UpdateSettingsOnStart.Value) {
                DatabaseManager.Current.Settings.UpdateSettingsOnStart.Value = false;

                var showSettingsMessage = new ShowMessageNotification(Main_FirstStart, NotificationTypes.Informational, NotificationButtons.YesNo);
                NotificationManager.Current.Raise(showSettingsMessage);

                if (showSettingsMessage.Result == true) {
                    if (this.OpenSettings.CanExecute(null))
                        this.OpenSettings.Execute(null);
                }
            }
        }
        private void CheckDatabaseEmpty() {
            if (!DatabaseManager.Current.Clubs.Any()) { 
                var showSettingsMessage = new ShowMessageNotification(Main_NoData, NotificationTypes.Question, NotificationButtons.YesNo);
                NotificationManager.Current.Raise(showSettingsMessage);

                if (showSettingsMessage.Result == true) {
                    if (this.Update.CanExecute(null))
                        this.Update.Execute(null);
                }
            }
        }
        private void CheckForCrash() {
            // if there are files in the backup directory, that's probably due to a crash
            var backupIds = DatabaseManager.Current.MatchBackup.MatchIds;
            if (backupIds.Any()) {
                var wiz = new WizardViewModel();
                var newRestoreMatches = new RestoreBackupsViewModel(wiz);
                wiz.CurrentPanel.Value = newRestoreMatches;

                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);

                if (n.Result) {
                    foreach (var m in newRestoreMatches.SelectedMatches) {
                        var vm = new CompetitiveMatchViewModel(m);
                        vm.Dirty.Value = true;
                        AddMatch(vm);
                    }
                    foreach (var m in newRestoreMatches.FoundMatches) {
                        if (!newRestoreMatches.SelectedMatches.Contains(m))
                            DatabaseManager.Current.MatchBackup[m.UniqueId] = null; // delete matches that the user doesn't want restored
                    }
                }
            }
        }
        private void ShowWelcome() {
            var welcomeVm = new WelcomeViewModel();
            var n = new ShowDialogNotification(welcomeVm);
            NotificationManager.Current.Raise(n);
            if (n.Result) {
                switch (welcomeVm.Choice) {
                    case WelcomeChoices.NewMatchday:
                        this.NewMatchday.Execute(null);
                        break;
                    case WelcomeChoices.NewCustomMatch:
                        this.NewCustomMatch.Execute(null);
                        break;
                    case WelcomeChoices.OpenCustomMatch:
                        this.Open.Execute(null);
                        break;
                    case WelcomeChoices.UpdateDatabase:
                        this.Update.Execute(null);
                        break;
                }
            }
        }
        private void ScreenClick(ScreenInfo sender) {
            if (sender == null)
                return; // weird
            foreach (var s in this.Screens) {
                if (s == sender)
                    s.IsActive.Value = true;
                else
                    s.IsActive.Value = false;
            }
            var compMatch = sender.Context as CompetitiveMatchViewModel;
            if (compMatch != null) {
                _matchContainer.ActiveMatch.Value = compMatch;
                if (CurrentScreen.Value != _matchContainer) {
                    CurrentScreen.Value = _matchContainer;
                }
            } else {
                CurrentScreen.Value = sender.Context;
            }
        }

        private void AddMatch(CompetitiveMatchViewModel m) {
            if (m.IsOfficial.Value) {
                if (ActiveMatches.Any(c => c.IsOfficial.Value && c.UniqueId == m.UniqueId))
                    return; // the official match is already open
            } else if (m.Filename.Value != null) {
                if (ActiveMatches.Any(c => !c.IsOfficial.Value && c.Filename.Value == m.Filename.Value))
                    return; // the file is already open
            }
            ActiveMatches.Add(m);
            var title = Cell.Derived(m.HomeTeam.Name, m.AwayTeam.Name, (h, a) => $"{ h }\r\n{ a }");
            Screens.Add(new ScreenInfo(this, title, false, m, ScreenClick));
            // show suspect semantics popup if the match contains a suspect semantic and if the match is the active match
            //m.Score.ContainsSuspectSemantics.ValueChanged += () => {
            //    if (m.Score.ContainsSuspectSemantics.Value && this.CurrentScreen.Value is MatchContainerViewModel) {
            //        var mcvm = this.CurrentScreen.Value as MatchContainerViewModel;
            //        if (mcvm!.ActiveMatch.Value == m) {
            //            NotificationManager.Current.Raise(new ShowPopupNotification() { Context = new SuspectSemanticsViewModel(), PlaceUnderOwner = true });
            //        }
            //    }
            //};
        }

        private bool CloseMatches(IEnumerable<CompetitiveMatchViewModel?> matches) {
            if (matches == null)
                return false;

            var dirtyOfficialMatches = matches.Where(m => m != null && m.IsOfficial.Value && m.Dirty.Value).ToList();
            if (dirtyOfficialMatches.Count > 0) {
                var sb = new StringBuilder();
                sb.AppendLine(Main_UploadWarningHeader + "\r\n");
                foreach (var dom in dirtyOfficialMatches) {
                    if (dom != null)
                        sb.AppendLine($" - { dom.HomeTeam.Name.Value } - { dom.AwayTeam.Name.Value } ({ dom.MatchId.Value })");
                }
                sb.Append("\r\n" + Main_UploadWarningFooter);
                var not = new ShowMessageNotification(sb.ToString(), NotificationTypes.Exclamation, NotificationButtons.YesNo);
                NotificationManager.Current.Raise(not);
                if (not.Result != true)
                    return false;
            }

            var dirtyUnofficialMatches = new List<SelectedMatchInfo>();
            foreach (var m in matches) {
                if (m != null && !m.IsOfficial.Value) {
                    if (m.Dirty.Value) {
                        dirtyUnofficialMatches.Add(new SelectedMatchInfo() {
                            MatchId = m.MatchId.Value,
                            HomeTeam = m.HomeTeam,
                            HomeMatchesWon = m.Score.HomeMatchesWon.Value,
                            AwayTeam = m.AwayTeam,
                            AwayMatchesWon = m.Score.AwayMatchesWon.Value,
                            Result = m.Score.Result.Value,
                            HasValidationErrors = false,
                            ValidationErrors = null,
                            MatchVm = m
                        });
                    }
                } 
            }
            if (dirtyUnofficialMatches.Count > 0) {
                var svm = new SaveViewModel(dirtyUnofficialMatches);
                var not = new ShowDialogNotification(svm);
                NotificationManager.Current.Raise(not);
                if (!not.Result) // user canceled
                    return false;
                if (svm.SaveMatches != null && svm.SaveMatches.Count > 0) {
                    foreach (var m in svm.SaveMatches) {
                        if (!SaveMatch(m.MatchVm))
                            return false; // user canceled
                    }
                }
            }

            foreach (var m in matches) {
                if (m != null) {
                    m.Close();
                    ActiveMatches.Remove(m);
                    var si = Screens.Where(c => c.Context == m).FirstOrDefault();
                    if (si != null) {
                        int index = Screens.IndexOf(si);
                        if (index >= 0)
                            Screens.RemoveAt(index);
                    }
                }
            }
            if (Screens.Count > 0)
                ScreenClick(Screens[0]);
            return true;
        }
        private bool SaveMatch(CompetitiveMatchViewModel match, bool forceDialog = false) {
            string? filename = match.Filename.Value;
            if (forceDialog || filename == null || filename == "") {
                var n = new FileDialogNotification(FileDialogTypes.SaveFile);
                n.Filter = Main_ScoreSheetFiles + " (*.ssjs)|*.ssjs";
                n.InitialFilename = $"{ SS(match.MatchId.Value, "[MATCH]") } - { SS(match.HomeTeam.Name.Value, "[HOME]") } - { SS(match.AwayTeam.Name.Value, "[AWAY]") }".SanitizeForFilename();

                NotificationManager.Current.Raise(n);
                if (n.SelectedPath == null)
                    return false; // user cancelled
                filename = n.SelectedPath;
            }

            var exporter = new MatchExporter();
            if (!Match.ToFile(exporter.ToMatch(match), filename)) {
                NotificationManager.Current.Raise(new ShowMessageNotification(Main_SaveError, NotificationTypes.Error));
            }
            match.Filename.Value = filename;
            match.Dirty.Value = false;
            ((SaveCommand)this.Save).RaiseCanExecuteChanged();
            return true;

            string SS(string s, string d) {
                if (s == null || s == "")
                    return d;
                return s;
            }
        }

        public ObservableCollection<ScreenInfo> Screens { get; private set; }
        public ObservableCollection<CompetitiveMatchViewModel> ActiveMatches { get; private set; }
        public ObservableCollection<AwayMatchInfo> AwayMatches { get; private set; }
        public Cell<object> CurrentScreen { get; private set; }
        public Cell<float> ZoomLevel { get; private set; }
        public Cell<string> LatestStatus { get; private set; }
        public Cell<bool> ProtectMatchInfo { get; private set; }
        public Cell<string> AppTitle { get; private set; }
        public Cell<bool> IsFullScreen { get; private set; }
        public Cell<bool> IsNavigationVisible { get; private set; }
        public Cell<string?> UpdateText { get; set; }
        public Cell<string> SelectedLanguage { get; }
        public ICommand About { get; private set; }
        public ICommand Update { get; private set; }
        public ICommand AppUpdate { get; private set; }
        public ICommand SaveUpdateFile { get; private set; }
        public ICommand NewMatchday { get; private set; }
        public ICommand NewCustomMatch { get; private set; }
        public ICommand ProtectMatchInfoClick { get; private set; }
        public ICommand ExportCsv { get; private set; }
        public ICommand Export { get; private set; }
        public ICommand Upload { get; private set; }
#if LIMBURG_FREETIME_SUPPORT
        public ICommand Email { get; private set; }
#endif
        public ICommand Open { get; private set; }
        public ICommand Save { get; private set; }
        public ICommand SaveAs { get; private set; }
        public ICommand Close { get; private set; }
        public ICommand CloseAll { get; private set; }
        public ICommand Quit { get; private set; } // this is the command that gets called when the user clicks on the Quit-menu 
        public ICommand DoQuit { get; private set; } // this is the command that gets called when the users clicks the X-button
        public ICommand FullScreen { get; private set; }
        public ICommand ShowNavigation { get; private set; }
        public ICommand HideNavigation { get; private set; }
        public ICommand Print { get; private set; }
        public ICommand OpenSettings { get; private set; }
        public ICommand ShowLogBook { get; private set; }
        public ICommand TranslationError { get; private set; }
        public ICommand NewDivisionDay { get; private set; }
        public ICommand SelectLanguage { get; private set; }

        private MatchContainerViewModel _matchContainer;
        private Cell<bool> _hideNavigation;
        private List<IDisposable> _services;
        private ScreenInfo _overviewScreen;

        public event Action ApplicationExiting;
        public event Action ApplicationExit;

        public class ExportCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public ExportCommand(MainWindowViewModel parent) {
                _parent = parent;
                parent.ActiveMatches.CollectionChanged += (a, b) => CanExecuteChanged?.Invoke(this, new EventArgs());
            }

            public bool CanExecute(object parameter) {
                return _parent.ActiveMatches.Count > 0;
            }

            public async void Execute(object parameter) {
                _wizard = new WizardViewModel();
                var selectedMatchesPanel = new SelectMatchesViewModel(_wizard, _parent.ActiveMatches, Wizard_Export, Wizard_ExportDesc, Wizard_ExportMessage, ExportTypes.FileExport, OnExport);
                _wizard.CurrentPanel.Value = selectedMatchesPanel;

                var n = new ShowDialogNotification(_wizard);
                NotificationManager.Current.Raise(n);
            }
            private void OnExport(IEnumerable<CompetitiveMatchViewModel> selectedMatches) {
                _wizard.CurrentPanel.Value = new SelectExportTypeViewModel(_wizard, selectedMatches);
            }
            private MainWindowViewModel _parent;
            private WizardViewModel _wizard;
        }
        public class UploadCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public UploadCommand(MainWindowViewModel parent) {
                _parent = parent;
                parent.ActiveMatches.CollectionChanged += (a, b) => CanExecuteChanged?.Invoke(this, new EventArgs());
            }

            public bool CanExecute(object parameter) {
                return _parent.ActiveMatches.Count > 0;
            }

            public async void Execute(object parameter) {
                var wiz = new WizardViewModel();
                var selectedMatchesPanel = new SelectMatchesViewModel(wiz, _parent.ActiveMatches, Wizard_Upload, Wizard_UploadDesc, Wizard_UploadMessage, ExportTypes.Upload,
                    OnUpload,
                    m => m.IsOfficial.Value && m.MatchSystem.IsCompetitive && m.UploadStatus.Value != UploadStatus.Uploaded,
                    m => m.IsOfficial.Value && m.UploadStatus.Value == UploadStatus.Uploaded,
                    m => !m.MatchSystem.IsCompetitive);
                wiz.CurrentPanel.Value = selectedMatchesPanel;

                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);
            }
            private async void OnUpload(IEnumerable<CompetitiveMatchViewModel> selectedMatches) {
                var uploader = new MatchUploader(true);
                (var errorCode, var errors) = await uploader.Upload(selectedMatches.ToArray());
                switch (errorCode) {
                    case TabTErrorCode.NetworkError:
                    case TabTErrorCode.InvalidCredentials:
                    case TabTErrorCode.DataError:
                        NotificationManager.Current.Raise(new ShowMessageNotification(FormatError(errorCode, errors), NotificationTypes.Error, NotificationButtons.OK));
                        break;
                    case TabTErrorCode.NoError:
                        NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_UploadSuccessful, NotificationTypes.Informational, NotificationButtons.OK));
                        break;
                }
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
            }
            private string FormatError(TabTErrorCode error, IEnumerable<string> errors) {
                var sb = new StringBuilder();
                switch (error) {
                    case TabTErrorCode.DataError:
                        sb.AppendLine(Wizard_ServerError);
                        break;
                    case TabTErrorCode.NetworkError:
                        sb.AppendLine(Wizard_NetworkError);
                        break;
                    case TabTErrorCode.InvalidCredentials:
                        sb.AppendLine(Wizard_InvalidCredentials);
                        break;
                }

                if (errors != null && errors.Any()) {
                    sb.AppendLine();
                    sb.AppendLine(Wizard_ServerErrors);
                    foreach (var e in errors) {
                        sb.AppendLine(" - " + e);
                    }
                }
                return sb.ToString();
            }
            private MainWindowViewModel _parent;
        }
#if LIMBURG_FREETIME_SUPPORT
        public class EmailCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public EmailCommand(MainWindowViewModel parent) {
                _parent = parent;
                _isSending = false;
                parent.ActiveMatches.CollectionChanged += (a, b) => CanExecuteChanged?.Invoke(this, new EventArgs());
            }

            public bool CanExecute(object parameter) {
                return !_isSending && _parent.ActiveMatches.Where(m => !m.MatchSystem.IsCompetitive).Count() > 0;
            }

            public async void Execute(object parameter) {
                var wiz = new WizardViewModel();
                var selectedMatchesPanel = new SelectMatchesViewModel(wiz, _parent.ActiveMatches.Where(m => !m.MatchSystem.IsCompetitive), Wizard_Email, Wizard_EmailDesc, Wizard_EmailMessage, ExportTypes.Email, OnEmail);
                wiz.CurrentPanel.Value = selectedMatchesPanel;

                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);
            }
            private async void OnEmail(IEnumerable<CompetitiveMatchViewModel> selectedMatches) {
                _isSending = true;
                CanExecuteChanged?.Invoke(this, new EventArgs());

                // this is an ideal time, because we are probably connected to the internet
                ServiceLocator.Resolve<PhoneHomeService>()?.CallHome();
                ServiceLocator.Resolve<AppUpdateService>()?.CheckForUpdate();

                var tempVms = selectedMatches.Select(c => c.MatchSystem.GenerateTemplate(c));
                var exporter = ServiceLocator.Resolve<IExportService>();
                var tempFile = DatabaseManager.Current.GetTempFilename(".pdf");
                if (exporter.ToPdfFile(tempVms, tempFile, tempVms.First().IsLandscape)) {
                    var client = new MailClient();
                    var matches = new StringBuilder();
                    foreach (var m in selectedMatches) {
                        matches.AppendLine($" - { m.HomeTeam.Name.Value } tegen { m.AwayTeam.Name.Value } ({ m.MatchId.Value })");
                    }
                    if (await client.Send(tempFile, matches.ToString())) {
                        foreach (var m in selectedMatches) {
                            m.Dirty.Value = false;
                        }
                        NotificationManager.Current.Raise(new ShowMessageNotification(Safe.Format(Limburg_ResultsSent, DatabaseManager.Current.Settings.FreeTimeMailTo.Value), NotificationTypes.Informational, NotificationButtons.OK));
                    } else {
                        NotificationManager.Current.Raise(new ShowMessageNotification(Limburg_ResultsError, NotificationTypes.Error, NotificationButtons.OK));
                    }
                } else {
                    NotificationManager.Current.Raise(new ShowMessageNotification(Limburg_ExportError, NotificationTypes.Error, NotificationButtons.OK));
                }
                _isSending = false;
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
            }
            private MainWindowViewModel _parent;
            private bool _isSending;
        }
#endif
        public class OpenSettingsCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public OpenSettingsCommand(MainWindowViewModel parent) {
                _parent = parent;
            }

            public bool CanExecute(object parameter) {
                return true;
            }

            public async void Execute(object parameter) {
                var settings = new SettingsViewModel(this._parent);
                var n = new ShowDialogNotification(settings);
                NotificationManager.Current.Raise(n);
            }
            private MainWindowViewModel _parent;
        }
        #region AboutCommand
        private class AboutCommand : ICommand {
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var n = new ShowDialogNotification(new AboutWindowViewModel());
                NotificationManager.Current.Raise(n);
            }
        }
        #endregion
        #region AppUpdateCommand
        private class AppUpdateCommand : ICommand {
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var n = new ShowDialogNotification(new UpdateAppViewModel());
                NotificationManager.Current.Raise(n);
            }
        }
        #endregion
        #region UpdateCommand
        private class UpdateCommand : ICommand {
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var wiz = new WizardViewModel();
                wiz.CurrentPanel.Value = new UpdateStartViewModel(wiz);
                
                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);
            }
        }
        #endregion
        #region SaveUpdateFileCommand
        private class SaveUpdateFileCommand : ICommand {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var wiz = new WizardViewModel();
                wiz.CurrentPanel.Value = new UpdateStartViewModel(wiz);

                var n = new FileDialogNotification(FileDialogTypes.SaveFile) {
                    Filter = Main_ScoreSheetUpdateFile + " (*.ssjsu)|*.ssjsu",
                    InitialFilename = "export.ssjsu"
                };
                NotificationManager.Current.Raise(n);
                if (n.SelectedPath != null) {
                    DatabaseManager.Current.Export(n.SelectedPath);
                }
            }
        }
        #endregion
        #region NewMatchdayCommand
        private class NewMatchdayCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public NewMatchdayCommand(MainWindowViewModel parent) {
                _parent = parent;
            }

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var wiz = new WizardViewModel();
                var newDayVm = new NewMatchdayViewModel(wiz);
                wiz.CurrentPanel.Value = newDayVm;

                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);

                if (n.Result) {
                    CompetitiveMatchViewModel? last = null;
                    foreach (var m in newDayVm.Matches) {
                        var fullMatch = DatabaseManager.Current.OfficialMatches[m.MatchId];
                        if (fullMatch != null)
                            last = new CompetitiveMatchViewModel(fullMatch);
                        else
                            last = new CompetitiveMatchViewModel(m);
                        _parent.AddMatch(last);
                    }
                }
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region NewDivisionDayCommand
        private class NewDivisionDayCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public NewDivisionDayCommand(MainWindowViewModel parent) {
                _parent = parent;
            }

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var wiz = new WizardViewModel();
                var newDivDayVm = new NewDivisionDayViewModel(wiz);
                wiz.CurrentPanel.Value = newDivDayVm;

                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);

                if (n.Result) {
                    CompetitiveMatchViewModel? last = null;
                    var updater = new TabTUpdater();
                    foreach (var m in newDivDayVm.SelectedMatches) {
                        var fullMatch = DatabaseManager.Current.OfficialMatches[m.MatchId];
                        if (fullMatch != null)
                            last = new CompetitiveMatchViewModel(fullMatch);
                        else
                            last = new CompetitiveMatchViewModel(m);
                        _parent.AddMatch(last);
                        updater.RefreshMemberList(m.HomeClub, m.PlayerCategory.Value);
                        updater.RefreshMemberList(m.AwayClub, m.PlayerCategory.Value);
                    }
                }
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region NewCustomMatchCommand
        private class NewCustomMatchCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public NewCustomMatchCommand(MainWindowViewModel parent) {
                _parent = parent;
            }

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var wiz = new WizardViewModel();
                //HandicapTable? selectedTable = null;
                MatchSystem? selectedMatchSystem = null;
                //var chooseHandicap = new ChooseHandicapViewModel(wiz, table => {
                //    selectedTable = table;
                //    NotificationManager.Current.Raise(new CloseDialogNotification(true));
                //});
                var newCustMatch = new NewCustomMatchViewModel(wiz, ms => {
                    selectedMatchSystem = ms;
                    //wiz.CurrentPanel.Value = chooseHandicap;
                    NotificationManager.Current.Raise(new CloseDialogNotification(true));
                });
                wiz.CurrentPanel.Value = newCustMatch;

                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);

                if (n.Result) {
                    _parent.AddMatch(new CompetitiveMatchViewModel(selectedMatchSystem!));
                }
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region OpenCommand
        private class OpenCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public OpenCommand(MainWindowViewModel parent) {
                _parent = parent;
            }

            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var n = new FileDialogNotification(FileDialogTypes.OpenFile);
                n.Filter = Main_ScoreSheetFiles + " (*.ssjs)|*.ssjs";
                NotificationManager.Current.Raise(n);
                if (n.SelectedPath != null) {
                    var match = Match.FromFile(n.SelectedPath);
                    if (match == null) {
                        NotificationManager.Current.Raise(new ShowMessageNotification(Main_OpenError, NotificationTypes.Error));
                    } else {
                        _parent.AddMatch(new CompetitiveMatchViewModel(match, n.SelectedPath));
                    }
                }
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region SaveCommand
        private class SaveCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public SaveCommand(MainWindowViewModel parent) {
                _parent = parent;
                parent.CurrentScreen.ValueChanged += () => CanExecuteChanged?.Invoke(this, new EventArgs());
                parent._matchContainer.ActiveMatch.ValueChanged += () => CanExecuteChanged?.Invoke(this, new EventArgs());
            }
            public bool CanExecute(object parameter) {
                return _parent.CurrentScreen.Value == _parent._matchContainer
                    && _parent._matchContainer.ActiveMatch.Value != null
                    && _parent._matchContainer.ActiveMatch.Value.IsOfficial.Value == false
                    && _parent._matchContainer.ActiveMatch.Value.Filename.Value != null;
            }
            public void Execute(object parameter) {
                var match = _parent._matchContainer.ActiveMatch.Value;
                if (match == null || match.Filename.Value == null)
                    return;
                _parent.SaveMatch(match);
            }
            internal void RaiseCanExecuteChanged() {
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region SaveAsCommand
        private class SaveAsCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public SaveAsCommand(MainWindowViewModel parent) {
                _parent = parent;
                parent.CurrentScreen.ValueChanged += () => CanExecuteChanged?.Invoke(this, new EventArgs());
                parent._matchContainer.ActiveMatch.ValueChanged += () => CanExecuteChanged?.Invoke(this, new EventArgs());
            }
            public bool CanExecute(object parameter) {
                return _parent.CurrentScreen.Value == _parent._matchContainer 
                    && _parent._matchContainer.ActiveMatch.Value != null
                    && _parent._matchContainer.ActiveMatch.Value.IsOfficial.Value == false;
            }
            public void Execute(object parameter) {
                var match = _parent._matchContainer.ActiveMatch.Value;
                if (match == null)
                    return;
                _parent.SaveMatch(match, true);
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region CloseCommand
        private class CloseCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public CloseCommand(MainWindowViewModel parent) {
                _parent = parent;
                parent.CurrentScreen.ValueChanged += () => CanExecuteChanged?.Invoke(this, new EventArgs());
            }
            public bool CanExecute(object parameter) {
                return _parent.CurrentScreen.Value == _parent._matchContainer;
            }
            public void Execute(object parameter) {
                _parent.CloseMatches(new CompetitiveMatchViewModel?[] { _parent._matchContainer.ActiveMatch?.Value });
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region CloseAllCommand
        private class CloseAllCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public CloseAllCommand(MainWindowViewModel parent) {
                _parent = parent;
                parent.ActiveMatches.CollectionChanged += (sender, e) => CanExecuteChanged?.Invoke(this, new EventArgs());
            }
            public bool CanExecute(object parameter) {
                return _parent.ActiveMatches.Count > 0;
            }
            public void Execute(object parameter) {
                _parent.CloseMatches(_parent.ActiveMatches.ToList());
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region QuitCommand
        private class QuitCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public QuitCommand(MainWindowViewModel parent) {
                _parent = parent;
            }
            public bool CanExecute(object parameter) {
                return true;
            }
            public void Execute(object parameter) {
                if (_parent.DoQuit.CanExecute(parameter))
                    _parent.DoQuit.Execute(parameter);
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region DoQuitCommand
        private class DoQuitCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public DoQuitCommand(MainWindowViewModel parent) {
                _parent = parent;
            }
            public bool CanExecute(object parameter) {
                return _parent.CloseMatches(_parent.ActiveMatches.ToList());
            }
            public void Execute(object parameter) {
                _parent.ApplicationExiting?.Invoke();
                _parent.ApplicationExit?.Invoke();
            }

            private MainWindowViewModel _parent;
        }
        #endregion
        #region PrintCommand
        private class PrintCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public PrintCommand(MainWindowViewModel parent) {
                _parent = parent;
                parent.CurrentScreen.ValueChanged += () => CanExecuteChanged?.Invoke(this, new EventArgs());
            }
            public bool CanExecute(object parameter) {
                return _parent.CurrentScreen.Value == _parent._matchContainer;
            }
            public void Execute(object parameter) {
                var wiz = new WizardViewModel();
                var printVm = new PrintViewModel(wiz, _parent._matchContainer.ActiveMatch.Value!);
                wiz.CurrentPanel.Value = printVm;

                var n = new ShowDialogNotification(wiz);
                NotificationManager.Current.Raise(n);
            }

            private MainWindowViewModel _parent;
        }
        #endregion

        public class ScreenInfo {
            public ScreenInfo(MainWindowViewModel parent, string text, bool active, object dataContext, Action<ScreenInfo> click)
                : this(parent, Cell.Create(text), active, dataContext, click) {
            }
            public ScreenInfo(MainWindowViewModel parent, Cell<string> text, bool active, object dataContext, Action<ScreenInfo> click, MatchStatus initialStatus = MatchStatus.None) {
                _parent = parent;
                this.IsActive = Cell.Create(active);
                this.Text = text;
                this.Context = dataContext;
                this.Click = new RelayCommand<ScreenInfo>(click);
                this.Close = new CloseCommand(this);
                this.Print = new PrintCommand(this);

                // if match, change status depending on match updates
                var match = dataContext as CompetitiveMatchViewModel;
                if (match != null) {
                    this.Status = match.MatchStatus;
                    this.ShowResults = DatabaseManager.Current.Settings.ShowResultsInNavigation;
                    this.HomeMatchesWon = match.Score.HomeMatchesWon;
                    this.AwayMatchesWon = match.Score.AwayMatchesWon;
                } else {
                    this.Status = Cell.Create(initialStatus);
                    this.ShowResults = Cell.Create(false);
                }
            }
            public Cell<bool> ShowResults { get; }
            public Cell<int> HomeMatchesWon { get; }
            public Cell<int> AwayMatchesWon { get; }

            public Cell<bool> IsActive { get; set; }
            public ICommand Click { get; private set; }
            public Cell<string> Text { get; set; }
            public object Context { get; set; }
            public Cell<MatchStatus> Status { get; set; }
            
            private MainWindowViewModel _parent;

            public ICommand Close { get; private set; }
            public ICommand Print { get; private set; }

            private class CloseCommand : ICommand {
                public event EventHandler CanExecuteChanged;

                public CloseCommand(ScreenInfo parent) {
                    _parent = parent;
                }
                public bool CanExecute(object parameter) {
                    return _parent.Context is CompetitiveMatchViewModel;
                }
                public void Execute(object parameter) {
                    _parent._parent.CloseMatches(new CompetitiveMatchViewModel?[] { _parent.Context as CompetitiveMatchViewModel });
                }

                private ScreenInfo _parent;
            }
            private class PrintCommand : ICommand {
                public event EventHandler CanExecuteChanged;

                public PrintCommand(ScreenInfo parent) {
                    _parent = parent;
                }
                public bool CanExecute(object parameter) {
                    return _parent.Context is CompetitiveMatchViewModel;
                }
                public void Execute(object parameter) {
                    var wiz = new WizardViewModel();
                    var printVm = new PrintViewModel(wiz, _parent.Context as CompetitiveMatchViewModel);
                    wiz.CurrentPanel.Value = printVm;
                    var n = new ShowDialogNotification(wiz);
                    NotificationManager.Current.Raise(n);
                }

                private ScreenInfo _parent;
            }
        }
    }
}
