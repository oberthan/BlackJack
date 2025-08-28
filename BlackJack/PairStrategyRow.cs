using Blackjack;

public class StrategyRow
{
    public Decision Vs2 { get; set; }
    public Decision Vs3 { get; set; }
    public Decision Vs4 { get; set; }
    public Decision Vs5 { get; set; }
    public Decision Vs6 { get; set; }
    public Decision Vs7 { get; set; }
    public Decision Vs8 { get; set; }
    public Decision Vs9 { get; set; }
    public Decision Vs10 { get; set; }
    public Decision VsA { get; set; }
}

public class PairStrategyRow : StrategyRow
{
    public CardValue Pair { get; set; }
}

public class SoftStrategyRow : StrategyRow
{
    public int Total { get; set; }
}

public class HardStrategyRow : StrategyRow
{
    public int Total { get; set; }
}
