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

public record PlayerResult(
    Player Player,
    RoundResult RoundResult
    );

public class Game
{
    public readonly Dealer Dealer = new(); // dealer uses same Player class but different flow
    public readonly Deck Deck = new(); // 8-deck shoe with 0.7 penetration by default
    public readonly List<Player> Players = new();


    public Strategy strategy = Strategy.Instance;


    public Game(int numberOfPlayers = 1)
    {
        for (var i = 0; i < numberOfPlayers; i++)
        {
            Players.Add(new Player());
        }
    }

    public void Reset()
    {
        Deck.SetupShoe();
        Dealer.Reset();
        Players.ForEach(p => p.Reset());
    }
    public List<PlayerResult> PlayOneRound()
    {
        Players.ForEach(p => p.Reset());
        Dealer.Reset();
        Deck.EndOfGame();

        // initial deal
        Players.ForEach(p => p.AddCard(Deck.DrawCard()));
        Dealer.AddCard(Deck.DrawCard()); // dealer upcard
        Players.ForEach(p => p.AddCard(Deck.DrawCard()));
        Dealer.AddCard(Deck.DrawCard()); // dealer hole card

        return PlayOneRoundWithHand();
    }

    public List<PlayerResult> PlayOneRoundWithHand()
    {
        var results = new List<PlayerResult>();
        List<Player> activePlayers = new();

        var avaibleUnits = 4;

        var dEval = HandEvaluator.Evaluate(Dealer.Hand, true);

        Players.ForEach(p =>
        {
            var pEval = p.EvaluateHand(true);
            if (pEval.IsBlackjack) p.DidBlackjack = true;
            if (InitialCheckForBlackjack(dEval, pEval, p, out var rr))
            {
                results.Add(new PlayerResult(p, rr));
            }
            else
            {
                activePlayers.Add(p);
            }


        });
        // evaluate Blackjacks (initial only)

        if (activePlayers.Count == 0)
            return results;

        
        Players.ForEach(p => avaibleUnits = PlayerTurn(p, avaibleUnits));


        if (dEval.IsBlackjack)
        {
            foreach (var p in activePlayers)
            {
                var rr = new RoundResult(
                    Outcome.DealerBlackjack,
                    -p.Bet,
                    p.Bet,
                    p.DidBlackjack,
                    p.DidSplit,
                    p.DidDouble);
                results.Add(new PlayerResult(p, rr));
            }
            return results;
        }

        Dealer.Play(Deck);

        foreach (var p in activePlayers)
        {
            var netUnits = 0;

            int mainResult = GetHandOutcome(p, false);
            netUnits += SettleAgainstDealer(p, false);
            Outcome mainOutcome = GetOutcomeFromResult(mainResult, p, Dealer);


            Outcome? splitOutcome = null;
            if (p.SplitHandPlayer != null)
            {
                int splitResult = GetHandOutcome(p.SplitHandPlayer, true);
                netUnits += SettleAgainstDealer(p.SplitHandPlayer, true);
                splitOutcome = GetOutcomeFromResult(splitResult, p.SplitHandPlayer, Dealer);
            }

            int totalStake = p.Bet; // original hand (already doubled if double down)
            if (p.SplitHandPlayer != null)
                totalStake += p.SplitHandPlayer.Bet;

            var rr = Summarize(mainOutcome, splitOutcome, netUnits, totalStake, p);
            results.Add(new PlayerResult(p, rr));
        }
        return results;
    }

    private RoundResult Summarize(Outcome mainOutcome, Outcome? splitOutcome, int netUnits, int totalStake, Player p)
    {
        if (mainOutcome == Outcome.PlayerWinWithCharlie || splitOutcome == Outcome.PlayerWinWithCharlie)
            return new RoundResult(Outcome.PlayerWinWithCharlie, netUnits, totalStake, p.DidBlackjack, p.DidSplit, p.DidDouble);

        // Dealer blackjack (after Charlie check)
        if (mainOutcome == Outcome.DealerBlackjack || splitOutcome == Outcome.DealerBlackjack)
            return new RoundResult(Outcome.DealerBlackjack, netUnits, totalStake, p.DidBlackjack, p.DidSplit, p.DidDouble);

        if (mainOutcome == Outcome.Bust || splitOutcome == Outcome.Bust)
            return new RoundResult(Outcome.Bust, netUnits, totalStake, p.DidBlackjack, p.DidSplit, p.DidDouble);

        if (mainOutcome == Outcome.DealerBust || splitOutcome == Outcome.DealerBust)
            return new RoundResult(Outcome.DealerBust, netUnits, totalStake, p.DidBlackjack, p.DidSplit, p.DidDouble);

        if (netUnits > 0)
            return new RoundResult(Outcome.PlayerWin, netUnits, totalStake, p.DidBlackjack, p.DidSplit, p.DidDouble);
        if (netUnits < 0)
            return new RoundResult(Outcome.DealerWin, netUnits, totalStake, p.DidBlackjack, p.DidSplit, p.DidDouble);
        return new RoundResult(Outcome.Push, 0, totalStake, p.DidBlackjack, p.DidSplit, p.DidDouble);
    }


    private int PlayerTurn(Player Player, int remainingUnits)
    {
        var netUnits = 0;
        var afterSplit = false;
        Player.unitsAvaible = remainingUnits;
        // Optional very-simple strategy:
        // - Split Aces always; otherwise split only equal 8s; no resplit allowed by design.


        // Play a single hand and (optionally) the split hand
        PlaySingleHand(Player, afterSplit, false);
        if (Player.SplitHandPlayer != null)
        {
            Player.SplitHandPlayer.unitsAvaible = Player.unitsAvaible;
            var unitsSplit = PlaySingleHand(Player.SplitHandPlayer, true,
                Player.SplitHandPlayer.Hand[0] == CardValue.Ace);

            Player.unitsAvaible = Player.SplitHandPlayer.unitsAvaible;
        }

        return Player.unitsAvaible;
    }

    private bool InitialCheckForBlackjack(HandEval dEval, HandEval pEval, Player p, out RoundResult playOneRoundWithHand)
    {
        // REQUIREMENT: Dealer peeks for Blackjack when showing Ace
        if (Rules.Instance.DealerPeeksOnAce && Dealer.Hand[0] == CardValue.Ace || Rules.Instance.DealerPeeksOnAce && Dealer.Hand[0] == CardValue.Ten)
            if (dEval.IsBlackjack)
            {
                if (pEval.IsBlackjack)
                {
                    playOneRoundWithHand = new RoundResult(Outcome.Push, 0, p.Bet, p.DidBlackjack, p.DidSplit,
                        p.DidDouble);
                    return true;
                }

                playOneRoundWithHand = new RoundResult(Outcome.DealerBlackjack, -p.Bet, p.Bet, p.DidBlackjack, p.DidSplit,
                    p.DidDouble);
                return true;
            }

        // If dealer upcard not Ace, we still need to handle natural BJ payoff
        if (pEval.IsBlackjack)
        {
            if (dEval.IsBlackjack)
            {
                playOneRoundWithHand = new RoundResult(Outcome.Push, 0, p.Bet, p.DidBlackjack, p.DidSplit,
                    p.DidDouble);
                return true;
            }

            // REQUIREMENT: Blackjack pays 3:2
            var units = p.Bet * Rules.Instance.BlackjackPayout;
            playOneRoundWithHand = new RoundResult(Outcome.PlayerBlackjack, units, p.Bet, p.DidBlackjack, p.DidSplit,
                p.DidDouble);
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
            var action = strategy.Decide(handOwner, Dealer.Hand[0], afterSplit);
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