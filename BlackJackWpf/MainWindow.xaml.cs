using BlackJack; // Reference your core library
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlackJackWpf.ViewModels;
using static BlackJack.Program;

namespace BlackJackWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
        private long rounds = 10000000;

        private async void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            ResultsText.Text = "Running simulation...";
            SimulationProgress.Value = 0;
            await RunSimulation();
            UpdateStatus("Done");
        }

        private void UpdateStatus(string text)
        {
            StatusText.Text = text;
        }

        private void UpdateProgress(float progress)
        {
            SimulationProgress.Value = progress * 100f;
        }

        private async Task RunSimulation()
        {

            UpdateStatus(
                $"Running {rounds:n0} rounds of Blackjack simulation on {Environment.ProcessorCount} threads...\n");

            long wins = 0, losses = 0, pushes = 0;
            double units = 0;
            long blackjacks = 0;
            long splits = 0;
            long doubles = 0;
            long stake = 0;

            var nTasks = Environment.ProcessorCount;

            var tasks = new Task[nTasks];
            var simulators = new BlackjackSimulator[nTasks];
            for (var i = 0; i < nTasks; i++)
            {
                simulators[i] = new BlackjackSimulator
                {
                    Rounds = rounds / nTasks,
                };
            }

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < nTasks; i++)
            {
                var taskIndex = i; // capture loop variable
                tasks[i] = Task.Run(() => { simulators[taskIndex].RunSimulation(); });
            }

            long previousRounds = 0;
            var previousTime = stopwatch.Elapsed;
            while (!tasks.All(x => x.IsCompleted))
            {
                var sum = BlackjackSimulator.Sum(simulators);
                wins = sum.wins;
                losses = sum.losses;
                pushes = sum.pushes;
                units = sum.units;
                blackjacks = sum.blackjacks;
                splits = sum.splits;
                doubles = sum.doubles;
                stake = sum.stake;

                UpdateResults(sum.i, previousRounds);
                previousRounds = sum.i;
                previousTime = stopwatch.Elapsed;

                var progress = 30 * (float)sum.i / rounds;
                UpdateStatus(
                    $"[{new string('#', (int)progress)}{new string('-', (int)(30 - progress))}] {100f * sum.i / rounds:F1}%");
                await Task.Delay(500);
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            previousTime = new TimeSpan(0);
            UpdateResults(rounds, 0);

            void UpdateResults(long simulatedRounds, long previous = 0)
            {
                var str = new StringBuilder();
                str.AppendLine("|---- Blackjack Simulation Results ----|");
                str.AppendLine($"Rounds: {simulatedRounds:n0}");
                str.AppendLine($"Player wins: {wins:n0}, Dealer wins: {losses:n0}, Pushes: {pushes:n0}");
                str.AppendLine($"Stake: {stake:n0}");
                str.AppendLine($"Total return: {(units + stake):n0}");
                str.AppendLine($"Net units: {units:n0}");
                str.AppendLine($"Average Bet: {(stake / (float)simulatedRounds):n5}");
                str.AppendLine($"Blackjacks: {blackjacks:n0}, Splits: {splits:n0}, Doubles: {doubles:n0}");
                str.AppendLine($"RTP: {((units + stake) / (float)stake):n9}");
                str.AppendLine($"Net units per round + 1: {((units) / (float)simulatedRounds) + 1:n9}");
                str.AppendLine("\n|-------- Technical Statistics --------|");
                str.AppendLine($"Elapsed time: {(stopwatch.Elapsed)}");
                str.AppendLine(
                    $"Average time per round: {(stopwatch.Elapsed.TotalMilliseconds / simulatedRounds):n9} ms");
                str.AppendLine(
                    $"Rounds per sec: {((simulatedRounds - previous) / (stopwatch.Elapsed.TotalSeconds - previousTime.TotalSeconds)):n1} / Second");
                UpdateProgress((float)simulatedRounds / rounds);
                ResultsText.Text = str.ToString();
            }
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;

            var viewModel = new SettingsViewModel();
            settingsWindow.ViewModel = viewModel;
            viewModel.Rounds = rounds;

            var isAccepted = settingsWindow.ShowDialog();
            if (isAccepted == true)
            {
                // Apply all settings to Rules.Instance
                Rules.Instance.DealerStandsOnSoft17 = viewModel.DealerStandsOnSoft17;
                Rules.Instance.BlackjackPayout = viewModel.BlackjackPayout;
                Rules.Instance.AllowResplit = viewModel.AllowResplit;
                Rules.Instance.DoubleOnAnyTwo = viewModel.DoubleOnAnyTwo;
                Rules.Instance.DoubleAfterSplit = viewModel.DoubleAfterSplit;
                Rules.Instance.DoubleAfterSplit11 = viewModel.DoubleAfterSplit11;
                Rules.Instance.DoubleAfterSplitAces = viewModel.DoubleAfterSplitAces;
                Rules.Instance.SixCardCharlieCount = viewModel.SixCardCharlieCount;
                Rules.Instance.DealerPeeksOnAce = viewModel.DealerPeeksOnAce;

                rounds = viewModel.Rounds;

            }
        }
        private void ShowStrategy_Click(object sender, RoutedEventArgs e)
        {
            var strategyWindow = new StrategyWindow();
            strategyWindow.Owner = this;

            var viewModel = new StrategyViewModel();
            strategyWindow.ViewModel = viewModel;

            var isAccepted = strategyWindow.ShowDialog();
            if (isAccepted == true)
            {
                Strategy.Instance.HardStrategy = viewModel.HardStrategy.ToList();
                Strategy.Instance.SoftStrategy = viewModel.SoftStrategy.ToList();
                Strategy.Instance.PairStrategy = viewModel.PairStrategy.ToList();
            }
        }
    }

}