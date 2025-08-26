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

public record RoundResult(Outcome Outcome, double UnitsWonOrLost, int Stake, bool Blackjack, bool Split, bool Doubled);

public class Game
{
    public readonly Dealer Dealer = new(); // dealer uses same Player class but different flow
    public readonly Deck Deck = new(); // 8-deck shoe with 0.7 penetration by default
    public readonly Player Player = new();

    public void Reset()
    {
        Deck.SetupShoe();
        Dealer.Reset();
        Player.Reset();
    }
    public RoundResult PlayOneRound()
    {
        Player.Reset();
        Dealer.Reset();
        Deck.EndOfGame();

        // initial deal
        Player.AddCard(Deck.DrawCard());
        Dealer.AddCard(Deck.DrawCard()); // dealer upcard
        Player.AddCard(Deck.DrawCard());
        Dealer.AddCard(Deck.DrawCard()); // dealer hole card

        return PlayOneRoundWithHand();
    }

    public RoundResult PlayOneRoundWithHand()
    {
        // evaluate Blackjacks (initial only)
        var pEval = HandEvaluator.Evaluate(Player.Hand, true);
        var dEval = HandEvaluator.Evaluate(Dealer.Hand, true);
        if (pEval.IsBlackjack) Player.DidBlackjack = true; // track for later

        if (InitialCheckForBlackjack(dEval, pEval, out var playOneRoundWithHand)) return playOneRoundWithHand;


        // PLAYER TURN(s)
        var netUnits = PlayerTurn();

        // DEALER TURN
        if (DealerTurn(dEval, out var roundResult)) return roundResult;

        // Resolve outcomes for hands that need comparing
        int mainResult = GetHandOutcome(Player, false);
        netUnits += SettleAgainstDealer(Player, false);
        Outcome mainOutcome = GetOutcomeFromResult(mainResult, Player, Dealer);

        Outcome? splitOutcome = null;
        if (Player.SplitHandPlayer != null)
        {
            int splitResult = GetHandOutcome(Player.SplitHandPlayer, true);
            netUnits += SettleAgainstDealer(Player.SplitHandPlayer, true);
            splitOutcome = GetOutcomeFromResult(splitResult, Player.SplitHandPlayer, Dealer);
        }

        int totalStake = Player.Bet; // original hand (already doubled if double down)
        if (Player.SplitHandPlayer != null)
            totalStake += Player.SplitHandPlayer.Bet;

        return Summarize(mainOutcome, splitOutcome, netUnits, totalStake);
    }

    private RoundResult Summarize(Outcome mainOutcome, Outcome? splitOutcome, int netUnits, int totalStake)
    {
        if (mainOutcome == Outcome.PlayerWinWithCharlie || splitOutcome == Outcome.PlayerWinWithCharlie)
            return new RoundResult(Outcome.PlayerWinWithCharlie, netUnits, totalStake, Player.DidBlackjack, Player.DidSplit, Player.DidDouble);

        // Dealer blackjack (after Charlie check)
        if (mainOutcome == Outcome.DealerBlackjack || splitOutcome == Outcome.DealerBlackjack)
            return new RoundResult(Outcome.DealerBlackjack, netUnits, totalStake, Player.DidBlackjack, Player.DidSplit, Player.DidDouble);

        if (mainOutcome == Outcome.Bust || splitOutcome == Outcome.Bust)
            return new RoundResult(Outcome.Bust, netUnits, totalStake, Player.DidBlackjack, Player.DidSplit, Player.DidDouble);

        if (mainOutcome == Outcome.DealerBust || splitOutcome == Outcome.DealerBust)
            return new RoundResult(Outcome.DealerBust, netUnits, totalStake, Player.DidBlackjack, Player.DidSplit, Player.DidDouble);

        if (netUnits > 0)
            return new RoundResult(Outcome.PlayerWin, netUnits, totalStake, Player.DidBlackjack, Player.DidSplit,
                Player.DidDouble);
        if (netUnits < 0)
            return new RoundResult(Outcome.DealerWin, netUnits, totalStake, Player.DidBlackjack, Player.DidSplit,
                Player.DidDouble);
        return new RoundResult(Outcome.Push, 0, totalStake, Player.DidBlackjack, Player.DidSplit, Player.DidDouble);
    }

    private bool DealerTurn(HandEval dEval, out RoundResult roundResult)
    {
        if (dEval.IsBlackjack)
        {
            roundResult = new RoundResult(Outcome.DealerBlackjack, -Player.Bet, Player.Bet, Player.DidBlackjack, Player.DidSplit,
                Player.DidDouble);
            return true;
        }

        Dealer.Play(Deck);

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
        PlaySingleHand(Player, afterSplit, false);
        if (Player.SplitHandPlayer != null)
        {
            var unitsSplit = PlaySingleHand(Player.SplitHandPlayer, true,
                Player.SplitHandPlayer.Hand[0] == CardValue.Ace);
            // Dealer plays once for both hands (standard shoe game): delay dealer play until both hands done.
            netUnits += unitsSplit; // will be combined after dealer plays
        }

        return netUnits;
    }

    private bool InitialCheckForBlackjack(HandEval dEval, HandEval pEval, out RoundResult playOneRoundWithHand)
    {
        // REQUIREMENT: Dealer peeks for Blackjack when showing Ace
        if (Rules.Instance.DealerPeeksOnAce && Dealer.Hand[0] == CardValue.Ace)
            if (dEval.IsBlackjack)
            {
                if (pEval.IsBlackjack)
                {
                    playOneRoundWithHand = new RoundResult(Outcome.Push, 0, Player.Bet, Player.DidBlackjack, Player.DidSplit,
                        Player.DidDouble);
                    return true;
                }

                playOneRoundWithHand = new RoundResult(Outcome.DealerBlackjack, -Player.Bet, Player.Bet, Player.DidBlackjack, Player.DidSplit,
                    Player.DidDouble);
                return true;
            }

        // If dealer upcard not Ace, we still need to handle natural BJ payoff
        if (pEval.IsBlackjack)
        {
            if (dEval.IsBlackjack)
            {
                playOneRoundWithHand = new RoundResult(Outcome.Push, 0, Player.Bet, Player.DidBlackjack, Player.DidSplit,
                    Player.DidDouble);
                return true;
            }

            // REQUIREMENT: Blackjack pays 3:2
            var units = Player.Bet * Rules.Instance.BlackjackPayout;
            playOneRoundWithHand = new RoundResult(Outcome.PlayerBlackjack, units, Player.Bet, Player.DidBlackjack, Player.DidSplit,
                Player.DidDouble);
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
        var dEval = HandEvaluator.Evaluate(Dealer.Hand, false);

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
            var action = Strategy.Instance.Decide(handOwner, Dealer.Hand[0], afterSplit);
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
                    handOwner.AddCard(Deck.DrawCard());
                    if (HandEvaluator.Evaluate(handOwner.Hand, false).Total > 21)
                        return 0; // bust
                    break;

                case Move.Stand:
                    return 0;

                case Move.Double:
                    if (handOwner.CanDouble(afterSplit, false))
                    {
                        handOwner.DoubleDown(Deck);
                        handOwner.DidDouble = true; // track for later
                    }

                    return 0; // after doubling you always stop

                case Move.Split:
                    if (handOwner.CanSplit())
                    {
                        handOwner.Split(Deck);
                        handOwner.DidSplit = true; // track for later

                        if (handOwner.Hand[0] == CardValue.Ace)
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

        var dEval = HandEvaluator.Evaluate(Dealer.Hand, false);

        // bust checks
        if (pEval.Total > 21) return -handOwner.Bet;
        if (dEval.Total > 21) return +handOwner.Bet;

        // compare totals (push on tie)
        if (pEval.Total > dEval.Total) return +handOwner.Bet;
        if (pEval.Total < dEval.Total) return -handOwner.Bet;
        return 0;
    }

}