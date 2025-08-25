namespace BlackJack;

public enum Move
{
    Hit,
    Stand,
    Double,
    Split
}

public class Strategy
{
    public static Strategy Instance { get; set; } = new Strategy();
    public List<PairStrategyRow> PairStrategy { get; set; } = new()
    {
        new PairStrategyRow { Pair = "2,2", Vs2 = "Y/N", Vs3 = "Y/N", Vs4 = "Y", Vs5 = "Y", Vs6 = "Y", Vs7 = "Y", Vs8 = "N", Vs9 = "N", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "3,3", Vs2 = "Y/N", Vs3 = "Y/N", Vs4 = "Y", Vs5 = "Y", Vs6 = "Y", Vs7 = "Y", Vs8 = "N", Vs9 = "N", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "4,4", Vs2 = "N", Vs3 = "N", Vs4 = "N", Vs5 = "Y/N", Vs6 = "Y/N", Vs7 = "N", Vs8 = "N", Vs9 = "N", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "5,5", Vs2 = "N", Vs3 = "N", Vs4 = "N", Vs5 = "N", Vs6 = "N", Vs7 = "N", Vs8 = "N", Vs9 = "N", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "6,6", Vs2 = "Y/N", Vs3 = "Y", Vs4 = "Y", Vs5 = "Y", Vs6 = "Y", Vs7 = "N", Vs8 = "N", Vs9 = "N", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "7,7", Vs2 = "Y", Vs3 = "Y", Vs4 = "Y", Vs5 = "Y", Vs6 = "Y", Vs7 = "Y", Vs8 = "N", Vs9 = "N", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "8,8", Vs2 = "Y", Vs3 = "Y", Vs4 = "Y", Vs5 = "Y", Vs6 = "Y", Vs7 = "Y", Vs8 = "Y", Vs9 = "Y", Vs10 = "N", VsA = "Y"},
        new PairStrategyRow { Pair = "9,9", Vs2 = "Y", Vs3 = "Y", Vs4 = "Y", Vs5 = "Y", Vs6 = "Y", Vs7 = "N", Vs8 = "Y", Vs9 = "Y", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "10,10", Vs2 = "N", Vs3 = "N", Vs4 = "N", Vs5 = "N", Vs6 = "N", Vs7 = "N", Vs8 = "N", Vs9 = "N", Vs10 = "N", VsA = "N"},
        new PairStrategyRow { Pair = "11,11", Vs2 = "Y", Vs3 = "Y", Vs4 = "Y", Vs5 = "Y", Vs6 = "Y", Vs7 = "Y", Vs8 = "Y", Vs9 = "Y", Vs10 = "Y", VsA = "Y"}
    };

    public List<SoftStrategyRow> SoftStrategy { get; set; } = new()
    {
        new SoftStrategyRow { Total = 13, Vs2 = "H", Vs3 = "H", Vs4 = "H", Vs5 = "D", Vs6 = "D", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new SoftStrategyRow { Total = 14, Vs2 = "H", Vs3 = "H", Vs4 = "H", Vs5 = "D", Vs6 = "D", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new SoftStrategyRow { Total = 15, Vs2 = "H", Vs3 = "H", Vs4 = "D", Vs5 = "D", Vs6 = "D", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new SoftStrategyRow { Total = 16, Vs2 = "H", Vs3 = "H", Vs4 = "D", Vs5 = "D", Vs6 = "D", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new SoftStrategyRow { Total = 17, Vs2 = "H", Vs3 = "D", Vs4 = "D", Vs5 = "D", Vs6 = "D", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new SoftStrategyRow { Total = 18, Vs2 = "S", Vs3 = "Ds", Vs4 = "Ds", Vs5 = "Ds", Vs6 = "Ds", Vs7 = "S", Vs8 = "S", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new SoftStrategyRow { Total = 19, Vs2 = "S", Vs3 = "S", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "S", Vs8 = "S", Vs9 = "S", Vs10 = "S", VsA = "S"},
        new SoftStrategyRow { Total = 20, Vs2 = "S", Vs3 = "S", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "S", Vs8 = "S", Vs9 = "S", Vs10 = "S", VsA = "S"}
    };


    public List<HardStrategyRow> HardStrategy { get; set; } = new()
    {
        new HardStrategyRow { Total = 8, Vs2 = "H", Vs3 = "H", Vs4 = "H", Vs5 = "H", Vs6 = "H", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 9, Vs2 = "H", Vs3 = "D", Vs4 = "D", Vs5 = "D", Vs6 = "D", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 10, Vs2 = "D", Vs3 = "D", Vs4 = "D", Vs5 = "D", Vs6 = "D", Vs7 = "D", Vs8 = "D", Vs9 = "D", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 11, Vs2 = "D", Vs3 = "D", Vs4 = "D", Vs5 = "D", Vs6 = "D", Vs7 = "D", Vs8 = "D", Vs9 = "D", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 12, Vs2 = "H", Vs3 = "H", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 13, Vs2 = "S", Vs3 = "S", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 14, Vs2 = "S", Vs3 = "S", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 15, Vs2 = "S", Vs3 = "S", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 16, Vs2 = "S", Vs3 = "S", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "H", Vs8 = "H", Vs9 = "H", Vs10 = "H", VsA = "H"},
        new HardStrategyRow { Total = 17, Vs2 = "S", Vs3 = "S", Vs4 = "S", Vs5 = "S", Vs6 = "S", Vs7 = "S", Vs8 = "S", Vs9 = "S", Vs10 = "S", VsA = "S"}
    };

    public Move Decide(Player player, Card dealerUp, bool afterSplit)
    {
        var eval = HandEvaluator.Evaluate(player.Hand, false);
        var total = eval.Total;
        var isSoft = eval.IsSoft;
        var cards = player.Hand;

        var canSplit = player.CanSplit();
        // --- Pair Splitting ---
        if (canSplit)
        {
            var pair = $"{cards[0].PipValue},{cards[1].PipValue}";
            var row = PairStrategy.FirstOrDefault(r => r.Pair == pair);
            if (row != null && ParseBool(LookupAction(row, dealerUp.PipValue)))
                return Move.Split;
        }



        var canDouble = player.CanDouble(afterSplit, player.IsSplitAces);

        // --- Soft Totals ---
        if (isSoft)
        {
            if (cards.Count == 5) return Move.Hit; // Hit soft 5-card hands
            var row = SoftStrategy.FirstOrDefault(r => r.Total == total);
            if (row != null)
                return ParseMove(LookupAction(row, dealerUp.PipValue), canDouble);
        }

        // --- Hard Totals ---
        if (!isSoft)
        {

            if (total > HardStrategy.MaxBy(r => r.Total).Total)
                return Move.Stand; // fallback for totals outside the strategy range
            if (total < HardStrategy.MinBy(r => r.Total).Total)
                return Move.Hit; // fallback for totals outside the strategy range
            var row = HardStrategy.FirstOrDefault(r => r.Total == total);
            if (row != null)
                return ParseMove(LookupAction(row, dealerUp.PipValue), canDouble);
        }

        return Move.Hit;
    }
    public static Move DecideOld(Player player, Card dealerUp, bool afterSplit)
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
                    if (up >= 3 && up <= 6 && canDouble) return Move.Double;
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
    private static string LookupAction(dynamic row, int up) =>
        up switch
        {
            2 => row.Vs2,
            3 => row.Vs3,
            4 => row.Vs4,
            5 => row.Vs5,
            6 => row.Vs6,
            7 => row.Vs7,
            8 => row.Vs8,
            9 => row.Vs9,
            10 => row.Vs10,
            11 => row.VsA,
            _ => "H"
        };
    private static bool LookupBool(dynamic row, int up) =>
        up switch
        {
            2 => row.Vs2,
            3 => row.Vs3,
            4 => row.Vs4,
            5 => row.Vs5,
            6 => row.Vs6,
            7 => row.Vs7,
            8 => row.Vs8,
            9 => row.Vs9,
            10 => row.Vs10,
            11 => row.VsA,
            _ => false
        };

    private static Move ParseMove(string action, bool canDouble) => action switch
    {
        "H" => Move.Hit,
        "S" => Move.Stand,
        "D" => canDouble ? Move.Double : Move.Hit,
        "P" => Move.Split,
        _ => canDouble ? Move.Double : Move.Stand
    };
    private static bool ParseBool(string value) => value.ToLower() switch
    {
        "n" => false,
        "y" => true,
        _ => Rules.Instance.DoubleAfterSplit ? true : false
    };
}