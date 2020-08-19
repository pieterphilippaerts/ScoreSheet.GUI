using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Notifications {
    public class FileDialogNotification : Notification {
        public FileDialogNotification(FileDialogTypes type) {
            this.Type = type;
            this.InitialDirectory = DatabaseManager.Current.Settings.LastOpenSaveDirectory.Value;
            this.AfterRaise += n => {
                if (this.SelectedPath != null)
                    DatabaseManager.Current.Settings.LastOpenSaveDirectory.Value = new FileInfo(this.SelectedPath).Directory.FullName;
            };
        }
        public string? SelectedPath { get; set; }
        public string InitialDirectory { get; set; }
        public string Filter { get; set; }
        public string? Title { get; set; }
        public string? InitialFilename { get; set; }
        public FileDialogTypes Type { get; set; }

    }
    public enum FileDialogTypes { 
        OpenFile,
        SaveFile
    }
}
