using System.ComponentModel;

namespace Blackjack;

public enum Move
{
    Hit,
    Stand,
    Double,
    Split
}

public enum Decision
{
    H, // Hit
    S, // Stand
    D, // Double if allowed, otherwise Hit
    Ds, // Double if allowed, otherwise Stand
    P, // Split
    Ph, // Split if allowed, otherwise Hit
    Ps, // Split if allowed, otherwise Stand
    Rh, // Surrender if allowed, otherwise Hit
    Rs, // Surrender if allowed, otherwise Stand
    N  // Not possible (for pairs)
}

public class Strategy
{
    public static Strategy Instance { get; set; } = new Strategy();
    public List<PairStrategyRow> PairStrategy { get; set; } = new()
    {
        new PairStrategyRow { Pair = "2,2", Vs2 = Decision.P, Vs3 = Decision.P, Vs4 = Decision.P, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.P, Vs8 = Decision.N, Vs9 = Decision.N, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "3,3", Vs2 = Decision.P, Vs3 = Decision.P, Vs4 = Decision.P, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.P, Vs8 = Decision.N, Vs9 = Decision.N, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "4,4", Vs2 = Decision.N, Vs3 = Decision.N, Vs4 = Decision.N, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.N, Vs8 = Decision.N, Vs9 = Decision.N, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "5,5", Vs2 = Decision.N, Vs3 = Decision.N, Vs4 = Decision.N, Vs5 = Decision.N, Vs6 = Decision.N, Vs7 = Decision.N, Vs8 = Decision.N, Vs9 = Decision.N, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "6,6", Vs2 = Decision.P, Vs3 = Decision.P, Vs4 = Decision.P, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.N, Vs8 = Decision.N, Vs9 = Decision.N, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "7,7", Vs2 = Decision.P, Vs3 = Decision.P, Vs4 = Decision.P, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.P, Vs8 = Decision.N, Vs9 = Decision.N, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "8,8", Vs2 = Decision.P, Vs3 = Decision.P, Vs4 = Decision.P, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.P, Vs8 = Decision.P, Vs9 = Decision.P, Vs10 = Decision.P, VsA = Decision.P},
        new PairStrategyRow { Pair = "9,9", Vs2 = Decision.P, Vs3 = Decision.P, Vs4 = Decision.P, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.N, Vs8 = Decision.P, Vs9 = Decision.P, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "10,10", Vs2 = Decision.N, Vs3 = Decision.N, Vs4 = Decision.N, Vs5 = Decision.N, Vs6 = Decision.N, Vs7 = Decision.N, Vs8 = Decision.N, Vs9 = Decision.N, Vs10 = Decision.N, VsA = Decision.N},
        new PairStrategyRow { Pair = "11,11", Vs2 = Decision.P, Vs3 = Decision.P, Vs4 = Decision.P, Vs5 = Decision.P, Vs6 = Decision.P, Vs7 = Decision.P, Vs8 = Decision.P, Vs9 = Decision.P, Vs10 = Decision.P, VsA = Decision.P}
    };

    public List<SoftStrategyRow> SoftStrategy { get; set; } = new()
    {
        new SoftStrategyRow { Total = 13, Vs2 = Decision.H, Vs3 = Decision.H, Vs4 = Decision.H, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new SoftStrategyRow { Total = 14, Vs2 = Decision.H, Vs3 = Decision.H, Vs4 = Decision.H, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new SoftStrategyRow { Total = 15, Vs2 = Decision.H, Vs3 = Decision.H, Vs4 = Decision.D, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new SoftStrategyRow { Total = 16, Vs2 = Decision.H, Vs3 = Decision.H, Vs4 = Decision.D, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new SoftStrategyRow { Total = 17, Vs2 = Decision.H, Vs3 = Decision.D, Vs4 = Decision.D, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new SoftStrategyRow { Total = 18, Vs2 = Decision.S, Vs3 = Decision.Ds, Vs4 = Decision.Ds, Vs5 = Decision.Ds, Vs6 = Decision.Ds, Vs7 = Decision.S, Vs8 = Decision.S, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new SoftStrategyRow { Total = 19, Vs2 = Decision.S, Vs3 = Decision.S, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.S, Vs8 = Decision.S, Vs9 = Decision.S, Vs10 = Decision.S, VsA = Decision.S},
        new SoftStrategyRow { Total = 20, Vs2 = Decision.S, Vs3 = Decision.S, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.S, Vs8 = Decision.S, Vs9 = Decision.S, Vs10 = Decision.S, VsA = Decision.S}
    };


    public List<HardStrategyRow> HardStrategy { get; set; } = new()
    {
        new HardStrategyRow { Total = 8, Vs2 = Decision.H, Vs3 = Decision.H, Vs4 = Decision.H, Vs5 = Decision.H, Vs6 = Decision.H, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 9, Vs2 = Decision.H, Vs3 = Decision.D, Vs4 = Decision.D, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 10, Vs2 = Decision.D, Vs3 = Decision.D, Vs4 = Decision.D, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.D, Vs8 = Decision.D, Vs9 = Decision.D, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 11, Vs2 = Decision.D, Vs3 = Decision.D, Vs4 = Decision.D, Vs5 = Decision.D, Vs6 = Decision.D, Vs7 = Decision.D, Vs8 = Decision.D, Vs9 = Decision.D, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 12, Vs2 = Decision.H, Vs3 = Decision.H, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 13, Vs2 = Decision.S, Vs3 = Decision.S, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 14, Vs2 = Decision.S, Vs3 = Decision.S, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 15, Vs2 = Decision.S, Vs3 = Decision.S, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 16, Vs2 = Decision.S, Vs3 = Decision.S, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.H, Vs8 = Decision.H, Vs9 = Decision.H, Vs10 = Decision.H, VsA = Decision.H},
        new HardStrategyRow { Total = 17, Vs2 = Decision.S, Vs3 = Decision.S, Vs4 = Decision.S, Vs5 = Decision.S, Vs6 = Decision.S, Vs7 = Decision.S, Vs8 = Decision.S, Vs9 = Decision.S, Vs10 = Decision.S, VsA = Decision.S}
    };

    private readonly int hardStrategyMaxTotal = 17;
    private readonly int hardStrategyMinTotal = 8;

    public Move Decide(Player player, Card dealerUp, bool afterSplit)
    {
        var eval = HandEvaluator.Instance.Evaluate(player.Hand, false);
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
            // six card charlie strats
            if (cards.Count == 5 && total >= 19) return Move.Hit;
            if (cards.Count >= 4 && total == 18 && !(dealerUp.PipValue >= 3 && dealerUp.PipValue <= 6)) return Move.Hit;
            if (cards.Count >= 4 && total == 19 && dealerUp.PipValue == 10) return Move.Hit;


            var row = SoftStrategy.FirstOrDefault(r => r.Total == total);
            if (row != null)
                return ParseMove(LookupAction(row, dealerUp.PipValue), canDouble);
        }

        // --- Hard Totals ---
        if (!isSoft)
        {

            // six card charlie strats
            if (cards.Count >= 4 && total == 12 && dealerUp.PipValue >= 4 && dealerUp.PipValue <= 6) return Move.Hit;
            if (cards.Count >= 4 && total == 13 && dealerUp.PipValue == 2 && dealerUp.PipValue == 3) return Move.Hit;
            if (cards.Count == 5 && total >= 13 && total <= 15 && dealerUp.PipValue >= 2 && dealerUp.PipValue <= 6) return Move.Hit;
            if (cards.Count == 5 && total == 16 && dealerUp.PipValue == 2 && dealerUp.PipValue == 3) return Move.Hit;
            if (cards.Count == 5 && total == 17 && dealerUp.PipValue >= 9 && dealerUp.PipValue <= 11) return Move.Hit;


            if (total > hardStrategyMaxTotal)
                return Move.Stand; // fallback for totals outside the strategy range
            if (total < hardStrategyMinTotal)
                return Move.Hit; // fallback for totals outside the strategy range
            var row = HardStrategy.FirstOrDefault(r => r.Total == total);
            if (row != null)
                return ParseMove(LookupAction(row, dealerUp.PipValue), canDouble);
        }

        return Move.Hit;
    }
    public static Move DecideOld(Player player, Card dealerUp, bool afterSplit)
    {
        var eval = HandEvaluator.Instance.Evaluate(player.Hand, false);
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
    private static Decision LookupAction(StrategyRow row, int up) =>
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
            _ => Decision.H
        };
    //private static string LookupAction(dynamic row, int up) =>
    //    up switch
    //    {
    //        2 => row.Vs2,
    //        3 => row.Vs3,
    //        4 => row.Vs4,
    //        5 => row.Vs5,
    //        6 => row.Vs6,
    //        7 => row.Vs7,
    //        8 => row.Vs8,
    //        9 => row.Vs9,
    //        10 => row.Vs10,
    //        11 => row.VsA,
    //        _ => "H"
    //    };

    private static Move ParseMove(Decision action, bool canDouble) => action switch
    {
        Decision.H => Move.Hit,
        Decision.S => Move.Stand,
        Decision.D => canDouble ? Move.Double : Move.Hit,
        Decision.P => Move.Split,
        Decision.Ds => canDouble ? Move.Double : Move.Stand
    };
    private static bool ParseBool(Decision value) => value switch
    {
        Decision.N => false,
        Decision.P => true,
        _ => Rules.Instance.DoubleAfterSplit
    };
}