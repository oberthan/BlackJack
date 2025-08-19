using System.Diagnostics;

namespace BlackJack;

internal class Program
{
    private static void Main()
    {
        var game = new Game();
        var rounds = 10000000;

        int wins = 0, losses = 0, pushes = 0;
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
            tasks[i] = Task.Run(() =>
            {
                simulators[taskIndex].RunSimulation();
            });
        }

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

            DisplayInformation(sum.i);

            var progress = 30 * (float)sum.i / rounds;
            Console.WriteLine($"\n[{new string('#', (int)progress)}{new string('-', (int)(30 - progress))}] {100f * sum.i / rounds:F1}%");
            Thread.Sleep(500);
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        DisplayInformation(rounds);

        void DisplayInformation(int rounds)
        {
            //Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("|---- Blackjack Simulation Results ----|");
            Console.WriteLine($"Rounds: {rounds:n0}");
            Console.WriteLine($"Player wins: {wins:n0}, Dealer wins: {losses:n0}, Pushes: {pushes:n0}");
            Console.WriteLine($"Stake: {stake:n0}");
            Console.WriteLine($"Total return: {(units + stake):n0}");
            Console.WriteLine($"Net units: {units:n0}");
            Console.WriteLine($"Average Bet: {(stake / (float)rounds):n5}");
            Console.WriteLine($"Blackjacks: {blackjacks:n0}, Splits: {splits:n0}, Doubles: {doubles:n0}");
            Console.WriteLine($"RTP: {((units + stake) / (float)stake):n9}");
            Console.WriteLine($"Net units per round + 1: {((units) / (float)rounds)+1:n9}");
            Console.WriteLine("\n|-------- Technical Statistics --------|");
            Console.WriteLine($"Elapsed time: {(stopwatch.Elapsed)}");
            Console.WriteLine($"Average time per round: {(stopwatch.Elapsed.TotalMilliseconds / rounds):n9} ms");
            Console.WriteLine($"Rounds per sec: {(rounds / stopwatch.Elapsed.TotalSeconds):n1} / Second");
        }
    }

    public class BlackjackSimulator
    {
        public int Rounds { get; set; } = 10000000;
        public Game Game { get; } = new();

        public int wins = 0, losses = 0, pushes = 0;
        public double units = 0;
        public long blackjacks = 0;
        public long splits = 0;
        public long doubles = 0;
        public long stake = 0;
        public int i;

        public static BlackjackSimulator Sum(IEnumerable<BlackjackSimulator> simulators)
        {
            var result = new BlackjackSimulator();
            foreach (var sim in simulators)
            {
                result.i += sim.i;
                result.wins += sim.wins;
                result.losses += sim.losses;
                result.pushes += sim.pushes;
                result.units += sim.units;
                result.blackjacks += sim.blackjacks;
                result.splits += sim.splits;
                result.doubles += sim.doubles;
                result.stake += sim.stake;
            }
            return result;
        }

        public void RunSimulation()
        {
            for (i = 0; i < Rounds; i++)
            {
                var res = Game.PlayOneRound();

                units += res.UnitsWonOrLost;
                stake += res.Stake;
                if (res.Blackjack) blackjacks++;
                if (res.Split) splits++;
                if (res.Double) doubles++;

                switch (res.Outcome)
                {
                    case Outcome.PlayerWin: wins++; break;
                    case Outcome.DealerBlackjack: losses++; break;
                    case Outcome.Bust: losses++; break;
                    case Outcome.PlayerBlackjack: wins++; break;
                    case Outcome.DealerBust: wins++; break;
                    case Outcome.DealerWin: losses++; break;
                    case Outcome.PlayerWinWithCharlie: wins++; break;
                    default: pushes++; break;
                }

            }
        }
    }
}