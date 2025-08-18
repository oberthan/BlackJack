using System.Collections.Generic;

namespace BlackjackApp
{
    class Player
    {
        public List<Card> Hand { get; private set; } = new List<Card>();
        public int Bet { get; set; } = 1;

        public void ResetHand() => Hand.Clear();

        public void AddCard(Card card) => Hand.Add(card);

        public void DoubleDown(Deck deck)
        {
            Bet *= 2;
            AddCard(deck.DrawCard());
        }
        public bool CanDouble()
        {
            if(Hand.Count != 2) return false;

            return true;
        }


        public void Play(Deck deck)
        {

        }

        public int GetHandValue()
        {
            int total = 0;
            int aces = 0;

            foreach (var card in Hand)
            {
                if (card.Value == "A")
                {
                    total += 11;
                    aces++;
                }
                else
                    total += int.Parse(card.Value);
            }

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }

        public bool IsBusted() => GetHandValue() > 21;
        public bool HasBlackjack() => GetHandValue() == 21 && Hand.Count == 2;
    }

    class SplitPlayer : Player
    {

    }
}
