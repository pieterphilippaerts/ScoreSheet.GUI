using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;
using static PieterP.ScoreSheet.Localization.Errors;
using static PieterP.ScoreSheet.Localization.Strings;

namespace PieterP.ScoreSheet.ViewModels.Wizards {
    public class UpdateFromInternetViewModel : WizardPanelViewModel {
        public UpdateFromInternetViewModel(WizardViewModel parent) : base(parent) {
            this.Provinces = new ProvinceItem[] { 
                new ProvinceItem(Province.Antwerp, "Antwerpen"),
                new ProvinceItem(Province.WalloonBrabantBrussels, "Bruxelles & Brabant Wallon"),
                new ProvinceItem(Province.Hainaut, "Hainaut"),
                new ProvinceItem(Province.Liege, "Liège"),
                new ProvinceItem(Province.Limburg, "Limburg"),
                new ProvinceItem(Province.Luxemburg, "Luxembourg"),
                new ProvinceItem(Province.Namur, "Namur"),
                new ProvinceItem(Province.EastFlanders, "Oost-Vlaanderen"),
                new ProvinceItem(Province.FlemishBrabantBrussels, "Vlaams Brabant"),
                new ProvinceItem(Province.WestFlanders, "West-Vlaanderen")
            };
            this.SelectedProvince = Cell.Create(this.Provinces.First());
            this.SelectedProvince.ValueChanged += SelectedProvince_ValueChanged;
            this.Clubs = Cell.Create<IEnumerable<Club>?>(DatabaseManager.Current.Clubs.ByProvince(this.SelectedProvince.Value.Province).OrderBy(c => c.UniqueIndex));
            this.SelectedClub = Cell.Create<Club?>(null);
            this.Next = new RelayCommand(OnNext, () => SelectedClub.Value != null);
            this.SelectedClub.ValueChanged += () => Next.RaiseCanExecuteChanged();
            this.RefreshClubs = new RelayCommand(OnRefreshClubs);
            this.IsUpdating = Cell.Create(false);

            if (this.Clubs.Value.Count() == 0) {
                OnRefreshClubs();
            } else {
                // select default club
                var homeClub = DatabaseManager.Current.Settings.HomeClubId.Value;
                if (homeClub != null) {
                    var club = DatabaseManager.Current.Clubs[homeClub];
                    if (club != null && club.Province != null) {
                        this.SelectedProvince.Value = this.Provinces.Where(c => c.Province == club.Province.Value).FirstOrDefault();
                        this.SelectedClub.Value = this.Clubs.Value.Where(c => c.UniqueIndex == club.UniqueIndex).FirstOrDefault();
                    }
                }
            }
        }

        private void SelectedProvince_ValueChanged() {
            this.SelectedClub.Value = null;
            this.Clubs.Value = DatabaseManager.Current.Clubs.ByProvince(this.SelectedProvince.Value.Province).OrderBy(c => c.UniqueIndex);
        }

        public IEnumerable<ProvinceItem> Provinces { get; private set; }
        public Cell<ProvinceItem> SelectedProvince { get; private set; }
        public Cell<IEnumerable<Club>?> Clubs { get; private set; }
        public Cell<Club?> SelectedClub { get; private set; }

        public Cell<bool> IsUpdating { get; private set; }
        public ICommand RefreshClubs { get; private set; }
        private async void OnRefreshClubs() {
            this.IsUpdating.Value = true;

            if (await DatabaseManager.Current.UpdateClubs()) {
                SelectedProvince_ValueChanged();
            } else {
                // uhoh.. error
                NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_ClubUpdateFailed, NotificationTypes.Error));
            }

            this.IsUpdating.Value = false;
        }

        public RelayCommand<object> Next { get; private set; }
        private void OnNext() {
            Parent.CurrentPanel.Value = new UpdatingFromInternetViewModel(Parent, SelectedClub.Value!);
        }

        public override string Title => Wizard_Update;
        public override string Description => Wizard_UpdateDesc;

        public class ProvinceItem {
            public ProvinceItem(Province prov, string name) {
                this.Province = prov;
                this.Name = name;
            }
            public Province Province { get; private set; }
            public string Name { get; private set; }
        }
    }
}