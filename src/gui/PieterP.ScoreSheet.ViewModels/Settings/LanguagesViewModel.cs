using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization.Views.Settings;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Helpers;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class LanguagesViewModel {
        public LanguagesViewModel() {
            this.ActiveCulture = DatabaseManager.Current.Settings.ActiveCulture;
            this.AvailableCultures = DatabaseManager.Current.CultureSettings.SupportedCultures.ToList();
            // first move the active culture to the top
            int index = this.AvailableCultures.IndexOf(this.ActiveCulture.Value);
            if (index != -1) {
                this.AvailableCultures.RemoveAt(index);
                this.AvailableCultures.Insert(0, this.ActiveCulture.Value);
            }
            // then move the default user language (the setting from Windows) to the top
            var defLcid = GetUserDefaultLCID();
            if (defLcid != 0x0c00) { // 0x0c00 is a special language code for the `newer' language formats (e.g., `Dutch (Aruba)')
                try {
                    var userDefault = new CultureInfo(defLcid);
                    var supportedDefault = AvailableCultures.Where(c => c.StartsWith(userDefault.TwoLetterISOLanguageName)).FirstOrDefault();
                    if (supportedDefault != null) {
                        AvailableCultures.Remove(supportedDefault);
                        AvailableCultures.Insert(0, supportedDefault);
                    }
                } catch {
                    // eat exceptions (Windows' default culture is probably something weird)
                }
            }

            this.MustRestart = Cell.Derived(this.ActiveCulture, culture => Thread.CurrentThread.CurrentUICulture.Name.ToLower() != culture.ToLower());
            this.RestartMessage = Cell.Derived(this.ActiveCulture, culture => {
                switch (culture) {
                    case "nl-BE":
                        return Resources.Language_RestartNl;
                    case "fr-BE":
                        return Resources.Language_RestartFr;
                    case "de-DE":
                        return Resources.Language_RestartDe;
                    default:
                        return Resources.Language_RestartEn;
                }
            });
            this.OpenHelpUrl = new LaunchCommand("https://score.pieterp.be/Help/Translation");
        }

        public IList<string> AvailableCultures { get; private set; }
        public Cell<string> ActiveCulture { get; private set; }
        public Cell<bool> MustRestart { get; private set; }
        public ICommand OpenHelpUrl { get; private set; }
        public Cell<string> RestartMessage { get; private set; }

        [DllImport("kernel32.dll")]
        private static extern int GetUserDefaultLCID();
    }
}
