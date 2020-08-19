using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Information;
using PieterP.Shared;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.Model.WebServer {
    using System.Collections.Specialized;
    using System.Linq;
#if NETSTANDARD
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.DependencyInjection;

    public class WebServer : IDisposable, IStartup {
        public WebServer(Uri baseUri) {
            this.Started = Cell.Create(false);
            _routes = new Dictionary<string, Dictionary<HttpMethods, Func<RoutingContext, RouteResult>>>();
            _baseUri = baseUri;
            _host = new WebHostBuilder()
              .ConfigureServices(services =>
              {
                  services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(typeof(IStartup), this));
              })
              .UseKestrel(OnConfigure)
              .UseWebRoot(DatabaseManager.Current.WwwPath)
              .Build();
        }
        public async Task Start() {
            try {
                this.Started.Value = true;
                await _host.RunAsync();
                this.Started.Value = false;
            } catch (Exception e) {
                this.Started.Value = false;
                Logger.Log(e);
            }
        }
        private void OnConfigure(KestrelServerOptions options) {
            IPAddress[] ips;
            if (_baseUri.Host == "0.0.0.0" || _baseUri.Host == "::0") {
                ips = new IPAddress[] { IPAddress.Any };
            } else {
                ips = Dns.GetHostAddresses(_baseUri.Host);
                if (ips == null || ips.Length == 0)
                    return;
            }
            options.Listen(ips[0], _baseUri.Port /*, listenOptions => listenOptions.UseHttps( ... )*/ );
        }

        private Task? OnRoute(HttpContext context) {
            try {
                AddHeaders(context.Response.Headers);
                string path = context.Request.Path.Value;
                var pathUri = new Uri(_baseUri, path.ToLower());
                if (pathUri.AbsolutePath == "/" || pathUri.AbsolutePath == "/index.htm" || pathUri.AbsolutePath == "/index.html") {
                    return SendIndex(context);
                }
                Dictionary<HttpMethods, Func<RoutingContext, RouteResult>> callbackDict;
                if (_routes.TryGetValue(pathUri.AbsolutePath, out callbackDict)) {
                    if (callbackDict.TryGetValue(VerbToMethod(context.Request.Method), out var callback))
                        return ProcessCallback(callback, context);
                    else if (callbackDict.TryGetValue(HttpMethods.Any, out var callback2))
                        return ProcessCallback(callback2, context);
                }

                if (VerbToMethod(context.Request.Method) == HttpMethods.Get) {
                    var file = Path.Combine(DatabaseManager.Current.WwwPath, path);
                    if (File.Exists(file)) {
                        return SendFile(context, file);
                    }
                }
                return Send404(context);
            } catch (Exception e) {
                return Send500(context, e.ToString());
            }
        }
        private Task ProcessCallback(Func<RoutingContext, RouteResult> callback, HttpContext context) {
            var rc = new RoutingContext();
            using (var sr = new StreamReader(context.Request.Body)) {
                rc.Body = sr.ReadToEnd();
            }
            rc.Query = new Dictionary<string, string>();
            foreach (var p in context.Request.Query) {
                rc.Query[p.Key] = p.Value.FirstOrDefault();
            }
            var result = callback(rc);
            byte[] data;
            if (result.Result == null) {
                data = new byte[0];
            } else {
                data = Encoding.UTF8.GetBytes(result.Result);
            }
            return SendReply(context, result.StatusCode, data, result.ContentType);
        }
        public Task Send500(HttpContext context, string error) {
            var page = GetResourceString("PieterP.ScoreSheet.Model.WebServer.500.html");
            page = page.Replace("<pre></pre>", $"<pre>{ WebUtility.HtmlEncode(error) }</pre>");
            return SendReply(context, 500, Encoding.UTF8.GetBytes(page));
        }
        public Task Send404(HttpContext context) {
            return SendReply(context, 404, GetResourceBytes("PieterP.ScoreSheet.Model.WebServer.404.html"));
        }
        public Task Send400(HttpContext context) {
            return SendReply(context, 400, GetResourceBytes("PieterP.ScoreSheet.Model.WebServer.400.html"));
        }
        public Task SendIndex(HttpContext context) {
            var page = GetResourceString("PieterP.ScoreSheet.Model.WebServer.Index.html");
            page = page.Replace("$WWWDIR$", DatabaseManager.Current.WwwPath);
            return SendReply(context, 200, Encoding.UTF8.GetBytes(page));
        }
        private string GetResourceString(string name) {
            return Encoding.UTF8.GetString(GetResourceBytes(name));
        }
        private byte[] GetResourceBytes(string name) {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(name)) {
                var bytes = new byte[stream.Length];
                int read = stream.Read(bytes, 0, bytes.Length);
                while (read < bytes.Length) {
                    int r = stream.Read(bytes, read, bytes.Length - read);
                    if (r <= 0)
                        break;
                    read += r;
                }
                return bytes;
            }
        }
        private async Task SendReply(HttpContext context, int code, byte[] data, string contentType = "text/html") {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = contentType;
            response.ContentLength = data.Length;
            await response.Body.WriteAsync(data, 0, data.Length);
            await response.Body.FlushAsync();
            response.Body.Close();
        }
        private void AddHeaders(IHeaderDictionary headers) {
            headers["X-Powered-By"] = _poweredBy;
            headers["Access-Control-Allow-Origin"] = "*";
        }
        private HttpMethods VerbToMethod(string verb) {
            switch (verb) {
                case "GET":
                    return HttpMethods.Get;
                case "POST":
                    return HttpMethods.Post;
                case "PUT":
                    return HttpMethods.Put;
                default:
                    return HttpMethods.Any;
            }
        }
        private async Task SendFile(HttpContext context, string localFile) {
            var fi = new FileInfo(localFile);
            var response = context.Response;
            response.StatusCode = 200;
            response.ContentType = MimeTypeMap.GetMimeType(fi.Extension);
            response.ContentLength = fi.Length;
            using (var file = fi.OpenRead()) {
                await file.CopyToAsync(response.Body);
                await response.Body.FlushAsync();
                response.Body.Close();
            }
        }

        /// <param name="path">Must be lower case, without URL variables</param>
        /// <param name="callback">If the callback is null, the path is removed from the list of registered paths</param>
        public void RegisterPath(string path, Func<RoutingContext, RouteResult> callback, HttpMethods verb = HttpMethods.Any) {
            if (callback == null) {
                if (_routes.ContainsKey(path) ) {
                    if (verb == HttpMethods.Any) {
                        _routes.Remove(path);
                    } else {
                        var pd = _routes[path];
                        if (pd.ContainsKey(verb))
                            pd.Remove(verb);
                    }
                }
            } else {
                Dictionary<HttpMethods, Func<RoutingContext, RouteResult>> callbackDict;
                if (!_routes.TryGetValue(path, out callbackDict)) {
                    callbackDict = new Dictionary<HttpMethods, Func<RoutingContext, RouteResult>>();
                    _routes[path] = callbackDict;
                }
                callbackDict[verb] = callback;
            }
        }

        public void Dispose() {
            _host.Dispose();
            this.Started.Value = false;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services) {
            services.AddMvcCore();
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app) {
            _router = new RequestRouter(OnRoute);            
            app.UseStaticFiles();
            app.UseRouter(_router);
        }

        public Cell<bool> Started { get; private set; }

        private IWebHost _host;
        private RequestRouter? _router;
        private Dictionary<string, Dictionary<HttpMethods, Func<RoutingContext, RouteResult>>> _routes;
        private Uri _baseUri;
        private static string _poweredBy = "ScoreSheet-" + Application.Version.ToString(3);
    }
#else
    using System.Web;
    public class WebServer : IDisposable {
        public WebServer(string baseUri) {
            this.Started = Cell.Create(false);
            _routes = new Dictionary<string, Dictionary<HttpMethods, Func<RoutingContext, RouteResult>>>();
            _baseUri = baseUri;
        }
        public async Task Start() {
            _listener = new HttpListener();
            _listener.Prefixes.Clear();
            _listener.Prefixes.Add(_baseUri);
            try {
                _listener.Start();
                this.Started.Value = true;
                while (true) {
                    var context = await _listener.GetContextAsync();
                    if (_listener != null && context != null)
                        await OnRoute(context);
                    else
                        break;
                }
                this.Started.Value = false;
            } catch (Exception e) {
                Logger.Log(e);
                _listener = null;
            }
            return;
        }

        private Task OnRoute(HttpListenerContext context) {
            try {
                AddHeaders(context.Response.Headers);
                var absolutePath = context.Request.Url.AbsolutePath.ToLower();
                if (absolutePath == "/" || absolutePath == "/index.htm" || absolutePath == "/index.html") {
                    return SendIndex(context);
                }
                Dictionary<HttpMethods, Func<RoutingContext, RouteResult>> callbackDict;
                if (_routes.TryGetValue(absolutePath, out callbackDict)) {
                    if (callbackDict.TryGetValue(VerbToMethod(context.Request.HttpMethod), out var callback))
                        return ProcessCallback(callback, context);
                    else if (callbackDict.TryGetValue(HttpMethods.Any, out var callback2))
                        return ProcessCallback(callback2, context);
                }

                if (VerbToMethod(context.Request.HttpMethod) == HttpMethods.Get) {
                    var file = Path.Combine(DatabaseManager.Current.WwwPath, context.Request.Url.AbsolutePath.TrimStart('/', '\\'));
                    if (File.Exists(file)) {
                        return SendFile(context, file);
                    }
                }
                return Send404(context);
            } catch (Exception e) {
                return Send500(context, e.ToString());
            }
        }
        private Task ProcessCallback(Func<RoutingContext, RouteResult> callback, HttpListenerContext context) {
            var rc = new RoutingContext();
            using (var sr = new StreamReader(context.Request.InputStream)) {
                rc.Body = sr.ReadToEnd();
            }
            rc.Query = new Dictionary<string, string>();
            foreach (var p in context.Request.QueryString.AllKeys) {
                rc.Query[p] = context.Request.QueryString[p];
            }
            var result = callback(rc);
            byte[] data;
            if (result.Result == null) {
                data = new byte[0];
            } else {
                data = Encoding.UTF8.GetBytes(result.Result);
            }
            return SendReply(context, result.StatusCode, data, result.ContentType);
        }
        private void AddHeaders(NameValueCollection headers) {
            headers["X-Powered-By"] = _poweredBy;
            headers["Access-Control-Allow-Origin"] = "*";
        }
        private HttpMethods VerbToMethod(string verb) {
            switch (verb) {
                case "GET":
                    return HttpMethods.Get;
                case "POST":
                    return HttpMethods.Post;
                case "PUT":
                    return HttpMethods.Put;
                default:
                    return HttpMethods.Any;
            }
        }
        private async Task SendFile(HttpListenerContext context, string localFile) {
            var fi = new FileInfo(localFile);
            var response = context.Response;
            response.StatusCode = 200;
            response.ContentType = MimeTypeMap.GetMimeType(fi.Extension);
            using (var file = fi.OpenRead()) {
                await file.CopyToAsync(response.OutputStream);
                await response.OutputStream.FlushAsync();
                response.OutputStream.Close();
            }
        }
        public Task Send500(HttpListenerContext context, string error) {
            var page = GetResourceString("PieterP.ScoreSheet.Model.WebServer.500.html");
            page = page.Replace("<pre></pre>", $"<pre>{ WebUtility.HtmlEncode(error) }</pre>");
            return SendReply(context, 500, Encoding.UTF8.GetBytes(page));
        }
        public Task Send404(HttpListenerContext context) {
            return SendReply(context, 404, GetResourceBytes("PieterP.ScoreSheet.Model.WebServer.404.html"));
        }
        public Task Send400(HttpListenerContext context) {
            return SendReply(context, 400, GetResourceBytes("PieterP.ScoreSheet.Model.WebServer.400.html"));
        }
        public Task SendIndex(HttpListenerContext context) {
            var page = GetResourceString("PieterP.ScoreSheet.Model.WebServer.Index.html");
            page = page.Replace("$WWWDIR$", DatabaseManager.Current.WwwPath);
            return SendReply(context, 200, Encoding.UTF8.GetBytes(page));
        }
        private string GetResourceString(string name) {
            return Encoding.UTF8.GetString(GetResourceBytes(name));
        }
        private byte[] GetResourceBytes(string name) {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(name)) {
                var bytes = new byte[stream.Length];
                int read = stream.Read(bytes, 0, bytes.Length);
                while (read < bytes.Length) {
                    int r = stream.Read(bytes, read, bytes.Length - read);
                    if (r <= 0)
                        break;
                    read += r;
                }
                return bytes;
            }
        }
        private async Task SendReply(HttpListenerContext context, int code, byte[] data, string contentType = "text/html") {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = contentType;
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            await response.OutputStream.FlushAsync();
            response.OutputStream.Close();
        }
        public void RegisterPath(string path, Func<RoutingContext, RouteResult> callback, HttpMethods verb = HttpMethods.Any) {
            if (callback == null) {
                if (_routes.ContainsKey(path)) {
                    if (verb == HttpMethods.Any) {
                        _routes.Remove(path);
                    } else {
                        var pd = _routes[path];
                        if (pd.ContainsKey(verb))
                            pd.Remove(verb);
                    }
                }
            } else {
                Dictionary<HttpMethods, Func<RoutingContext, RouteResult>> callbackDict;
                if (!_routes.TryGetValue(path, out callbackDict)) {
                    callbackDict = new Dictionary<HttpMethods, Func<RoutingContext, RouteResult>>();
                    _routes[path] = callbackDict;
                }
                callbackDict[verb] = callback;
            }
        }

        public void Dispose() {
            var l = _listener;
            _listener = null;
            l?.Stop();
        }

        public Cell<bool> Started { get; private set; }

        private HttpListener? _listener;
        private Dictionary<string, Dictionary<HttpMethods, Func<RoutingContext, RouteResult>>> _routes;
        private string _baseUri;
        private static string _poweredBy = "ScoreSheet-" + Application.Version.ToString(3);
    }
#endif
    public class RoutingContext { 
        public IDictionary<string, string> Query { get; set; }
        public string Body { get; set; }
    }
    public class RouteResult { 
        public int StatusCode { get; set; }
        public string Result { get; set; }
        public string ContentType { get; set; }
    }
}
