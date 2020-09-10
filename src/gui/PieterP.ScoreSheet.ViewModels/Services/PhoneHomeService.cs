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

namespace PieterP.ScoreSheet.ViewModels.Services {
    public class PhoneHomeService : IDisposable {
        public PhoneHomeService(MainWindowViewModel mainVm) {
            ServiceLocator.RegisterInstance<PhoneHomeService>(this);
            _mainVm = mainVm;
#if !DEBUG
            _timer = ServiceLocator.Resolve<ITimerService>();
            _timer.Tick += o => CallHome();
            _timer.Start(new TimeSpan(0, 5, 0));
            ServiceLocator.Resolve<INetworkAvailabilityService>().NetworkAvailable += CallHome;
#endif 
        }
        public async void CallHome() {
#if DEBUG
            return;
#endif
            if (_hasSent)
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
            var info = new TrackBackInfo();
            info.UniqueId = DatabaseManager.Current.Settings.UniqueId.Value;
            info.SuccessfulUploads = DatabaseManager.Current.Settings.SuccessfulUploads.Value;
            info.LatestUploadClubId = DatabaseManager.Current.Settings.LatestUploadClubId.Value;
            info.ClubId = DatabaseManager.Current.Settings.HomeClubId.Value;
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
            info.ActiveCulture = DatabaseManager.Current.Settings.ActiveCulture.Value;
            info.PrintsDirectly = DatabaseManager.Current.Settings.PrintDirect.Value;
            info.SecondScreenEnabled = DatabaseManager.Current.Settings.EnableSecondScreen.Value;
            info.LiveUpdatesEnabled = DatabaseManager.Current.Settings.EnableLiveUpdates.Value;
            info.FollowAwayEnabled = DatabaseManager.Current.Settings.FollowAway.Value;
            info.WebServiceEnabled = DatabaseManager.Current.Settings.EnableJsonService.Value;
            return info;
        }
        public void Dispose() {
            _timer?.Stop();
            _timer = null;
        }
        private MainWindowViewModel _mainVm;
        private ITimerService? _timer;
        private bool _hasSent = false;
        private byte[]? _infoBytes;
        private const string CallbackUrl = "https://score.pieterp.be/TrackBack/Register";
    }    
}