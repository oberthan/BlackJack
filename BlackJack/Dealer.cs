namespace BlackJack;

public class Dealer : Player
{
    public new void Play(Deck deck)
    {
        while (true)
        {
            var eval = HandEvaluator.Evaluate(Hand, false);
            if (eval.Total < 17) AddCard(deck.DrawCard());
            else if (eval.Total == 17 && !Rules.DealerStandsOnSoft17 && eval.IsSoft) AddCard(deck.DrawCard());
            else break; // REQUIREMENT: dealer stands on ALL 17s (including soft)
        }
    }
}