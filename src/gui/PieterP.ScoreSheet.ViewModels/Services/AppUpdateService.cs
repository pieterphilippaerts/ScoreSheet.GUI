using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Information;
using PieterP.ScoreSheet.Model.Interfaces;
using PieterP.ScoreSheet.ViewModels.Helpers;
using PieterP.ScoreSheet.ViewModels.Services.Json;
using PieterP.Shared.Cells;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class AppUpdateService : IDisposable {
        public AppUpdateService() {
            ServiceLocator.RegisterInstance<AppUpdateService>(this);
            this.Status = Cell.Create(UpdateStatus.NoUpdate);
            this.BytesDownloaded = Cell.Create(0);
            this.TotalBytes = Cell.Create(0);
            _isUpdating = false;
            _syncroot = new object();
            _timer = ServiceLocator.Resolve<ITimerService>();
            _timer.Tick += t => CheckForUpdate();
            _timer.Start(new TimeSpan(0, 5, 0));

            _timerOnce = ServiceLocator.Resolve<ITimerService>();
            _timerOnce.Tick += t => CheckForUpdateOnce();
            _timerOnce.Start(new TimeSpan(0, 0, 5)); // check after 5 seconds

            ServiceLocator.Resolve<INetworkAvailabilityService>().IsNetworkAvailable.ValueChanged += CheckForUpdate;
        }

        private void CheckForUpdateOnce() {
            _timerOnce?.Stop();
            CheckForUpdate();
        }

        public async void CheckForUpdate() {
            lock (_syncroot) {
                if (_isUpdating)
                    return;
                if (this.Status.Value == UpdateStatus.Updated)
                    return; // we have already updated; user must restart
                if (_lastCheck != null && DateTime.Now.Subtract(_lastCheck.Value).TotalMinutes < 5)
                    return; // we have checked only a short while ago; wait at least 5 minutes
                _lastCheck = DateTime.Now;
                _isUpdating = true;
            }
            
            this.Status.Value = UpdateStatus.Checking;
            try {
                var ms = new MemoryStream();
                if (!await Download(new Uri(BaseUri, UpdateUrl), ms)) {
                    this.Status.Value = UpdateStatus.Error;
                    return;
                }
                string json = Encoding.UTF8.GetString(ms.ToArray());
                var update = DataSerializer.Deserialize<UpdateInfo>(json);

                var onlineVersion = new Version(update.LatestVersion);
                if (onlineVersion <= DatabaseManager.Current.Settings.LatestInstalledVersion.Value) {
                    this.Status.Value = UpdateStatus.NoUpdate;
                    return;
                }

                if (update.InstallFileUrl != null) {
                    // download update
                    this.TotalBytes.Value = update.Size ?? 0;
                    this.BytesDownloaded.Value = 0;
                    this.Status.Value = UpdateStatus.DownloadingUpdate;
                    ms = new MemoryStream();
                    if (!await Download(new Uri(BaseUri, update.InstallFileUrl), ms)) {
                        this.Status.Value = UpdateStatus.Error;
                        return;
                    }


                    // check hash
                    if (update.Hash == null) {
                        Logger.Log(LogType.Warning, Strings.AppUpdate_MissingHash);
                    } else {
                        var originalHash = Convert.FromBase64String(update.Hash);
                        ms.Position = 0;
                        var downloadHash = ComputeHash(ms);
                        if (!CompareBytes(originalHash, downloadHash)) {
                            Logger.Log(LogType.Exception, Errors.AppUpdate_InvalidHash);
                            this.Status.Value = UpdateStatus.Error;
                            return;
                        }
                    }

                    // extract update
                    this.Status.Value = UpdateStatus.Installing;
                    ms.Position = 0;
                    var archive = new ZipArchive(ms);
                    string basePath = DatabaseManager.Current.BasePath;
                    foreach (var entry in archive.Entries) {
                        if (!entry.FullName.EndsWith("/") && !entry.FullName.EndsWith("\\")) {
                            var fullPath = new FileInfo(Path.Combine(basePath, entry.FullName));
                            if (!fullPath.Directory.Exists)
                                Directory.CreateDirectory(fullPath.Directory.FullName);
                            entry.ExtractToFile(fullPath.FullName, true);
                        }
                    }
                }

                // execute post-install actions
                if (update.PostUnzipActions != null) {
                    foreach (var action in update.PostUnzipActions) {
                        await ExecuteAction(action);
                    }
                }

                // check whether we must force a database update on next startup
                if (update.DatabaseCompatibilityVersion != null) {
                    var databaseVersion = new Version(update.DatabaseCompatibilityVersion);
                    if (databaseVersion > DatabaseManager.Current.Settings.LatestInstalledVersion.Value) {
                        DatabaseManager.Current.Settings.UpdateDatabaseOnStart.Value = true; // force database update on next start
                    }
                }

                DatabaseManager.Current.Settings.LatestInstalledVersion.Value = onlineVersion;
                Logger.Log(LogType.Informational, Strings.AppUpdate_Restart);
                this.Status.Value = UpdateStatus.Updated;
                _timer?.Stop();
            } catch (Exception e) {
                Logger.Log(e);
                this.Status.Value = UpdateStatus.Error;
            } finally {
                lock (_syncroot) {
                    _isUpdating = false;
                }
            }
        }
        private async Task ExecuteAction(UpdateAction action) {
            try {
                switch (action.Type) {
                    case UpdateActionTypes.DeleteFile:
                        var deletePath = Path.Combine(DatabaseManager.Current.BasePath, action.Source);
                        if (deletePath.IsSubPathOf(DatabaseManager.Current.BasePath) && File.Exists(deletePath)) {
                            File.Delete(deletePath);
                        }
                        break;
                    case UpdateActionTypes.DeleteDirectory:
                        var deleteDirectory = Path.Combine(DatabaseManager.Current.BasePath, action.Source);
                        if (deleteDirectory.IsSubPathOf(DatabaseManager.Current.BasePath) && Directory.Exists(deleteDirectory)) {
                            Directory.Delete(deleteDirectory, true);
                        }
                        break;
                    case UpdateActionTypes.MoveDirectory:
                        var sourceDir = Path.Combine(DatabaseManager.Current.BasePath, action.Source);
                        var destDir = Path.Combine(DatabaseManager.Current.BasePath, action.Destination);
                        if (sourceDir.IsSubPathOf(DatabaseManager.Current.BasePath) && destDir.IsSubPathOf(DatabaseManager.Current.BasePath) && Directory.Exists(sourceDir) && !Directory.Exists(destDir)) {
                            Directory.Move(sourceDir, destDir);
                        }
                        break;
                    case UpdateActionTypes.MoveFile:
                        var sourceFile = Path.Combine(DatabaseManager.Current.BasePath, action.Source);
                        var destFile = Path.Combine(DatabaseManager.Current.BasePath, action.Destination);
                        if (sourceFile.IsSubPathOf(DatabaseManager.Current.BasePath) && destFile.IsSubPathOf(DatabaseManager.Current.BasePath) && File.Exists(sourceFile) && !File.Exists(destFile)) {
                            File.Move(sourceFile, destFile);
                        }
                        break;
                    case UpdateActionTypes.ExecuteFileBlocking:
                        var executeBFile = Path.Combine(DatabaseManager.Current.BasePath, action.Source);
                        if (executeBFile.IsSubPathOf(DatabaseManager.Current.BasePath) && File.Exists(executeBFile)) {
                            var startInfo = new ProcessStartInfo();
                            startInfo.UseShellExecute = true;
                            startInfo.FileName = executeBFile;
                            var p = Process.Start(startInfo);
                            p.WaitForExit();
                        }
                        break;
                    case UpdateActionTypes.ExecuteFileNonBlocking:
                        var executeNBFile = Path.Combine(DatabaseManager.Current.BasePath, action.Source);
                        if (executeNBFile.IsSubPathOf(DatabaseManager.Current.BasePath) && File.Exists(executeNBFile)) {
                            var startInfo = new ProcessStartInfo();
                            startInfo.UseShellExecute = true;
                            startInfo.FileName = executeNBFile;
                            var p = Process.Start(startInfo);
                        }
                        break;
                    case UpdateActionTypes.RedirectToUrl:
                        var psi = new ProcessStartInfo {
                            FileName = action.Source,
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                        break;
                    default:
                        Logger.Log(LogType.Exception, Strings.AppUpdate_InvalidActionType);
                        break;
                }
            } catch (Exception e) {
                Logger.Log(e);
            }
        }
        private async Task<bool> Download(Uri url, Stream result) {
            try {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var request = WebRequest.Create(url) as HttpWebRequest;
                if (request != null) {
                    var response = await request.GetResponseAsync();
                    // await response.GetResponseStream().CopyToAsync(result); => we want to update the user

                    var source = response.GetResponseStream();
                    byte[] buffer = new byte[10240];
                    int totalBytesRead = 0;
                    int bytesRead;
                    while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                        await result.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        this.BytesDownloaded.Value = totalBytesRead;
                    }
                    return true;
                }
            } catch (Exception e) {
                // not connected to the internet?
                Logger.Log(LogType.Debug, e.ToString());
            }
            return false; 
        }
        private byte[] ComputeHash(Stream s) {
            var hash = SHA256.Create();
            var buffer = new byte[1024];
            int read = s.Read(buffer, 0, buffer.Length);
            while (read > 0) {
                hash.TransformBlock(buffer, 0, read, buffer, 0);
                read = s.Read(buffer, 0, buffer.Length);
            }
            hash.TransformFinalBlock(buffer, 0, 0);
            return hash.Hash;
        }
        private bool CompareBytes(byte[] a1, byte[] a2) {
            if (a1 == a2) {
                return true;
            }
            if ((a1 != null) && (a2 != null)) {
                if (a1.Length != a2.Length) {
                    return false;
                }
                for (int i = 0; i < a1.Length; i++) {
                    if (a1[i] != a2[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public void Dispose() {
            _timer?.Stop();
            _timer = null;
            _timerOnce?.Stop();
            _timerOnce = null;
        }

        public Cell<UpdateStatus> Status { get; }
        public Cell<int> BytesDownloaded { get; }
        public Cell<int> TotalBytes { get; }

        private static readonly Uri BaseUri = new Uri("https://score.pieterp.be");
        private const string UpdateUrl = "/Update";
        private object _syncroot;
        private bool _isUpdating;
        private ITimerService? _timer, _timerOnce;
        private DateTime? _lastCheck;
    }
    public enum UpdateStatus : int {
        Checking = 1,
        Error = 2,
        DownloadingUpdate = 3,
        Installing = 4,
        Updated = 5,
        NoUpdate = 6
    }
}