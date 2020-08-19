using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Model.Database.Enums;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.ScoreSheet.ViewModels.Services;
using PieterP.Shared.Cells;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Information {
    public class InformationOrchestratorViewModel : IDisposable {
        public InformationOrchestratorViewModel(Cell<ScoreVisualizations> layoutType, ObservableCollection<CompetitiveMatchViewModel> activeMatches, ObservableCollection<AwayMatchInfo> awayMatches) {
            _layoutType = layoutType;
            _activeMatches = activeMatches;
            _awayMatches = awayMatches;
            this.Content = Cell.Create<ICanBeOrchestrated?>(null);

            ((INotifyPropertyChanged)_layoutType).PropertyChanged += OnLayoutTypeChanged;
            OnLayoutTypeChanged(null, null);
            
            _timer = ServiceLocator.Resolve<ITimerService>();
            _timer.Tick += t => OnTick();
            _timer.Start(new TimeSpan(0, 0, 10));

            _activeMatches.CollectionChanged += MatchCollectionChanged;
            _awayMatches.CollectionChanged += MatchCollectionChanged;
        }

        private void MatchCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnTick();
        }

        private void OnLayoutTypeChanged(object? sender, PropertyChangedEventArgs? e) {
            ICanBeOrchestrated? newVm;
            switch (_layoutType.Value) {
                case ScoreVisualizations.Compact:
                    newVm = new MatchesOverviewViewModel(_activeMatches, _awayMatches);
                    break;
                default:
                    newVm = new DetailedOverviewViewModel(_activeMatches, _awayMatches);
                    break;
            }
            var oldVm = Content.Value;
            if (oldVm != null)
                oldVm.Dispose();
            Content.Value = newVm;
        }
        private void OnTick() {
            if (Content.Value != null)
                Content.Value.UpdateScreen();
        }

        public Cell<ICanBeOrchestrated?> Content { get; }

        public void Dispose() {
            ((INotifyPropertyChanged)_layoutType).PropertyChanged -= OnLayoutTypeChanged;
            _activeMatches.CollectionChanged -= MatchCollectionChanged;
            _awayMatches.CollectionChanged -= MatchCollectionChanged;
            if (Content.Value != null)
                Content.Value.Dispose();
            _timer.Stop();
        }

        private Cell<ScoreVisualizations> _layoutType;
        private ObservableCollection<CompetitiveMatchViewModel> _activeMatches;
        private ObservableCollection<AwayMatchInfo> _awayMatches;
        private ITimerService _timer;
    }
}
