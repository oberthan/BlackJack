namespace Blackjack;

internal class Statistics
{
    public int TotalRounds { get; private set; }
    public int TotalStake { get; private set; }
    public int TotalReturn { get; private set; }

    public int NumBlackjacks { get; private set; }
    public int NumSplits { get; private set; }
    public int NumDoubles { get; private set; }

    public double AverageBet => TotalRounds == 0 ? 0 : (double)TotalStake / TotalRounds;
    public double ROI => TotalStake == 0 ? 0 : (double)TotalReturn / TotalStake;

    public void RecordRound(int stake, int result, bool Blackjack, bool split, bool doubled)
    {
        TotalRounds++;
        TotalStake += stake;
        TotalReturn += result;

        if (Blackjack) NumBlackjacks++;
        if (split) NumSplits++;
        if (doubled) NumDoubles++;
    }
}