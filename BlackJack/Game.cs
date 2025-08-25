namespace Blackjack;

public enum Outcome
{
    PlayerWin,
    Bust, // player busts
    DealerWin,
    Push,
    DealerBust, // dealer busts
    PlayerBlackjack, // player has Blackjack
    DealerBlackjack, // dealer has Blackjack
    PlayerWinWithCharlie // player wins with Charlie

}

public readonly struct RoundResult(Outcome o,double units, int stake, bool Blackjack, bool split, bool doubled)
{
    public Outcome Outcome { get; } = o;
    public double UnitsWonOrLost { get; } = units;
    public int Stake { get; } = stake;
    public bool Blackjack { get; } = Blackjack;
    public bool Split { get; } = split;
    public bool Double { get; } = doubled;
}

public class Game
{
    public readonly Dealer dealer = new(); // dealer uses same Player class but different flow
    public readonly Deck deck = new(); // 8-deck shoe with 0.7 penetration by default
    public readonly Player player = new();

    public void Reset()
    {
        deck.SetupShoe();
        dealer.Reset();
        player.Reset();
    }
    public RoundResult PlayOneRound()
    {
        player.Reset();
        dealer.Reset();
        deck.EndOfGame();

        // initial deal
        player.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard()); // dealer upcard
        player.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard()); // dealer hole card

        return PlayOneRoundWithHand();
    }

    public RoundResult PlayOneRoundWithHand()
    {
        // evaluate Blackjacks (initial only)
        var pEval = HandEvaluator.Evaluate(player.Hand, true);
        var dEval = HandEvaluator.Evaluate(dealer.Hand, true);
        if (pEval.IsBlackjack) player.DidBlackjack = true; // track for later

        if (InitialCheckForBlackjack(dEval, pEval, out var playOneRoundWithHand)) return playOneRoundWithHand;


        // PLAYER TURN(s)
        var netUnits = PlayerTurn();

        // DEALER TURN
        if (DealerTurn(dEval, out var roundResult)) return roundResult;

        // Resolve outcomes for hands that need comparing
        int mainResult = GetHandOutcome(player, false);
        netUnits += SettleAgainstDealer(player, false);
        Outcome mainOutcome = GetOutcomeFromResult(mainResult, player, dealer);

        Outcome? splitOutcome = null;
        if (player.SplitHandPlayer != null)
        {
            int splitResult = GetHandOutcome(player.SplitHandPlayer, true);
            netUnits += SettleAgainstDealer(player.SplitHandPlayer, true);
            splitOutcome = GetOutcomeFromResult(splitResult, player.SplitHandPlayer, dealer);
        }

        int totalStake = player.Bet; // original hand (already doubled if double down)
        if (player.SplitHandPlayer != null)
            totalStake += player.SplitHandPlayer.Bet;

        return Summarize(mainOutcome, splitOutcome, netUnits, totalStake);
    }

    private RoundResult Summarize(Outcome mainOutcome, Outcome? splitOutcome, int netUnits, int totalStake)
    {
        if (mainOutcome == Outcome.PlayerWinWithCharlie || splitOutcome == Outcome.PlayerWinWithCharlie)
            return new RoundResult(Outcome.PlayerWinWithCharlie, netUnits, totalStake, player.DidBlackjack, player.DidSplit, player.DidDouble);

        // Dealer blackjack (after Charlie check)
        if (mainOutcome == Outcome.DealerBlackjack || splitOutcome == Outcome.DealerBlackjack)
            return new RoundResult(Outcome.DealerBlackjack, netUnits, totalStake, player.DidBlackjack, player.DidSplit, player.DidDouble);

        if (mainOutcome == Outcome.Bust || splitOutcome == Outcome.Bust)
            return new RoundResult(Outcome.Bust, netUnits, totalStake, player.DidBlackjack, player.DidSplit, player.DidDouble);

        if (mainOutcome == Outcome.DealerBust || splitOutcome == Outcome.DealerBust)
            return new RoundResult(Outcome.DealerBust, netUnits, totalStake, player.DidBlackjack, player.DidSplit, player.DidDouble);

        if (netUnits > 0)
            return new RoundResult(Outcome.PlayerWin, netUnits, totalStake, player.DidBlackjack, player.DidSplit,
                player.DidDouble);
        if (netUnits < 0)
            return new RoundResult(Outcome.DealerWin, netUnits, totalStake, player.DidBlackjack, player.DidSplit,
                player.DidDouble);
        return new RoundResult(Outcome.Push, 0, totalStake, player.DidBlackjack, player.DidSplit, player.DidDouble);
    }

    private bool DealerTurn(HandEval dEval, out RoundResult roundResult)
    {
        if (dEval.IsBlackjack)
        {
            roundResult = new RoundResult(Outcome.DealerBlackjack, -player.Bet, player.Bet, player.DidBlackjack, player.DidSplit,
                player.DidDouble);
            return true;
        }

        dealer.Play(deck);

        roundResult = default;
        return false;
    }

    private int PlayerTurn()
    {
        var netUnits = 0;
        var afterSplit = false;

        // Optional very-simple strategy:
        // - Split Aces always; otherwise split only equal 8s; no resplit allowed by design.


        // Play a single hand and (optionally) the split hand
        var unitsMain = PlaySingleHand(player, afterSplit, false);
        if (player.SplitHandPlayer != null)
        {
            var unitsSplit = PlaySingleHand(player.SplitHandPlayer, true,
                player.SplitHandPlayer.Hand[0].Value == "A");
            // Dealer plays once for both hands (standard shoe game): delay dealer play until both hands done.
            netUnits += unitsSplit; // will be combined after dealer plays
        }

        return netUnits;
    }

    private bool InitialCheckForBlackjack(HandEval dEval, HandEval pEval, out RoundResult playOneRoundWithHand)
    {
        // REQUIREMENT: Dealer peeks for Blackjack when showing Ace
        if (Rules.Instance.DealerPeeksOnAce && dealer.Hand[0].Value == "A")
            if (dEval.IsBlackjack)
            {
                if (pEval.IsBlackjack)
                {
                    playOneRoundWithHand = new RoundResult(Outcome.Push, 0, player.Bet, player.DidBlackjack, player.DidSplit,
                        player.DidDouble);
                    return true;
                }

                playOneRoundWithHand = new RoundResult(Outcome.DealerBlackjack, -player.Bet, player.Bet, player.DidBlackjack, player.DidSplit,
                    player.DidDouble);
                return true;
            }

        // If dealer upcard not Ace, we still need to handle natural BJ payoff
        if (pEval.IsBlackjack)
        {
            if (dEval.IsBlackjack)
            {
                playOneRoundWithHand = new RoundResult(Outcome.Push, 0, player.Bet, player.DidBlackjack, player.DidSplit,
                    player.DidDouble);
                return true;
            }

            // REQUIREMENT: Blackjack pays 3:2
            var units = player.Bet * Rules.Instance.BlackjackPayout;
            playOneRoundWithHand = new RoundResult(Outcome.PlayerBlackjack, units, player.Bet, player.DidBlackjack, player.DidSplit,
                player.DidDouble);
            return true;
        }

        // Ensure out parameter is always assigned
        playOneRoundWithHand = default;
        return false;
    }

    // Helper to determine hand outcome for all enum values
    private int GetHandOutcome(Player handOwner, bool isSplitHand)
    {
        var pEval = HandEvaluator.Evaluate(handOwner.Hand, !isSplitHand);
        var dEval = HandEvaluator.Evaluate(dealer.Hand, false);

        // Six Card Charlie
        if (handOwner.Hand.Count >= Rules.Instance.SixCardCharlieCount && pEval.Total <= 21)
            return 1000; // special code for Charlie

        if (pEval.Total > 21) return -1000; // bust
        if (dEval.Total > 21) return 100; // dealer bust

        if (pEval.IsBlackjack && !isSplitHand) return 500; // player Blackjack
        if (dEval.IsBlackjack) return -500; // dealer Blackjack

        if (pEval.Total > dEval.Total) return 1;
        if (pEval.Total < dEval.Total) return -1;
        return 0;
    }

    private Outcome GetOutcomeFromResult(int result, Player handOwner, Player dealer)
    {
        if (result == 1000) return Outcome.PlayerWinWithCharlie;
        if (result == -1000) return Outcome.Bust;
        if (result == 100) return Outcome.DealerBust;
        if (result == 500) return Outcome.PlayerBlackjack;
        if (result == -500) return Outcome.DealerBlackjack;
        if (result == 1) return Outcome.PlayerWin;
        if (result == -1) return Outcome.DealerWin;
        return Outcome.Push;
    }


    private int PlaySingleHand(Player handOwner, bool afterSplit, bool isSplitAces)
    {


        while (true)
        {
            if (isSplitAces)
                return 0; // only one card dealt, no further play
            var eval = HandEvaluator.Evaluate(handOwner.Hand, false);

            // Six Card Charlie
            if (handOwner.Hand.Count >= Rules.Instance.SixCardCharlieCount && eval.Total <= 21)
                return 0;

            // Get strategy action
            var action = Strategy.Instance.Decide(handOwner, dealer.Hand[0], afterSplit);
/*            var oldAction = Strategy.DecideOld(handOwner, dealer.Hand[0], afterSplit);
            if (action != oldAction)
            {
                throw new InvalidOperationException(
                    $"Strategy mismatch: {action} vs {oldAction} for hand {handOwner.Hand[0]} and dealer upcard {dealer.Hand[0]}");
            }
*/
            switch (action)
            {
                case Move.Hit:
                    handOwner.AddCard(deck.DrawCard());
                    if (HandEvaluator.Evaluate(handOwner.Hand, false).Total > 21)
                        return 0; // bust
                    break;

                case Move.Stand:
                    return 0;

                case Move.Double:
                    if (handOwner.CanDouble(afterSplit, false))
                    {
                        handOwner.DoubleDown(deck);
                        handOwner.DidDouble = true; // track for later
                    }

                    return 0; // after doubling you always stop

                case Move.Split:
                    if (handOwner.CanSplit())
                    {
                        handOwner.Split(deck);
                        handOwner.DidSplit = true; // track for later

                        if (handOwner.Hand[0].Value == "A")
                        {
                            isSplitAces = true;
                        }

                        afterSplit = true;
                    }

                    break;
            }
        }
    }

    private int SettleAgainstDealer(Player handOwner, bool isSplitHand)
    {
        var pEval = HandEvaluator.Evaluate(handOwner.Hand,
            !isSplitHand); // 2-card 21 after split is NOT Blackjack

        // Charlie win first
        if (handOwner.Hand.Count >= Rules.Instance.SixCardCharlieCount && pEval.Total <= 21)
            return +handOwner.Bet; // REQUIREMENT: wins over everything

        var dEval = HandEvaluator.Evaluate(dealer.Hand, false);

        // bust checks
        if (pEval.Total > 21) return -handOwner.Bet;
        if (dEval.Total > 21) return +handOwner.Bet;

        // compare totals (push on tie)
        if (pEval.Total > dEval.Total) return +handOwner.Bet;
        if (pEval.Total < dEval.Total) return -handOwner.Bet;
        return 0;
    }

}