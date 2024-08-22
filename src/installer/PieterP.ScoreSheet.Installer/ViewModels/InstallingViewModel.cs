using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using PieterP.ScoreSheet.Installer.Models;
using ShellLink;
using static PieterP.ScoreSheet.Installer.Localization.Strings;

namespace PieterP.ScoreSheet.Installer.ViewModels {
    public class InstallingViewModel : INotifyPropertyChanged {
        public InstallingViewModel(MainViewModel parent) {
            _parent = parent;
            this.Status = "";
            ThreadPool.QueueUserWorkItem(StartUpdate);
        }
        private void ExtractZip(Stream s, string basePath) {
            var archive = new ZipArchive(s);
            foreach (var entry in archive.Entries) {
                if (!entry.FullName.EndsWith("/") && !entry.FullName.EndsWith("\\")) {
                    var fullPath = new FileInfo(Path.Combine(basePath, entry.FullName));
                    if (!fullPath.Directory.Exists)
                        Directory.CreateDirectory(fullPath.Directory.FullName);
                    ExtractToFile(entry, fullPath.FullName, true);
                }
            }
        }
        private void ExtractToFile(ZipArchiveEntry source, string destinationFileName, bool overwrite) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (destinationFileName == null) {
                throw new ArgumentNullException("destinationFileName");
            }
            FileMode mode = ((!overwrite) ? FileMode.CreateNew : FileMode.Create);
            using (Stream destination = File.Open(destinationFileName, mode, FileAccess.Write, FileShare.None)) {
                using (Stream stream = source.Open()) {
                    stream.CopyTo(destination);
                }
            }
            File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
        }
        private void StartUpdate(object o) {
            try {
                //var unzip = new FastZip();

                UpdateProgress(Installing_GUI);
                string basePath = DatabaseManager.Current.BasePath;
                var app = Application.GetResourceStream(new Uri("pack://application:,,,/archives/app.zip"));
                if (app == null) {
                    Finished(Installing_GUINotFound);
                    return;
                }
                //unzip.ExtractZip(app.Stream, basePath, FastZip.Overwrite.Always, null, null, null, true, true);
                ExtractZip(app.Stream, basePath);

                UpdateProgress(Installing_Launcher);
                var launcher = Application.GetResourceStream(new Uri("pack://application:,,,/archives/launcher.zip"));
                if (launcher == null) {
                    Finished(Installing_LauncherNotFound);
                    return;
                }
                //unzip.ExtractZip(launcher.Stream, basePath, FastZip.Overwrite.Always, null, null, null, true, true);
                ExtractZip(launcher.Stream, basePath);

                UpdateProgress(Installing_ShortcutDesktop);
                Shortcut.CreateShortcut(Path.Combine(basePath, "launcher.exe")).WriteToFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ScoreSheet.lnk"));

                UpdateProgress(Installing_ShortcutStartMenu);
                Shortcut.CreateShortcut(Path.Combine(basePath, "launcher.exe")).WriteToFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "ScoreSheet.lnk"));

                //UpdateProgress("Bezig met de laatste wijzigingen");
                //CreateUninstall();

                Finished();
            } catch (Exception e) {
                Finished(e.ToString());
            }
        }
        private void CreateUninstall() {
            // Requires Admin privileges... we don't want to ask those just to register the uninstaller...
            //try {
            //    using (var parentKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")) {
            //        using (var ssKey = parentKey.CreateSubKey("ScoreSheet")) {
            //            ssKey.SetValue("DisplayName", "ScoreSheet");
            //            ssKey.SetValue("Publisher", "PieterP.be");
            //            ssKey.SetValue("InstallLocation", DatabaseManager.Current.BasePath);
            //            ssKey.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
            //            ssKey.SetValue("NoModify", 1);
            //            ssKey.SetValue("NoRepair", 1);
            //            ssKey.SetValue("URLInfoAbout", "https://score.pieterp.be/");
            //            ssKey.SetValue("UninstallString", $"\"{ Path.Combine(DatabaseManager.Current.BasePath, "Launcher.exe")}\" uninstall");
            //        }
            //    }
            //} catch (Exception e) {
            //    UpdateProgress("Er was een fout bij het aanmaken van de uninstall-informatie.");
            //}
        }

        private void UpdateProgress(string progress) {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                new Action(() => {
                    this.Status = progress;
                }));
        }
        private void Finished(string error = null) {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                new Action(() => {
                    _parent.CurrentScreen = new InstallationCompleteViewModel(_parent, error);
                }));
        }

        public string Status {
            get {
                return _status;
            }
            set {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        private string _status;
        private MainViewModel _parent;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
