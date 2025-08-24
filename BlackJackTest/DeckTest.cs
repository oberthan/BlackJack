using System.Security.Cryptography;
using NUnit.Framework;

namespace BlackjackTest;

[TestFixture]
public class DeckTest
{
    [SetUp]
    public void Setup()
    {
        // Reset the random number generator to ensure consistent shuffling
        Blackjack.Rnd.Rng = new Random(12345);
    }

    [Test]
    public void DeckContainsEightSetOfCards()
    {
        var deck = new Blackjack.Deck();
        Assert.That(deck.Cards.Count, Is.EqualTo(8 * 52));
    }
}
