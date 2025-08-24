using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack.Tests
{
    [TestFixture]
    public class StrategyTest
    {
        [Test]
        public void Strategy_OldSameAsNew()
        {
            string[] cardValues = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            for (int i = 0; i < cardValues.Length; i++)
            {

                for (int k = 0; k < cardValues.Length; k++)
                {
                    for (int g = 0; g < cardValues.Length; g++)
                    {
                        string dealerValue = cardValues[g];
                        var player = new Player();
                        player.AddCard(new Card(cardValues[i], "S"));
                        player.AddCard(new Card(cardValues[k], "D"));
                        var dealerUp = new Card(dealerValue, "H");
                        var move = Strategy.Instance.Decide(player, dealerUp, false);
                        var oldMove = Strategy.DecideOld(player, dealerUp, false);

                        TestContext.WriteLine(player.Hand[0]);
                        TestContext.WriteLine(player.Hand[1]);
                        TestContext.WriteLine(dealerUp);
                        TestContext.WriteLine("\n");
                        Assert.That(move, Is.EqualTo(oldMove));
                    }
                }
            }
        }

        [TestCase(new[] {"7", "A"}, "3")]
        [TestCase(new[] {"10", "A"}, "2")]
        [TestCase(new[] {"2", "A", "3", "5"}, "J")]
        public void Strategy_OldSameAsNew_Specific(string[] cards, string dealerCard)
        {

            var player = new Player();
            foreach (var card in cards)
            {
            player.AddCard(new Card(card, "S"));

            }
            var dealerUp = new Card(dealerCard, "H");
            var move = Strategy.Instance.Decide(player, dealerUp, false);
            var oldMove = Strategy.DecideOld(player, dealerUp, false);

            TestContext.WriteLine(player.Hand[0]);
            TestContext.WriteLine(player.Hand[1]);
            TestContext.WriteLine(dealerUp);
            TestContext.WriteLine("\n");
            Assert.That(move, Is.EqualTo(oldMove));
        }
    }
}
