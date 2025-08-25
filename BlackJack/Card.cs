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
}