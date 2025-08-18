namespace BlackJack;

public static class Rnd
{
    public static Random Rng = new();
}

public class Deck
{
    private readonly int numDecks;
    private readonly int penetrationCut; // remaining-card threshold to reshuffle
    public List<Card> Cards { get; private set; }

    public Deck(int numDecks = 8, double penetration = 0.7)
    {
        this.numDecks = numDecks;
        var total = numDecks * 52;
        penetrationCut = (int)Math.Ceiling(total * (1.0 - penetration));
        SetupShoe();
    }

    private void SetupShoe()
    {
        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
        string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        Cards = new List<Card>(numDecks * 52);

        for (var n = 0; n < numDecks; n++)
            foreach (var suit in suits)
            foreach (var value in values)
                Cards.Add(new Card(value, suit));
        Shuffle();
    }

    public void Shuffle()
    {
        var n = Cards.Count;
        // Fisher-Yates shuffle algorithm (Knuth / Durstenfeld swap shuffle version https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle)
        for (var i = n - 1; i > 0; i--)
        {
            var j = Rnd.Rng.Next(i + 1);
            (Cards[i], Cards[j]) = (Cards[j], Cards[i]);
        }
    }


    public Card DrawCard()
    {
        if (Cards.Count == 0) throw new InvalidOperationException("Deck is empty!");
        var card = Cards[0];
        Cards.RemoveAt(0);
        return card;
    }

    public void EndOfGame()
    {
        if (Cards.Count < penetrationCut) SetupShoe();
    }
}