using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using static PInvoke.Shell32;
using static PInvoke.Kernel32;

namespace PieterP.ScoreSheet.Model.Information {
    public class RuntimeInformation { 
        public IEnumerable<string> DotNetFrameworksInstalled { get; set; }
        public IEnumerable<string> DotNetCoreRuntimesInstalled { get; set; }
        public IEnumerable<FrameworkInformation> DotNetCoreFrameworksInstalled { get; set; }
    }
    public class FrameworkInformation { 
        public string Name { get; set; }
        public IEnumerable<string> Versions { get; set; }
    }
    public class Runtime {
        public static RuntimeInformation GetRuntimeInformation() {
            var ret = new RuntimeInformation();
            // .NET Framework
            var path = @"SOFTWARE\Microsoft\NET Framework Setup\NDP";
            var frameworkVersions = new List<string>();
            using (var root = Registry.LocalMachine.OpenSubKey(path)) {
                foreach (var subkeyName in root.GetSubKeyNames()) {
                    using (var subkey = root.OpenSubKey(subkeyName)) {
                        var version = GetVersion(subkey);
                        if (version != null) {
                            frameworkVersions.Add(version);
                        }
                    }
                }
            }
            ret.DotNetFrameworksInstalled = frameworkVersions;

            // .NET Core
            var dnc = FindNetCorePath();
            if (dnc != null) {
                var rd = Path.Combine(Path.Combine(dnc, "host"), "fxr");
                if (Directory.Exists(rd)) {
                    var di = new DirectoryInfo(rd);
                    ret.DotNetCoreRuntimesInstalled = di.GetDirectories().Select(c => c.Name);
                }
                var fd = Path.Combine(dnc, "shared");
                if (Directory.Exists(fd)) {
                    var frameworks = new List<FrameworkInformation>();
                    var frxd = Directory.GetDirectories(fd);
                    foreach (var d in frxd) {
                        var di = new DirectoryInfo(d);
                        frameworks.Add(new FrameworkInformation() {
                            Name = di.Name,
                            Versions = di.GetDirectories().Select(c => c.Name)
                        });
                    }
                    ret.DotNetCoreFrameworksInstalled = frameworks;
                }
            }
            return ret;
        }
        private static string? GetVersion(RegistryKey key) {
            var v = key.GetValue("Version") as string;
            if (v != null)
                return v;
            foreach (var subkeyName in key.GetSubKeyNames()) {
                using (var subkey = key.OpenSubKey(subkeyName)) {
                    var version = GetVersion(subkey);
                    if (version != null) {
                        return version;
                    }
                }
            }
            return null;
        }
        private static string? FindNetCorePath() {
            // see https://github.com/dotnet/designs/blob/master/accepted/install-locations.md
            var is64bit = OperatingSystem.Is64bit;
            // C:\Program Files (x86)\dotnet
            try {
                string testPath = Path.Combine(SHGetKnownFolderPath(KNOWNFOLDERID.FOLDERID_ProgramFilesX86), "dotnet");
                if (Directory.Exists(testPath))
                    return testPath;
            } catch { }

            // C:\Program Files\dotnet
            if (is64bit) {
                try {
                    string testPath = Path.Combine(SHGetKnownFolderPath(KNOWNFOLDERID.FOLDERID_ProgramFilesX64), "dotnet");
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
            return null;
        }
    }
}
