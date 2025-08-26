using System.Collections.Concurrent;

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

public class HandEvaluator
{
    public static HandEvaluator Instance = new HandEvaluator();

    private ConcurrentDictionary<ulong, HandEval> _cache = new ConcurrentDictionary<ulong, HandEval>();

    // Helper to generate a unique key for a hand
    private ulong GetHandKey(IReadOnlyList<Card> hand, bool treatAsBJ)
    {
        ulong key = 0;
        for (int i = 0; i < hand.Count && i < 8; i++)
        {
            // Use 8 bits per card: 4 bits for pip, 4 bits for suit (assuming <= 16 values each)
            // This is a simple hash, not cryptographically secure
            int pip = hand[i].PipValue & 0xF;
            key |= (uint)(pip << i * 4);
        }
        key |= (ulong)hand.Count << 60; // encode count in high bits
        if (treatAsBJ)
        {
            key |= 1UL << 59; // flag for treating 2-card 21 as blackjack
        }
        return key;
    }

    public HandEval Evaluate(IReadOnlyList<Card> hand, bool treatTwoCard21AsBlackjack)
    {
        ulong key = GetHandKey(hand, treatTwoCard21AsBlackjack);
        if (_cache.TryGetValue(key, out var cachedEval))
        {
            return cachedEval;
        }

        int total = 0, aces = 0;
        int count = hand.Count;

        for (int i = 0; i < count; i++)
        {
            var card = hand[i];
            var value = card.Value;
            if (value.Length == 1 && value[0] == 'A')
            {
                total += 11;
                aces++;
            }
            else
            {
                total += card.PipValue;
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
        _cache.TryAdd(key, eval);
        return eval;
    }
}