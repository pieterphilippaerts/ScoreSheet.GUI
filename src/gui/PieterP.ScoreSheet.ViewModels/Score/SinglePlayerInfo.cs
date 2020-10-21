using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Score
{
    /// <summary>
    /// This class represents information about a single player in a match.
    /// </summary>
    public class SinglePlayerInfo : PlayerInfo
    {
        #region Constructors 

        public SinglePlayerInfo(TeamInfo team, string position, bool optional = false) : base(team)
        {
            Position = position;
            Optional = optional;

            Name = Cell.Create(string.Empty, RaiseDataChanged);
            ComputerNumber = Cell.Create(string.Empty, RaiseDataChanged);
            StrengthListPosition = Cell.Create(string.Empty, RaiseDataChanged);
            Index = Cell.Create(string.Empty, RaiseDataChanged);
            Ranking = Cell.Create(string.Empty, RaiseDataChanged);

            Captain = Cell.Create(false, RaiseDataChanged);
            Captain.ValueChanged += OnCaptainChanged;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event handler for when the captain value has changed.
        /// </summary>
        private void OnCaptainChanged() => ParentTeam.UpdateCaptain(Captain.Value ? this : null);

        #endregion

        #region Properties

        /// <summary>
        /// The position of the player.
        /// </summary>
        public string Position { get; private set; }

        /// <summary>
        /// The name of the player.
        /// </summary>
        public Cell<string> Name { get; private set; }

        /// <summary>
        /// The computer number of the player.
        /// </summary>
        public Cell<string> ComputerNumber { get; private set; }

        /// <summary>
        /// The position of the player in the strength list of the club.
        /// </summary>
        public Cell<string> StrengthListPosition { get; private set; }

        /// <summary>
        /// The index of the ranking in the strength list of the club.
        /// </summary>
        public Cell<string> Index { get; private set; }

        /// <summary>
        /// The ranking of the player.
        /// </summary>
        public Cell<string> Ranking { get; private set; }

        /// <summary>
        /// Whether the player is captain of the team.
        /// </summary>
        public Cell<bool> Captain { get; private set; }

        /// <summary>
        /// Whether the player is optional in the team.
        /// </summary>
        /// <remarks>In SUPER, the fourth player in the team is not required to be filled out.</remarks>
        public bool Optional { get; private set; }

        /// <summary>
        /// The type of player info.
        /// </summary>
        public override PlayerInfoTypes PlayerInfoType => PlayerInfoTypes.Single;

        #endregion
    }
}
