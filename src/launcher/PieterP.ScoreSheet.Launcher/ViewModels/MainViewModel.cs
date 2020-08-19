using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Launcher.Database;
using PieterP.ScoreSheet.Launcher.Models;
using static PieterP.ScoreSheet.Launcher.Localization.Strings;
using PieterP.ScoreSheet.Launcher.Localization;

namespace PieterP.ScoreSheet.Launcher.ViewModels {
    public class MainViewModel : INotifyPropertyChanged {
        public MainViewModel() {
            this.Debug = false;

            this.Versions = AppVersions.All.OrderByDescending(v => v.Version).Select(v => new VersionViewModel(this) {
                AppVersion = v,
                Title = $"v{ v.Version.ToString(3) }",
                SubTitle = Safe.Format(Main_InstalledOn, v.InstalledOn.ToString("dd/MM/yyy")),
                IsAvailable = v.IsExeAvailable,
                Default = false
            }).ToList();
            if (DatabaseManager.Current.LaunchSettings.DefaultVersion != null) {
                this.SelectedVersion = this.Versions.Where(v => v.AppVersion.Version == DatabaseManager.Current.LaunchSettings.DefaultVersion).FirstOrDefault();
            }
            this.Versions.Insert(0, new VersionViewModel(this) {
                AppVersion = null,
                Title = Main_HighestInstalled,
                SubTitle = Main_Recommended,
                IsAvailable = this.Versions.Where(v => v.IsAvailable).Count() > 0,
                Default = false
            });
            if (this.SelectedVersion == null) {
                this.SelectedVersion = this.Versions[0];
            }
            this.SelectedVersion.Default = true;

            var profileList = new ObservableCollection<ProfileViewModel>();
            foreach (var p in AppProfiles.All) {
                profileList.Add(new ProfileViewModel(this) {
                    AppProfile = p,
                    Title = p.Name,
                    SubTitle = Safe.Format(Main_CreatedOn, p.CreatedOn.ToString("dd/MM/yyy")),
                    Default = false
                });
            }
            this.Profiles = profileList;
            if (DatabaseManager.Current.LaunchSettings.DefaultProfile != null) {
                this.SelectedProfile = this.Profiles.Where(v => v.AppProfile.Name == DatabaseManager.Current.LaunchSettings.DefaultProfile).FirstOrDefault();
            }
            var def = this.Profiles.Where(p => p.AppProfile.Name == "default").FirstOrDefault();
            if (def != null) {
                this.Profiles.Remove(def);
                this.Profiles.Insert(0, def);
                def.SubTitle = Main_DefaultProfile;
            } else {
                this.Profiles.Insert(0, new ProfileViewModel(this) {
                    AppProfile = null,
                    Title = Main_Default,
                    SubTitle = Main_DefaultProfile
                }); ;
            }
            if (this.SelectedProfile == null) {
                this.SelectedProfile = this.Profiles[0];
            }
            this.SelectedProfile.Default = true;

            this.Start = new RelayCommand(OnStart, () => this.Versions.Where(v => v.IsAvailable).Count() > 1);
            this.NewProfile = new RelayCommand(OnNewProfile);
            this.CreateNewProfile = new CreateNewProfileCommand(this);
            this.CancelNewProfile = new RelayCommand(OnCancelNewProfile);
        }

        private void SetDefault(VersionViewModel vvm) {
            foreach (var v in this.Versions) {
                v.Default = v == vvm;
            }
            DatabaseManager.Current.LaunchSettings.DefaultVersion = vvm.AppVersion?.Version;
        }
        private void SetDefault(ProfileViewModel pvm) {
            foreach (var p in this.Profiles) {
                p.Default = p == pvm;
            }
            DatabaseManager.Current.LaunchSettings.DefaultProfile = pvm.AppProfile?.Name;
        }
        private void OnStart() {
            var version = this.SelectedVersion;
            if (version.AppVersion == null) {
                version = this.Versions.Skip(1).Where(v => v.IsAvailable).First(); 
            }
            version.AppVersion.Run(this.SelectedProfile.AppProfile, this.Debug);
        }
        private void OnNewProfile() {
            this.NewProfileName = "";
            RaisePropertyChanged(nameof(NewProfileName));
            this.ShowNewProfile = true;
            RaisePropertyChanged(nameof(ShowNewProfile));
        }
        private void OnCancelNewProfile() {
            this.ShowNewProfile = false;
            RaisePropertyChanged(nameof(ShowNewProfile));
        }
        private void OnCreateNewProfile() {
            this.Profiles.Add(new ProfileViewModel(this) {
                Default = false,
                Title = NewProfileName,
                SubTitle = Safe.Format(Main_CreatedOn, DateTime.Now.ToString("dd/MM/yyy")),
                AppProfile = AppProfile.CreateNew(NewProfileName)
            });
            this.ShowNewProfile = false;
            RaisePropertyChanged(nameof(ShowNewProfile));
        }

        public IList<VersionViewModel> Versions { get; private set; }
        public VersionViewModel SelectedVersion { get; set; }
        public IList<ProfileViewModel> Profiles { get; private set; }
        public ProfileViewModel SelectedProfile { get; set; }
        public bool Debug { get; set; }
        public bool ShowNewProfile { get; private set; }
        public string NewProfileName {
            get {
                return _newProfileName;
            }
            set {
                _newProfileName = value;
                RaisePropertyChanged(nameof(NewProfileName));
            }
        }
        public ICommand Start { get; private set; }
        public ICommand NewProfile { get; private set; }
        public ICommand CreateNewProfile { get; private set; }
        public ICommand CancelNewProfile { get; private set; }

        private string _newProfileName;

        private void RaisePropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public class CreateNewProfileCommand : ICommand {
            public CreateNewProfileCommand(MainViewModel mainVm) {
                _mainVm = mainVm;
                _mainVm.PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(mainVm.NewProfileName)) {
                        CanExecuteChanged?.Invoke(this, new EventArgs());
                    }
                };
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) {
                var invalidChars = Path.GetInvalidFileNameChars();
                return _mainVm.NewProfileName != null && 
                    _mainVm.NewProfileName.Length > 0 && 
                    !_mainVm.NewProfileName.Any(c => invalidChars.Contains(c)) &&
                    !_mainVm.Profiles.Any(p => p.Title == _mainVm.NewProfileName);
            }
            public void Execute(object parameter) {
                _mainVm.OnCreateNewProfile();
            }

            private MainViewModel _mainVm;
        }
        public class VersionViewModel : INotifyPropertyChanged {
            public VersionViewModel(MainViewModel parent) {
                this.SetDefault = new RelayCommand(() => parent.SetDefault(this), () => this.IsAvailable);
            }
            public string Title { get; set; }
            public string SubTitle { get; set; }
            public bool IsAvailable { get; set; }
            public bool Default {
                get {
                    return _default;
                }
                set {
                    _default = value;
                    RaisePropertyChanged(nameof(Default));
                }
            }
            public AppVersion AppVersion { get; set; }
            public ICommand SetDefault { get; private set; }

            private void RaisePropertyChanged(string name) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            public event PropertyChangedEventHandler PropertyChanged;
            private bool _default;
        }
        public class ProfileViewModel : INotifyPropertyChanged {
            public ProfileViewModel(MainViewModel parent) {
                this.SetDefault = new RelayCommand(() => parent.SetDefault(this));
            }
            public string Title { get; set; }
            public string SubTitle { get; set; }
            public bool Default {
                get {
                    return _default;
                }
                set {
                    _default = value;
                    RaisePropertyChanged(nameof(Default));
                }
            }
            public AppProfile AppProfile { get; set; }
            public ICommand SetDefault { get; private set; }

            private void RaisePropertyChanged(string name) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            public event PropertyChangedEventHandler PropertyChanged;
            private bool _default;
        }
    }
}