using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;
using PieterP.Shared.Cells;

namespace PieterP.Shared.Services {
    public class Logger {
        public Logger(string logfile, bool isDebug) {
            _logfile = logfile;
            _isDebug = isDebug;
            _logEntries = new ObservableCollection<LogEntry>();
            this.LatestMessage = Cell.Create(string.Empty);
        }

        // calls to this method are optimized away in Release builds
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static void LogDebug(string message) {
#if DEBUG
            ServiceLocator.Resolve<Logger>()?.LogMessage(LogType.Debug, message);
#endif
        }

        public static void Log(Exception? e) {
            ServiceLocator.Resolve<Logger>()?.LogException(e);
            var we = e as WebException;
            if (we == null && e != null)
                we = e.InnerException as WebException;
            if (we != null) {
                ServiceLocator.Resolve<Logger>()?.LogMessage(LogType.Exception, $"WebException status: {we.Status.ToString()}");
                ServiceLocator.Resolve<Logger>()?.LogMessage(LogType.Informational, $"Internet is not working? Please check your internet connection...");
            }
        }
        public static void Log(LogType type, string message) {
            ServiceLocator.Resolve<Logger>()?.LogMessage(type, message);
        }
        public void LogException(Exception? e) {
            if (e == null)
                LogMessage(LogType.Exception, "Unknown exception occurred.");
            else
                LogMessage(LogType.Exception, e.ToString());
        }
        public void LogMessage(LogType type, string message) {
            // write everything to debug output
            Debug.WriteLine($"{ type.ToString() } - { message }");

            if (type == LogType.Debug && !_isDebug)
                return; // do not show debug messages unless we're in a debug session

            // write exceptions to disk (or write everything to disk, if we're in a debug session)
            if (_logfile != null && (type == LogType.Exception || _isDebug)) {
                if (File.Exists(_logfile)) {
                    var fi = new FileInfo(_logfile);
                    if (fi.Length > MaxFileSize) { // truncate file; do not let the log grow indefinitely

                        using (var reader = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var writer = fi.Open(FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) {
                            writer.Seek(0, SeekOrigin.Begin);
                            reader.Seek(-TruncateFileSize, SeekOrigin.End);

                            var readTotal = 0;
                            byte[] buffer = new byte[4096 * 16];
                            int read = reader.Read(buffer, 0, Math.Min(buffer.Length, TruncateFileSize - readTotal));
                            while (readTotal < TruncateFileSize && read > 0) {
                                writer.Write(buffer, 0, read);
                                readTotal += read;
                                read = reader.Read(buffer, 0, Math.Min(buffer.Length, TruncateFileSize - readTotal));
                            }
                            // truncation done; 
                            writer.SetLength(TruncateFileSize);
                        }
                    }
                }
                using (var fsw = File.AppendText(_logfile)) {
                    fsw.WriteLine("{0} entry at {1}", type.ToString(), DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
                    fsw.WriteLine(message);
                    fsw.WriteLine("---");
                }
            }

            // add the log message to the log (viewable by the user in the log window)
            _logEntries.Add(new LogEntry(type, message, DateTime.Now));
            this.LatestMessage.Value = message;
        }

        public ReadOnlyObservableCollection<LogEntry> Entries => new ReadOnlyObservableCollection<LogEntry>(_logEntries);

        public Cell<string> LatestMessage { get; private set; }

        private string _logfile;
        private bool _isDebug;
        private const int MaxFileSize = 1024 * 1024;        // if the log file is larger than this size...
        private const int TruncateFileSize = 512 * 1024;    // ...truncate it to this size
        private ObservableCollection<LogEntry> _logEntries;

        public class LogEntry {
            public LogEntry(LogType type, string message, DateTime when) {
                this.Type = type;
                this.Message = message;
                this.When = when;
            }
            public LogType Type { get; private set; }
            public string Message { get; private set; }
            public DateTime When { get; private set; }
            public string LogWindowString {
                get {
                    string f = "?";
                    switch (this.Type) {
                        case LogType.Warning:
                            f = "W";
                            break;
                        case LogType.Exception:
                            f = "E";
                            break;
                        case LogType.Informational:
                            f = "I";
                            break;
                        case LogType.Debug:
                            f = "D";
                            break;
                    }
                    var m = Message ?? "";
                    if (m.Length > 1023)
                        m = m.Substring(0, 1023) + "⋯";
                    return $"[{When.ToString("HH:mm")}-{f}] {m}";
                }
            }
        }
    }
    public enum LogType {
        Exception,
        Warning,
        Informational,
        Debug
    }
}
