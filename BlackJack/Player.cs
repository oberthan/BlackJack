namespace Blackjack;

public class Player
{
    public List<Card> Hand { get; } = new();
    public int Bet { get; set; } = 1;
    public SplitPlayer? SplitHandPlayer { get; private set; }


    public bool DidBlackjack { get; set; }
    public bool DidSplit { get; set; }
    public bool DidDouble { get; set; }

    public bool IsSplitAces => SplitHandPlayer != null && Hand.Count == 2 && Hand[0].Value == "A";
    public bool IsOriginalAces => Hand.Count == 2 && Hand[0].Value == "A" && Hand[1].Value == "A";


    public void Reset()
    {
        Hand.Clear();
        SplitHandPlayer = null;
        Bet = 1;

        DidBlackjack = false;
        DidSplit = false;
        DidDouble = false;
    }

    public void AddCard(Card card)
    {
        Hand.Add(card);
    }


    //------------- Split functionality -------------//

    public virtual bool CanSplit()
    {
        if (Hand.Count != 2 || SplitHandPlayer != null) return false;
        if (!Rules.Instance.AllowSplit) return false;
        return Rules.Instance.CanSplitPair(Hand[0], Hand[1]);
    }

    public void Split(Deck deck)
    {
        if (!CanSplit()) throw new InvalidOperationException("Cannot split");

        SplitHandPlayer = new SplitPlayer(Bet);

        // move second card to split hand
        var second = Hand[1];
        Hand.RemoveAt(1);
        SplitHandPlayer.AddCard(second);

        // deal one new card to each
        AddCard(deck.DrawCard());
        SplitHandPlayer.AddCard(deck.DrawCard());

        // REQUIREMENT: Split Aces cannot be hit further (and usually not Blackjack-qualified later)
        // We don't enforce here — Game will enforce “no hits after split A” when playing.
    }


    //------------- Double Functionality -------------//

    public bool CanDouble(bool afterSplit, bool thisHandIsSplitAces)
    {
        if (Hand.Count != 2) return false;
        if (!Rules.Instance.AllowDouble) return false;
        if (!Rules.Instance.DoubleOnAnyTwo) return false;
        if (afterSplit && !Rules.Instance.DoubleAfterSplit) return false;
        if (afterSplit && !Rules.Instance.DoubleAfterSplit11 && GetHandValue() == 11) return false;
        if (afterSplit && thisHandIsSplitAces && !Rules.Instance.DoubleAfterSplitAces) return false;
        return true;
    }

    public void DoubleDown(Deck deck)
    {
        Bet *= 2;
        AddCard(deck.DrawCard()); // REQUIREMENT: double receives exactly one card, then stands (Game enforces stand)
    }


    public void Play(Deck deck)
    {
    }

    public int GetHandValue()
    {
        var total = 0;
        var aces = 0;

        foreach (var card in Hand)
            if (card.Value == "A")
            {
                total += 11;
                aces++;
            }
            else if (card.Value is "J" or "Q" or "K")
            {
                total += 10;
            }
            else
            {
                total += int.Parse(card.Value);
            }

        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        return total;
    }

    public bool IsBusted()
    {
        return GetHandValue() > 21;
    }

    public bool HasBlackjack()
    {
        return GetHandValue() == 21 && Hand.Count == 2;
    }
}

public class SplitPlayer : Player
{
    public SplitPlayer(int originalBet)
    {
        Bet = originalBet; // same bet as parent
    }

    public override bool CanSplit()
    {
        return false;
    }
}