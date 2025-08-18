namespace BlackJack;

internal class Dealer : Player
{
    public void Play(Deck deck)
    {
        var counter = 0;
        while (GetHandValue() < 17)
        {
            AddCard(deck.DrawCard());
            counter++;
            if (counter >= 13) throw new InvalidOperationException("Too many cards drawn!");
        }
    }
}