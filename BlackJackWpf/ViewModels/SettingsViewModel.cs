using Blackjack;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlackjackWpf.ViewModels
{
    public class SettingsViewModel
    {
        public bool DealerStandsOnSoft17
        {
            get => _dealerStandsOnSoft17;
            set { _dealerStandsOnSoft17 = value; OnPropertyChanged(nameof(DealerStandsOnSoft17)); }
        }
        private bool _dealerStandsOnSoft17 = Rules.Instance.DealerStandsOnSoft17;

        public double BlackjackPayout
        {
            get => _BlackjackPayout;
            set { _BlackjackPayout = value; OnPropertyChanged(nameof(BlackjackPayout)); }
        }
        private double _BlackjackPayout = Rules.Instance.BlackjackPayout;

        public bool AllowResplit
        {
            get => _allowResplit;
            set { _allowResplit = value; OnPropertyChanged(nameof(AllowResplit)); }
        }
        private bool _allowResplit = Rules.Instance.AllowResplit;
        public bool AllowSplit
        {
            get => _allowSplit;
            set { _allowSplit = value; OnPropertyChanged(nameof(AllowSplit)); }
        }
        private bool _allowSplit = Rules.Instance.AllowSplit;
        public bool AllowDouble
        {
            get => _allowDouble;
            set { _allowDouble = value; OnPropertyChanged(nameof(AllowDouble)); }
        }
        private bool _allowDouble = Rules.Instance.AllowDouble;

        public bool DoubleOnAnyTwo
        {
            get => _doubleOnAnyTwo;
            set { _doubleOnAnyTwo = value; OnPropertyChanged(nameof(DoubleOnAnyTwo)); }
        }
        private bool _doubleOnAnyTwo = Rules.Instance.DoubleOnAnyTwo;

        public bool DoubleAfterSplit
        {
            get => _doubleAfterSplit;
            set { _doubleAfterSplit = value; OnPropertyChanged(nameof(DoubleAfterSplit)); }
        }
        private bool _doubleAfterSplit = Rules.Instance.DoubleAfterSplit;

        public bool DoubleAfterSplit11
        {
            get => _doubleAfterSplit11;
            set { _doubleAfterSplit11 = value; OnPropertyChanged(nameof(DoubleAfterSplit11)); }
        }
        private bool _doubleAfterSplit11 = Rules.Instance.DoubleAfterSplit11;

        public bool DoubleAfterSplitAces
        {
            get => _doubleAfterSplitAces;
            set { _doubleAfterSplitAces = value; OnPropertyChanged(nameof(DoubleAfterSplitAces)); }
        }
        private bool _doubleAfterSplitAces = Rules.Instance.DoubleAfterSplitAces;

        public int SixCardCharlieCount
        {
            get => _sixCardCharlieCount;
            set { _sixCardCharlieCount = value; OnPropertyChanged(nameof(SixCardCharlieCount)); }
        }
        private int _sixCardCharlieCount = Rules.Instance.SixCardCharlieCount;

        public double UpperCashback
        {
            get => _upperCashback;
            set { _upperCashback = value; OnPropertyChanged(nameof(UpperCashback)); }
        }
        private double _upperCashback = Rules.Instance.UpperLimit;
        public double LowerCashback
        {
            get => _lowerCashback;
            set { _lowerCashback = value; OnPropertyChanged(nameof(LowerCashback)); }
        }
        private double _lowerCashback = Rules.Instance.LowerLimit;

        public long Rounds
        {
            get => _rounds;
            set { _rounds = value; OnPropertyChanged(nameof(Rounds)); }
        }
        private long _rounds = 0;

        public double Penetration
        {
            get => _penetration;
            set { _penetration = value; OnPropertyChanged(nameof(Penetration)); }
        }
        private double _penetration = Rules.Instance.Penetration;

        public bool DealerPeeksOnAce
        {
            get => _dealerPeeksOnAce;
            set { _dealerPeeksOnAce = value; OnPropertyChanged(nameof(DealerPeeksOnAce)); }
        }
        private bool _dealerPeeksOnAce = Rules.Instance.DealerPeeksOnAce;

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
