using Blackjack; // Reference your core library
using BlackjackWpf.ViewModels;
using System;
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
using static Blackjack.Program;
using static System.Net.WebRequestMethods;

namespace BlackjackWpf
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
            try
            {
                await RunSimulation();
            }
            catch(Exception ex)
            {
                ResultsText.Text = $"Error: {ex.Message}\n{ex.StackTrace}";
            }

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
            double unitsSquared = 0;
            Dictionary<double, long> dict = new();

            var nTasks = Environment.ProcessorCount-1;
            if (nTasks < 1) nTasks = 1;

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
                blackjacks = sum.Blackjacks;
                splits = sum.splits;
                doubles = sum.doubles;
                stake = sum.stake;
                unitsSquared = sum.unitsSquared; // <-- Add this line
                dict = sum.limitOverShoots;

                UpdateResults(sum.rounds, previousRounds);
                previousRounds = sum.rounds;
                previousTime = stopwatch.Elapsed;

                var progress = 30 * (float)sum.rounds / rounds;
                UpdateStatus(
                    $"[{new string('#', (int)progress)}{new string('-', (int)(30 - progress))}] {100f * sum.rounds / rounds:F1}%");

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

                double kelly = 0;
                double ev_sum = 0;
                double sigma_squared = 0;

                long sessions_sum = dict.Sum(x => x.Value);

                foreach (var kvp in dict)
                {
                    if (kvp.Key < 0)
                    {
                        ev_sum += kvp.Key * kvp.Value * (1 - Rules.Instance.Cashback);
                        sigma_squared += kvp.Value * (kvp.Key * (1 - Rules.Instance.Cashback) * kvp.Key * (1 - Rules.Instance.Cashback));
                    }
                    else
                    {
                        ev_sum += kvp.Key * kvp.Value;
                        sigma_squared += kvp.Key * kvp.Key * kvp.Value;
                    }

                }
                double ev = ev_sum / sessions_sum;

                sigma_squared /= sessions_sum;
                sigma_squared -= ev * ev;

                kelly = ev / sigma_squared;

                str.AppendLine($"Kelly: {kelly:n6}");
                str.AppendLine($"EV: {ev:n6}");
                str.AppendLine($"Kelly*EV: {kelly * ev:n6}");


                ResultsText.Text = str.ToString();
            }
        }

        private async void SearchStrategy_Click(object sender, RoutedEventArgs e)
        {



            var button = sender as System.Windows.Controls.Button;
            if (button != null)
                button.IsEnabled = false;

            int firstPassSimulations = 1_000_000;
            int secondPassSimulations = 25_000_000; // Fast, low-accuracy pass
            int finalSimulations = 100_000_000; // High-accuracy for close results
            double firstThreshold = 0.05; // Margin for "close" results
            double secondThreshold = 0.002;

            var strategy = Strategy.Instance;
            var pairRows = strategy.PairStrategy;

            // Map from "2"-"A" to the corresponding row in PairStrategy
            CardValue[] pairValues =
            [
                CardValue.Two, CardValue.Three, CardValue.Four, CardValue.Five, CardValue.Six, CardValue.Seven,
                CardValue.Eight, CardValue.Nine, CardValue.Ten, CardValue.Jack, CardValue.Queen, CardValue.King,
                CardValue.Ace
            ];
            var colNames = new[] { "Vs2", "Vs3", "Vs4", "Vs5", "Vs6", "Vs7", "Vs8", "Vs9", "Vs10", "VsA" };

            int totalSteps = pairValues.Length * colNames.Length;
            int currentStep = 0;

            double[,,] differences = new double[pairValues.Length, colNames.Length, 3];

            var watch = Stopwatch.StartNew();
            try
            {
            for (int i = 0; i < pairValues.Length; i++)
            {
                var pair = pairValues[i];
                var pCard = pair;
                // Find the row in PairStrategy where Pair == pair
                var row = pairRows.First(r => r.Pair == pCard);

                //if (row == null) continue;

                for (int j = 0; j < colNames.Length; j++)
                {
                    string col = colNames[j];
                    SetPairCell(row, col,  Decision.P);
                    double rtpY = await SimulateRTP(firstPassSimulations, [pCard, pCard], pairValues[j]) / firstPassSimulations;

                    SetPairCell(row, col, Decision.N);
                    double rtpN = await SimulateRTP(firstPassSimulations, [pCard, pCard],pairValues[j]) / firstPassSimulations;


                    // If results are close, re-run with higher accuracy
                    var firstPass = Math.Abs(rtpY - rtpN);
                    differences[i, j, 0] = firstPass;
                    if (firstPass < firstThreshold)
                    {
                        SetPairCell(row, col, Decision.P);
                        rtpY = await SimulateRTP(secondPassSimulations, [pCard, pCard], pairValues[j]) / secondPassSimulations;

                        SetPairCell(row, col, Decision.N);
                        rtpN = await SimulateRTP(secondPassSimulations, [pCard, pCard], pairValues[j]) / secondPassSimulations;

                        var secondPass = Math.Abs(rtpY - rtpN);
                        differences[i,j,1] = secondPass;

                        if (secondPass < secondThreshold)
                        {
                            SetPairCell(row, col, Decision.P);
                            rtpY = await SimulateRTP(finalSimulations, [pCard, pCard], pairValues[j]) / finalSimulations;

                            SetPairCell(row, col, Decision.N);
                            rtpN = await SimulateRTP(finalSimulations, [pCard, pCard], pairValues[j]) / finalSimulations;
                   
                            var finalPass = Math.Abs(rtpY - rtpN);
                            differences[i,j,2] = finalPass;
                        }
                    }




                    SetPairCell(row, col, rtpY >= rtpN ? Decision.P : Decision.N);

                    currentStep++;
                    UpdateProgress((float)currentStep / totalSteps);
                }
            }


            var str = new StringBuilder();

            str.AppendLine($"Pair strategy optimized! It took {watch.Elapsed}!");


            for (int i = pairValues.Length-1; i >= 0; i--)
            {
                str.Append($"({pairValues[i]}, {pairValues[i]})::: ");

                for (int j = 0; j < colNames.Length; j++)
                {
                    str.Append($"[{differences[i,j,0]:n5}:{differences[i, j, 1]:n5}:{differences[i, j, 2]:n5}] | ");
                }

                str.AppendLine("");

            }

            ResultsText.Text = str.ToString();
            }
            catch (Exception ex)
            {
                ResultsText.Text = $"Error: {ex.Message}\n{ex.StackTrace}";
            }

            if (button != null)
                button.IsEnabled = true;
        }

        // Helper to set a cell value by column name
        private void SetPairCell(PairStrategyRow row, string col, Decision decision)
        {
            switch (col)
            {
                case "Vs2": row.Vs2 = decision; break;
                case "Vs3": row.Vs3 = decision; break;
                case "Vs4": row.Vs4 = decision; break;
                case "Vs5": row.Vs5 = decision; break;
                case "Vs6": row.Vs6 = decision; break;
                case "Vs7": row.Vs7 = decision; break;
                case "Vs8": row.Vs8 = decision; break;
                case "Vs9": row.Vs9 = decision; break;
                case "Vs10": row.Vs10 = decision; break;
                case "VsA": row.VsA = decision; break;
            }
        }

        // Run a simulation and return RTP
        private async Task<double> SimulateRTP(int rounds, List<CardValue> playerHand, CardValue upCard)
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
                previousRounds = sum.rounds;
                previousTime = stopwatch.Elapsed;

                var progress = 30 * (float)sum.rounds / rounds;
                UpdateStatus(
                    $"[{new string('#', (int)progress)}{new string('-', (int)(30 - progress))}] {100f * sum.rounds / rounds:F1}%");

            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // RTP = (units + stake) / stake
            return sum.units;

            void UpdateResults(BlackjackSimulator sum, long previous = 0)
            {
                long simulatedRounds = sum.rounds;
                long wins = sum.wins;
                long losses = sum.losses;
                long pushes = sum.pushes;
                double units = sum.units;
                long stake = sum.stake;

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