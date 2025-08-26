using Blackjack;
using NUnit.Framework;

namespace BlackJackTest
{
    [TestFixture]
    public class GameTest
    {
        List<bool> BoolArray_from_String(string fromString)
        {
            var list = new List<bool>();
            foreach (char c in fromString)
            {
                list.Add(c == 'y' ? true : false);
            }

            return list;
        }
        List<List<Move>> MoveArray_from_String(string[] fromList)
        {
            List<List<Move>> array = new List<List<Move>>();
            foreach (string fromString in fromList)
            {
                var list = new List<Move>();
                foreach (char c in fromString)
                {
                    list.Add(c == 'h' ? Move.Hit : c == 's' ? Move.Stand : Move.Double);
                }
                array.Add(list);
            }

            return array;
        }

        [Test]
        public void PlayOneRound_PlayerBlackjack_DealerNoBlackjack_PlayerWinsWithBlackjackPayout()
        {
            var game = new Game();
            var deck = game.deck;
            var player = game.player;
            player.Reset();
            var dealer = game.dealer;
            dealer.Reset();


            player.AddCard(new Card("A", "S"));
            player.AddCard(new Card("K", "D"));

            dealer.AddCard(new Card("9", "H"));
            dealer.AddCard(new Card("7", "C"));

            var result = game.PlayOneRoundWithHand();

            Assert.That(result.Outcome, Is.EqualTo(Outcome.PlayerBlackjack));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(1 * Rules.Instance.BlackjackPayout));
            Assert.That(result.Blackjack, Is.True);
        }

        [Test]
        public void PlayOneRound_BothBlackjack_Push()
        {
            var game = new Game();
            var deck = game.deck;
            var player = game.player;
            player.Reset();
            var dealer = game.dealer;
            dealer.Reset();


            player.AddCard(new Card("A", "S"));
            player.AddCard(new Card("K", "D"));

            dealer.AddCard(new Card("A", "H"));
            dealer.AddCard(new Card("K", "C"));

            var result = game.PlayOneRoundWithHand();



            Assert.That(result.Outcome, Is.EqualTo(Outcome.Push));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(0));
            Assert.That(result.Blackjack, Is.True);
        }

        [Test]
        public void PlayOneRound_DealerBlackjack_PlayerNoBlackjack_DealerWins()
        {
            var game = new Game();
            var deck = game.deck;
            var player = game.player;
            player.Reset();
            var dealer = game.dealer;
            dealer.Reset();


            player.AddCard(new Card("9", "S"));
            dealer.AddCard(new Card("A", "H"));
            player.AddCard(new Card("K", "D"));
            dealer.AddCard(new Card("K", "C"));


            var result = game.PlayOneRoundWithHand();

            Assert.That(result.Outcome, Is.EqualTo(Outcome.DealerBlackjack));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(-1));
            Assert.That(result.Blackjack, Is.False);

        }

        [Test]
        public void PlayOneRound_PlayerBust_DealerWins()
        {
            Rnd.Rng = new Random(1232); // Ensure consistent shuffling for testing

            var game = new Game();
            var deck = game.deck;
            var player = game.player;
            player.Reset();
            var dealer = game.dealer;
            dealer.Reset();


            player.AddCard(new Card("9", "S"));
            player.AddCard(new Card("9", "D"));
            dealer.AddCard(new Card("7", "H"));
            player.AddCard(new Card("8", "D"));
            dealer.AddCard(new Card("6", "C"));


            var result = game.PlayOneRoundWithHand();

            TestContext.Out.WriteLine("Player Hand:");
            foreach (CardValue card in player.Hand)
            {
                TestContext.Out.WriteLine(card.ToString());

            }
            TestContext.Out.WriteLine("\n\nDealer Hand:");
            foreach (CardValue card in dealer.Hand)
            {
                TestContext.Out.WriteLine(card.ToString());

            }

            Assert.That(result.Outcome, Is.EqualTo(Outcome.Bust));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(-1));
        }

        [Test]
        public void PlayOneRound_PlayerCharlieWin_PlayerWins()
        {
            var game = new Game();
            var deck = game.deck;
            var player = game.player;
            player.Reset();
            var dealer = game.dealer;
            dealer.Reset();


            player.AddCard(CardValue.Two);
            player.AddCard(CardValue.Two);
            player.AddCard(CardValue.Two);
            player.AddCard(CardValue.Two);
            player.AddCard(CardValue.Two);
            dealer.AddCard(CardValue.Seven);
            player.AddCard(CardValue.Two);
            dealer.AddCard(CardValue.Six);

            var result = game.PlayOneRoundWithHand();


            Assert.That(result.Outcome, Is.EqualTo(Outcome.PlayerWinWithCharlie));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(1));
        }



        [TestCase("2", "yyyyyynnnn")]
        [TestCase("3", "yyyyyynnnn")]
        [TestCase("4", "nnnyynnnnn")]
        [TestCase("5", "nnnnnnnnnn")]
        [TestCase("6", "yyyyynnnnn")]
        [TestCase("7", "yyyyyynnnn")]
        [TestCase("8", "yyyyyyyyyy")]
        [TestCase("9", "yyyyynyynn")]
        [TestCase("10", "nnnnnnnnnn")]
        [TestCase("10", "nnnnnnnnnn", "J")]
        [TestCase("10", "nnnnnnnnnn", "Q")]
        [TestCase("10", "nnnnnnnnnn", "K")]
        [TestCase("J", "nnnnnnnnnn")]
        [TestCase("J", "nnnnnnnnnn", "Q")]
        [TestCase("J", "nnnnnnnnnn", "K")]
        [TestCase("Q", "nnnnnnnnnn")]
        [TestCase("Q", "nnnnnnnnnn", "K")]
        [TestCase("K", "nnnnnnnnnn")]
        [TestCase("A", "yyyyyyyyyy")]
        public void PlayOneRound_Split(string splitCard, string splitsString, string secondSplitCard = "")
        {
            //Rnd.Rng = new Random(123456); // Ensure consistent shuffling for testing
            secondSplitCard = secondSplitCard == "" ? splitCard : secondSplitCard;
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "A" };
            List<bool> pairSplits = BoolArray_from_String(splitsString);
            int i = 0;
            foreach (bool shouldSplit in pairSplits)
            {
                var game = new Game();
                var deck = game.deck;
                var player = game.player;
                player.Reset();
                var dealer = game.dealer;
                dealer.Reset();


                player.AddCard(new Card(splitCard, "S"));
                dealer.AddCard(new Card(values[i], "H"));
                player.AddCard(new Card(secondSplitCard, "D"));
                dealer.AddCard(new Card("6", "C"));

                var result = game.PlayOneRoundWithHand();

                TestContext.Out.WriteLine($"\n\n\n\nGame {i}");
                TestContext.Out.WriteLine("Player Hand 1:");
                foreach (var card in player.Hand)
                {
                    TestContext.Out.WriteLine(card.ToString());

                }

                if (player.SplitHandPlayer != null)
                {
                    TestContext.Out.WriteLine("\nPlayer Hand 2:");
                    foreach (var card in player.SplitHandPlayer.Hand)
                    {
                        TestContext.Out.WriteLine(card.ToString());

                    }
                }

                TestContext.Out.WriteLine("\n\nDealer Hand:");
                foreach (var card in dealer.Hand)
                {
                    TestContext.Out.WriteLine(card.ToString());

                }

                Assert.That(player.SplitHandPlayer, shouldSplit ? Is.Not.EqualTo(null) : Is.EqualTo(null));
                Assert.That(result.Stake, Is.EqualTo(shouldSplit ? ((player.DidDouble ? 2 : 1) + (player.SplitHandPlayer.DidDouble ? 2 : 1)) : (player.DidDouble ? 2 : 1)));
                /*            Assert.That(result.Outcome, Is.EqualTo(Outcome.DealerBust));
                            Assert.That(result.UnitsWonOrLost, Is.EqualTo(3)); // Player wins both hands*/
                i++;

            }

        }
        [Test]
        public void Split_Aces_Normal_Payout()
        {
            var game = new Game();
            var deck = game.deck;
            var player = game.player;
            player.Reset();
            var dealer = game.dealer;
            dealer.Reset();


            player.AddCard(new Card("A", "S"));
            dealer.AddCard(new Card("8", "H"));
            player.AddCard(new Card("A", "D"));
            dealer.AddCard(new Card("10", "C"));

            var Splitdecision = Strategy.Instance.Decide(player, dealer.Hand[0], false);

            Assert.That(Splitdecision, Is.EqualTo(Move.Split));

            var result = game.PlayOneRoundWithHand();

            TestContext.WriteLine($"{player.Hand[1]}, {player.SplitHandPlayer.Hand[1]}");


            Assert.That(result.UnitsWonOrLost, Is.EqualTo(2));

        }




        [Test]
        public void StrategyDecide_SoftDouble()
        {
            string[] expectedMoves =
            [
                "hhhddhhhhhhhh",
                "hhhddhhhhhhhh",
                "hhdddhhhhhhhh",
                "hhdddhhhhhhhh",
                "hddddhhhhhhhh",
                "sddddsshhhhhh",
                "sssssssssssss",
                "sssssssssssss"
            ];
            var parsedMoves = MoveArray_from_String(expectedMoves);

            string[] playerValues = { "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] dealerValues = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            for (int i = 0; i < playerValues.Length; i++)
            {
                string playerValue = playerValues[i];
                for (int k = 0; k < dealerValues.Length; k++)
                {
                    string dealerValue = dealerValues[k];
                    var player = new Player();
                    player.AddCard(new Card("A", "S"));
                    player.AddCard(new Card(playerValue, "D"));
                    var dealerUp = new Card(dealerValue, "H");
                    var move = Strategy.Instance.Decide(player, dealerUp, false);

                    Assert.That(move, Is.EqualTo(parsedMoves[i][k]));
                }
            }

        }


        [Test]
        public void StrategyDecide_HardDouble_FromCards()
        {
            string[] expectedMoves =
            [
                "hhhhhhhhhhhhh",
                "hddddhhhhhhhh",
                "ddddddddhhhhh",
                "ddddddddddddh",
                "hhssshhhhhhhh",
                "ssssshhhhhhhh",
                "ssssshhhhhhhh",
                "ssssshhhhhhhh",
                "ssssshhhhhhhh",
                "sssssssssssss"
            ];
            var parsedMoves = MoveArray_from_String(expectedMoves);

            string[] playerValues = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            string[] dealerValues = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            for (int i = 0; i < playerValues.Length; i++)
            {
                string playerValueOne = playerValues[i];

                for (int g = 0; g < playerValues.Length; g++)
                {
                    string playerValueTwo = playerValues[g];

                    var evaluation = HandEvaluator.Instance.Evaluate(new List<CardValue>
                    {
                        new Card(playerValueOne, "S"),
                        new Card(playerValueTwo, "D")
                    }, false);
                    if (!evaluation.IsSoft)
                    {


                        for (int k = 0; k < dealerValues.Length; k++)
                        {
                            string dealerValue = dealerValues[k];
                            var player = new Player();
                            player.AddCard(new Card(playerValueOne, "S"));
                            player.AddCard(new Card(playerValueTwo, "D"));
                            var dealerUp = new Card(dealerValue, "H");
                            var move = Strategy.Instance.Decide(player, dealerUp, false);

                            Assert.That(move, Is.EqualTo(parsedMoves[evaluation.Total - 8][k]));
                        }
                    }
                }
            }
        }

        [Test]
        public void PlayOneRound_PlayerDoubleWin_AllRoundResultValues()
        {
            var game = new Game();
            var player = game.player;
            var dealer = game.dealer;
            player.Reset();
            dealer.Reset();

            player.Bet = 2;
            player.AddCard(new Card("9", "S"));
            player.AddCard(new Card("2", "D"));
            dealer.AddCard(new Card("5", "H"));
            dealer.AddCard(new Card("10", "C"));

            player.DidDouble = true;

            var result = game.PlayOneRoundWithHand();

            Assert.That(result.Outcome, Is.EqualTo(Outcome.PlayerWin));
            Assert.That(result.UnitsWonOrLost, Is.GreaterThan(0));
            Assert.That(result.Stake, Is.EqualTo(50));
            Assert.That(result.Blackjack, Is.False);
            Assert.That(result.Split, Is.False);
            Assert.That(result.Double, Is.True);
        }

        [Test]
        public void PlayOneRound_SplitWin_AllRoundResultValues()
        {
            var game = new Game();
            var player = game.player;
            var dealer = game.dealer;
            player.Reset();
            dealer.Reset();

            player.Bet = 100;
            player.AddCard(new Card("8", "S"));
            player.AddCard(new Card("8", "D"));
            dealer.AddCard(new Card("6", "H"));
            dealer.AddCard(new Card("10", "C"));

            // Simulate split
            player.DidSplit = true;
            player.SplitHandPlayer = new SplitPlayer(100);
            player.SplitHandPlayer.AddCard(new Card("8", "C"));
            player.SplitHandPlayer.AddCard(new Card("3", "D"));

            var result = game.PlayOneRoundWithHand();

            Assert.That(result.Outcome, Is.AnyOf(Outcome.PlayerWin, Outcome.Push, Outcome.DealerWin));
            Assert.That(result.Stake, Is.EqualTo(200));
            Assert.That(result.Blackjack, Is.False);
            Assert.That(result.Split, Is.True);
            Assert.That(result.Double, Is.False);
        }

        [Test]
        public void PlayOneRound_PlayerBlackjack_AllRoundResultValues()
        {
            var game = new Game();
            var player = game.player;
            var dealer = game.dealer;
            player.Reset();
            dealer.Reset();

            player.Bet = 75;
            player.AddCard(new Card("A", "S"));
            player.AddCard(new Card("K", "D"));
            dealer.AddCard(new Card("9", "H"));
            dealer.AddCard(new Card("7", "C"));

            var result = game.PlayOneRoundWithHand();

            Assert.That(result.Outcome, Is.EqualTo(Outcome.PlayerBlackjack));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(75 * Rules.Instance.BlackjackPayout));
            Assert.That(result.Stake, Is.EqualTo(75));
            Assert.That(result.Blackjack, Is.True);
            Assert.That(result.Split, Is.False);
            Assert.That(result.Double, Is.False);
        }
    }
}
