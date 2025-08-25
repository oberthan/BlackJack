namespace Blackjack;

public class Card
{
    public Card(string value, string suit)
    {
        Value = value;
        Suit = suit;
    }

    public string Suit { get; }
    public string Value { get; }

    public int PipValue => Value[0] switch
            {
                'J' or 'Q' or 'K' or '1' => 10,
        'A' => 11, // treated as 11 first; logic adjusts for soft
                _ => int.Parse(Value)
            };

    public override string ToString()
    {
        return $"{Value} of {Suit}";
    }
}