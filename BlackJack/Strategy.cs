namespace BlackJack;

public enum Move
{
    Hit,
    Stand,
    Double,
    Split
}

public static class Strategy
{
    public static Move Decide(Player player, Card dealerUp, bool afterSplit)
    {
        var eval = HandEvaluator.Evaluate(player.Hand, false);
        var total = eval.Total;
        var isSoft = eval.IsSoft;
        var up = dealerUp.PipValue; // 2–11 where 11 = Ace
        var cards = player.Hand;

        // --- Pair Splitting ---
        if (!afterSplit && cards.Count == 2 && cards[0].PipValue == cards[1].PipValue)
        {
            var pairVal = cards[0].PipValue;

            if (pairVal == 11) return Move.Split; // AA
            if (pairVal == 8) return Move.Split; // 8s
            if (pairVal == 9 && up >= 2 && up <= 9 && up != 7) return Move.Split;
            if (pairVal == 7 && up <= 7) return Move.Split;
            if (pairVal == 6 && up <= 6) return Move.Split;
            if (pairVal == 4 && (up == 5 || up == 6)) return Move.Split;
            if (pairVal == 3 && up <= 7) return Move.Split;
            if (pairVal == 2 && up <= 7) return Move.Split;
        }

        // --- Soft Totals (A + something) ---
        var canDouble = player.CanDouble(afterSplit, player.IsSplitAces);
        if (isSoft)
            switch (total)
            {
                case 20: return Move.Stand;
                case 19: return Move.Stand;
                case 18:
                    if (up >= 3 && up <= 6) return Move.Double;
                    if (up == 9 || up == 10 || up == 11) return Move.Hit;
                    return Move.Stand;
                case 17:
                    return up >= 3 && up <= 6 && canDouble ? Move.Double : Move.Hit;
                case 16:
                case 15:
                    return up >= 4 && up <= 6 && canDouble ? Move.Double : Move.Hit;
                case 14:
                case 13:
                    return up == 5 || up == 6 && canDouble ? Move.Double : Move.Hit;
            }

        // --- Hard Totals ---
        if (!isSoft)
        {
            if (total >= 17) return Move.Stand;
            if (total >= 13 && total <= 16)
                return up >= 2 && up <= 6 ? Move.Stand : Move.Hit;
            if (total == 12)
                return up >= 4 && up <= 6 ? Move.Stand : Move.Hit;
            if (total == 11) return up <= 10 && canDouble ? Move.Double : Move.Hit;
            if (total == 10) return up <= 9 && canDouble ? Move.Double : Move.Hit;
            if (total == 9) return up >= 3 && up <= 6 && canDouble ? Move.Double : Move.Hit;
            if (total <= 8) return Move.Hit;
        }

        return Move.Stand; // fallback
    }
}