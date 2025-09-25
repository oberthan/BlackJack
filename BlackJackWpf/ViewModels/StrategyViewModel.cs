using System.Collections.ObjectModel;
using System.ComponentModel;
using Blackjack;

namespace BlackjackWpf.ViewModels
{
    public class StrategyViewModel : INotifyPropertyChanged
    {
        private StrategyManager _strategyManager;
        private double _selectedLocalUnit;

        public ObservableCollection<double> AvailableLocalUnits { get; set; }
        public ObservableCollection<PairStrategyRow> PairStrategy { get; set; }
        public ObservableCollection<SoftStrategyRow> SoftStrategy { get; set; }
        public ObservableCollection<HardStrategyRow> HardStrategy { get; set; }

        public double SelectedLocalUnit
        {
            get => _selectedLocalUnit;
            set
            {
                if (_selectedLocalUnit != value)
                {
                    _selectedLocalUnit = value;
                    OnPropertyChanged(nameof(SelectedLocalUnit));
                    LoadStrategyForSelectedUnit();
                }
            }
        }

        public StrategyViewModel(StrategyManager strategyManager)
        {
            _strategyManager = strategyManager;
            AvailableLocalUnits = new ObservableCollection<double>(_strategyManager.GetLocalUnits());

            // Set default selection
            if (AvailableLocalUnits.Contains(0.0))
            {
                _selectedLocalUnit = 0.0;
            }
            else if (AvailableLocalUnits.Any())
            {
                _selectedLocalUnit = AvailableLocalUnits.First();
            }

            LoadStrategyForSelectedUnit();
        }

        public StrategyViewModel()
        {
            // Assuming Strategy.Instance.PairStrategy, SoftStrategy, HardStrategy are IEnumerable<T>
            PairStrategy = new ObservableCollection<PairStrategyRow>(Strategy.Instance.PairStrategy.OrderByDescending(x => (int)x.Pair));
            SoftStrategy = new ObservableCollection<SoftStrategyRow>(Strategy.Instance.SoftStrategy.OrderByDescending(x => x.Total));
            HardStrategy = new ObservableCollection<HardStrategyRow>(Strategy.Instance.HardStrategy.OrderByDescending(x => x.Total));
        }

        private void LoadStrategyForSelectedUnit()
        {
            var strategy = _strategyManager.GetStrategy(_selectedLocalUnit);
            PairStrategy = new ObservableCollection<PairStrategyRow>(strategy.PairStrategy.OrderByDescending(x => (int)x.Pair));
            SoftStrategy = new ObservableCollection<SoftStrategyRow>(strategy.SoftStrategy.OrderByDescending(x => x.Total));
            HardStrategy = new ObservableCollection<HardStrategyRow>(strategy.HardStrategy.OrderByDescending(x => x.Total));
            OnPropertyChanged(nameof(PairStrategy));
            OnPropertyChanged(nameof(SoftStrategy));
            OnPropertyChanged(nameof(HardStrategy));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
