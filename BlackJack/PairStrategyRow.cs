using Blackjack;

public class StrategyRow
{
    public StrategyRow Clone()
    {
        return (StrategyRow)this.MemberwiseClone();
    }

    public int Total { get; set; }
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
    public new PairStrategyRow Clone()
    {
        return (PairStrategyRow)base.Clone();
    }
    public CardValue Pair { get; set; }
}

public class SoftStrategyRow : StrategyRow
{
    public new SoftStrategyRow Clone()
    {
        return (SoftStrategyRow)base.Clone();
    }
}

public class HardStrategyRow : StrategyRow
{
    public new HardStrategyRow Clone()
    {
        return (HardStrategyRow)base.Clone();
    }
}
