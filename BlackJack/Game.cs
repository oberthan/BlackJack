namespace BlackJack;

internal enum Outcome
{
    PlayerWin,
    DealerWin,
    Push
}

internal readonly struct RoundResult(Outcome o,double units, int stake, bool blackjack, bool split, bool doubled)
{
    public Outcome Outcome { get; } = o;
    public double UnitsWonOrLost { get; } = units;
    public int Stake { get; } = stake;
    public bool Blackjack { get; } = blackjack;
    public bool Split { get; } = split;
    public bool Double { get; } = doubled;
}

internal class Game
{
    private readonly Player dealer = new(); // dealer uses same Player class but different flow
    private readonly Deck deck = new(); // 8-deck shoe with 0.7 penetration by default
    private readonly Player player = new();


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

        // evaluate blackjacks (initial only)
        var pEval = HandEvaluator.Evaluate(player.Hand, true);
        var dEval = HandEvaluator.Evaluate(dealer.Hand, true);
        if (pEval.IsBlackjack) player.DidBlackjack = true; // track for later


        // REQUIREMENT: Dealer peeks for blackjack when showing Ace
        if (Rules.DealerPeeksOnAce && dealer.Hand[0].Value == "A")
            if (dEval.IsBlackjack)
            {
                if (pEval.IsBlackjack)
                    return new RoundResult(Outcome.Push, 0, player.Bet, player.DidBlackjack, player.DidSplit,
                        player.DidDouble); // REQUIREMENT: push if both BJ
                return new RoundResult(Outcome.DealerWin, -player.Bet, player.Bet, player.DidBlackjack, player.DidSplit,
                    player.DidDouble); // dealer BJ ends round
            }

        // If dealer upcard not Ace, we still need to handle natural BJ payoff
        if (pEval.IsBlackjack)
        {
            if (dEval.IsBlackjack)
                return new RoundResult(Outcome.Push, 0, player.Bet, player.DidBlackjack, player.DidSplit,
                    player.DidDouble);
            // REQUIREMENT: Blackjack pays 3:2
            var units = player.Bet * Rules.BlackjackPayout;
            return new RoundResult(Outcome.PlayerWin, units, player.Bet, player.DidBlackjack, player.DidSplit,
                player.DidDouble);
        }

        if (dEval.IsBlackjack)
            return new RoundResult(Outcome.DealerWin, -player.Bet, player.Bet, player.DidBlackjack, player.DidSplit,
                player.DidDouble);

        // PLAYER TURN(s)
        var netUnits = 0;
        var afterSplit = false;

        // Optional very-simple strategy:
        // - Split Aces always; otherwise split only equal 8s; no resplit allowed by design.
        if (player.CanSplit())
        {
            var shouldSplit =
                player.IsOriginalAces ||
                (player.Hand[0].PipValue == 8 && player.Hand[1].PipValue == 8);

            if (shouldSplit)
            {
                player.Split(deck);
                player.DidSplit = true; // track for later
                afterSplit = true;
            }
        }

        // Play a single hand and (optionally) the split hand
        var unitsMain = PlaySingleHand(player, afterSplit, player.IsOriginalAces);
        if (player.SplitHandPlayer != null)
        {
            var unitsSplit = PlaySingleHand(player.SplitHandPlayer, true,
                player.SplitHandPlayer.Hand.Count == 2 &&
                player.SplitHandPlayer.Hand[0].Value == "A");
            // Dealer plays once for both hands (standard shoe game): delay dealer play until both hands done.
            netUnits += unitsSplit; // will be combined after dealer plays
        }

        // DEALER TURN (only if at least one player hand hasn't already resolved to Charlie bust/win)
        var dEvalPre = HandEvaluator.Evaluate(dealer.Hand, false);
        if (dEvalPre.Total < 17) DealerPlay();

        // Resolve outcomes for hands that need comparing
        netUnits += SettleAgainstDealer(player, false);
        if (player.SplitHandPlayer != null)
            netUnits += SettleAgainstDealer(player.SplitHandPlayer, true);


        int totalStake = player.Bet; // original hand (already doubled if double down)
        if (player.SplitHandPlayer != null)
            totalStake += player.SplitHandPlayer.Bet;

        // summarize
        if (netUnits > 0)
            return new RoundResult(Outcome.PlayerWin, netUnits, totalStake, player.DidBlackjack, player.DidSplit,
                player.DidDouble);
        if (netUnits < 0)
            return new RoundResult(Outcome.DealerWin, netUnits, totalStake, player.DidBlackjack, player.DidSplit,
                player.DidDouble);
        return new RoundResult(Outcome.Push, 0, totalStake, player.DidBlackjack, player.DidSplit, player.DidDouble);
    }

    private void DealerPlay()
    {
        while (true)
        {
            var eval = HandEvaluator.Evaluate(dealer.Hand, false);
            if (eval.Total < 17) dealer.AddCard(deck.DrawCard());
            else if (eval.Total == 17 && !Rules.DealerStandsOnSoft17 && eval.IsSoft) dealer.AddCard(deck.DrawCard());
            else break; // REQUIREMENT: dealer stands on ALL 17s (including soft)
        }
    }

    private int PlaySingleHand(Player handOwner, bool afterSplit, bool isSplitAces)
    {
        if (isSplitAces)
            return 0; // only one card dealt, no further play

        while (true)
        {
            var eval = HandEvaluator.Evaluate(handOwner.Hand, false);

            // Six Card Charlie
            if (handOwner.Hand.Count >= Rules.SixCardCharlieCount && eval.Total <= 21)
                return 0;

            // Get strategy action
            var action = Strategy.Decide(handOwner, dealer.Hand[0], afterSplit);

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
                    // handled before PlaySingleHand — skip here
                    return 0;
            }
        }
    }

    private int SettleAgainstDealer(Player handOwner, bool isSplitHand)
    {
        var pEval = HandEvaluator.Evaluate(handOwner.Hand,
            !isSplitHand); // 2-card 21 after split is NOT blackjack

        // Charlie win first
        if (handOwner.Hand.Count >= Rules.SixCardCharlieCount && pEval.Total <= 21)
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


/*
        public string PlayGame()
        {
            player.Reset();
            dealer.Reset();

            // Initial deal
            player.AddCard(deck.DrawCard());
            player.AddCard(deck.DrawCard());

            dealer.AddCard(deck.DrawCard());
            dealer.AddCard(deck.DrawCard());

            // Player auto strategy: hit until 17 or higher
            var counter = 0;
            while (player.GetHandValue() < 17)
            {
                player.AddCard(deck.DrawCard());
                if (counter >= 13) throw new InvalidOperationException("Too many cards drawn!");

            }

            // Dealer turn
            dealer.Play(deck);

            deck.EndOfGame();


            // Determine outcome
            if (player.IsBusted()) return "Dealer";
            if (dealer.IsBusted()) return "Player";

            int playerTotal = player.GetHandValue();
            int dealerTotal = dealer.GetHandValue();

            if (playerTotal > dealerTotal) return "Player";
            if (dealerTotal > playerTotal) return "Dealer";


            return "Push";
        }
*/
}