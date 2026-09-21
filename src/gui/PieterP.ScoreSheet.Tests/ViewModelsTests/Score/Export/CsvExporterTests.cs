using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Database.Entities;
using PieterP.ScoreSheet.Model.Information;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.ScoreSheet.ViewModels.Score.MatchSystems;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Tests.ViewModelsTests.Score.Export
{
    [TestClass]
    [DoNotParallelize]
    public class CsvExporterTests
    {
        private const string Header = "C;Match result exported by Scoresheet, Copyright (c) PieterP.be\r\n";
        private const string CompleteResult = "4/-7/9/0";

        private string _temporaryDirectory = null!;
        private Logger _logger = null!;
        private CsvExporter _subject = null!;

        [TestInitialize]
        public void Initialize()
        {
            _temporaryDirectory = Path.Combine(Path.GetTempPath(), "PieterP.ScoreSheet.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_temporaryDirectory);

            ServiceLocator.RegisterInstance<ITimerService>(new TestTimerService());

            var database = new DatabaseManager("CsvExporterTests");
            var basePathField = typeof(DatabaseManager).GetField("_basePath", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(basePathField, "The test fixture could not isolate the database base path.");
            basePathField.SetValue(database, _temporaryDirectory);
            ServiceLocator.RegisterInstance(database);

            _logger = new Logger(Path.Combine(_temporaryDirectory, "test.log"), false);
            ServiceLocator.RegisterInstance(_logger);

            database.Settings.CurrentSeason.Value = new Season { Id = 42, Name = "Test season" };
            _subject = new CsvExporter();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_temporaryDirectory))
                Directory.Delete(_temporaryDirectory, true);
        }

        [TestMethod]
        public void ExportSingleMatchAsString_IncludesHeaderAndMatch()
        {
            var match = CreateCompleteMatch();

            var actual = _subject.Export(match);

            Assert.AreEqual(Header + ExpectedCompleteLine(), actual);
        }

        [TestMethod]
        public void ExportMatchesAsString_IncludesOneHeaderAndEveryMatch()
        {
            var first = CreateCompleteMatch();
            var second = CreateCompleteMatch();
            second.MatchId.Value = "M-2";

            var actual = _subject.Export(new[] { first, second });

            Assert.AreEqual(Header + ExpectedCompleteLine() + ExpectedCompleteLine(matchId: "M-2"), actual);
        }

        [TestMethod]
        public void ExportEmptySequenceAsString_ReturnsOnlyHeader()
        {
            var actual = _subject.Export(Array.Empty<CompetitiveMatchViewModel>());

            Assert.AreEqual(Header, actual);
        }

        [TestMethod]
        public void ExportMatchesToStringBuilder_AppendsHeaderAndMatches()
        {
            var output = new StringBuilder("existing:");

            _subject.Export(new[] { CreateCompleteMatch() }, output);

            Assert.AreEqual("existing:" + Header + ExpectedCompleteLine(), output.ToString());
        }

        [TestMethod]
        public void ExportSingleMatchToStringBuilder_AppendsMatchWithoutHeader()
        {
            var output = new StringBuilder("existing:");

            _subject.Export(CreateCompleteMatch(), output);

            Assert.AreEqual("existing:" + ExpectedCompleteLine(), output.ToString());
        }

        [TestMethod]
        public void ExportSingleMatchToFile_WritesHeaderAndMatch()
        {
            var filename = Path.Combine(_temporaryDirectory, "single.csv");

            _subject.Export(CreateCompleteMatch(), filename);

            Assert.AreEqual(Header + ExpectedCompleteLine(), File.ReadAllText(filename));
        }

        [TestMethod]
        public void ExportMatchesToFile_WritesOneHeaderAndEveryMatch()
        {
            var filename = Path.Combine(_temporaryDirectory, "many.csv");
            var first = CreateCompleteMatch();
            var second = CreateCompleteMatch();
            second.MatchId.Value = "M-2";

            _subject.Export(new[] { first, second }, filename);

            Assert.AreEqual(
                Header + ExpectedCompleteLine() + ExpectedCompleteLine(matchId: "M-2"),
                File.ReadAllText(filename));
        }

        [TestMethod]
        public void ExportWithoutCurrentSeason_UsesApplicationDefaultAndLogsWarning()
        {
            DatabaseManager.Current.Settings.CurrentSeason.Value = null;

            var actual = ExportLine(CreateCompleteMatch());

            StringAssert.StartsWith(actual, $"I;{Application.DefaultSeasonId};M-1;");
            Assert.AreEqual(1, WarningCount());
        }

        [TestMethod]
        public void ExportMissingIdentifiers_LeavesColumnsEmptyAndLogsWarnings()
        {
            var match = CreateCompleteMatch();
            match.MatchId.Value = string.Empty;
            match.HomeTeam.ClubId.Value = string.Empty;
            match.AwayTeam.ClubId.Value = string.Empty;

            var actual = ExportLine(match);

            StringAssert.StartsWith(actual, "I;42;;;;1001;2001;");
            Assert.AreEqual(3, WarningCount());
        }

        [TestMethod]
        public void ExportNullClubIdentifier_LeavesColumnEmptyWithoutMissingIdentifierWarning()
        {
            var match = CreateCompleteMatch();
            match.HomeTeam.ClubId.Value = null!;

            var actual = ExportLine(match);

            StringAssert.StartsWith(actual, "I;42;M-1;;AFTT456;");
            Assert.AreEqual(0, WarningCount());
        }

        [TestMethod]
        public void Export_NormalizesFlemishClubIdentifier()
        {
            var actual = ExportLine(CreateCompleteMatch());

            StringAssert.StartsWith(actual, "I;42;M-1;VLB123;AFTT456;");
        }

        [TestMethod]
        public void ExportTeam_FormatsSinglesOptionalPlayersSubstitutesAndDoubles()
        {
            var match = CreateCompleteMatch();
            var primary = (SinglePlayerInfo)match.HomeTeam.Players[0];

            var optional = new SinglePlayerInfo(match.HomeTeam, "2", optional: true);
            var dash = new SinglePlayerInfo(match.HomeTeam, "3");
            dash.ComputerNumber.Value = "-";
            var invalid = new SinglePlayerInfo(match.HomeTeam, "4");
            invalid.Name.Value = "Invalid player";
            invalid.ComputerNumber.Value = "not-a-number";
            var doubles = new DoublePlayerInfo(
                match.HomeTeam,
                new[] { new DoublePlayerOption("1/2+3\\4") });
            var substitute = new SubstitutePlayerInfo(match.HomeTeam, primary);
            substitute.SelectedTransferMatch.Value = match.Matches[0];

            match.HomeTeam.Players.Add(optional);
            match.HomeTeam.Players.Add(dash);
            match.HomeTeam.Players.Add(invalid);
            match.HomeTeam.Players.Add(doubles);
            match.HomeTeam.Players.Add(substitute);

            var actual = ExportLine(match);

            StringAssert.Contains(actual, ";1001+1,?,,1-2-3-4;2001;");
            Assert.AreEqual(1, WarningCount());
        }

        [TestMethod]
        [DataRow("11:4|7:11|11:9|11:0", "4/-7/9/0", DisplayName = "Ordinary home win")]
        [DataRow("4:11|11:7|9:11|0:11", "-4/7/-9/-0", DisplayName = "Ordinary away win")]
        [DataRow(":WO", "9", DisplayName = "Home wins by walkover")]
        [DataRow("WO:", "8", DisplayName = "Away wins by walkover")]
        [DataRow(":FF", "0/0/0", DisplayName = "Home wins by forfeit")]
        [DataRow("FF:", "-0/-0/-0", DisplayName = "Away wins by forfeit")]
        [DataRow("WO:WO", "-", DisplayName = "Neither player wins")]
        public void Export_EncodesIndividualMatchResult(string encodedSets, string expectedResult)
        {
            var match = CreateCompleteMatch();
            SetScore(match, encodedSets);

            var actual = ExportLine(match);

            StringAssert.Contains(actual, $";P{expectedResult};");
        }

        [TestMethod]
        [DataRow("", DisplayName = "Empty individual match")]
        [DataRow("1:1", DisplayName = "Invalid individual match")]
        public void ExportPartial_WithIncompleteOrInvalidResult_UsesPlaceholderAndOmitsOptionalColumns(string encodedSets)
        {
            var match = CreateCompleteMatch();
            SetScore(match, encodedSets);

            var actual = ExportLine(match, partial: true);

            Assert.AreEqual("I;42;M-1;VLB123;AFTT456;1001;2001;P?\r\n", actual);
        }

        [TestMethod]
        public void ExportSingleMatchAsString_WithPartialResult_PreservesHeaderAndPartialShape()
        {
            var match = CreateCompleteMatch();
            SetScore(match, string.Empty);

            var actual = _subject.Export(match, partial: true);

            Assert.AreEqual(Header + "I;42;M-1;VLB123;AFTT456;1001;2001;P?\r\n", actual);
        }

        [TestMethod]
        public void ExportPartial_WithCompleteResult_IncludesOptionalColumns()
        {
            var actual = ExportLine(CreateCompleteMatch(), partial: true);

            Assert.AreEqual(ExpectedCompleteLine(), actual);
        }

        [TestMethod]
        [DataRow("", DisplayName = "Empty individual match")]
        [DataRow("1:1", DisplayName = "Invalid individual match")]
        public void ExportNonPartial_WithUnexportableResult_UsesEmptyResultAndIncludesOptionalColumns(string encodedSets)
        {
            var match = CreateCompleteMatch();
            SetScore(match, encodedSets);

            var actual = ExportLine(match);

            Assert.AreEqual(ExpectedCompleteLine(result: string.Empty), actual);
        }

        [TestMethod]
        public void ExportMatchWithMultipleIndividualMatches_SeparatesResultsWithComma()
        {
            var match = CreateCompleteMatch();
            var secondMatch = new MatchInfo(
                2,
                match.HomeTeam.Players[0],
                match.AwayTeam.Players[0]);
            match.Matches.Add(secondMatch);
            SetMatchScore(secondMatch, "WO:");
            match.Score.Refresh();

            var actual = ExportLine(match);

            StringAssert.Contains(actual, $";P{CompleteResult},8;");
        }

        [TestMethod]
        public void ExportBye_UsesByeResultAndPlaceholderOfficials()
        {
            var match = CreateCompleteMatch();
            match.HomeTeam.ClubId.Value = "Vrij";
            match.Score.Refresh();

            var actual = ExportLine(match, partial: true);

            Assert.AreEqual(
                "I;42;M-1;bye;AFTT456;1001;2001;U0-0;-,-,-,-;18:30,21:45\r\n",
                actual);
        }

        [TestMethod]
        public void ExportForfeit_MarksForfeitingPlayersAndUsesPlaceholderOfficials()
        {
            var match = CreateCompleteMatch();
            match.HomeTeam.Forfeit.Value = true;
            match.Score.Refresh();

            var actual = ExportLine(match);

            StringAssert.Contains(actual, ";-1001;2001;P8;-,-,-,-;");
        }

        [TestMethod]
        public void ExportInvalidOfficials_UsesPlaceholdersAndLogsWarnings()
        {
            var match = CreateCompleteMatch();
            match.HomeCaptain.ComputerNumber.Value = "home";
            match.AwayCaptain.ComputerNumber.Value = "away";
            match.ChiefReferee.ComputerNumber.Value = "referee";
            match.RoomCommissioner.ComputerNumber.Value = "commissioner";

            var actual = ExportLine(match);

            StringAssert.Contains(actual, $";P{CompleteResult};-,-,-,-;18:30,21:45");
            Assert.AreEqual(4, WarningCount());
        }

        [TestMethod]
        public void ExportNumericOfficials_PreservesSignedNumbers()
        {
            var match = CreateCompleteMatch();
            match.HomeCaptain.ComputerNumber.Value = "-1";
            match.AwayCaptain.ComputerNumber.Value = "+2";
            match.ChiefReferee.ComputerNumber.Value = "003";
            match.RoomCommissioner.ComputerNumber.Value = "4";

            var actual = ExportLine(match);

            StringAssert.Contains(actual, $";P{CompleteResult};-1,+2,003,4;");
            Assert.AreEqual(0, WarningCount());
        }

        [TestMethod]
        public void Export_WhenEitherTimeIsInvalid_LeavesTimeColumnEmpty()
        {
            var match = CreateCompleteMatch();
            match.EndHour.Value = "invalid";

            var actual = ExportLine(match);

            Assert.AreEqual(ExpectedCompleteLine(times: string.Empty), actual);
        }

        [TestMethod]
        public void ExportComments_EscapesBackslashesLineBreaksAndQuotes()
        {
            var match = CreateCompleteMatch();
            match.Comments.Value = "Folder\\file\r\n\"quoted\"";

            var actual = ExportLine(match);

            Assert.AreEqual(
                ExpectedCompleteLine(comment: "\"Folder\\\\file\\n'quoted'\""),
                actual);
        }

        private CompetitiveMatchViewModel CreateCompleteMatch()
        {
            var match = new CompetitiveMatchViewModel(new CsvTestMatchSystem());
            SetInitializing(match, true);

            match.MatchId.Value = "M-1";
            match.HomeTeam.ClubId.Value = "Vl-B123";
            match.AwayTeam.ClubId.Value = "AFTT456";
            ((SinglePlayerInfo)match.HomeTeam.Players[0]).ComputerNumber.Value = "1001";
            ((SinglePlayerInfo)match.AwayTeam.Players[0]).ComputerNumber.Value = "2001";
            match.HomeCaptain.ComputerNumber.Value = "3001";
            match.AwayCaptain.ComputerNumber.Value = "3002";
            match.ChiefReferee.ComputerNumber.Value = "3003";
            match.RoomCommissioner.ComputerNumber.Value = string.Empty;
            match.StartHour.Value = "18:30";
            match.EndHour.Value = "21:45";
            match.Comments.Value = string.Empty;
            SetScore(match, "11:4|7:11|11:9|11:0");

            return match;
        }

        private string ExportLine(CompetitiveMatchViewModel match, bool partial = false)
        {
            var output = new StringBuilder();
            _subject.Export(match, output, partial);
            return output.ToString();
        }

        private static string ExpectedCompleteLine(
            string matchId = "M-1",
            string result = CompleteResult,
            string officials = "3001,3002,3003,-",
            string times = "18:30,21:45",
            string? comment = null)
        {
            var commentColumn = comment == null ? string.Empty : ";" + comment;
            return $"I;42;{matchId};VLB123;AFTT456;1001;2001;P{result};{officials};{times}{commentColumn}\r\n";
        }

        private static void SetScore(CompetitiveMatchViewModel match, string encodedSets)
        {
            SetMatchScore(match.Matches[0], encodedSets);
            match.Score.Refresh();
        }

        private static void SetMatchScore(MatchInfo individualMatch, string encodedSets)
        {
            foreach (var set in individualMatch.Sets)
            {
                set.LeftScore.Value = string.Empty;
                set.RightScore.Value = string.Empty;
            }

            if (encodedSets.Length > 0)
            {
                var sets = encodedSets.Split('|');
                for (var index = 0; index < sets.Length; index++)
                {
                    var scores = sets[index].Split(':');
                    individualMatch.Sets[index].LeftScore.Value = scores[0];
                    individualMatch.Sets[index].RightScore.Value = scores[1];
                }
            }
        }

        private static void SetInitializing(CompetitiveMatchViewModel match, bool value)
        {
            var property = typeof(CompetitiveMatchViewModel).GetProperty(
                "IsInitializing",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var setter = property?.GetSetMethod(nonPublic: true);
            Assert.IsNotNull(setter, "The test fixture could not suppress background persistence while arranging a match.");
            setter.Invoke(match, new object[] { value });
        }

        private int WarningCount()
        {
            return _logger.Entries.Count(entry => entry.Type == LogType.Warning);
        }

        private sealed class CsvTestMatchSystem : VMMatchSystem
        {
            public override void Initialize(TeamInfo home, TeamInfo away, IList<MatchInfo> matches)
            {
                var homePlayer = new SinglePlayerInfo(home, "1");
                var awayPlayer = new SinglePlayerInfo(away, "1");
                home.Players.Add(homePlayer);
                away.Players.Add(awayPlayer);
                matches.Add(new MatchInfo(1, homePlayer, awayPlayer));
            }

            public override int Id => 0;
            public override int SetCount => 3;
            public override int PointCount => 11;
            public override int PlayerCount => 1;
            public override int MatchCount => 1;
            public override string Name => "CSV exporter test system";
            public override bool IsCompetitive => true;
            public override int SingleMatchCount => 1;
            public override int DoubleMatchCount => 0;
        }

        private sealed class TestTimerService : ITimerService
        {
            event Action<ITimerService> ITimerService.Tick
            {
                add { }
                remove { }
            }

            public void Start(TimeSpan interval)
            {
            }

            public void Stop()
            {
            }
        }
    }
}
