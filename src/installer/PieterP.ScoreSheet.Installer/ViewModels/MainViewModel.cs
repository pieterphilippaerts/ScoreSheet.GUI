using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Installer.Models;

namespace PieterP.ScoreSheet.Installer.ViewModels {
    public class MainViewModel : INotifyPropertyChanged {
        public MainViewModel() {
            this.CurrentScreen = new LanguagesViewModel(this);
        }

        public object CurrentScreen { 
            get {
                return _currentScreen;
            }
            set {
                _currentScreen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentScreen)));
            }
        }
        private object _currentScreen;

        public event PropertyChangedEventHandler PropertyChanged;    
    }
}
