using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJack;

namespace BlackJackWpf.ViewModels
{
    public class StrategyViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<PairStrategyRow> PairStrategy { get; set; } = new();
        public ObservableCollection<SoftStrategyRow> SoftStrategy { get; set; } = new();
        public ObservableCollection<HardStrategyRow> HardStrategy { get; set; } = new();

        public StrategyViewModel()
        {
            // Assuming Strategy.Instance.PairStrategy, SoftStrategy, HardStrategy are IEnumerable<T>
            PairStrategy = new ObservableCollection<PairStrategyRow>(Strategy.Instance.PairStrategy.OrderByDescending(x => int.Parse(x.Pair.Split(',')[0])));
            SoftStrategy = new ObservableCollection<SoftStrategyRow>(Strategy.Instance.SoftStrategy.OrderByDescending(x => x.Total));
            HardStrategy = new ObservableCollection<HardStrategyRow>(Strategy.Instance.HardStrategy.OrderByDescending(x => x.Total));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
