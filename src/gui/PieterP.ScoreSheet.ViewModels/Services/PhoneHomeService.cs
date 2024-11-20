using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Win32;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Information;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;
using PieterP.ScoreSheet.ViewModels.Services.Json;
using PieterP.ScoreSheet.Model.Interfaces;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class PhoneHomeService : IDisposable {
        public PhoneHomeService(MainWindowViewModel mainVm) {
            ServiceLocator.RegisterInstance<PhoneHomeService>(this);
            _mainVm = mainVm;
#if !DEBUG
            _timer = ServiceLocator.Resolve<ITimerService>();
            _timer.Tick += o => CallHome();
            _timer.Start(new TimeSpan(0, 5, 0));
            _isNetworkAvailable = ServiceLocator.Resolve<INetworkAvailabilityService>().IsNetworkAvailable;
            _isNetworkAvailable.ValueChanged += CallHome;
#endif
        }
        public async void CallHome() {
#if DEBUG
            return;
#endif
            if (_hasSent || _isNetworkAvailable.Value == false)
                return;
            try {
                var request = WebRequest.Create(CallbackUrl) as HttpWebRequest;
                if (request != null) {
                    request.Method = "POST";
                    request.AllowAutoRedirect = true;
                    var s = await request.GetRequestStreamAsync();
                    if (_infoBytes == null) {
                        string info = DataSerializer.Serialize(GetInfo());
                        _infoBytes = Encoding.UTF8.GetBytes(info);
                    }
                    await s.WriteAsync(_infoBytes, 0, _infoBytes.Length);

                    var response = await request.GetResponseAsync() as HttpWebResponse;
                    if (response != null) {
                        if (((int)response.StatusCode / 100) != 2) {
                            Logger.Log(LogType.Debug, $"Server replied with response { response.StatusDescription } ({ response.StatusCode })");
                        }
                    }
                    _timer?.Stop();
                    _timer = null;
                    _hasSent = true;
                }
            } catch (Exception e) {
                /* we never fail */
                Logger.Log(LogType.Debug, e.ToString());
            }
        }
        private TrackBackInfo GetInfo() {
            var s = DatabaseManager.Current.Settings;
            var info = new TrackBackInfo();
            info.UniqueId = s.UniqueId.Value;
            info.SuccessfulUploads = s.SuccessfulUploads.Value;
            info.LatestUploadClubId = s.LatestUploadClubId.Value;
            info.ClubId = s.HomeClubId.Value;
            info.AppVersion = Application.Version.ToString(3);
            info.OsVersion = Model.Information.OperatingSystem.Version.ToString();
            info.OsVersionString = $"{ Model.Information.OperatingSystem.Name } ({ Model.Information.OperatingSystem.ServicePack })";
            info.Architecture = Model.Information.OperatingSystem.Is64bit ? "x64" : "x86";
            info.CpuType = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            info.ScreenResolution = $"{ Screen.PrimaryScreen.Bounds.Width }x{ Screen.PrimaryScreen.Bounds.Height }";
            info.HasMultipleScreens = Screen.AllScreens.Count > 1;
            //info.Drives = PieterP.ScoreSheet.Model.Information.System.Drives;
            info.MemorySize = PieterP.ScoreSheet.Model.Information.System.TotalMemory;
            info.Runtimes = Runtime.GetRuntimeInformation();
            info.ActiveCulture = s.ActiveCulture.Value;
            info.PrintsDirectly = s.PrintDirect.Value;
            info.SecondScreenEnabled = s.EnableSecondScreen.Value;
            info.LiveUpdatesEnabled = s.EnableLiveUpdates.Value;
            info.FollowAwayEnabled = s.FollowAway.Value;
            info.WebServiceEnabled = s.EnableJsonService.Value;
            info.Theme = s.ThemePath.Value;
            info.AutoUploadEnabled = s.EnableAutoUpload.Value;
            info.ShowWatermark = s.ShowWatermark.Value;
            info.OverviewVisualization = s.OverviewVisualization.Value.ToString();
            info.SecondScreenVisualization = s.SecondScreenVisualization.Value.ToString();
            return info;
        }
        public void Dispose() {
            _timer?.Stop();
            _timer = null;
        }
        private MainWindowViewModel _mainVm;
        private ITimerService? _timer;
        private Cell<bool> _isNetworkAvailable;
        private bool _hasSent = false;
        private byte[]? _infoBytes;
        private const string CallbackUrl = "https://score.pieterp.be/TrackBack/Register";
    }    
}