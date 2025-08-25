using System.Diagnostics;

namespace Blackjack;

public class Program
{
    private static void Main()
    {
        var game = new Game();
        long rounds = 100000000;
        Console.Write($"Enter the number of simulations to run or leave blank to run {rounds} simulations: ");
        var input = Console.ReadLine();
        if (input != "")
        {
            rounds = long.Parse(input);
        }
        Console.Clear();

        Console.WriteLine($"Running {rounds:n0} rounds of Blackjack simulation on {Environment.ProcessorCount} threads...\n");

        long wins = 0, losses = 0, pushes = 0;
        double units = 0;
        double unitsSquared = 0;
        long Blackjacks = 0;
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
        long previousRounds = 0;
        var previousTime = stopwatch.Elapsed;
        while (!tasks.All(x => x.IsCompleted))
        {
            var sum = BlackjackSimulator.Sum(simulators);
            wins = sum.wins;
            losses = sum.losses;
            pushes = sum.pushes;
            units = sum.units;
            unitsSquared = sum.unitsSquared; // Added
            Blackjacks = sum.Blackjacks;
            splits = sum.splits;
            doubles = sum.doubles;
            stake = sum.stake;



            DisplayInformation(sum.i, previousRounds);
            previousRounds = sum.i;
            previousTime = stopwatch.Elapsed;

            var progress = 30 * (float)sum.i / rounds;
            Console.WriteLine($"\n[{new string('#', (int)progress)}{new string('-', (int)(30 - progress))}] {100f * sum.i / rounds:F1}%");
            Thread.Sleep(500);
        }

        Task.WaitAll(tasks);
        stopwatch.Stop();

        previousTime = new TimeSpan(0);
        DisplayInformation(rounds, 0);

        void DisplayInformation(long rounds, long previous = 0)
        {
            //Console.Clear();
            Console.SetCursorPosition(0, 1);
            Console.WriteLine("|---- Blackjack Simulation Results ----|");
            Console.WriteLine($"Rounds: {rounds:n0}");
            Console.WriteLine($"Player wins: {wins:n0}, Dealer wins: {losses:n0}, Pushes: {pushes:n0}");
            Console.WriteLine($"Stake: {stake:n0}");
            Console.WriteLine($"Total return: {(units + stake):n0}");
            Console.WriteLine($"Net units: {units:n0}");
            Console.WriteLine($"Average Bet: {(stake / (float)rounds):n5}");
            Console.WriteLine($"Blackjacks: {Blackjacks:n0}, Splits: {splits:n0}, Doubles: {doubles:n0}");
            Console.WriteLine($"RTP: {((units + stake) / (float)stake):n9}");
            Console.WriteLine($"Net units per round + 1: {((units) / (float)rounds) + 1:n9}");
            Console.WriteLine("\n|-------- Technical Statistics --------|");
            Console.WriteLine($"Elapsed time: {(stopwatch.Elapsed)}");
            Console.WriteLine($"Average time per round: {(stopwatch.Elapsed.TotalMilliseconds / rounds):n9} ms");
            Console.WriteLine($"Rounds per sec: {((rounds - previous) / (stopwatch.Elapsed.TotalSeconds - previousTime.TotalSeconds)):n1} / Second");
            Console.WriteLine($"Units Squared: {unitsSquared:n0}"); // Add this line where you want to display
        }
    }

    public class BlackjackSimulator
    {
        public long Rounds { get; set; } = 10000000;
        public Game Game { get; } = new();

        public long wins = 0, losses = 0, pushes = 0;
        public double units = 0;
        public double unitsSquared = 0;
        public long Blackjacks = 0;
        public long splits = 0;
        public long doubles = 0;
        public long stake = 0;
        public long i;
        public Dictionary<double, int> limitOverShoots = new();

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
                result.unitsSquared += sim.unitsSquared;
                result.Blackjacks += sim.Blackjacks;
                result.splits += sim.splits;
                result.doubles += sim.doubles;
                result.stake += sim.stake;

                // Sum up the dictionary limitOverShoots
                foreach (var kvp in sim.limitOverShoots)
                {
                    if (!result.limitOverShoots.TryAdd(kvp.Key, kvp.Value))
                        result.limitOverShoots[kvp.Key] += kvp.Value;
                }
            }
            return result;
        }


        public void RunSimulation()
        {
            double localUnits = 0;
            for (i = 0; i < Rounds; i++)
            {
                var res = Game.PlayOneRound();

                units += res.UnitsWonOrLost;

                unitsSquared += res.UnitsWonOrLost * res.UnitsWonOrLost; // <-- Add this line

                stake += res.Stake;
                if (res.Blackjack) Blackjacks++;
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

                localUnits += res.UnitsWonOrLost;

                if (localUnits <= Rules.Instance.LowerLimit || localUnits >= Rules.Instance.UpperLimit)
                {
                    if (!limitOverShoots.TryAdd(localUnits, 1))
                        limitOverShoots[localUnits]++;

                    localUnits = 0;
                }
            }
        }
        public void ForceStartingHand(List<Card> playerhand, Card upCard)
        {
            double localUnits = 0;
            for (i = 0; i < Rounds; i++)
            {
                Game.player.Reset();
                Game.dealer.Reset();
                Game.deck.EndOfGame();
                Game.player.AddCard(playerhand[0]);
                Game.player.AddCard(playerhand[1]);

                Game.dealer.AddCard(upCard);
                Game.dealer.AddCard(Game.deck.DrawCard());
                var res = Game.PlayOneRoundWithHand();

                units += res.UnitsWonOrLost;

                unitsSquared += res.UnitsWonOrLost * res.UnitsWonOrLost; // <-- Add this line

                stake += res.Stake;
                if (res.Blackjack) Blackjacks++;
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

                localUnits += res.UnitsWonOrLost;

                if (localUnits <= Rules.Instance.LowerLimit || localUnits >= Rules.Instance.UpperLimit)
                {
                    if (!limitOverShoots.TryAdd(localUnits, 1))
                        limitOverShoots[localUnits]++;

                    localUnits = 0;
                }
            }
        }

    }
}