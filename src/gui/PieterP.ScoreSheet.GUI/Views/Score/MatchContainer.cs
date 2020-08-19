using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PieterP.ScoreSheet.GUI.Views.Score {
    public class MatchContainer : Panel {
        public MatchContainer() {
            _match = new CompetitiveMatch();
            var contextBinding = new Binding("ActiveMatch.Value");
            _match.SetBinding(CompetitiveMatch.DataContextProperty, contextBinding);
            this.InternalChildren.Add(_match);
        }
        protected override Size ArrangeOverride(Size finalSize) {
            _match.Arrange(new Rect(finalSize));
            return finalSize;

        }
        protected override Size MeasureOverride(Size availableSize) {
            _match.Measure(availableSize);
            return _match.DesiredSize;
        }
        private CompetitiveMatch _match;
    }
}