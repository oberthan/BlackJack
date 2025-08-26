namespace Blackjack;

public class Card
{
    public Card(string value, string suit)
    {
        Value = value;
        Suit = suit;

        int val = Value[0] switch
        {
            'J' or 'Q' or 'K' or '1' => 10,
            'A' => 11,
            _ => int.Parse(Value)
        };
        PipValue = val;
    }

    public string Suit { get; }
    public string Value { get; }

    // Cache for PipValue
    public int PipValue { get; }

    public override string ToString()
    {
        return $"{Value} of {Suit}";
    }
    
    public static implicit operator CardValue(Card card)
    {
        return (CardValue)card.PipValue;
    }
}

public enum CardValue
{
    Ace = 11,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 10,
    Queen = 10,
    King = 10,
    Mask = 0x0F // lower 4 bits
}

public enum CardSuit
{
    Hearts = 1 << 4,
    Diamonds = 2 << 4,
    Clubs = 3 << 4,
    Spades = 4 << 4
}