using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Notifications;
using PieterP.Shared.Cells;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Settings {
    public class LayoutViewModel {
        public LayoutViewModel() {
            this.AvailableThemes = new ThemeInfo[] {
                new ThemeInfo() { Name=Strings.Layout_Default, Path="/Themes/DefaultTheme.xaml" },
                new ThemeInfo() { Name=Strings.Layout_HighContrast, Path="/Themes/ContrastTheme.xaml" },
                new ThemeInfo() { Name=Strings.Layout_PieterP, Path="/Themes/PieterPTheme.xaml" },
                new ThemeInfo() { Name=Strings.Layout_Yellow, Path="/Themes/YellowTheme.xaml" },
                new ThemeInfo() { Name=Strings.Layout_Custom, Path="" }
            };
            this.SelectedTheme = DatabaseManager.Current.Settings.ThemePath;

            this.SelectedBackgroundColor = DatabaseManager.Current.Settings.SelectedBackgroundColor;
            this.SelectedTextBoxColor = DatabaseManager.Current.Settings.SelectedTextBoxColor;
            this.SelectedTextColor = DatabaseManager.Current.Settings.SelectedTextColor;
            this.SelectedErrorBackgroundColor = DatabaseManager.Current.Settings.SelectedErrorBackgroundColor;
            this.SelectedErrorTextColor = DatabaseManager.Current.Settings.SelectedErrorTextColor;
            
            this.SelectBackgroundColor = new SelectColorCommand(this.SelectedBackgroundColor);
            this.SelectTextBoxColor = new SelectColorCommand(this.SelectedTextBoxColor);
            this.SelectTextColor = new SelectColorCommand(this.SelectedTextColor);
            this.SelectErrorBackgroundColor = new SelectColorCommand(this.SelectedErrorBackgroundColor);
            this.SelectErrorTextColor = new SelectColorCommand(this.SelectedErrorTextColor);
        }
        public IEnumerable<ThemeInfo> AvailableThemes { get; private set; }
        public Cell<string> SelectedTheme { get; private set; }

        public Cell<string> SelectedBackgroundColor { get; private set; }
        public Cell<string> SelectedTextBoxColor { get; private set; }
        public Cell<string> SelectedTextColor { get; private set; }
        public Cell<string> SelectedErrorBackgroundColor { get; private set; }
        public Cell<string> SelectedErrorTextColor { get; private set; }

        public ICommand SelectBackgroundColor { get; private set; }
        public ICommand SelectTextBoxColor { get; private set; }
        public ICommand SelectTextColor { get; private set; }
        public ICommand SelectErrorBackgroundColor { get; private set; }
        public ICommand SelectErrorTextColor { get; private set; }

        private class SelectColorCommand : ICommand {
            public event EventHandler? CanExecuteChanged;

            public SelectColorCommand(Cell<string> result) {
                _result = result;
            }
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) {
                var scNot = new ColorDialogNotification();
                // grey values
                scNot.CustomColors.Add("#000000");
                scNot.CustomColors.Add("#242424");
                scNot.CustomColors.Add("#494949");
                scNot.CustomColors.Add("#6D6D6D");
                scNot.CustomColors.Add("#929292");
                scNot.CustomColors.Add("#B6B6B6");
                scNot.CustomColors.Add("#DADADA");
                scNot.CustomColors.Add("#FFFFFF");
                // ScoreSheet colors
                scNot.CustomColors.Add("#C2D69B");
                scNot.CustomColors.Add("#D1E5AA");
                scNot.CustomColors.Add("#365F91");
                scNot.CustomColors.Add("#FF0000");
                scNot.CustomColors.Add("#E5B8B7");
                scNot.CustomColors.Add("#FFFBC3");
                scNot.CustomColors.Add("#FFFDE8");
                scNot.CustomColors.Add("#6482A8");
                scNot.InitialColor = _result.Value;
                NotificationManager.Current.Raise(scNot);
                if (scNot.SelectedColor != null) {
                    _result.Value = scNot.SelectedColor;
                }
            }
            private Cell<string> _result;
        }
    }
    public class ThemeInfo { 
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
