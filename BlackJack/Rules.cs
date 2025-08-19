namespace BlackJack;

public static class Rules
{
    // REQUIREMENT: Dealer stands on ALL 17s (including soft 17)
    public const bool DealerStandsOnSoft17 = true;

    // REQUIREMENT: Blackjack pays 3:2
    public const double BlackjackPayout = 1.5;

    // REQUIREMENT: No resplit allowed
    public const bool AllowResplit = false;

    // REQUIREMENT: Double allowed on any first two cards
    public const bool DoubleOnAnyTwo = true;

    // REQUIREMENT: Double after split allowed except on split Aces
    public const bool DoubleAfterSplit = true;
    public const bool DoubleAfterSplitAces = false;

    // REQUIREMENT: Six Card Charlie – player wins if reaches 6 cards <= 21
    public const int SixCardCharlieCount = 6;

    // Dealer peek rule: dealer checks for blackjack when showing Ace
    public const bool DealerPeeksOnAce = true;

    // REQUIREMENT: Split only on first two cards of equal value
    public static bool CanSplitPair(Card a, Card b)
    {
        return CardValueForSplit(a) == CardValueForSplit(b);
    }

    // Face cards are same value (J/Q/K = 10) for “same value” comparisons
    private static int CardValueForSplit(Card c)
    {
        return c.Value switch
        {
            "J" or "Q" or "K" => 10,
            "A" => 11, // just to distinguish Aces as a pair
            _ => int.Parse(c.Value)
        };
    }
}