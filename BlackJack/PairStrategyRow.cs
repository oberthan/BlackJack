using Blackjack;


public class StrategyRow
{
    public Decision Vs2;
    public Decision Vs3;
    public Decision Vs4;
    public Decision Vs5;
    public Decision Vs6;
    public Decision Vs7;
    public Decision Vs8;
    public Decision Vs9;
    public Decision Vs10;
    public Decision VsA;
}


public class PairStrategyRow : StrategyRow
{
    public CardValue Pair;
}

public class SoftStrategyRow : StrategyRow
{
    public int Total;
}

public class HardStrategyRow : StrategyRow
{
    public int Total;
}