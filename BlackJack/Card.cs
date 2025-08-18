namespace BlackJack;

internal class Card
{
    public Card(string value, string suit)
    {
        Value = value;
        Suit = suit;
    }

    public string Suit { get; }
    public string Value { get; }

    public int PipValue => Value switch
    {
        "J" or "Q" or "K" => 10,
        "A" => 11, // treated as 11 first; logic adjusts for soft
        _ => int.Parse(Value)
    };

    public override string ToString()
    {
        return $"{Value} of {Suit}";
    }
}