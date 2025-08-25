using NUnit.Framework;
using BlackJack;

namespace BlackJack.Tests
{
    [TestFixture]
    public class RoundResultTest
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var result = new RoundResult(
                Outcome.PlayerWin,
                1.5,
                100,
                blackjack: true,
                split: false,
                doubled: true);

            Assert.That(result.Outcome, Is.EqualTo(Outcome.PlayerWin));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(1.5));
            Assert.That(result.Stake, Is.EqualTo(100));
            Assert.That(result.Blackjack, Is.True);
            Assert.That(result.Split, Is.False);
            Assert.That(result.Double, Is.True);
        }

        [TestCase(Outcome.Bust, -100, 100, false, false, false)]
        [TestCase(Outcome.DealerWin, -50, 50, false, true, false)]
        [TestCase(Outcome.Push, 0, 200, false, false, true)]
        public void Properties_AreSetCorrectly(
            Outcome outcome,
            double units,
            int stake,
            bool blackjack,
            bool split,
            bool doubled)
        {
            var result = new RoundResult(outcome, units, stake, blackjack, split, doubled);

            Assert.That(result.Outcome, Is.EqualTo(outcome));
            Assert.That(result.UnitsWonOrLost, Is.EqualTo(units));
            Assert.That(result.Stake, Is.EqualTo(stake));
            Assert.That(result.Blackjack, Is.EqualTo(blackjack));
            Assert.That(result.Split, Is.EqualTo(split));
            Assert.That(result.Double, Is.EqualTo(doubled));
        }
    }
}
