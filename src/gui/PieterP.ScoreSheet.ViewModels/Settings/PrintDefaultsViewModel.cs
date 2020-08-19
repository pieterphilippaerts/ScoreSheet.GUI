using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Templates;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class PrintDefaultsViewModel {
        public PrintDefaultsViewModel() {
            this.PrintDirect = DatabaseManager.Current.Settings.PrintDirect;
            this.PrintViaAdobe = DatabaseManager.Current.Settings.PrintViaAdobe;
            this.PrintSponsors = DatabaseManager.Current.Settings.PrintSponsors;
            this.AdobePath = DatabaseManager.Current.Settings.AdobePath;
            this.Search = new RelayCommand(OnSearch);
            this.Browse = new RelayCommand(OnBrowse);
            this.RefereeTypes = RefereeLayout.All;
            this.PrintRefereeType = Cell.Create<RefereeLayout?>(this.RefereeTypes.Where(r => r.Id == DatabaseManager.Current.Settings.DefaultRefereeLayoutOption.Value).FirstOrDefault());
            this.PrintRefereeType.ValueChanged += () => DatabaseManager.Current.Settings.DefaultRefereeLayoutOption.Value = this.PrintRefereeType?.Value.Id ?? RefereeLayoutOptions.Default;
            // USE DERIVED OR CONVERTER CELL HERE
        }        

        private void OnSearch() {
            string? adobe = null;
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (pf != null && pf.Length > 0)
                adobe = FindAdobe(pf);

            if (adobe == null) {
                var pf64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (pf64 != null && pf64.Length > 0)
                    adobe = FindAdobe(pf64);
            }

            if (adobe == null) {
                NotificationManager.Current.Raise(new ShowMessageNotification(Strings.Print_AcrobatNotFound));
            } else {
                this.AdobePath.Value = adobe;
            }
        }
        private string? FindAdobe(string dir) {
            if (dir == null)
                return null;
            string sdir = Path.Combine(dir, "Adobe");
            if (!Directory.Exists(sdir))
                return null;
            string[] files = Directory.GetFiles(sdir, "AcroRd32.exe", SearchOption.AllDirectories);
            if (files.Length > 0)
                return files[0];
            files = Directory.GetFiles(sdir, "Acrobat.exe", SearchOption.AllDirectories);
            if (files.Length > 0)
                return files[0];
            return null;
        }
       
        private void OnBrowse() {
            var not = new FileDialogNotification(FileDialogTypes.OpenFile);
            not.Filter = string.Format("Adobe Acrobat (Reader)|AcroRd32.exe;Acrobat.exe|{0} (*.*)|*.*", Strings.Print_AllFiles);
            not.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            not.InitialFilename = "AcroRd32.exe";
            NotificationManager.Current.Raise(not);
            if (not.SelectedPath != null) {
                this.AdobePath.Value = not.SelectedPath;
            }
        }

        public Cell<bool> PrintDirect { get; private set; }
        public Cell<bool> PrintViaAdobe { get; private set; }
        public Cell<bool> PrintSponsors { get; private set; }
        public Cell<string> AdobePath { get; private set; }
        public Cell<RefereeLayout?> PrintRefereeType { get; private set; }
        public IEnumerable<RefereeLayout> RefereeTypes { get; private set; }
        public ICommand Browse { get; private set; }
        public ICommand Search { get; private set; }
    }
}
