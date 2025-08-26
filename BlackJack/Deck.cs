namespace Blackjack;

public static class Rnd
{
    public static Random Rng = new();
}

public class Deck
{
    private readonly int numDecks;
    private readonly int penetrationCut; // remaining-card threshold to reshuffle
    public int[] Cards { get; private set; }
    public int CardsLeft;

    public Deck(int numDecks = 8)
    {
        this.numDecks = numDecks;
        var total = numDecks * 52;
        penetrationCut = (int)Math.Ceiling(total * (1.0 - Rules.Instance.Penetration));
        SetupShoe();
    }

    public void SetupShoe()
    {
        CardSuit[] suits = [CardSuit.Hearts, CardSuit.Diamonds, CardSuit.Clubs, CardSuit.Spades];
        CardValue[] values =
        [
            CardValue.Two, CardValue.Three, CardValue.Four, CardValue.Five, CardValue.Six, CardValue.Seven,
            CardValue.Eight, CardValue.Nine, CardValue.Ten, CardValue.Jack, CardValue.Queen, CardValue.King,
            CardValue.Ace
        ];

        Cards = new int[numDecks * 52];

        for (var n = 0; n < numDecks; n++)
            for (var si = 0; si < suits.Length; si++)
                for (var vi = 0; vi < values.Length; vi++)
                    Cards[n * 52 + si * values.Length + vi] = (int)values[vi] | (int)suits[si];
        Shuffle();
    }

    public void Shuffle()
    {
        var n = Cards.Length;
        CardsLeft = n;
        // Fisher-Yates shuffle algorithm (Knuth / Durstenfeld swap shuffle version https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle)
        for (var i = n - 1; i > 0; i--)
        {
            var j = Rnd.Rng.Next(i + 1);
            (Cards[i], Cards[j]) = (Cards[j], Cards[i]);
        }
    }


    public CardValue DrawCard()
    {
        if (CardsLeft == 0) throw new InvalidOperationException("Deck is empty!");
        var card = Cards[CardsLeft - 1];
        CardsLeft--;
        return (CardValue)(card&(int)CardValue.Mask);
    }

    public void EndOfGame()
    {
        if (CardsLeft < penetrationCut) Shuffle();
    }
}