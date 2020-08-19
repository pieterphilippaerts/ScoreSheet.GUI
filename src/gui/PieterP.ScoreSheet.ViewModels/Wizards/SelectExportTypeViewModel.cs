using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.ScoreSheet.ViewModels.Score.MatchSystems;
using PieterP.Shared;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class SelectExportTypeViewModel : WizardPanelViewModel {
        public SelectExportTypeViewModel(WizardViewModel parent, IEnumerable<CompetitiveMatchViewModel> selectedMatches) : base(parent) {
            _selectedMatches = selectedMatches;
            this.Export = new RelayCommand<string>(OnExport);
        }

        private void OnExport(string parameter) {
            bool exported = false;
            switch (parameter) {
                case "CSV":
                    var csvPath = GetSavePath("Comma-separated values (*.csv)|*.csv", "export.csv");
                    if (csvPath != null) {
                        var exporter = new CsvExporter();
                        exporter.Export(_selectedMatches, csvPath);
                        exported = true;
                    }
                    break;
                case "PDF":
                case "XPS":
                    string filter, initialFile;
                    if (parameter == "PDF") {
                        filter = "Portable Document Format (*.pdf)|*.pdf";
                        initialFile = "export.pdf";
                    } else {
                        filter = "XML Paper Specification (*.xps)|*.xps";
                        initialFile = "export.xps";
                    }
                    var exPath = GetSavePath(filter, initialFile);
                    if (exPath != null) {
                        var tempVms = _selectedMatches.Select(c => c.MatchSystem.GenerateTemplate(c));
                        var exporter = ServiceLocator.Resolve<IExportService>();
                        if (parameter == "PDF") {
                            exporter.ToPdfFile(tempVms, exPath, tempVms.First().IsLandscape);
                        } else {
                            exporter.ToXpsFile(tempVms, exPath, tempVms.First().IsLandscape);
                        }
                        exported = true;
                    }
                    break;
            }
            if (exported)
                NotificationManager.Current.Raise(new CloseDialogNotification(true));
        }
        private string? GetSavePath(string filter, string initialFilename) {
            var sf = new FileDialogNotification(FileDialogTypes.SaveFile) {
                Filter = filter,
                InitialFilename =initialFilename
            };
            NotificationManager.Current.Raise(sf);
            return sf.SelectedPath;
        }

        public ICommand Export { get; private set; }

        private IEnumerable<CompetitiveMatchViewModel> _selectedMatches;

        public override string Title => Wizard_Export;
        public override string Description => Wizard_ExportDesc;
    }
}
