using System.Security.Cryptography;
using NUnit.Framework;

namespace BlackJackTest;

[TestFixture]
public class DeckTest
{
    [SetUp]
    public void Setup()
    {
        // Reset the random number generator to ensure consistent shuffling
        BlackJack.Rnd.Rng = new Random(12345);
    }

    [Test]
    public void DeckContainsEightSetOfCards()
    {
        var deck = new BlackJack.Deck();
        Assert.That(deck.Cards.Count, Is.EqualTo(8 * 52));
    }
}
