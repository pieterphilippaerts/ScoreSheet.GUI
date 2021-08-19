using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
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
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        private bool _started;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            SetupExceptionHandling();

            // By default, resource files work with sattelite assemblies. Because we don't want those sattelites in our installation program,
            // we hack the translated resource files into the ResourceManager that is used by the Strings class
            AddResources();

            var appSettings = ParseArguments(e.Args);
            try {
                var ci = new CultureInfo(appSettings.Culture);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                Strings.Culture = ci;
            } catch (Exception ex) {
                HandleException(ex, false);
            }

            if (appSettings.EnableTls) {
                if (TryEnableTls()) {
                    ShowNativeMessageBox(Strings.App_TlsEnabled, App_ChangesMade, MessageBoxButton.OK, MessageBoxImage.Information);
                } else {
                    ShowNativeMessageBox(App_TlsNotEnabled, App_NoChangesMade, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Application.Current.Shutdown();
                return;
            }

            _started = true;
            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainViewModel();
            mainWindow.Show();
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
                    AddResource("PieterP.ScoreSheet.Installer.Localization.StringsNl.resources", oldResourceTable, newResourceTable, "nl", "nl-BE");
                    AddResource("PieterP.ScoreSheet.Installer.Localization.StringsFr.resources", oldResourceTable, newResourceTable, "fr", "fr-BE");
                    AddResource("PieterP.ScoreSheet.Installer.Localization.StringsDe.resources", oldResourceTable, newResourceTable, "de", "de-DE");
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
        private ApplicationSettings ParseArguments(string[] args) {
            var settings = new ApplicationSettings();
            try {
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
            } catch (Exception e) {
                HandleException(e, false);
            }
            return settings;
        }
        private void SetupExceptionHandling() {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                HandleException(e.ExceptionObject as Exception, e.IsTerminating);
            };
            DispatcherUnhandledException += (s, e) => {
                if (_started) {
                    e.Handled = true;
                    HandleException(e.Exception, false);
                } // else: let the UnhandledException handle it
            };
        }
        private void HandleException(Exception e, bool isTerminating) {
            // log the exception
            string file = "";
            try {
                file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ScoreSheet-installer-error.txt");
                using (var writer = new StreamWriter(file, true)) {
                    writer.WriteLine("***************");
                    writer.WriteLine("ScoreSheet Installer Error at " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    if (e != null)
                        writer.WriteLine(e.ToString());
                    else
                        writer.WriteLine("No exception object found.");
                    if (isTerminating)
                        writer.WriteLine("The installer is now terminating.");
                    writer.WriteLine("~~~~~~~~~~~~~~~\r\n");
                }
            } catch { }
            // show the exception to the user
            try {
                string message = e == null ? "No exception object." : (e.Message ?? "No exception message.");
                string shutdown = "";
                if (isTerminating)
                    shutdown = "\r\n\r\nUnfortunately, the setup program cannot continue.";

                ShowNativeMessageBox($"An unexpected error occurred. The error message is: '{ message }'. A more-detailed description can be found in the log file on your desktop ({ file }). Please email this file to score@pieterp.be so we can fix the problem. Thanks!" + shutdown, "Installer problem...", MessageBoxButton.OK, isTerminating ? MessageBoxImage.Error : MessageBoxImage.Warning);
            } catch { }
        }
        private void ShowNativeMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage image) {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) { // if we're not in an STA thread, we cannot show the messagebox
                MessageBox(IntPtr.Zero, message, caption, (uint)buttons | (uint)image);
            }
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
