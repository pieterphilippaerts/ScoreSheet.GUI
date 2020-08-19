using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.Model.Information;
using PieterP.Shared.Cells;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class SecondScreenViewModel {
        public SecondScreenViewModel() {
            this.EnableSecondScreen= DatabaseManager.Current.Settings.EnableSecondScreen;
            this.ChooseScreenAutomatically = DatabaseManager.Current.Settings.ChooseScreenAutomatically;
            this.ChooseScreenManually = DatabaseManager.Current.Settings.ChooseScreenManually;
            this.Screens = Screen.AllScreens;
            string? selectedDevice = DatabaseManager.Current.Settings.SelectedScreen.Value;
            if (selectedDevice == null || selectedDevice == "")
                selectedDevice = this.Screens.FirstOrDefault()?.DeviceName;
            this.SelectedScreen = Cell.Create(this.Screens.Where(s => s.DeviceName == selectedDevice).FirstOrDefault());
            this.SelectedScreen.ValueChanged += SaveSelectedScreen;
            SaveSelectedScreen(); 
        }
        private void SaveSelectedScreen() {
            DatabaseManager.Current.Settings.SelectedScreen.Value = this.SelectedScreen.Value?.DeviceName;
        }

        public Cell<bool> EnableSecondScreen { get; private set; }
        public Cell<bool> ChooseScreenAutomatically { get; private set; }
        public Cell<bool> ChooseScreenManually { get; private set; }
        public IList<Screen> Screens { get; private set; }
        public Cell<Screen> SelectedScreen { get; private set; }
    }
}