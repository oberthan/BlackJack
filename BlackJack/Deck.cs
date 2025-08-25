namespace BlackJack;

public static class Rnd
{
    public static Random Rng = new();
}

public class Deck
{
    private readonly int numDecks;
    private readonly int penetrationCut; // remaining-card threshold to reshuffle
    public Card[] Cards { get; private set; }
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
        string[] suits = { "H", "D", "V", "S" };
        string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        Cards = new Card[numDecks * 52];

        for (var n = 0; n < numDecks; n++)
            for (var si = 0; si < suits.Length; si++)
            for (var vi = 0; vi< values.Length; vi++)
                Cards[n*52+si*values.Length+vi] = new Card(values[vi], suits[si]);
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


    public Card DrawCard()
    {
        if (CardsLeft == 0) throw new InvalidOperationException("Deck is empty!");
        var card = Cards[CardsLeft-1];
        CardsLeft--;
        return card;
    }

    public void EndOfGame()
    {
        if (CardsLeft < penetrationCut) Shuffle();
    }
}