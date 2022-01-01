using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Connector;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Helpers {
    public class ConnectorFactory : IConnectorFactory {
        public async Task<(IConnector?, TabTErrorCode)> Create(bool allowAnonymous = false, bool showUi = true) {
            var connector = ServiceLocator.Resolve<IConnector>();
            bool isOk = false;
            var vm = new AskPasswordViewModel();
            vm.Username = DatabaseManager.Current.Settings.TabTUsername.Value;
            vm.Password = DatabaseManager.Current.Settings.TabTPassword.Value;
            vm.Remember = !(vm.Username == "" && vm.Password == "");

            if (allowAnonymous && vm.Username == "" && vm.Password == "") {
                isOk = true;
            } else if (vm.Username != "" && vm.Password != "") {
                connector.SetDefaultCredentials(vm.Username, vm.Password);
                var result = await connector.TestAsync();
                isOk = result.ErrorCode == TabTErrorCode.NoError;
                vm.IsOk = isOk;
                if (!isOk && result.ErrorCode != TabTErrorCode.InvalidCredentials) // if we can't test the credentials (e.g., network down), return immediately
                    return (null, result.ErrorCode);
            }
            while (!isOk && showUi) {
                var not = new ShowDialogNotification(vm);
                NotificationManager.Current.Raise(not);
                if (!not.Result)
                    return (null, TabTErrorCode.InvalidCredentials); // user declined to enter a valid username/password combination
                connector.SetDefaultCredentials(vm.Username, vm.Password);
                if ((vm.Username == null || vm.Username == "") && (vm.Password == null || vm.Password == "") && allowAnonymous) {
                    isOk = true;
                } else if (vm.Username  != null && vm.Username != "" && vm.Password != null && vm.Password != "") {
                    var result = await connector.TestAsync();
                    isOk = result.ErrorCode == TabTErrorCode.NoError;
                    if (!isOk && result.ErrorCode != TabTErrorCode.InvalidCredentials) // if we can't test the credentials (e.g., network down), return immediately
                        return (null, result.ErrorCode);
                }
                vm.IsOk = false;
            }
            if (isOk && vm.Remember) {
                DatabaseManager.Current.Settings.TabTUsername.Value = vm.Username ?? "";
                DatabaseManager.Current.Settings.TabTPassword.Value = vm.Password ?? "";
            }
            if (isOk)
                return (connector, TabTErrorCode.NoError);
            return (null, TabTErrorCode.InvalidCredentials); // invalid credentials, and we can't show a UI to ask the user
        }
    }
}
