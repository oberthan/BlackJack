using Blackjack;
using Blackjack.Gpu;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace BlackJackTest
{
    [TestFixture]
    public class GpuKernelTest
    {
        private static int UpIdx(int up) => up == 11 ? 9 : up - 2; // 2..A -> 0..9

        private static int ClampHard(int total) => total < 8 ? 0 : (total > 17 ? 9 : total - 8);

        private static int ClampSoft(int total) => total < 12 ? 0 : (total > 20 ? 8 : total - 12);

        private static int PairIdx(int rank) => rank - 2;
        private static byte DecidePair(StrategyTables t, int pairRank, int up)
            => t.Pairs[PairIdx(pairRank) * 10 + UpIdx(up)];

        private static byte DecideHard(StrategyTables t, int total, int up)
            => t.Hard[ClampHard(total) * 10 + UpIdx(up)];


        private static byte DecideSoft(StrategyTables t, int total, int up)
            => t.Soft[ClampSoft(total) * 10 + UpIdx(up)];
        private static byte PlayPlayerHand(int total, int soft, int cards, int up)
        {

            StrategyTables t = Strategy.Instance.ToTables();

            bool firstDecision = true;

            //while (true)
            //{

                if (cards >= 6) return 1;

                byte action;


                if (soft > 0 && total >= 12) // TODO: Uhm 12??
                {
                    action = DecideSoft(t, total, up);
                }
                else
                {
                    // --- HARD TOTALS ---

                    // Add the same fallback logic as in Strategy:
                    // if total > hardStrategyMaxTotal -> Stand
                    // if total < hardStrategyMinTotal -> Hit
                    if (total > 17)
                    {
                        action = 1; // Stand
                    }
                    else if (total < 8)
                    {
                        action = 0; // Hit
                    }
                    else
                    {
                        // inside strategy table range -> consult table
                        action = DecideHard(t, total, up);
                    }
                }

                return action;

                //Debug.Assert(action == Strategy.Instance.DecideGpuCheck(total, soft > 0 && total >= 12, cards, up, firstDecision && allowDouble));


                //}
        }

        [Test]
        public void GpuStrategy_Same_As_Cpu()
        {
            for (int p = 4; p < 21; p++)
            {
                for (int up = 2; up <= 11; up++)
                {
                    for (int s = 0; s <= Math.Floor((float)p/11); s++)
                    {

                        var cpuResponce = Strategy.Instance.DecideGpuCheck(p, s > 0 && p >= 12, 2, up, true);
                        var gpuResponce = PlayPlayerHand(p, s, 2, up);
                        Assert.That(gpuResponce, Is.EqualTo(cpuResponce), $"Mismatch for {p} {(s > 0 ? "Soft" : "Hard")} vs {up}");
                    
                    }
                }

            }
        }

        [Test]
        public void NextInt_DistributionIsUniform()
        {
            const int bound = 13;
            const int numSamples = 1000000000;
            const double expectedFrequency = numSamples / (double)bound;
            const double tolerance = 0.01; // 1% tolerance

            Random rngSeed = new Random();
            ulong seed = (ulong)rngSeed.NextInt64(long.MaxValue);
            var rng = new XorShift128Plus(seed, 0);

            int[] counts = new int[bound];

            // Generate samples
            for (int i = 0; i < numSamples; i++)
            {
                int value = rng.NextInt(bound);
                counts[value]++;
            }

            // Check distribution
            for (int i = 0; i < bound; i++)
            {
                double frequency = counts[i];
                double ratio = frequency / expectedFrequency;
                TestContext.WriteLine($"{i+1}: {frequency}");
                Assert.That(ratio, Is.EqualTo(1.0).Within(tolerance),
                    $"Value {i+1} occurred {frequency} times (expected ~{expectedFrequency})");
            }
        }
    }
}