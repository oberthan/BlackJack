using System;
using System.Collections.Generic;

namespace BlackjackApp
{
    class Program
    {
        static void Main()
        {
            int simulations = 10000000;
            var results = new Dictionary<string, int>
            {
                { "Player", 0 },
                { "Dealer", 0 },
                { "Push", 0 }
            };

            Game game = new Game();

            for (int i = 0; i < simulations; i++)
            {
                string result = game.PlayGame();
                results[result]++;
            }

            Console.WriteLine($"After {simulations} games:");
            Console.WriteLine($"Player wins: {results["Player"]}");
            Console.WriteLine($"Dealer wins: {results["Dealer"]}");
            Console.WriteLine($"Pushes: {results["Push"]}");

        }
    }
}
