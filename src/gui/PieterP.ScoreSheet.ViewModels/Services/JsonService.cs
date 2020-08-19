using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.WebServer;
using PieterP.ScoreSheet.ViewModels.Score.Export;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

#if NETSTANDARD
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
#else
using System.Web;
#endif

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class JsonService : IDisposable {
        public JsonService(MainWindowViewModel mainVm) {
            ServiceLocator.RegisterInstance<JsonService>(this);
            _mainVm = mainVm;
            this.IsActive = Cell.Create(false);
            this.BaseUri = Cell.Create<string?>(null);
            DatabaseManager.Current.Settings.EnableJsonService.ValueChanged += ModifyServer;
            ModifyServer();
        }

        private void ModifyServer() {
            if (DatabaseManager.Current.Settings.EnableJsonService.Value) {
                if (_server == null) {
                    try {
                        this.BaseUri.Value = $"http://{ DatabaseManager.Current.Settings.JsonServiceHost.Value }:{ DatabaseManager.Current.Settings.JsonServicePort.Value }/";
                        _server = new WebServer(this.BaseUri.Value);
                        _server.RegisterPath("/api/matches.json", OnMatches);
                        _server.RegisterPath("/api/points.json", OnGetPoints, Model.WebServer.HttpMethods.Get);
                        _server.RegisterPath("/api/points.json", OnPutPoints, Model.WebServer.HttpMethods.Put);
                        _server.Started.ValueChanged += () => this.IsActive.Value = _server?.Started.Value ?? false;
                        Task.Run(_server.Start);
                    } catch (Exception e) {
                        Logger.Log(e);
                    }
                }
            } else {
                _server?.Dispose();
                _server = null;
                this.IsActive.Value = false;
            }
        }

        private RouteResult FileNotFound() {
            return new RouteResult() {
                StatusCode = 404,
                Result = "The requested item could not be found"
            };
        }
        private RouteResult BadRequest() {
            return new RouteResult() {
                StatusCode = 400,
                Result = "The client sent an invalid request"
            };
        }
        private RouteResult NoContent() {
            return new RouteResult() {
                StatusCode = 206,
                Result = ""
            };
        }
        private RouteResult Json(string result) {
            return new RouteResult() {
                StatusCode = 200,
                Result = result,
                ContentType = "application/json"
        };
        }
        private RouteResult OnMatches(RoutingContext context) {
            var jsonConverter = new JsonExporter();
            var matches = _mainVm.ActiveMatches.Select(m => jsonConverter.ToJson(m));
            var json = DataSerializer.Serialize(matches);
            return Json(json);
        }
        private RouteResult OnGetPoints(RoutingContext context) {
            int matchIndex = 0;
            if (!context.Query.ContainsKey("MatchIndex") || !int.TryParse(context.Query["MatchIndex"], out matchIndex)) {
                return FileNotFound();
            }
            string matchNr;
            if (!context.Query.ContainsKey("MatchNr")) {
                return BadRequest();
            }
            matchNr = context.Query["MatchNr"];
            if (matchNr == null || matchNr.Length == 0) {
                return BadRequest();
            }
            var match = _mainVm.ActiveMatches.Where(m => m.MatchId.Value == matchNr).FirstOrDefault();
            if (match == null) {
                return FileNotFound();
            }
            var matchInfo = match.Matches.Where(c => c.Position == matchIndex).FirstOrDefault();
            if (matchInfo == null) {
                return FileNotFound();
            }

            var jsonConverter = new JsonExporter();
            var json = DataSerializer.Serialize(jsonConverter.ToJson(matchInfo));
            return Json(json);
        }
        private RouteResult OnPutPoints(RoutingContext context) {
            int matchIndex = 0;
            if (!context.Query.ContainsKey("MatchIndex") || !int.TryParse(context.Query["MatchIndex"], out matchIndex)) {
                return BadRequest();
            }
            string matchNr;
            if (!context.Query.ContainsKey("MatchNr")) {
                return BadRequest();
            }
            matchNr = context.Query["MatchNr"];
            if (matchNr == null || matchNr.Length == 0) {
                return BadRequest();
            }
            var match = _mainVm.ActiveMatches.Where(m => m.MatchId.Value == matchNr).FirstOrDefault();
            if (match == null) {
                return FileNotFound();
            }
            var matchInfo = match.Matches.Where(c => c.Position == matchIndex).FirstOrDefault();
            if (matchInfo == null) {
                return FileNotFound();
            }

            if (context.Body == null || context.Body.Length == 0) {
                return BadRequest();
            }

            var setCount = matchInfo.Sets.Count;
            var result = DataSerializer.Deserialize<List<string>>(context.Body);
            if (result == null || result.Count != setCount * 2) {
                return BadRequest();
            }

            for (int i = 0; i < setCount * 2; i += 2) {
                matchInfo.Sets[i / 2].LeftScore.Value = result[i];
                matchInfo.Sets[i / 2].RightScore.Value = result[i + 1];
            }
            return NoContent();
        }

        public void Dispose() {
            _server?.Dispose();
        }

        public Cell<string?> BaseUri { get; private set; }
        public Cell<bool> IsActive { get; private set; }

        private MainWindowViewModel _mainVm;
        private WebServer? _server;
    }
}
