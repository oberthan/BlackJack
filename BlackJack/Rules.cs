namespace Blackjack;

public class Rules
{
    public static Rules Instance { get; set; } = new Rules();
    // REQUIREMENT: Dealer stands on ALL 17s (including soft 17)
    public bool DealerStandsOnSoft17 { get; set; } = true;

    // REQUIREMENT: Blackjack pays 3:2
    public double BlackjackPayout { get; set; } = 1.5;

    public double Penetration { get; set; } = 0.7; // 70% penetration

    public bool AllowSplit { get; set; } = true;

    // REQUIREMENT: No resplit allowed
    public bool AllowResplit { get; set; } = false;

    public bool AllowDouble { get; set; } = true;

    // REQUIREMENT: Double allowed on any first two cards
    public bool DoubleOnAnyTwo { get; set; } = true;

    // REQUIREMENT: Double after split allowed except on split Aces
    public bool DoubleAfterSplit { get; set; } = true;
    public bool DoubleAfterSplit11 { get; set; } = true; // 11 = total,
    public bool DoubleAfterSplitAces { get; set; } = false;

    // REQUIREMENT: Six Card Charlie – player wins if reaches 6 cards <= 21
    public int SixCardCharlieCount { get; set; } = 6;

    // Dealer peek rule: dealer checks for Blackjack when showing Ace
    public bool DealerPeeksOnAce { get; set; } = true;

    // REQUIREMENT: Split only on first two cards of equal value
    public bool CanSplitPair(CardValue a, CardValue b)
    {
        return (int)a == (int)b;
    }

    public double UpperLimit = 3;
    public double LowerLimit = -3;
    public double Cashback = 0.1; // 10% cashback on losses
}