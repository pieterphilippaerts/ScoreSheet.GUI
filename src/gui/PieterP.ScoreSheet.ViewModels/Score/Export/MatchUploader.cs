using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Interfaces;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Score.Export {
    public class MatchUploader {
        public MatchUploader(bool showUi) {
            _showUi = showUi;
        }
        public async Task<(TabTErrorCode, IEnumerable<string>)> Upload(params CompetitiveMatchViewModel[] matches) {
            var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
            var connectorResult = await connectorFactory.Create(showUi: _showUi);
            var connector = connectorResult.Connector;
            if (connector == null) {
                return (connectorResult.ErrorCode, new string[] {
                    connectorResult.ErrorCode == TabTErrorCode.InvalidCredentials ? Errors.MatchUploader_NoCredentials : Errors.MatchUploader_GeneralError
                });
            }

            // this is an ideal time, because we are probably connected to the internet
            ServiceLocator.Resolve<INetworkAvailabilityService>().TriggerManually();

            var exporter = new CsvExporter();
            (var errorCode, var errors) = await connector.UploadAsync(exporter.Export(matches));
            switch (errorCode) {
                case TabTErrorCode.NetworkError:
                case TabTErrorCode.InvalidCredentials:
                case TabTErrorCode.DataError:
                    foreach (var m in matches) {
                        m.UploadStatus.Value = UploadStatus.Failed;
                    }
                    break;
                case TabTErrorCode.NoError:
                    foreach (var m in matches) {
                        m.Dirty.Value = false;
                        m.UploadStatus.Value = UploadStatus.Uploaded;
                    }
                    var settings = DatabaseManager.Current.Settings;
                    if (settings.LatestUploadClubId.Value == null || settings.LatestUploadClubId.Value == settings.HomeClubId.Value) {
                        settings.SuccessfulUploads.Value = settings.SuccessfulUploads.Value + 1;
                    } else {
                        // user changed clubs; reset upload count
                        settings.SuccessfulUploads.Value = 1;
                    }
                    settings.LatestUploadClubId.Value = settings.HomeClubId.Value;
                    break;
            }
            return (errorCode, errors);
        }

        private bool _showUi;
    }
}
