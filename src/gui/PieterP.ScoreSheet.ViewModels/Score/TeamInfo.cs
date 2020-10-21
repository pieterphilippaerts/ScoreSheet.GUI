using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score {
    /// <summary>
    /// This class represents information about a team in a match.
    /// </summary>
    public class TeamInfo
    {
        #region Commands

        /// <summary>
        /// Command to be invoked when the list of players should be browsed.
        /// </summary>
        public ICommand BrowseForPlayersCommand { get; private set; }

        #endregion

        #region Constructors

        public TeamInfo(CompetitiveMatchViewModel match, Action onDataChanged) {
            ParentMatch = match;
            OnDataChangedAction = onDataChanged;

            Name = Cell.Create(string.Empty, onDataChanged);
            Forfeit = Cell.Create(false, onDataChanged);
            ClubId = Cell.Create(string.Empty, onDataChanged);
            Captain = Cell.Create<SinglePlayerInfo?>(null);
            IsBye = Cell.Derived(ClubId, cid => MatchStartInfo.IsByeIndex(cid));

            Forfeit.ValueChanged += ForfeitValueChanged;

            var players = new ObservableCollection<PlayerInfo>();
            players.CollectionChanged += OnPlayersCollectionChanged;
            Players = players;

            BrowseForPlayersCommand = new RelayCommand(BrowseForPlayers);
        }

        #endregion

        #region Events

        /// <summary>
        /// Callback for when the collection of players changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnPlayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            foreach (var newItem in eventArgs.NewItems)
            {
                if (newItem is PlayerInfo playerInfo)
                    playerInfo.DataChanged += () => OnDataChangedAction?.Invoke();
                
                if (newItem is SinglePlayerInfo singlePlayerInfo)
                {
                    singlePlayerInfo.Captain.ValueChanged += () => {
                        if (singlePlayerInfo.Captain.Value)
                            UpdateCaptain(singlePlayerInfo);
                        else if (!Players.Any(p => p is SinglePlayerInfo spi && spi.Captain.Value))
                            UpdateCaptain(null);
                    };
                }
            }
        }

        /// <summary>
        /// Callback for when the value of Forfeit changed.
        /// </summary>
        private void ForfeitValueChanged() {
            if (!Forfeit.Value)
                return;

            foreach (var player in Players) {
                if (player is SinglePlayerInfo spi && spi.ComputerNumber.Value.Length == 0) {
                    spi.ComputerNumber.Value = "?";
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Action to invoke when the data changes.
        /// </summary>
        private Action OnDataChangedAction { get; set; }

        /// <summary>
        /// The name of the team.
        /// </summary>
        public Cell<string> Name { get; private set; }

        /// <summary>
        /// Whether the team is forfeited.
        /// </summary>
        public Cell<bool> Forfeit { get; private set; }

        /// <summary>
        /// The club id of the team.
        /// </summary>
        public Cell<string> ClubId { get; private set; }

        /// <summary>
        /// The list of players in the team.
        /// </summary>
        public IList<PlayerInfo> Players { get; private set; }

        /// <summary>
        /// Whether the team is bye.
        /// </summary>
        public Cell<bool> IsBye { get; private set; }

        /// <summary>
        /// A reference to the parent match.
        /// </summary>
        public CompetitiveMatchViewModel ParentMatch { get; private set; }

        /// <summary>
        /// The captain of the team.
        /// </summary>
        public Cell<SinglePlayerInfo?> Captain { get; private set; }

        #endregion

        #region Functions

        /// <summary>
        /// Calls the parent match to browse for players for this team.
        /// </summary>
        public void BrowseForPlayers() => ParentMatch.BrowseForPlayers(this);

        /// <summary>
        /// Updates the captain to the passed captain.
        /// If a captain is passed, it is ensured that no other players are still captain because a team can only have one captain.
        /// </summary>
        /// <param name="newCaptain">The new captain for the team. Can be null to reset the captain.</param>
        public void UpdateCaptain(SinglePlayerInfo? newCaptain)
        {
            Captain.Value = newCaptain;

            if (newCaptain != null)
            {
                // There can only be one captain.
                var otherCaptains = Players.Where(p => p is SinglePlayerInfo spi && spi != newCaptain && spi.Captain.Value).Cast<SinglePlayerInfo>().ToList();
                foreach (var otherCaptain in otherCaptains)
                    otherCaptain.Captain.Value = false;
            }
        }

        #endregion
    }
}