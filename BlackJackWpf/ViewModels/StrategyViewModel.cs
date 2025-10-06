using System.Collections.ObjectModel;
using System.ComponentModel;
using Blackjack;
using System.Linq;

namespace BlackjackWpf.ViewModels
{
    public class StrategyViewModel : INotifyPropertyChanged
    {
        private StrategyManager _strategyManager;


        public ObservableCollection<StrategyDisplay> Strategies { get; set; }
        public ObservableCollection<double> AvailableLocalUnits { get; set; }

        private double _selectedLocalUnit;
        public double SelectedLocalUnit
        {
            get => _selectedLocalUnit;
            set
            {
                if (_selectedLocalUnit != value)
                {
                    _selectedLocalUnit = value;
                    OnPropertyChanged(nameof(SelectedLocalUnit));
                    UpdateSelectedStrategy();
                }
            }
        }
        private ObservableCollection<PairStrategyRow> _pairStrategy = new();
        public ObservableCollection<PairStrategyRow> PairStrategy
        {
            get => _pairStrategy;
            set { _pairStrategy = value; OnPropertyChanged(nameof(PairStrategy)); }
        }

        private ObservableCollection<SoftStrategyRow> _softStrategy = new();
        public ObservableCollection<SoftStrategyRow> SoftStrategy
        {
            get => _softStrategy;
            set { _softStrategy = value; OnPropertyChanged(nameof(SoftStrategy)); }
        }

        private ObservableCollection<HardStrategyRow> _hardStrategy = new();
        public ObservableCollection<HardStrategyRow> HardStrategy
        {
            get => _hardStrategy;
            set { _hardStrategy = value; OnPropertyChanged(nameof(HardStrategy)); }
        }
        // --- Constructor for runtime (with StrategyManager) ---
        public StrategyViewModel(StrategyManager strategyManager)
        {
            _strategyManager = strategyManager ?? throw new ArgumentNullException(nameof(strategyManager));

            var allStrategies = _strategyManager.GetAllStrategies();

            Strategies = new ObservableCollection<StrategyDisplay>(
                allStrategies.Select(kvp => new StrategyDisplay
                {
                    LocalUnit = kvp.Key,
                    PairStrategy = new ObservableCollection<PairStrategyRow>(
                        kvp.Value.PairStrategy.OrderByDescending(x => (int)x.Pair)),
                    SoftStrategy = new ObservableCollection<SoftStrategyRow>(
                        kvp.Value.SoftStrategy.OrderByDescending(x => x.Total)),
                    HardStrategy = new ObservableCollection<HardStrategyRow>(
                        kvp.Value.HardStrategy.OrderByDescending(x => x.Total))
                })
            );

            AvailableLocalUnits = new ObservableCollection<double>(allStrategies.Keys);
            SelectedLocalUnit = AvailableLocalUnits.FirstOrDefault();
            UpdateSelectedStrategy();
        }


        // --- Constructor for design-time preview ---
        public StrategyViewModel()
        {
            var strategy = Strategy.Instance;
            Strategies = new ObservableCollection<StrategyDisplay>
            {
                new StrategyDisplay
                {
                    LocalUnit = 0.0,
                    PairStrategy = new ObservableCollection<PairStrategyRow>(
                        strategy.PairStrategy.OrderByDescending(x => (int)x.Pair)),
                    SoftStrategy = new ObservableCollection<SoftStrategyRow>(
                        strategy.SoftStrategy.OrderByDescending(x => x.Total)),
                    HardStrategy = new ObservableCollection<HardStrategyRow>(
                        strategy.HardStrategy.OrderByDescending(x => x.Total))
                }
            };

            AvailableLocalUnits = new ObservableCollection<double> { 0.0 };
            SelectedLocalUnit = 0.0;
            UpdateSelectedStrategy();
        }

        private void UpdateSelectedStrategy()
        {
            var strategy = Strategies.FirstOrDefault(s => s.LocalUnit == SelectedLocalUnit);
            if (strategy != null)
            {
                PairStrategy = strategy.PairStrategy;
                SoftStrategy = strategy.SoftStrategy;
                HardStrategy = strategy.HardStrategy;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class StrategyDisplay
    {
        public double LocalUnit { get; set; }
        public ObservableCollection<PairStrategyRow> PairStrategy { get; set; }
        public ObservableCollection<SoftStrategyRow> SoftStrategy { get; set; }
        public ObservableCollection<HardStrategyRow> HardStrategy { get; set; }
    }
}
