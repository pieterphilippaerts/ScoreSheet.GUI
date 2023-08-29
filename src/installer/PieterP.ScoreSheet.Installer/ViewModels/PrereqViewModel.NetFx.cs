#if NETFX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using Microsoft.Win32;
using PieterP.ScoreSheet.Installer.Models;
using PieterP.ScoreSheet.Installer.ViewModels;
using static PieterP.ScoreSheet.Installer.Localization.Strings;

namespace PieterP.ScoreSheet.Installer.ViewModels {
    public class PrereqViewModel {
        public PrereqViewModel(MainViewModel parent) {
            _parent = parent;
            IsWindows7OrHigher = CheckWindows();
            IsTls1_2Enabled = CheckTls();
            IsFrameworkAvailable = CheckFrameworkVersion();

            ArePrereqsMet = IsWindows7OrHigher && IsTls1_2Enabled;

            this.InstallPath = DatabaseManager.Current.BasePath;
            this.Install = new RelayCommand(OnInstall);

            var prereqs = new List<PrereqInfo>();
            if (IsWindows7OrHigher) {
                prereqs.Add(new PrereqInfo() {
                    Title = Prereq_Windows,
                    Description = Prereq_WindowsOk,
                    IsOk = true
                });
            } else {
                prereqs.Add(new PrereqInfo() {
                    Title = Prereq_Windows,
                    Description = Prereq_WindowsErr,
                    IsOk = false
                });
            }
            if (IsFrameworkAvailable) {
                prereqs.Add(new PrereqInfo() {
                    Title = Prereq_Netfx,
                    Description = Prereq_NetfxOk,
                    IsOk = true
                });
            } else {
                prereqs.Add(new PrereqInfo() {
                    Title = Prereq_Netfx,
                    Description = Prereq_NetfxErr,
                    IsOk = false,
                    LinkText = Prereq_NetfxDownload,
                    Click = new RelayCommand(() => OpenUrl("https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer"))
                });
            }
            if (IsTls1_2Enabled) {
                prereqs.Add(new PrereqInfo() {
                    Title = Prereq_Tls,
                    Description = Prereq_TlsOk,
                    IsOk = true
                });
            } else {
                if (IsWindows7()) {
                    prereqs.Add(new PrereqInfo() {
                        Title = Prereq_Tls,
                        Description = Prereq_TlsErrW7,
                        IsOk = false,
                        LinkText = Prereq_TlsEnable,
                        Click = new RelayCommand(() => EnableTls())
                    });
                //} else { // we don't support Vista anymore
                //    prereqs.Add(new PrereqInfo() {
                //        Title = Prereq_Tls,
                //        Description = Prereq_TlsErrWV,
                //        IsOk = false,
                //    });
                }
            }
            this.Prereqs = prereqs.OrderBy(c => c.IsOk).Cast<object>().ToList();

        }

        private void OnInstall() {
            _parent.CurrentScreen = new InstallingViewModel(_parent);
        }

        private void EnableTls() {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = Assembly.GetExecutingAssembly().Location;
            startInfo.Arguments = "enabletls culture=" + Thread.CurrentThread.CurrentUICulture.Name;
            startInfo.Verb = "runas"; // start privileged
            try {
                Process.Start(startInfo);
            } catch { }
        }
        private void OpenUrl(string url) {
            var psi = new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        private bool CheckWindows() {
            // Windows must be Windows 7 or higher
            return Environment.OSVersion.Version.Major > 6 /* Windows 10 or higher */
                || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 0) /* Windows 7, 8, 8.1 */;
        }
        private bool IsWindows7() { 
            return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;
        }
        private bool CheckFrameworkVersion() {
            // .NET 4.8 required
            // HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x86\InstallLocation
            try {
                var version = (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Release", null);
                return version >= 528040;
            } catch { }
            return false;
        }
        private bool CheckTls() {
            var osver = Environment.OSVersion.Version;
            if (osver.Major > 6 || (osver.Major == 6 && osver.Minor > 1))
                return true; // Windows 8+ has it enabled by default

            var protocolsKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols");
            if (protocolsKey == null)
                return false;
            var names = protocolsKey.GetSubKeyNames();
            if (names.Contains("TLS 1.2")) // doesn't neceserally mean they're enabled, but in that case it's a specific choice of the admin
                return true;
            return false;
        }

        public IList<object> Prereqs { get; private set; }
        public bool IsWindows7OrHigher { get; private set; }
        public bool IsTls1_2Enabled { get; private set; }
        public bool IsFrameworkAvailable { get; private set; }
        public bool ArePrereqsMet { get; private set; }
        public string InstallPath { get; private set; }
        public ICommand Install { get; private set; }

        private MainViewModel _parent;

        private class PrereqInfo {
            public string Title { get; set; }
            public string Description { get; set; }
            public bool ShowLink => LinkText != null && LinkText.Length > 0 && Click != null;
            public string LinkText { get; set; }
            public ICommand Click { get; set; }
            public bool IsOk { get; set; }
        }
    }
}
#endif