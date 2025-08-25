using BlackJack; // Reference your core library
using BlackJackWpf.ViewModels;
using System.Diagnostics;
using System.Text;
using System.Windows;
using static BlackJack.Program;

namespace BlackJackWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
        private long rounds = 10_000_000;

        private async void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
                button.IsEnabled = false;
            ResultsText.Text = "Running simulation...";
            SimulationProgress.Value = 0;
            await RunSimulation();
            UpdateStatus("Done");

            
            if (button != null)
                button.IsEnabled = true;
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
            double unitsSquared = 0; // <-- Add this variable to store units squared
            Dictionary<double, int> dict = new();

            var nTasks = Environment.ProcessorCount;

            var tasks = new Task[nTasks];
            var simulators = new BlackjackSimulator[nTasks];
            for (var i = 0; i < nTasks; i++)
            {
                simulators[i] = new BlackjackSimulator
                {
                    Rounds = (rounds / nTasks) + ((i < (rounds % nTasks)) ? 1 : 0),
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
                await Task.Delay(500);
                var sum = BlackjackSimulator.Sum(simulators);
                wins = sum.wins;
                losses = sum.losses;
                pushes = sum.pushes;
                units = sum.units;
                blackjacks = sum.blackjacks;
                splits = sum.splits;
                doubles = sum.doubles;
                stake = sum.stake;
                unitsSquared = sum.unitsSquared; // <-- Add this line
                dict = sum.limitOverShoots;

                UpdateResults(sum.i, previousRounds);
                previousRounds = sum.i;
                previousTime = stopwatch.Elapsed;

                var progress = 30 * (float)sum.i / rounds;
                UpdateStatus(
                    $"[{new string('#', (int)progress)}{new string('-', (int)(30 - progress))}] {100f * sum.i / rounds:F1}%");

            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();





            previousTime = new TimeSpan(0);
            UpdateResults(previousRounds, 0);

            void UpdateResults(long simulatedRounds, long previous = 0)
            {
                double xbar = units / simulatedRounds;
                double variance = (unitsSquared / simulatedRounds) - (xbar * xbar);
                double stddev = Math.Sqrt(variance);
                double stdError = stddev / Math.Sqrt(simulatedRounds);
                double conf95 = 1.96 * stdError;
                double conf999 = 3.291 * stdError;

                var str = new StringBuilder();
                str.AppendLine("\n|-------- Technical Statistics --------|");
                str.AppendLine($"Elapsed time: {(stopwatch.Elapsed)}");
                str.AppendLine(
                    $"Average time per round: {(stopwatch.Elapsed.TotalMilliseconds / simulatedRounds):n9} ms");
                str.AppendLine(
                    $"Rounds per sec: {((simulatedRounds - previous) / (stopwatch.Elapsed.TotalSeconds - previousTime.TotalSeconds)):n1} / Second");
                UpdateProgress((float)simulatedRounds / rounds);

                str.AppendLine("\n|---- Blackjack Simulation Results ----|");
                str.AppendLine($"Rounds: {simulatedRounds:n0}");
                str.AppendLine($"Player wins: {wins:n0}, Dealer wins: {losses:n0}, Pushes: {pushes:n0}");
                str.AppendLine($"Stake: {stake:n0}");
                str.AppendLine($"Total return: {(units + stake):n0}");
                str.AppendLine($"Net units: {units:n0}");
                str.AppendLine($"Average Bet: {(stake / (float)simulatedRounds):n5}");
                str.AppendLine($"Blackjacks: {blackjacks:n0}, Splits: {splits:n0}, Doubles: {doubles:n0}");
                str.AppendLine($"RTP: {((units + stake) / (float)stake):n9}");
                //str.AppendLine($"Indledende RTP: {((units+stake) / (float)simulatedRounds):n9}");
                str.AppendLine($"Net units per round + 1: {((units) / (float)simulatedRounds) + 1:n9}");
                str.AppendLine($"Confidence 95%: ±{conf95:n9} [{((units) / (float)simulatedRounds) + 1 + conf95:n6}, {((units) / (float)simulatedRounds) + 1 - conf95:n6}]");
                str.AppendLine($"Confidence 99.9%: ±{conf999:n9} [{((units) / (float)simulatedRounds) + 1 + conf999:n6}, {((units) / (float)simulatedRounds) + 1 - conf999:n6}]");


                double cashback = 0;
                double sessions = 0;
                foreach (var kvp in dict)
                {
                    if (kvp.Key < 0) cashback -= kvp.Key * kvp.Value * Rules.Instance.Cashback;
                    sessions += kvp.Value;
                }
                str.AppendLine($"Cashback: {cashback:n0}");
                str.AppendLine($"Units with Cashback: {units + cashback:n0}");
                str.AppendLine($"EV with Cashback: {(units + cashback) / sessions}");

                dict = dict.OrderByDescending(x => x.Key).ToDictionary();
                str.AppendLine($"\nUnit limits: \n{string.Join(Environment.NewLine, dict)}");

                str.AppendLine($"Sum of all keys*value in dict: {dict.Sum(x => x.Key * x.Value) / dict.Sum(x => x.Value):n6}");

                double edge = 0;
                double variance = 0;
                double kelly = 0;

                double wasd = 0;

                foreach (var kvp in dict)
                {
                    if (kvp.Key < 0) wasd -= kvp.Key * kvp.Value * Rules.Instance.Cashback;

                }







                ResultsText.Text = str.ToString();
            }
        }

        private async void SearchStrategy_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
                button.IsEnabled = false;

            int initialSimulations = 5_000_000; // Fast, low-accuracy pass
            int finalSimulations = 20_000_000; // High-accuracy for close results
            double threshold = 0.002; // Margin for "close" results

            var strategy = Strategy.Instance;
            var pairRows = strategy.PairStrategy;

            // Map from "2"-"A" to the corresponding row in PairStrategy
            var pairValues = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "A" };
            var colNames = new[] { "Vs2", "Vs3", "Vs4", "Vs5", "Vs6", "Vs7", "Vs8", "Vs9", "Vs10", "VsA" };

            int totalSteps = pairValues.Length * colNames.Length;
            int currentStep = 0;

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < pairValues.Length; i++)
            {
                string pair = pairValues[i];
                var pCard = new Card(pair, "D");
                // Find the row in PairStrategy where Pair == pair
                var row = pairRows.FirstOrDefault(r => r.Pair == $"{pCard.PipValue},{pCard.PipValue}");

                //if (row == null) continue;

                for (int j = 0; j < colNames.Length; j++)
                {
                    string col = colNames[j];
                    SetPairCell(row, col, "Y");
                    double rtpY = await SimulateRTP(initialSimulations, [pCard, pCard], new Card(pairValues[j], "S")) / initialSimulations;

                    SetPairCell(row, col, "N");
                    double rtpN = await SimulateRTP(initialSimulations, [pCard, pCard], new Card(pairValues[j], "S")) / initialSimulations;

                    // If results are close, re-run with higher accuracy
                    if (Math.Abs(rtpY - rtpN) < threshold)
                    {
                        SetPairCell(row, col, "Y");
                        rtpY = await SimulateRTP(finalSimulations, [pCard, pCard], new Card(pairValues[j], "S"));

                        SetPairCell(row, col, "N");
                        rtpN = await SimulateRTP(finalSimulations, [pCard, pCard], new Card(pairValues[j], "S"));
                    }

                    SetPairCell(row, col, rtpY <= rtpN ? "Y" : "N");

                    currentStep++;
                    UpdateProgress((float)currentStep / totalSteps);
                }
            }

            ResultsText.Text = $"Pair strategy optimized! It took {watch.Elapsed}!";

            if (button != null)
                button.IsEnabled = true;
        }

        // Helper to set a cell value by column name
        private void SetPairCell(PairStrategyRow row, string col, string value)
        {
            switch (col)
            {
                case "Vs2": row.Vs2 = value; break;
                case "Vs3": row.Vs3 = value; break;
                case "Vs4": row.Vs4 = value; break;
                case "Vs5": row.Vs5 = value; break;
                case "Vs6": row.Vs6 = value; break;
                case "Vs7": row.Vs7 = value; break;
                case "Vs8": row.Vs8 = value; break;
                case "Vs9": row.Vs9 = value; break;
                case "Vs10": row.Vs10 = value; break;
                case "VsA": row.VsA = value; break;
            }
        }

        // Run a simulation and return RTP
        private async Task<double> SimulateRTP(int rounds, List<Card> playerHand, Card upCard)
        {
            var nTasks = Environment.ProcessorCount;
            var simulators = new BlackjackSimulator[nTasks];
            var tasks = new Task[nTasks];

            BlackjackSimulator sum = null;
            long previousRounds = 0;
            var stopwatch = Stopwatch.StartNew();
            var previousTime = stopwatch.Elapsed;

            for (int i = 0; i < nTasks; i++)
                simulators[i] = new BlackjackSimulator { Rounds = rounds / nTasks };

            for (int i = 0; i < nTasks; i++)
            {
                int idx = i;
                tasks[i] = Task.Run(() => simulators[idx].ForceStartingHand(playerHand, upCard));
            }
            while (!tasks.All(x => x.IsCompleted))
            {
                await Task.Delay(500);
                sum = BlackjackSimulator.Sum(simulators);


                UpdateResults(sum, previousRounds);
                previousRounds = sum.i;
                previousTime = stopwatch.Elapsed;

                var progress = 30 * (float)sum.i / rounds;
                UpdateStatus(
                    $"[{new string('#', (int)progress)}{new string('-', (int)(30 - progress))}] {100f * sum.i / rounds:F1}%");

            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // RTP = (units + stake) / stake
            return sum.units;

            void UpdateResults(BlackjackSimulator sum, long previous = 0)
            {
                var simulatedRounds = sum.i;
                var wins = sum.wins;
                var losses = sum.losses;
                var pushes = sum.pushes;
                var units = sum.units;
                var stake = sum.stake;

                var str = new StringBuilder();
                str.AppendLine("|---- Blackjack Simulation Results ----|");
                str.AppendLine($"Rounds: {simulatedRounds:n0}");
                str.AppendLine($"Player wins: {wins:n0}, Dealer wins: {losses:n0}, Pushes: {pushes:n0}");
                str.AppendLine($"Stake: {stake:n0}");
                str.AppendLine($"Total return: {(units + stake):n0}");
                str.AppendLine($"Net units: {units:n0}");
                str.AppendLine($"Average Bet: {(stake / (float)simulatedRounds):n5}");
                str.AppendLine($"RTP: {((units + stake) / (float)stake):n9}");
                //str.AppendLine($"Indledende RTP: {((units+stake) / (float)simulatedRounds):n9}");
                str.AppendLine($"Net units per round + 1: {((units) / (float)simulatedRounds) + 1:n9}");
                str.AppendLine("\n|-------- Technical Statistics --------|");
                str.AppendLine($"Elapsed time: {(stopwatch.Elapsed)}");
                str.AppendLine(
                    $"Average time per round: {(stopwatch.Elapsed.TotalMilliseconds / simulatedRounds):n9} ms");
                str.AppendLine(
                    $"Rounds per sec: {((simulatedRounds - previous) / (stopwatch.Elapsed.TotalSeconds - previousTime.TotalSeconds)):n1} / Second");

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
                Rules.Instance.AllowDouble = viewModel.AllowDouble;
                Rules.Instance.AllowSplit = viewModel.AllowSplit;
                Rules.Instance.UpperLimit = viewModel.UpperCashback;
                Rules.Instance.LowerLimit = viewModel.LowerCashback;

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