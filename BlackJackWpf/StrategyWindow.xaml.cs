using BlackJackWpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlackJackWpf
{
    /// <summary>
    /// Interaction logic for StrategyWindow.xaml
    /// </summary>
    public partial class StrategyWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(StrategyViewModel), typeof(StrategyWindow), new PropertyMetadata(default(StrategyViewModel)));
        public StrategyWindow()
        {
            InitializeComponent();
            ViewModel = new StrategyViewModel();
            DataContext = ViewModel;

            // (optional) seed some test data so you see rows immediately
            ViewModel.PairStrategy.Add(new PairStrategyRow { Pair = "2,2", Vs2 = "P", Vs3 = "P" });
            ViewModel.SoftStrategy.Add(new SoftStrategyRow { Total = 13, Vs2 = "H", Vs3 = "H" });
            ViewModel.HardStrategy.Add(new HardStrategyRow { Total = 9, Vs2 = "H", Vs3 = "D" });
        }
        public StrategyViewModel ViewModel
        {
            get => (StrategyViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
