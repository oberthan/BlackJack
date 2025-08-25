using BlackJackWpf.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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

        }
        public StrategyViewModel ViewModel
        {
            get => (StrategyViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Width = new DataGridLength(50);

            if (e.Column is DataGridTextColumn textColumn)
            {
                var style = new Style(typeof(DataGridCell));

                // Default colors
                style.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Black));

                if (e.PropertyName != "Pair" && e.PropertyName != "Total") style.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Aquamarine));

                // Highlight based on cell text
                var hitTrigger = new DataTrigger()
                {
                    Binding = new Binding(e.PropertyName),
                    Value = "H"   // Hit
                };
                hitTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.White));
                style.Triggers.Add(hitTrigger);

                var standTrigger = new DataTrigger()
                {
                    Binding = new Binding(e.PropertyName),
                    Value = "S"   // Stand
                };
                standTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Gold));
                style.Triggers.Add(standTrigger);

                var doubleTrigger = new DataTrigger()
                {
                    Binding = new Binding(e.PropertyName),
                    Value = "D"   // Double
                };
                doubleTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Lime));
                style.Triggers.Add(doubleTrigger);

                var splitTrigger = new DataTrigger()
                {
                    Binding = new Binding(e.PropertyName),
                    Value = "P"   // Split
                };
                splitTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.DeepSkyBlue));
                style.Triggers.Add(splitTrigger);

                var YTrigger = new DataTrigger()
                {
                    Binding = new Binding(e.PropertyName),
                    Value = "Y"   // Split
                };
                YTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Lime));
                style.Triggers.Add(YTrigger);

                var NTrigger = new DataTrigger()
                {
                    Binding = new Binding(e.PropertyName),
                    Value = "N"   // Split
                };
                NTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.White));
                style.Triggers.Add(NTrigger);

                // Ensure selection doesn’t override
                var selectedTrigger = new Trigger
                {
                    Property = DataGridCell.IsSelectedProperty,
                    Value = true
                };
                selectedTrigger.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Black));
                style.Triggers.Add(selectedTrigger);

                textColumn.CellStyle = style;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
