using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using PieterP.ScoreSheet.Installer.Localization;
using PieterP.ScoreSheet.Installer.ViewModels;
using PieterP.ScoreSheet.Installer.Views;
using static PieterP.ScoreSheet.Installer.Localization.Strings;

namespace PieterP.ScoreSheet.Installer {
    /// <summary>
    /// This application must be a single executable! Hence, we cannot use any libraries except what's available in .NET Framework 3.5.
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // By default, resource files work with sattelite assemblies. Because we don't want those sattelites in our installation program,
            // we hack the translated resource files into the ResourceManager that is used by the Strings class
            AddResources();

            var appSettings = ParseArguments(e.Args);
            try {
                var ci = new CultureInfo(appSettings.Culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                Strings.Culture = ci;
            } catch { }

            if (appSettings.EnableTls) {
                if (TryEnableTls()) {
                    MessageBox.Show(Strings.App_TlsEnabled, App_ChangesMade, MessageBoxButton.OK, MessageBoxImage.Information);
                } else {
                    MessageBox.Show(App_TlsNotEnabled, App_NoChangesMade, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Application.Current.Shutdown();
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainViewModel();
            mainWindow.Show();
        }
        private void AddResources() {
            var manager = Strings.ResourceManager;
            var managerType = manager.GetType();
            var field = managerType.GetField("ResourceSets", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            var resourceTable = field.GetValue(manager) as Hashtable;
            if (resourceTable != null) {
                AddResource("PieterP.ScoreSheet.Installer.Localization.StringsNl.resources", resourceTable, new CultureInfo("nl"), new CultureInfo("nl-BE"));
                AddResource("PieterP.ScoreSheet.Installer.Localization.StringsFr.resources", resourceTable, new CultureInfo("fr"), new CultureInfo("fr-BE"));
                AddResource("PieterP.ScoreSheet.Installer.Localization.StringsDe.resources", resourceTable, new CultureInfo("de"), new CultureInfo("de-DE"));
            }
        }
        private void AddResource(string resourceName, Hashtable resourceTable, params CultureInfo[] cultures) {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                var reader = new ResourceReader(stream);
                var resourceSet = new ResourceSet(reader);
                foreach (var c in cultures) {
                    resourceTable[c] = resourceSet;
                }
            }
        }
        private ApplicationSettings ParseArguments(string[] args) {
            var settings = new ApplicationSettings();
            if (args != null && args.Length > 0) {
                foreach (var a in args) {
                    if (a != null) {
                        var l = a.ToLower();
                        if (l == "enabletls") {
                            settings.EnableTls = true;
                        } else if (l.StartsWith("culture=")) {
                            settings.Culture = l.Substring(8);
                        }

                    }
                }
            }
            return settings;
        }

        private bool TryEnableTls() {
            try {
                var clientKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Client");
                clientKey?.SetValue("DisabledByDefault", 0, RegistryValueKind.DWord);
                clientKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client");
                clientKey?.SetValue("DisabledByDefault", 0, RegistryValueKind.DWord);
                return true;
            } catch {
                return false;
            }
        }

        private class ApplicationSettings {
            public ApplicationSettings() {
                this.EnableTls = false;
                this.Culture = "en-US";
            }
            public bool EnableTls { get; set; }
            public string Culture { get; set; }
        }
    }
}
