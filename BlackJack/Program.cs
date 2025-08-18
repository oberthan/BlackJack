using System.Diagnostics;

namespace BlackJack;

internal class Program
{
    private static void Main()
    {
        var game = new Game();
        var rounds = 10000000;

        int wins = 0, losses = 0, pushes = 0;
        long units = 0;
        long blackjacks = 0;
        long splits = 0;
        long doubles = 0;
        long stake = 0;

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < rounds; i++)
        {
            var res = game.PlayOneRound();
            units += res.UnitsWonOrLost;
            stake += res.Stake;
            if (res.Blackjack) blackjacks++;
            if (res.Split) splits++;
            if (res.Double) doubles++;

            switch (res.Outcome)
            {
                case Outcome.PlayerWin: wins++; break;
                case Outcome.DealerWin: losses++; break;
                default: pushes++; break;
            }
            if(i%1000000 == 0)
            {
                DisplayInformation(i);
                Console.WriteLine($"\n[{new string('#', 30*(int)i/rounds)}{new string('-', 30- 30*(int)i/rounds)}] {100*i/rounds}%");
            }
        }
        stopwatch.Stop();

        DisplayInformation(rounds);

        void DisplayInformation(int rounds)
        {
            //Console.Clear();
            Console.SetCursorPosition(0,0);
            Console.WriteLine("|---- Blackjack Simulation Results ----|");
            Console.WriteLine($"Rounds: {rounds:n0}");
            Console.WriteLine($"Player wins: {wins:n0}, Dealer wins: {losses:n0}, Pushes: {pushes:n0}");
            Console.WriteLine($"Stake: {stake:n0}");
            Console.WriteLine($"Total return: {(units + stake):n0}");
            Console.WriteLine($"Net units: {units:n0}");
            Console.WriteLine($"Average Bet: {(stake / (float)rounds):n5}");
            Console.WriteLine($"Blackjacks: {blackjacks:n0}, Splits: {splits:n0}, Doubles: {doubles:n0}");
            Console.WriteLine($"RTP: {((units + rounds) / (float)rounds):n9}");
            Console.WriteLine("\n|-------- Technical Statistics --------|");
            Console.WriteLine($"Elapsed time: {(stopwatch.Elapsed)}");
            Console.WriteLine($"Average time per round: {(stopwatch.Elapsed.TotalMilliseconds / rounds):n9} ms");
            Console.WriteLine($"Rounds per sec: {(rounds / stopwatch.Elapsed.TotalSeconds):n1} / Second");
        }
    }
}