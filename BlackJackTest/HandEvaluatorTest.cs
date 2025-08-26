using Blackjack;
using NUnit.Framework;

namespace BlackJackTest
{
    [TestFixture]
    public class HandEvaluatorTest
    {
        private Card Ace => new("A", "S");
        private Card Ten => new("10", "H");
        private Card Nine => new("9", "D");
        private Card Two => new("2", "C");
        private Card King => new("K", "S");
        private Card Five => new("5", "H");

        [Test]
        public void Evaluate_TwoCardBlackjack_ReturnsBlackjack()
        {
            var hand = new List<Card> { Ace, King };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: true);

            Assert.That(eval.Total, Is.EqualTo(21));
            Assert.That(eval.IsSoft, Is.True);
            Assert.That(eval.IsBlackjack, Is.True);
        }

        [Test]
        public void Evaluate_TwoCard21_NotBlackjackIfFlagFalse()
        {
            var hand = new List<Card> { Ace, Ten };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: false);

            Assert.That(eval.Total, Is.EqualTo(21));
            Assert.That(eval.IsSoft, Is.True);
            Assert.That(eval.IsBlackjack, Is.False);
        }

        [Test]
        public void Evaluate_ThreeCard21_NotBlackjack()
        {
            var hand = new List<Card> { Ace, Five, Five };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: true);

            Assert.That(eval.Total, Is.EqualTo(21));
            Assert.That(eval.IsSoft, Is.True);
            Assert.That(eval.IsBlackjack, Is.False);
        }

        [Test]
        public void Evaluate_SoftHand_ReturnsIsSoftTrue()
        {
            var hand = new List<Card> { Ace, Nine };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: false);

            Assert.That(eval.Total, Is.EqualTo(20));
            Assert.That(eval.IsSoft, Is.True);
            Assert.That(eval.IsBlackjack, Is.False);
        }

        [Test]
        public void Evaluate_HardHand_ReturnsIsSoftFalse()
        {
            var hand = new List<Card> { Ten, Nine, Two };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: false);

            Assert.That(eval.Total, Is.EqualTo(21));
            Assert.That(eval.IsSoft, Is.False);
            Assert.That(eval.IsBlackjack, Is.False);
        }

        [Test]
        public void Evaluate_BustHand_ReturnsTotalOver21()
        {
            var hand = new List<Card> { Ten, Ten, Five };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: false);

            Assert.That(eval.Total, Is.EqualTo(25));
            Assert.That(eval.IsSoft, Is.False);
            Assert.That(eval.IsBlackjack, Is.False);
        }

        [Test]
        public void Evaluate_MultipleAces_SoftenedCorrectly()
        {
            var hand = new List<Card> { Ace, Ace, Nine };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: false);

            Assert.That(eval.Total, Is.EqualTo(21));
            Assert.That(eval.IsSoft, Is.True);
            Assert.That(eval.IsBlackjack, Is.False);
        }

        [Test]
        public void Evaluate_MultipleAces_BustIfTooMany()
        {

            var hand = new List<Card> { Ace, Ace, Ten, Nine };
            var eval = HandEvaluator.Instance.Evaluate(hand, treatTwoCard21AsBlackjack: false);

            Assert.That(eval.Total, Is.EqualTo(21));
            Assert.That(eval.IsSoft, Is.False);
            Assert.That(eval.IsBlackjack, Is.False);
        }
    }
}
