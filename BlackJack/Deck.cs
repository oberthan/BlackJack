using System;
using System.Collections.Generic;

namespace BlackjackApp
{
    class Deck
    {
        private List<Card> cards;
        private Random rng = new Random();

        public Deck(int numDecks = 1)
        {
            SetupShoe(numDecks);
        }

        private void SetupShoe(int numDecks)
        {
            string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "10", "10", "10", "A" };

            cards = new List<Card>();

            for (int n = 0; n < numDecks; n++)
                foreach (var suit in suits)
                    foreach (var value in values)
                        cards.Add(new Card(value, suit));
            Shuffle();
        }

        public void Shuffle()
        {
            int n = cards.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }

        private int counter = 0;
        public Card DrawCard()
        {
            if (cards.Count == 0) throw new InvalidOperationException("Deck is empty!");
            Card card = cards[0];
            cards.RemoveAt(0);
            counter++;
            return card;
        }

        public void EndOfGame(Player player, Dealer dealer)
        {
            if(cards.Count < (int)(2.4f * 52.0f))
            {
                SetupShoe(8);


            }


        }
    }
}
