using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.Model.Database.Updater;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Views.Wizards.Resources;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class NewDivisionDayViewModel : WizardPanelViewModel {
        public NewDivisionDayViewModel(WizardViewModel parent) : base(parent) {
            this.Regions = new ObservableCollection<RegionItem>() {
                new RegionItem(Region.Super, "Super"),
                new RegionItem(Region.National, NewDivisionDay_National),
                new RegionItem(Region.RegionalVTTL, "Regionaal (Vlaanderen)"),
                new RegionItem(Region.RegionalIWB, "Régionale (Wallonie)"),
                new RegionItem(Region.Antwerpen, "Antwerpen"),
                new RegionItem(Region.BruxellesBrabantWallon, "Bruxelles & Brabant Wallon"),
                new RegionItem(Region.Hainaut, "Hainaut"),
                new RegionItem(Region.Liege, "Liège"),
                new RegionItem(Region.Limburg, "Limburg"),
                new RegionItem(Region.Luxembourg, "Luxembourg"),
                new RegionItem(Region.Namur, "Namur"),
                new RegionItem(Region.OostVlaanderen, "Oost-Vlaanderen"),
                new RegionItem(Region.VlaamsBrabant, "Vlaams Brabant"),
                new RegionItem(Region.WestVlaanderen, "West-Vlaanderen")
            };
            this.Divisions = new ObservableCollection<Division>();
            this.Dates = new ObservableCollection<string>();
            this.SelectedRegion = Cell.Create<RegionItem?>(null, OnRegionSelected);
            this.SelectedDivision = Cell.Create<Division?>(null, OnDivisionSelected);
            this.SelectedDate = Cell.Create<string?>(null);
            this.Load = new RelayCommand(OnLoad, Cell.Derived(this.SelectedDate, v => v != null));
            this.Loading = Cell.Create(false);
        }

        private async void OnRegionSelected() {
            var selected = this.SelectedRegion.Value;
            if (selected == null)
                return;

            this.Divisions.Clear();
            this.Dates.Clear();

            var updater = new TabTUpdater();
            this.Loading.Value = true;
            try {
                var divisions = await updater.GetDivisions(selected.Region);
                if (divisions != null) {
                    foreach (var division in divisions.OrderBy(d => d.Name)) {
                        this.Divisions.Add(division);
                    }
                }
            } finally {
                this.Loading.Value = false;
            }
        }
        private async void OnDivisionSelected() {
            var selected = this.SelectedDivision.Value;
            if (selected == null)
                return;

            this.Dates.Clear();

            var updater = new TabTUpdater();
            this.Loading.Value = true;
            try {
                var matches = await updater.GetDivisionMatches(selected);
                if (matches != null) {
                    foreach (var match in matches.OrderBy(m => m.Date.ToDate())) {
                        if (match.Date != null) {
                            if (!this.Dates.Contains(match.Date))
                                this.Dates.Add(match.Date);
                        }
                    }
                    _allMatches = matches;
                }
            } finally {
                this.Loading.Value = false;
            }
        }

        private void OnLoad() {
            var selected = this.SelectedDate.Value;
            if (selected == null)
                return;

            var matches = new List<MatchStartInfo>();
            foreach (var match in _allMatches) {
                if (match.Date == selected) {
                    matches.Add(match);
                }
            }
            this.SelectedMatches = matches;
            NotificationManager.Current.Raise(new CloseDialogNotification(true));
        }

        public ObservableCollection<RegionItem> Regions { get; private set; }
        public ObservableCollection<Division> Divisions { get; private set; }
        public ObservableCollection<string> Dates { get; private set; }

        public Cell<RegionItem?> SelectedRegion { get; private set; }
        public Cell<Division?> SelectedDivision { get; private set; }
        public Cell<string?> SelectedDate { get; private set; }
        
        public IEnumerable<MatchStartInfo> SelectedMatches { get; private set; }
        
        private IEnumerable<MatchStartInfo> _allMatches;

        public ICommand Load { get; private set; }
        public Cell<bool> Loading { get; private set; }

        public override string Title => NewDivisionDay_Title;
        public override string Description => NewDivisionDay_Description;
    }
    public class RegionItem {
        public RegionItem(Region reg, string name) {
            this.Region = reg;
            this.Name = name;
        }
        public Region Region { get; private set; }
        public string Name { get; private set; }
    }
}