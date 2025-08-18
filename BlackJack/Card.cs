namespace BlackjackApp
{
    class Card
    {
        public string Suit { get; }
        public string Value { get; }

        public Card(string value, string suit)
        {
            Value = value;
            Suit = suit;
        }

        public override string ToString() => $"{Value} of {Suit}";
    }
}
