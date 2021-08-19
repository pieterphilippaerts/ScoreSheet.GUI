using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PieterP.ScoreSheet.Launcher.Database;
using PieterP.ScoreSheet.Launcher.Models;
using PieterP.ScoreSheet.Launcher.ViewModels;
using PieterP.ScoreSheet.Launcher.Views;
using static PInvoke.User32;
using MBO = PInvoke.User32.MessageBoxOptions;
using static PieterP.ScoreSheet.Launcher.Localization.Strings;
using System.Globalization;
using System.Threading;
using PieterP.ScoreSheet.Launcher.Localization;
using System.Reflection;
using System.Collections;
using System.Resources;

namespace PieterP.ScoreSheet.Launcher {
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            SetupExceptionHandling();

            // By default, resource files work with sattelite assemblies. Because there seem to be some problems with resource files on
            // .NET Framework 3.5, we hack the translated resource files into the ResourceManager that is used by the Strings class
            AddResources();

            var appSettings = ParseArguments(e.Args);

            try {
                var ci = new CultureInfo(DatabaseManager.Current.CultureSettings.DefaultCulture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                Strings.Culture = ci;
            } catch {}

            if (appSettings.EnableTls) {
                if (TryEnableTls()) {
                    MessageBox(IntPtr.Zero, App_TlsEnabled, App_ChangesMade, MBO.MB_TOPMOST | MBO.MB_SETFOREGROUND | MBO.MB_ICONINFORMATION);
                } else {
                    MessageBox(IntPtr.Zero, App_TlsNotEnabled, App_NoChangesMade, MBO.MB_TOPMOST | MBO.MB_SETFOREGROUND | MBO.MB_ICONERROR);
                }
                Application.Current.Shutdown();
                return;
            }

            if (!appSettings.SkipChecks) {
                try {
                    var prereqVm = new PrereqViewModel();
                    if (!prereqVm.ArePrereqsMet) {
                        var prereqWin = new PrereqWindow();
                        prereqWin.DataContext = prereqVm;
                        prereqWin.ShowDialog();
                        //if (prereqWin.ShowDialog() != true) {
                        //    Application.Current.Shutdown();
                        //    return;
                        //}
                    }
                } catch {
                    // weird
                }
            }

            var useProfile = FindProfile(appSettings.Profile);
            bool hasNotBeenStarted = true;
            if (AppProfiles.All.Count() > 1) {
                string profileName = (useProfile?.Name) ?? "default";
                using (var mutex = new Mutex(true, "ScoreSheet-" + profileName, out hasNotBeenStarted)) { }

            }

            if (IsShiftDown() || appSettings.Choose || !hasNotBeenStarted) {
                var window = new MainWindow();
                window.DataContext = new MainViewModel();
                window.ShowDialog();
            } else {
                // start ScoreSheet
                if (!StartScoreSheet(useProfile)) {
                    MessageBox(IntPtr.Zero, App_NoVersionFound, "ScoreSheet", MBO.MB_TOPMOST | MBO.MB_SETFOREGROUND | MBO.MB_ICONERROR);
                }
            }
            Application.Current.Shutdown();
        }
        private AppProfile FindProfile(string commandLineProfile) {
            var defaultProfile = DatabaseManager.Current.LaunchSettings.DefaultProfile;
            AppProfile selectedProfile = null;
            if (commandLineProfile != null) {
                selectedProfile = AppProfiles.All.Where(p => p.Name == commandLineProfile).FirstOrDefault();
            }
            if (selectedProfile == null) {
                selectedProfile = AppProfiles.All.Where(p => p.Name == defaultProfile).FirstOrDefault();
            }
            return selectedProfile;
        }
        private void AddResources() {
            try {
                var manager = Strings.ResourceManager;
                var managerType = manager.GetType();
                var oldField = managerType.GetField("ResourceSets", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                var oldResourceTable = oldField?.GetValue(manager) as Hashtable; // .NET 3.5

                var newField = managerType.GetField("_resourceSets", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                var newResourceTable = newField?.GetValue(manager) as Dictionary<string, ResourceSet>; // .NET 4.0+

                if (oldResourceTable != null || newResourceTable != null) {
                    AddResource("PieterP.ScoreSheet.Launcher.Localization.StringsNl.resources", oldResourceTable, newResourceTable, "nl", "nl-BE");
                    AddResource("PieterP.ScoreSheet.Launcher.Localization.StringsFr.resources", oldResourceTable, newResourceTable, "fr", "fr-BE");
                    AddResource("PieterP.ScoreSheet.Launcher.Localization.StringsDe.resources", oldResourceTable, newResourceTable, "de", "de-DE");
                }
            } catch (Exception e) {
                HandleException(e, false);
            }
        }
        private void AddResource(string resourceName, Hashtable oldResourceTable, Dictionary<string, ResourceSet> newResourceTable, params string[] cultures) {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                var reader = new ResourceReader(stream);
                var resourceSet = new ResourceSet(reader);
                foreach (var c in cultures) {
                    if (oldResourceTable != null)
                        oldResourceTable[new CultureInfo(c)] = resourceSet;
                    if (newResourceTable != null)
                        newResourceTable[c] = resourceSet;
                }
            }
        }
        private bool StartScoreSheet(AppProfile selectedProfile) {
            var defaultVersion = DatabaseManager.Current.LaunchSettings.DefaultVersion;
            var selectedVersion = AppVersions.All.Where(v => v.Version == defaultVersion).FirstOrDefault();
            if (selectedVersion == null)
                selectedVersion = AppVersions.All.Where(w => w.IsExeAvailable).OrderByDescending(v => v.Version).FirstOrDefault();

            if (selectedVersion != null) {
                selectedVersion.Run(selectedProfile);
                return true;
            }
            return false;
        }
        private IDictionary<Version, DirectoryInfo> GetVersions(string versionsPath) {
            var appsDir = new DirectoryInfo(DatabaseManager.Current.VersionsPath);
            var subdirs = appsDir.GetDirectories();
            var ret = new Dictionary<Version, DirectoryInfo>();
            foreach (var subdir in subdirs) {
                try {
                    var v = new Version(subdir.Name);
                    ret[v] = subdir;
                } catch { }
            }
            return ret;
        }

        private ApplicationSettings ParseArguments(string[] args) {
            var settings = new ApplicationSettings();
            try {
                if (args != null && args.Length > 0) {
                    foreach (var a in args) {
                        if (a != null) {
                            var l = a.ToLower();
                            if (l == "skipchecks") {
                                settings.SkipChecks = true;
                            } else if (l == "enabletls") {
                                settings.EnableTls = true;
                            } else if (l == "choose") {
                                settings.Choose = true;
                            } else if (l.StartsWith("profile=")) {
                                settings.Profile = l.Substring(8).Trim('"');
                            }
                        }
                    }
                }
            } catch (Exception e) {
                HandleException(e, false);
            }
            return settings;
        }
        private void SetupExceptionHandling() {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                HandleException(e.ExceptionObject as Exception, e.IsTerminating);
            };
        }
        private void HandleException(Exception e, bool isTerminating) {
            // log the exception
            string file = "";
            try {
                file = Path.Combine(DatabaseManager.Current.BasePath, "ScoreSheet-launcher-error.txt");
                using (var writer = new StreamWriter(file, true)) {
                    writer.WriteLine("***************");
                    writer.WriteLine("ScoreSheet Launcher Error at " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    if (e != null)
                        writer.WriteLine(e.ToString());
                    else
                        writer.WriteLine("No exception object found.");
                    if (isTerminating)
                        writer.WriteLine("The launcher is now terminating.");
                    writer.WriteLine("~~~~~~~~~~~~~~~\r\n");
                }
            } catch { }
            // show the exception to the user
            try {
                string message = e == null ? "No exception object." : (e.Message ?? "No exception message.");
                MessageBox(IntPtr.Zero, $"An unexpected error occurred while launching ScoreSheet. The error message is: '{ message }'. A more-detailed description can be found in the log file ({ file }). Please email this file to score@pieterp.be so we can fix the problem. Thanks!", "Launcher problem...", MBO.MB_OK | MBO.MB_ICONERROR | MBO.MB_TOPMOST);
            } catch { }
        }

        private bool TryEnableTls() {
            try {
                //var clientKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Client");
                //clientKey?.SetValue("DisabledByDefault", 0, RegistryValueKind.DWord);
                var clientKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client");
                clientKey?.SetValue("DisabledByDefault", 0, RegistryValueKind.DWord);
                return true;
            } catch { 
                return false;
            }
        }

        private bool IsShiftDown() {
            return (GetAsyncKeyState((int)VirtualKey.VK_SHIFT) & 0x8000) != 0; 
        }

        private class ApplicationSettings {
            public ApplicationSettings() {
                this.SkipChecks = false;
                this.EnableTls = false;
                this.Choose = false;
            }
            public bool SkipChecks { get; set; }
            public bool EnableTls { get; set; }
            public bool Choose { get; set; }
            public string Profile { get; set; }
        }
    }    
}