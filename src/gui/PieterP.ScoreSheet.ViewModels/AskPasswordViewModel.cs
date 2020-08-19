using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PieterP.ScoreSheet.ViewModels {
    public class AskPasswordViewModel {
        public AskPasswordViewModel() {
            this.OK = new CloseDialogCommand(true);
            this.Cancel = new CloseDialogCommand(false);
            this.IsOk = true;
            this.Remember = true;
        }

        public bool IsOk { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        
        public ICommand OK { get; private set; }
        public ICommand Cancel { get; private set; }
    }
}
