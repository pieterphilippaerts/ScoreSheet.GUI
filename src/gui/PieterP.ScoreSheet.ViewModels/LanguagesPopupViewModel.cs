using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.ViewModels.Settings;

namespace PieterP.ScoreSheet.ViewModels {
    public class LanguagesPopupViewModel {
        public LanguagesPopupViewModel() {
            this.Content = new LanguagesViewModel();
        }
        public object Content { get; private set; }
    }
}
