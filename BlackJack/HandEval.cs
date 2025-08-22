namespace BlackJack;

public readonly struct HandEval
{
    public int Total { get; }
    public bool IsSoft { get; }
    public bool IsBlackjack { get; } // only for original 2-card hands (not split 21)

    public HandEval(int total, bool isSoft, bool isBlackjack)
    {
        Total = total;
        IsSoft = isSoft;
        IsBlackjack = isBlackjack;
    }
}

public static class HandEvaluator
{
    public static HandEval Evaluate(IReadOnlyList<Card> hand, bool treatTwoCard21AsBlackjack)
    {
        var total = 0;
        var aces = 0;

        foreach (var c in hand)
            if (c.Value == "A")
            {
                total += 11;
                aces++;
            }
            else
            {
                total += c.PipValue;
            }

        // soften as needed
        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        var isSoft = aces > 0 && total <= 20;
        var isBJ = treatTwoCard21AsBlackjack && hand.Count == 2 && total == 21;

        return new HandEval(total, isSoft, isBJ);
    }
}