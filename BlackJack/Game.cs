using System;
using System.Diagnostics.Metrics;

namespace BlackjackApp
{
    class Game
    {
        private Deck deck;
        private Player player;
        private Dealer dealer;

        public Game()
        {
            deck = new Deck(8); // 8 decks combined
            deck.Shuffle();

            player = new Player();
            dealer = new Dealer();
        }

        public string PlayGame()
        {
            player.ResetHand();
            dealer.ResetHand();

            // Initial deal
            player.AddCard(deck.DrawCard());
            player.AddCard(deck.DrawCard());

            dealer.AddCard(deck.DrawCard());
            dealer.AddCard(deck.DrawCard());

            // Player auto strategy: hit until 17 or higher
            var counter = 0;
            while (player.GetHandValue() < 17)
            {
                player.AddCard(deck.DrawCard());
                if (counter >= 13) throw new InvalidOperationException("Too many cards drawn!");

            }

            // Dealer turn
            dealer.Play(deck);

            deck.EndOfGame(player, dealer);


            // Determine outcome
            if (player.IsBusted()) return "Dealer";
            if (dealer.IsBusted()) return "Player";

            int playerTotal = player.GetHandValue();
            int dealerTotal = dealer.GetHandValue();

            if (playerTotal > dealerTotal) return "Player";
            if (dealerTotal > playerTotal) return "Dealer";


            return "Push";
        }
    }
}
