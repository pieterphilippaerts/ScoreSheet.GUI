#if CORE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using PieterP.ScoreSheet.Installer.Models;

namespace PieterP.ScoreSheet.Installer.ViewModels {
    public class PrereqViewModel {
        public PrereqViewModel(MainViewModel parent) {
            _parent = parent;
            IsWindows7OrHigher = CheckWindows();
            IsAddDllDirectoryAvailable = CheckAddDllDirectory();
            IsUCRTAvailable = CheckUCRT();
            CheckNetCore();
            IsTls1_1Enabled = CheckTls();

            ArePrereqsMet = IsWindows7OrHigher && IsAddDllDirectoryAvailable && IsUCRTAvailable && IsNetCoreInstalled && IsWpfInstalled && IsTls1_1Enabled;

            this.InstallPath = DatabaseManager.Current.BasePath;
            this.Install = new RelayCommand(OnInstall);


            var prereqs = new List<PrereqInfo>();
            if (IsWindows7OrHigher) {
                prereqs.Add(new PrereqInfo() {
                    Title = "Windows 7 of hoger vereist",
                    Description = "Jouw versie van Windows voldoet.",
                    IsOk = true
                });
            } else {
                prereqs.Add(new PrereqInfo() {
                    Title = "Windows 7 of hoger vereist",
                    Description = "Je werkt op een te oude versie van Windows. Deze versie wordt niet meer ondersteund.",
                    IsOk = false
                });
            }
            if (IsAddDllDirectoryAvailable) {
                prereqs.Add(new PrereqInfo() {
                    Title = "Windows Update KB2533623 vereist",
                    Description = "Windows update KB2533623 is geïnstalleerd op dit systeem.",
                    IsOk = true
                });
            } else {
                prereqs.Add(new PrereqInfo() {
                    Title = "Windows Update KB2533623 vereist",
                    Description = "Windows update KB2533623 is niet geïnstalleerd op dit systeem. Deze update wordt automatisch geïnstalleerd als u Windows updatet. U kan de update ook manueel downloaden en installeren.",
                    IsOk = false,
                    LinkText = "Update downloaden",
                    Click = new RelayCommand(() => OpenUrl("https://support.microsoft.com/en-us/help/2533623/microsoft-security-advisory-insecure-library-loading-could-allow-remot"))
                });
            }
            if (IsUCRTAvailable) {
                prereqs.Add(new PrereqInfo() {
                    Title = "Windows Update KB2999226 vereist",
                    Description = "Windows update KB2999226 is geïnstalleerd op dit systeem.",
                    IsOk = true
                });
            } else {
                prereqs.Add(new PrereqInfo() {
                    Title = "Windows Update KB2999226 vereist",
                    Description = "Windows update KB2999226 is niet geïnstalleerd op dit systeem. Deze update wordt automatisch geïnstalleerd als u Windows updatet. U kan de update ook manueel downloaden en installeren.",
                    IsOk = false,
                    LinkText = "Update downloaden",
                    Click = new RelayCommand(() => OpenUrl("https://support.microsoft.com/en-us/help/2999226/update-for-universal-c-runtime-in-windows"))
                });
            }
            if (IsNetCoreInstalled) {
                prereqs.Add(new PrereqInfo() {
                    Title = ".NET Core 3.0 (of hoger) vereist",
                    Description = ".NET Core 3.0 (of hoger) is geïnstalleerd op dit systeem.",
                    IsOk = true
                });
            } else { // .NET Core Desktop Installer
                prereqs.Add(new PrereqInfo() {
                    Title = ".NET Core 3.0 (of hoger) Runtime vereist",
                    Description = $"De .NET Core 3.0 Runtime is niet geïnstalleerd op dit systeem. U kan deze runtime downloaden vanop de onderstaande link. Kies op de downloadpagina voor zowel de '.NET Core Installer ({ (IsWindows64bit() ? "x64" : "x86") })' en de '.NET Core Desktop Installer ({ (IsWindows64bit() ? "x64" : "x86") })'. Installeer beide.",
                    IsOk = false,
                    LinkText = ".NET Core 3.0 Runtime downloaden",
                    Click = new RelayCommand(() => OpenUrl("https://dotnet.microsoft.com/download/dotnet-core/3.0"))
                });
            }
            if (IsNetCoreInstalled) {
                if (IsWpfInstalled) {
                    prereqs.Add(new PrereqInfo() {
                        Title = "WindowsDesktop framework vereist",
                        Description = "Het WindowsDesktop framework is geïnstalleerd op dit systeem.",
                        IsOk = true
                    });
                } else {
                    prereqs.Add(new PrereqInfo() {
                        Title = "WindowsDesktop framework vereist",
                        Description = $"Het WindowsDesktop framework is niet geïnstalleerd op dit systeem. U kan deze runtime downloaden vanop de onderstaande link. Kies op de downloadpagina voor de '.NET Core Desktop Installer ({ (IsWindows64bit() ? "x64" : "x86") })'.",
                        IsOk = false,
                        LinkText = "WindowsDesktop framework downloaden",
                        Click = new RelayCommand(() => OpenUrl("https://dotnet.microsoft.com/download/dotnet-core/3.0"))
                    });
                }
            }
            if (IsTls1_1Enabled) {
                prereqs.Add(new PrereqInfo() {
                    Title = "TLS 1.1 of hoger vereist",
                    Description = "Dit systeem ondersteunt TLS 1.1 of hoger.",
                    IsOk = true
                });
            } else {
                prereqs.Add(new PrereqInfo() {
                    Title = "TLS 1.1 of hoger vereist",
                    Description = "De ondersteuning voor TLS 1.1 of hoger staat niet aan op dit systeem. Hierdoor kan ScoreSheet mogelijk geen data downloaden van de competitiewebsite. Gelukkig ondersteunt Windows 7 dit protocol wel, dus je kan het gewoon aanzetten.",
                    IsOk = false,
                    LinkText = "TLS 1.1/1.2-ondersteuning aanzetten",
                    Click = new RelayCommand(() => EnableTls())
                });
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
            var osver = Environment.OSVersion.Version;
            return osver.Major > 6 || (osver.Major == 6 && osver.Minor > 0);
        }
        private bool CheckAddDllDirectory() {
            var hModule = LoadLibrary("Kernel32.dll");
            try {
                if (hModule != INVALID_HANDLE_VALUE && hModule != IntPtr.Zero) {
                    IntPtr hFarProc = GetProcAddress(hModule, "AddDllDirectory");
                    if (hFarProc != IntPtr.Zero) {
                        return true;
                    } // else: KB2533623 not installed
                }
                return false;
            } finally {
                CloseHandle(hModule);
            }
        }
        private bool CheckUCRT() {
            var hModule = LoadLibraryEx("UCRTBASE.dll", IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32);
            try {
                if (hModule == INVALID_HANDLE_VALUE || hModule == IntPtr.Zero)
                    return false; // KB2999226 not installed
                return true;
            } finally {
                CloseHandle(hModule);
            }
        }
        private void CheckNetCore() {
            // find .NET Core installation path
            string dnc = FindNetCorePath();
            if (dnc == null) {
                IsNetCoreInstalled = false;
                return;
            }

            // make sure .NET Core 3.0 (or higher) is installed
            var versions = GetVersions(Path.Combine(Path.Combine(dnc, "host"), "fxr"));
            var minVersion = new Version(3, 0, 0);
            if (!versions.Any(v => v >= minVersion)) {
                IsNetCoreInstalled = false;
                return;
            }
            IsNetCoreInstalled = true;

            var frameworks = GetFrameworks(Path.Combine(dnc, "shared"));
            IsWpfInstalled = frameworks.Contains("Microsoft.WindowsDesktop.App");
        }
        private IList<Version> GetVersions(string path) {
            var subdirs = Directory.GetDirectories(path);
            var ret = new List<Version>();
            foreach (var subdir in subdirs) {
                try {
                    var di = new DirectoryInfo(subdir);
                    var v = new Version(di.Name);
                    ret.Add(v);
                } catch { }
            }
            return ret;
        }
        private IEnumerable<string> GetFrameworks(string path) {
            return Directory.GetDirectories(path).Select(sd => new DirectoryInfo(sd).Name);
        }

        private string FindNetCorePath() {
            // see https://github.com/dotnet/designs/blob/master/accepted/install-locations.md
            var is64bit = IsWindows64bit();
            // C:\Program Files (x86)\dotnet
            try {
                string testPath = Path.Combine(SHGetKnownFolderPath(ProgramFilesX86), "dotnet");
                if (Directory.Exists(testPath))
                    return testPath;
            } catch { }

            // C:\Program Files\dotnet
            if (is64bit) {
                try {
                    string testPath = Path.Combine(SHGetKnownFolderPath(ProgramFilesX64), "dotnet");
                    if (Directory.Exists(testPath))
                        return testPath;
                } catch { }
            }

            // HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x86\InstallLocation
            try {
                var testPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\dotnet\Setup\InstalledVersions\x86", "InstallLocation", null) as string;
                if (testPath != null && Directory.Exists(testPath))
                    return testPath;
            } catch { }

            // HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x64\InstallLocation
            if (is64bit) {
                try {
                    var testPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\dotnet\Setup\InstalledVersions\x64", "InstallLocation", null) as string;
                    if (testPath != null && Directory.Exists(testPath))
                        return testPath;
                } catch { }
            }

            // DOTNET_ROOT environment variable
            try {
                var testPath = System.Environment.GetEnvironmentVariable("DOTNET_ROOT");
                if (testPath != null && Directory.Exists(testPath))
                    return testPath;
            } catch { }

            // DOTNET_ROOT(x86) environment variable
            if (is64bit) {
                try {
                    var testPath = System.Environment.GetEnvironmentVariable("DOTNET_ROOT(x86)");
                    if (testPath != null && Directory.Exists(testPath))
                        return testPath;
                } catch { }
            }

            return null;
        }
        private bool CheckTls() {
            var osver = Environment.OSVersion.Version;
            if (osver.Major > 6 || (osver.Major == 6 && osver.Minor > 1))
                return true; // Windows 8+ has it enabled by default

            var protocolsKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols");
            if (protocolsKey == null)
                return false;
            var names = protocolsKey.GetSubKeyNames();
            if (names.Contains("TLS 1.1") || names.Contains("TLS 1.2")) // doesn't neceserally mean they're enabled, but in that case it's a specific choice of the admin
                return true;
            return false;
        }

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        [DllImport("Kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("shell32.dll")]
        private unsafe static extern int SHGetKnownFolderPath(
             [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
             uint dwFlags,
             IntPtr hToken,
             out char* pszPath  // API uses CoTaskMemAlloc
             );

        public static readonly Guid ProgramFilesX86 = new Guid("7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E");
        public static readonly Guid ProgramFilesX64 = new Guid("6D809377-6AF0-444B-8957-A3773F02200E");

        private unsafe static string SHGetKnownFolderPath(Guid rfid, uint dwFlags = 0, IntPtr hToken = default(IntPtr)) {
            char* ppszPath;
            SHGetKnownFolderPath(rfid, dwFlags, hToken, out ppszPath);
            try {
                return new string(ppszPath);
            } finally {
                Marshal.FreeCoTaskMem((IntPtr)(void*)ppszPath);
            }
        }

        [System.Flags]
        enum LoadLibraryFlags : uint {
            None = 0,
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
            LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
            LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
            LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
            LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );
        private bool IsWindows64bit() {
            if (IntPtr.Size == 8)
                return true; // we're running as a 64-bit process
            // are we a 32-bit process on 64-bit windows?
            using (var p = Process.GetCurrentProcess()) {
                bool retVal;
                if (!IsWow64Process(p.Handle, out retVal)) {
                    return false;
                }
                return retVal;
            }
        }

        public IList<object> Prereqs { get; private set; }
        public bool IsWindows7OrHigher { get; private set; }
        public bool IsAddDllDirectoryAvailable { get; private set; }
        public bool IsUCRTAvailable { get; private set; }
        public bool IsNetCoreInstalled { get; private set; }
        public bool IsWpfInstalled { get; private set; }
        public bool IsTls1_1Enabled { get; private set; }
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