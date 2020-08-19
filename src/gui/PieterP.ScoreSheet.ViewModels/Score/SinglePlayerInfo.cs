using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.Shared.Cells;
using static PieterP.ScoreSheet.ViewModels.MainWindowViewModel;

namespace PieterP.ScoreSheet.ViewModels.Score {
    public class SinglePlayerInfo : PlayerInfo {
        public SinglePlayerInfo(TeamInfo team, string position, bool optional = false) : base(team) {
            this.Position = position;
            this.Name = Cell.Create("", RaiseDataChanged);
            this.ComputerNumber = Cell.Create("", RaiseDataChanged);
            this.StrengthListPosition = Cell.Create<string>("", RaiseDataChanged);
            this.Index = Cell.Create<string>("", RaiseDataChanged);
            this.Ranking = Cell.Create("", RaiseDataChanged);
            this.Captain = Cell.Create(false, RaiseDataChanged);
            this.Captain.ValueChanged += OnCaptainChanged;
            this.Optional = optional;
            this.Browse = new RelayCommand(OnBrowse);
        }

        private void OnCaptainChanged() {
            if (this.Captain.Value) {
                foreach (var player in ParentTeam.Players) {
                    var spi = player as SinglePlayerInfo;
                    if (spi != null && spi != this) {
                        spi.Captain.Value = false;
                    }
                }
            }
        }

        private void OnBrowse() {
            ParentTeam.ParentMatch.BrowseForPlayers(ParentTeam);
        }

        public string Position { get; private set; }
        public Cell<string> Name { get; private set; }
        public Cell<string> ComputerNumber { get; private set; }
        public Cell<string> StrengthListPosition { get; private set; }
        public Cell<string> Index { get; private set; }
        public Cell<string> Ranking { get; private set; }
        public Cell<bool> Captain { get; private set; }
        public bool Optional { get; private set; }
        public override PlayerInfoTypes PlayerInfoType => PlayerInfoTypes.Single;
        public ICommand Browse { get; private set; }
    }
}
