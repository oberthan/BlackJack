namespace Blackjack;

public readonly struct HandEval(byte total, bool isSoft, bool isBlackjack)
{
    public readonly byte Total = total;
    public readonly bool IsSoft = isSoft;
    public readonly bool IsBlackjack = isBlackjack; // only for original 2-card hands (not split 21)
}

public static class HandEvaluator
{
    public static HandEval Evaluate(List<CardValue> hand, bool treatTwoCard21AsBlackjack)
    {
        byte total = 0;
        int aces = 0;
        int count = hand.Count;

        for (int i = 0; i < count; i++)
        {
            var card = hand[i];
            var value = card;
            total += (byte)value;
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