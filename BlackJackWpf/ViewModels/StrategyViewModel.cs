using System.Collections.ObjectModel;
using System.ComponentModel;
using Blackjack;

namespace BlackjackWpf.ViewModels
{
    public class StrategyViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<PairStrategyRow> PairStrategy { get; set; }
        public ObservableCollection<SoftStrategyRow> SoftStrategy { get; set; }
        public ObservableCollection<HardStrategyRow> HardStrategy { get; set; }

        public StrategyViewModel()
        {
            // Assuming Strategy.Instance.PairStrategy, SoftStrategy, HardStrategy are IEnumerable<T>
            PairStrategy = new ObservableCollection<PairStrategyRow>(Strategy.Instance.PairStrategy.OrderByDescending(x => (int)x.Pair));
            SoftStrategy = new ObservableCollection<SoftStrategyRow>(Strategy.Instance.SoftStrategy.OrderByDescending(x => x.Total));
            HardStrategy = new ObservableCollection<HardStrategyRow>(Strategy.Instance.HardStrategy.OrderByDescending(x => x.Total));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
