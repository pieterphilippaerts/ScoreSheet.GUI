#if NETFX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Microsoft.Win32;
using static PInvoke.Kernel32;
using static PInvoke.Shell32;
using static PieterP.ScoreSheet.Launcher.Localization.Strings;
using PieterP.ScoreSheet.Launcher.Localization;

namespace PieterP.ScoreSheet.Launcher.ViewModels {
    public class PrereqViewModel {
        public PrereqViewModel() {
            IsWindows7OrHigher = CheckWindows();
            IsTls1_2Enabled = CheckTls();
            IsFrameworkAvailable = CheckFrameworkVersion();

            ArePrereqsMet = IsWindows7OrHigher && IsTls1_2Enabled && IsFrameworkAvailable;

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
                //        LinkText = Prereq_ReadMore,
                //        Click = new RelayCommand(() => OpenUrl("https://score.pieterp.be/Help/Vista"))
                //    });
                }
            }
            this.Prereqs = prereqs.OrderBy(c => c.IsOk).Cast<object>().ToList();

        }
        private bool IsWindows7() {
            return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;
        }
        private void EnableTls() {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = Assembly.GetExecutingAssembly().Location;
            startInfo.Arguments = "enabletls";
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