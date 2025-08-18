namespace BlackJack;

internal class Deck
{
    private readonly int numDecks;
    private readonly int penetrationCut; // remaining-card threshold to reshuffle
    private readonly Random rng = new();
    private List<Card> cards;

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

        cards = new List<Card>(numDecks * 52);

        for (var n = 0; n < numDecks; n++)
            foreach (var suit in suits)
            foreach (var value in values)
                cards.Add(new Card(value, suit));
        Shuffle();
    }

    public void Shuffle()
    {
        var n = cards.Count;
        for (var i = n - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
    }


    public Card DrawCard()
    {
        if (cards.Count == 0) throw new InvalidOperationException("Deck is empty!");
        var card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    public void EndOfGame()
    {
        if (cards.Count < penetrationCut) SetupShoe();
    }
}