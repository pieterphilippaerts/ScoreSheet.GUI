using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using PieterP.ScoreSheet.Installer.Localization;
using PieterP.ScoreSheet.Installer.Models;

namespace PieterP.ScoreSheet.Installer.ViewModels {
    public class LanguagesViewModel {
        public LanguagesViewModel(MainViewModel parent) {
            _parent = parent;
            this.Continue = new RelayCommand<string>(c => {
                DatabaseManager.Current.CultureSettings.DefaultCulture = c;
                var ci = new CultureInfo(c);
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = ci;
                Strings.Culture = ci;
                _parent.CurrentScreen = new PrereqViewModel(_parent);
            });
            _supportedCultures = DatabaseManager.Current.CultureSettings.SupportedCultures.ToList();

            // if we support the default user language (the setting from Windows), make sure it's first
            // in the list
            try {
                CultureInfo userDefault = null;
                if (Environment.OSVersion.Version.Major >= 6) { // Windows Vista or higher
                    var sb = new StringBuilder(LOCALE_NAME_MAX_LENGTH);
                    if (GetUserDefaultLocaleName(sb, LOCALE_NAME_MAX_LENGTH) > 0)
                        userDefault = new CultureInfo(sb.ToString());
                }
                if (userDefault == null) // fallback for Windows XP and if it fails on Vista or higher
                    userDefault = new CultureInfo(GetUserDefaultLCID());
                var supportedDefault = _supportedCultures.Where(c => c.StartsWith(userDefault.TwoLetterISOLanguageName)).FirstOrDefault();
                if (supportedDefault != null) {
                    _supportedCultures.Remove(supportedDefault);
                    _supportedCultures.Insert(0, supportedDefault);
                }
            } catch { 
                // ignore it; this can happen if the user has a weird LCID that is not supported by .NET framework 3.5
            }
        }
        public IList<string> SupportedCultures {
            get {
                return _supportedCultures;
            }
        }

        public ICommand Continue { get; private set; }

        private MainViewModel _parent;
        private IList<string> _supportedCultures;

        internal const int LOCALE_NAME_MAX_LENGTH = 85;
        [DllImport("kernel32.dll")]
        private static extern int GetUserDefaultLCID();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetUserDefaultLocaleName(StringBuilder buf, int bufferLength);
    }
}
