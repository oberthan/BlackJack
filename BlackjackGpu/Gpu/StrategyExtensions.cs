using Blackjack;
using Blackjack.Gpu;

public static class StrategyExtensions
{
    private static byte ToByte(Decision d) => d switch
    {
        Decision.H => StrategyTables.H,
        Decision.S => StrategyTables.S,
        Decision.D => StrategyTables.D,
        Decision.P => StrategyTables.P,
        Decision.Ds => StrategyTables.D, // handled like D in the flat tables
        Decision.N => StrategyTables.N,
        _ => StrategyTables.H
    };

    public static StrategyTables ToTables(this Strategy strategy)
    {
        var t = new StrategyTables();

        // Hard totals
        foreach (var row in strategy.HardStrategy)
        {
            for (int up = 2; up <= 11; up++)
                t.SetHard(row.Total, up, ToByte(Lookup(row, up)));
        }

        // Soft totals
        foreach (var row in strategy.SoftStrategy)
        {
            for (int up = 2; up <= 11; up++)
                t.SetSoft(row.Total, up, ToByte(Lookup(row, up)));
        }

        // Pairs
        foreach (var row in strategy.PairStrategy)
        {
            int pairVal = (int)row.Pair;
            for (int up = 2; up <= 11; up++)
                t.SetPair(pairVal, up, ToByte(Lookup(row, up)));
        }

        return t;
    }

    private static Decision Lookup(StrategyRow row, int up) =>
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
}