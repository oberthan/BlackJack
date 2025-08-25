namespace Blackjack;

using System.Collections.Concurrent;

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
    // Cache for hand evaluations
    private static readonly ConcurrentDictionary<(string, bool), HandEval> _evalCache = new();

    public static HandEval Evaluate(IReadOnlyList<Card> hand, bool treatTwoCard21AsBlackjack)
    {
        // Create a cache key based on card values/suits and the flag
        var key = (string.Join("|", hand.Select(c => c.Value + c.Suit)), treatTwoCard21AsBlackjack);
        if (_evalCache.TryGetValue(key, out var cached))
            return cached;

        int total = 0, aces = 0;
        int count = hand.Count;

        // Use for-loop for better performance and cache properties
        for (int i = 0; i < count; i++)
        {
            var card = hand[i];
            var value = card.Value;
            if (value.Length == 1 && value[0] == 'A') // Faster than string comparison
            {
                total += 11;
                aces++;
            }
            else
            {
                total += card.PipValue;
            }
        }

        // soften as needed
        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        var isSoft = aces > 0 && total <= 20;
        var isBJ = treatTwoCard21AsBlackjack && hand.Count == 2 && total == 21;
        var result = new HandEval(total, isSoft, isBJ);
        _evalCache[key] = result;
        return result;
    }
}