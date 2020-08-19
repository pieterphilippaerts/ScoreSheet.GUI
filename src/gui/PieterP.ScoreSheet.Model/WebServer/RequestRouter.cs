#if NETSTANDARD
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PieterP.ScoreSheet.Model.Information;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Model.WebServer {
    public class RequestRouter : IRouter {
        public RequestRouter(RequestDelegate route) {
            _route = route;
        }
        public VirtualPathData GetVirtualPath(VirtualPathContext context) {
            return null!;
        }
        public async Task RouteAsync(RouteContext context) {
            context.Handler = _route;
            return;
        }
        private RequestDelegate _route;
    }
}
#endif