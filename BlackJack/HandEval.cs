namespace Blackjack;

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
    public static HandEval Evaluate(List<CardValue> hand, bool treatTwoCard21AsBlackjack)
    {
        int total = 0, aces = 0;
        int count = hand.Count;

        for (int i = 0; i < count; i++)
        {
            var card = hand[i];
            var value = card;
            total += (int)value;
            if (value == CardValue.Ace)
            {
                aces++;
            }
        }

        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        var isSoft = aces > 0 && total <= 20;
        var isBJ = treatTwoCard21AsBlackjack && hand.Count == 2 && total == 21;

        var eval = new HandEval(total, isSoft, isBJ);
        return eval;
    }
}