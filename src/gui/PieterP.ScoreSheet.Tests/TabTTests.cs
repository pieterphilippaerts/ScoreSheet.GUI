using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PieterP.ScoreSheet.Connector;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PieterP.ScoreSheet.Tests {
    [TestClass]
    public class TabTTests {
        [TestMethod]
        public async Task TestInitialize() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);

            var result = await connector.TestAsync();

            Assert.IsTrue(result.ErrorCode == TabTErrorCode.NoError);
            if (result.Info == null) {
                Assert.Fail();
            } else {
                Assert.AreEqual(result.Info.ApiVersion, new Version(0, 7, 23));
                Assert.AreEqual(result.Info.Culture, new CultureInfo("nl"));
                Assert.AreEqual(result.Info.DatabaseName, "vttl");
                Assert.IsTrue(result.Info.ValidAccount);
            }
        }
        [TestMethod]
        public async Task TestInvalidLogin() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials("invalid", "login");
            var result = await connector.TestAsync();
            Assert.IsTrue(result.ErrorCode == TabTErrorCode.InvalidCredentials);
        }
        [TestMethod]
        public async Task TestGetActiveSeason() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);
            var result = await connector.GetActiveSeason();
            Assert.AreEqual(result.Id, 20);
        }
        [TestMethod]
        public async Task TestGetClubs() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);
            var result = await connector.GetClubsAsync();
            Assert.IsTrue(result.Count() > 0);
        }
        [TestMethod]
        public async Task TestGetMatchSystems() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);
            var result = await connector.GetMatchSystemsAsync();
            Assert.IsTrue(result.Count() > 0);
        }
        [TestMethod]
        public async Task TestGetTeams() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);
            var result = await connector.GetTeams("VLB234");
            Assert.IsTrue(result.Count() > 0);
        }
        [TestMethod]
        public async Task TestGetDivisions() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);
            var result = await connector.GetDivisions(TabTDivisionRegion.VlaamsBrabant);
            Assert.IsTrue(result.Count() > 0);
        }

        [TestMethod]
        public async Task TestGetMatches() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);
            var teams = await connector.GetTeams("VLB234");
            var matches = await connector.GetMatches("VLB234", teams.Where(t => t.Team == "A").First());
            Assert.IsTrue(matches.Count() > 0);
        }

        [TestMethod]
        public async Task TestGetMembers() {
            var connector = new TabTConnector();
            connector.SetDefaultCredentials(AuthorizedUser, TestPass);
            var members = await connector.GetMembers("LK052", 1, false);
            Assert.IsTrue(members.Count() > 0);
        }

        private string AuthorizedUser { 
            get {
                AssertConfig();
                return _configRoot!["VttlAuthorizedUser"];
            }
        }
        private string UnauthorizedUser {
            get {
                AssertConfig();
                return _configRoot!["VttlUnauthorizedUser"];
            }
        }
        private string TestPass{
            get {
                AssertConfig();
                return _configRoot!["VttlTestUserPass"];
            }
        }
        private void AssertConfig() {
            if (_configRoot != null)
                return;
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<TabTTests>();
            _configRoot = builder.Build();
        }
        private IConfigurationRoot? _configRoot;
    }
}
