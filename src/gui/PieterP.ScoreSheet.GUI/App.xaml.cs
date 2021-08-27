using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PieterP.ScoreSheet.ViewModels;
using PieterP.ScoreSheet.GUI.Views;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared;
using PieterP.ScoreSheet.Model.Database.MatchSystems;
using PieterP.ScoreSheet.ViewModels.Score.MatchSystems;
using PieterP.ScoreSheet.GUI.Services;
using PieterP.ScoreSheet.ViewModels.Notifications;
using System.IO;
using PieterP.ScoreSheet.GUI.ViewModels;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.Shared.Interfaces;
using PieterP.ScoreSheet.ViewModels.Helpers;
using System.Windows.Interop;
using PieterP.ScoreSheet.GUI.Helpers;
using System.Windows.Input;
using System.Threading;
using PieterP.Shared.Services;
using PieterP.ScoreSheet.Model;
using System.Windows.Media;
using PieterP.Shared.Cells;
using System.Globalization;
using System.Diagnostics;
using System.Net;
using PieterP.ScoreSheet.Model.Interfaces;

namespace PieterP.ScoreSheet.GUI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var appSettings = ParseArguments(e.Args);

            // by default, only SSL3 and TLS1.0 are allowed
            try {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, policyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)0x3000 /* Tls13*/;
            } catch (NotSupportedException) {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // older version of the framework maybe?
            }

            InitializeServices();
            SetupNotificationHandling();
            SetupExceptionHandling();

            var c = new CultureInfo(DatabaseManager.Current.Settings.ActiveCulture.Value);
            Thread.CurrentThread.CurrentCulture = c;
            Thread.CurrentThread.CurrentUICulture = c;

            if (!appSettings.Launched) {
                // not started from launcher; run launcher and exit
                string launcher = Path.Combine(DatabaseManager.Current.BasePath, "launcher.exe");
                if (File.Exists(launcher)) {
                    Process.Start(launcher);
                    Application.Current.Shutdown();
                    return;
                }
            }

            Logger.Log(LogType.Informational, Safe.Format(Strings.App_ApplicationStarted, DateTime.Now.ToString("D"), DateTime.Now.ToString("T")));

            _mutex = new Mutex(true, "ScoreSheet-" + appSettings.Profile, out var created);
            if (!created) {
                _mutex?.Dispose();
                NotificationManager.Current.Raise(new ShowMessageNotification(Strings.App_MultipleInstances, NotificationTypes.Exclamation));
                Application.Current.Shutdown();
                return;
            }

            var settings = DatabaseManager.Current.Settings;
            if (settings.UniqueId.Value == null)
                settings.UniqueId.Value = Guid.NewGuid();

            ThemeChanged = Cell.Derived(settings.ThemePath,
                settings.SelectedBackgroundColor, settings.SelectedTextBoxColor, settings.SelectedTextColor,
                settings.SelectedErrorBackgroundColor, settings.SelectedErrorTextColor,
                (tp, bgc, tbc, tc, ebc, etc)
                => new SelectedTheme() {
                    Uri = tp == "" ? null : new Uri(tp, UriKind.Relative),
                    BackgroundColor = bgc,
                    TextBoxColor = tbc,
                    TextColor = tc,
                    ErrorBackgroundColor = ebc,
                    ErrorTextColor = etc
                });
            ThemeChanged.ValueChanged += () => ChangeTheme(ThemeChanged.Value);
            ThemeChanged.NotifyObservers();

            // create the main window
            var window = new MainWindow();
            var windowVm = new MainWindowViewModel(appSettings.Debug);
            windowVm.ApplicationExit += () => {
                Logger.Log(LogType.Debug, Safe.Format(Strings.App_ApplicationStarted, DateTime.Now.ToString("D"), DateTime.Now.ToString("T")));
                _mutex?.Dispose();
                _mutex = null;
                Application.Current.Shutdown();
            };
            window.DataContext = windowVm;
            window.Show();
            windowVm.DoStartupChecks();
        }

        private ApplicationSettings ParseArguments(string[] args) {
            var settings = new ApplicationSettings();
            if (args != null && args.Length > 0) {
                foreach (var a in args) {
                    if (a != null) {
                        var l = a.ToLower();
                        if (l == "debug") {
                            settings.Debug = true;
                        } else if (l.StartsWith("profile=")) {
                            var p = a.Substring(8).SanitizeForFilename();
                            if (p != null)
                                settings.Profile = p;
                        } else if (l == "launched") {
                            settings.Launched = true;
                        }
                    }
                }
            }
            return settings;
        }
        private void InitializeServices() {
            ServiceLocator.RegisterInstance<DatabaseManager>(new DatabaseManager(appSettings.Profile));
            ServiceLocator.RegisterInstance<Logger>(new Logger(Path.Combine(ServiceLocator.Resolve<DatabaseManager>().ActiveProfilePath, "log.txt"), appSettings.Debug));
            ServiceLocator.RegisterType<IRegionFinder, Model.Information.RegionFinder>();
            ServiceLocator.RegisterType<IConnector, AutoRetryConnector>();
            ServiceLocator.RegisterInstance<MatchSystemFactory>(new MatchSystemFactory(new MatchSystem[] {
                new InterclubMenMatchSystem(),
                new InterclubWomenVeteransMatchSystem(),
                new SuperMatchSystem(),
                new Youth2v2MatchSystem(),
                new FreeTimeMatchSystem(),
                new BelgianCupMatchSystem(),
                new FlemishCupMatchSystem(),
                new AfttCupMatchSystem(),
                new InterclubMenOldMatchSystem(),
                new InterclubWomenVeteransOldMatchSystem()
            }));
            ServiceLocator.RegisterInstance<WindowService>(new WindowService());
            ServiceLocator.RegisterInstance<IMainWindowHandle>(new MainWindowHandle());
            ServiceLocator.RegisterInstance<IActiveWindowHandle>(new ActiveWindowHandle());
            ServiceLocator.RegisterInstance<IWindowHandleLookup>(new WindowHandleLookup());
            ServiceLocator.RegisterType<ITimerService, WpfTimerService>();
            ServiceLocator.RegisterType<IExportService, ExportService>();
            ServiceLocator.RegisterInstance<IConnectorFactory>(new ConnectorFactory());
            ServiceLocator.RegisterInstance<INetworkAvailabilityService>(new NetworkAvailabilityService());
        }

        private void SetupNotificationHandling() {
            var nm = new NotificationManager();
            nm.RegisterFor<ShowDialogNotification>(k => {
                k.Result = ServiceLocator.Resolve<WindowService>().ShowDialog(k.Contents);
            });
            nm.RegisterFor<CloseDialogNotification>(k => {
                ServiceLocator.Resolve<WindowService>().CloseDialog(k.Result);
            });
            nm.RegisterFor<ShowMessageNotification>(k => {
                k.Result = ServiceLocator.Resolve<WindowService>().ShowMessage(k.Message, k.Type, k.Buttons);
            });
            nm.RegisterFor<FileDialogNotification>(k => {
                k.SelectedPath = ServiceLocator.Resolve<WindowService>().ShowFileDialog(k.Type, k.InitialDirectory, k.Filter, k.InitialFilename, k.Title);
            });
            nm.RegisterFor<CloseWindowNotification>(k => {
                ServiceLocator.Resolve<WindowService>().CloseWindow(k.Window);
            });
            nm.RegisterFor<ShowWindowNotification>(k => {
                k.Window = ServiceLocator.Resolve<WindowService>().ShowWindow(k.ViewModel);
            });
            nm.RegisterFor<ColorDialogNotification>(k => {
                k.SelectedColor = ServiceLocator.Resolve<WindowService>().ShowColorDialog(k.InitialColor, k.CustomColors);
            });
            nm.RegisterFor<ShowPopupNotification>(k => {
                ServiceLocator.Resolve<WindowService>().OpenPopup(k.Context, k.Owner, k.PlaceUnderOwner);
            });
            ServiceLocator.RegisterInstance<NotificationManager>(nm);
        }
        private void SetupExceptionHandling() {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                _mutex?.Dispose();
                _mutex = null;
                HandleException(e.ExceptionObject as Exception, e.IsTerminating);
            };
            DispatcherUnhandledException += (s, e) => { e.Handled = true; HandleException(e.Exception, false); };
            TaskScheduler.UnobservedTaskException += (s, e) => HandleException(e.Exception, false);
        }
        private void HandleException(Exception? e, bool isTerminating) {
            ServiceLocator.Resolve<Logger>().LogException(e);
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) // if we're not in an STA thread, we cannot show the error UI
                ServiceLocator.Resolve<WindowService>().ShowDialog(new ErrorWindowViewModel() { ErrorText = e?.ToString() ?? Strings.App_UnknownError, IsTerminating = isTerminating });
        }

        public ResourceDictionary ThemeDictionary {
            // You could probably get it via its name with some query logic as well.
            get {
                return Resources.MergedDictionaries[0];
            }
        }
        public void ChangeTheme(SelectedTheme theme) {
            if (theme.Uri == null) {
                var bc = new BrushConverter();
                var dict = new ResourceDictionary();
                dict["BackgroundBrush"] = bc.ConvertFromString(theme.BackgroundColor);
                dict["EditableBackgroundBrush"] = bc.ConvertFromString(theme.TextBoxColor);
                dict["TextBrush"] = bc.ConvertFromString(theme.TextColor);
                dict["ErrorTextBrush"] = bc.ConvertFromString(theme.ErrorTextColor);
                dict["ErrorBrush"] = bc.ConvertFromString(theme.ErrorBackgroundColor);
                ThemeDictionary.MergedDictionaries.Add(dict);
            } else {
                ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = theme.Uri });
            }
        }

        private Mutex? _mutex;
        public Cell<SelectedTheme> ThemeChanged { get; private set; }
    }

    public class ApplicationSettings {
        public ApplicationSettings() {
            this.Debug = false;
            this.Launched = false;
            this.Profile = "default";
        }
        public bool Debug { get; set; }
        public string Profile { get; set; }
        public bool Launched { get; set; }
    }
    public class SelectedTheme { 
        public Uri? Uri { get; set; }
        public string BackgroundColor { get; set; }
        public string TextBoxColor { get; set; }
        public string TextColor { get; set; }
        public string ErrorTextColor { get; set; }
        public string ErrorBackgroundColor { get; set; }
    }
}