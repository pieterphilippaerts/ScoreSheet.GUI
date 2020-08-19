using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using PieterP.ScoreSheet.ViewModels.Templates;

namespace PieterP.ScoreSheet.GUI.Converters {
    public class AfttTemplateSelector : DataTemplateSelector {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element != null && item != null) {
                switch (item) { 
                    case OfficialSinglePlayerInfo _:
                        return App.Current.FindResource("AfttSinglePlayer") as DataTemplate;
                    case OfficialDoublePlayerInfo _:
                        return App.Current.FindResource("AfttDoublePlayer") as DataTemplate;
                }
            }
            return null;
        }
    }
}
