
using ILGPU;
using ILGPU.Algorithms;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Blackjack.Gpu
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceRules
    {
        // Use ints (0/1) instead of bool for GPU-friendliness
        public int HitSoft17;
        public int DoubleAfterSplitAllowed;
        public int DoubleAfterSplitAcesAllowed;
        public int AllowSplit;
        public int ResplitAllowed;
        public int SixCardCharlie;
        public int PeekOnlyOnAce;

        public int BlackjackPayoutNumerator;
        public int BlackjackPayoutDenominator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int B(bool v) => v ? 1 : 0;
    }

    public readonly struct DeviceTables
    {
        public readonly ArrayView<byte> Pairs; // 10x10 (ranks 2..A vs up 2..A)
        public readonly ArrayView<byte> Soft;  // 9x10  (totals 12..20 vs up 2..A)
        public readonly ArrayView<byte> Hard;  // 17x10 (totals 4..20 vs up 2..A)

        public DeviceTables(ArrayView<byte> pairs, ArrayView<byte> soft, ArrayView<byte> hard)
        {
            Pairs = pairs; Soft = soft; Hard = hard;
        }
    }

    // PRNG: xorshift128+
    public struct XorShift128Plus
    {
        private ulong s0, s1;
        public XorShift128Plus(ulong seed, ulong seq)
        {
            ulong z1 = SplitMix64(seed);
            ulong z2 = SplitMix64(seed + 0x9E3779B97F4A7C15UL + seq);
            s0 = z1; s1 = z2;
            if ((s0 | s1) == 0) s1 = 0xBAD5EEDUL;
        }

        public ulong NextULong()
        {
            ulong x = s0;
            ulong y = s1;
            s0 = y;
            x ^= x << 23;
            x ^= x >> 17;
            x ^= y ^ (y >> 26);
            s1 = x;
            return x + y;
        }
        public int NextInt(int bound) => (int)((NextULong() >> 1) % (uint)bound);
        private static ulong SplitMix64(ulong x)
        {
            ulong z = x + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    public static class GpuKernel
    {
        public static void SimKernel(
            Index1D index,
            ArrayView<long> outUnits,
            ArrayView<ulong> seeds,
            long roundsPerThread,
            DeviceRules rules,
            DeviceTables t)
        {
            XorShift128Plus rng = new XorShift128Plus(seeds[index], (ulong)index.X);
            long units = 0;

            for (int r = 0; r < roundsPerThread; r++)
                units += SimOneRound(ref rng, rules, t);

            outUnits[index] = units;
        }

        private static int DrawCard(ref XorShift128Plus rng, out bool isAce)
        {
            int rank = rng.NextInt(13) + 1; // 1..13
            if (rank == 1) { isAce = true; return 11; }
            isAce = false;
            if (rank >= 10) return 10;
            return rank;
        }

        private static int UpIdx(int up) => up == 11 ? 9 : up - 2; // 2..A -> 0..9
        private static int ClampHard(int total)
        {
            if (total < 4) return 0;
            if (total > 20) return 16;
            return total - 4;
        }
        private static int ClampSoft(int total)
        {
            if (total < 12) return 0;
            if (total > 20) return 8;
            return total - 12;
        }
        private static int PairIdx(int rank) => rank - 2;

        private static void AddCardPlayer(ref int total, ref int softAces, ref int cards, ref bool bjEligible, ref XorShift128Plus rng)
        {
            bool ace;
            int v = DrawCard(ref rng, out ace);
            cards++;
            if (ace) { softAces++; total += 11; }
            else total += v;

            while (total > 21 && softAces > 0) { total -= 10; softAces--; }
            if (cards > 2) bjEligible = false;
        }

        private static void AddCardNoBJ(ref int total, ref int softAces, ref int cards, ref XorShift128Plus rng)
        {
            bool ace;
            int v = DrawCard(ref rng, out ace);
            cards++;
            if (ace) { softAces++; total += 11; }
            else total += v;

            while (total > 21 && softAces > 0) { total -= 10; softAces--; }
        }

        private static bool IsBlackjack(int total, bool bjEligible) => bjEligible && total == 21;

        private static byte DecidePair(DeviceTables t, int pairRank, int up)
            => t.Pairs[PairIdx(pairRank) * 10 + UpIdx(up)];
        private static byte DecideHard(DeviceTables t, int total, int up)
            => t.Hard[ClampHard(total) * 10 + UpIdx(up)];
        private static byte DecideSoft(DeviceTables t, int total, int up)
            => t.Soft[ClampSoft(total) * 10 + UpIdx(up)];



        private static void DealerPlay(ref XorShift128Plus rng, DeviceRules rules, ref int dTotal, ref int dSoft, ref int dCards)
        {
            // Dealer stands on all 17 (S17)
            while (true)
            {
                if (dTotal > 21) break;
                if (dTotal > 17) break;
                if (dTotal == 17)
                {
                    if (rules.HitSoft17 == 1 && dSoft > 0) { AddCardNoBJ(ref dTotal, ref dSoft, ref dCards, ref rng); continue; }
                    break;
                }
                AddCardNoBJ(ref dTotal, ref dSoft, ref dCards, ref rng);
            }
        }

        private static long SimOneRound(ref XorShift128Plus rng, DeviceRules rules, DeviceTables t)
        {
            // Deal
            int pTotal = 0, pSoft = 0, pCards = 0;
            int dTotal = 0, dSoft = 0, dCards = 0;
            bool pBJElig = true, dBJElig = true;

            AddCardPlayer(ref pTotal, ref pSoft, ref pCards, ref pBJElig, ref rng); // P1
            // Dealer up
            bool upAce;
            int up = DrawCard(ref rng, out upAce);
            if (upAce) dSoft++;
            dTotal += up; dCards++;
            AddCardPlayer(ref pTotal, ref pSoft, ref pCards, ref pBJElig, ref rng); // P2
            // Dealer hole
            AddCardNoBJ(ref dTotal, ref dSoft, ref dCards, ref rng);

            bool playerBJ = IsBlackjack(pTotal, pBJElig);


            // Peek only on Ace
            if (rules.PeekOnlyOnAce == 1 && up == 11)
            {
                bool dealerBJ = IsBlackjack(dTotal, dBJElig);
                if (dealerBJ) return playerBJ ? 0 : -1*2;
                if (playerBJ) return (long)(rules.BlackjackPayoutNumerator / rules.BlackjackPayoutDenominator)*2;
            }
            else if (playerBJ)
            {
                // Defer settlement until we check if dealer has a natural (without peeking on 10)
                bool dealerBJ = IsBlackjack(dTotal, dBJElig);
                if (dealerBJ) return 0;
                return (long)(rules.BlackjackPayoutNumerator / rules.BlackjackPayoutDenominator)*2;
            }

            // Split check (first decision only)
            float units = 0;
            int stake = 1;

            bool isPair = (pCards == 2) &&
                          ((pSoft == 2 && pTotal == 22) || // A,A
                           (pSoft == 0 && ((pTotal & 1) == 0) && pTotal >= 4 && pTotal <= 20));

            if (rules.AllowSplit == 1 && isPair)
            {
                int pairRank = (pSoft == 2 && pTotal == 22) ? 11 : (pTotal / 2);
                byte pairDecision = DecidePair(t, pairRank, up);
                if (pairDecision == 3) // Split
                {
                    // We'll fully play both hands, then dealer once, then settle
                    int h1Total = 0, h1Soft = 0, h1Cards = 0; bool h1Bust = false; int h1Stake = 1;
                    int h2Total = 0, h2Soft = 0, h2Cards = 0; bool h2Bust = false; int h2Stake = 1;

                    if (pairRank == 11) // Aces: one card each, no hits, no doubles (DAS on Aces not allowed)
                    {
                        // Hand 1
                        h1Cards = 1; h1Soft = 1; h1Total = 11;
                        AddCardNoBJ(ref h1Total, ref h1Soft, ref h1Cards, ref rng);

                        // Hand 2
                        h2Cards = 1; h2Soft = 1; h2Total = 11;
                        AddCardNoBJ(ref h2Total, ref h2Soft, ref h2Cards, ref rng);
                    }
                    else
                    {
                        // Starting from the split card
                        h1Cards = 1; h1Total = (pairRank == 10) ? 10 : pairRank; h1Soft = 0;
                        h2Cards = 1; h2Total = (pairRank == 10) ? 10 : pairRank; h2Soft = 0;

                        // Each gets one extra card
                        AddCardNoBJ(ref h1Total, ref h1Soft, ref h1Cards, ref rng);
                        AddCardNoBJ(ref h2Total, ref h2Soft, ref h2Cards, ref rng);

                        // Play each hand (DAS allowed except Aces already handled)
                        bool d1 = false, d2 = false;
                        float u1 = PlayPlayerHand(ref rng, rules, t, up, ref h1Total, ref h1Soft, ref h1Cards, allowDouble: rules.DoubleAfterSplitAllowed == 1, ref d1);
                        if (u1 == float.NegativeInfinity) h1Bust = true;
                        h1Stake = d1 ? 2 : 1;

                        float u2 = PlayPlayerHand(ref rng, rules, t, up, ref h2Total, ref h2Soft, ref h2Cards, allowDouble: rules.DoubleAfterSplitAllowed == 1, ref d2);
                        if (u2 == float.NegativeInfinity) h2Bust = true;
                        h2Stake = d2 ? 2 : 1;

                        // 6-card Charlie immediate wins
                        if (rules.SixCardCharlie == 1 && !h1Bust && h1Cards >= 6 && h1Total <= 21) units += h1Stake;
                        if (rules.SixCardCharlie == 1 && !h2Bust && h2Cards >= 6 && h2Total <= 21) units += h2Stake;
                        if ((rules.SixCardCharlie == 1 && h1Cards >= 6 && h1Total <= 21) &&
                            (rules.SixCardCharlie == 1 && h2Cards >= 6 && h2Total <= 21))
                        {
                            return (long)(units*2); // both won by Charlie, no need to play dealer
                        }
                    }

                    // Play dealer once
                    DealerPlay(ref rng, rules, ref dTotal, ref dSoft, ref dCards);

                    // Settle hand 1
                    if (pairRank == 11)
                    {
                        if (dTotal > 21 || h1Total > dTotal) units += 1;
                        else if (h1Total < dTotal) units -= 1;
                    }
                    else
                    {
                        if (h1Bust) units -= h1Stake;
                        else if (dTotal > 21 || h1Total > dTotal) units += h1Stake;
                        else if (h1Total < dTotal) units -= h1Stake;
                    }

                    // Settle hand 2
                    if (pairRank == 11)
                    {
                        if (dTotal > 21 || h2Total > dTotal) units += 1;
                        else if (h2Total < dTotal) units -= 1;
                    }
                    else
                    {
                        if (h2Bust) units -= h2Stake;
                        else if (dTotal > 21 || h2Total > dTotal) units += h2Stake;
                        else if (h2Total < dTotal) units -= h2Stake;
                    }

                    return (long)(units * 2);
                }
            }

            // No split, play the hand (double allowed on first decision)
            bool doubled = false;
            float cont = PlayPlayerHand(ref rng, rules, t, up, ref pTotal, ref pSoft, ref pCards, allowDouble: true, ref doubled);
            if (cont == float.NegativeInfinity) return (long)-(doubled ? 4 : 2);
            if (rules.SixCardCharlie == 1 && pCards >= 6 && pTotal <= 21) return (long)(doubled ? 4 : 2);

            // Dealer plays
            DealerPlay(ref rng, rules, ref dTotal, ref dSoft, ref dCards);

            if (dTotal > 21) return (long)(doubled ? 4 : 2);
            if (pTotal > dTotal) return (long)(doubled ? 4 : 2);
            if (pTotal < dTotal) return (long)-(doubled ? 4 : 2);
            return 0;
        }

        // Returns: 0 (continue), NegativeInfinity (busted), or 0 after stand/double
        private static float PlayPlayerHand(
            ref XorShift128Plus rng,
            DeviceRules rules,
            DeviceTables t,
            int up,
            ref int total,
            ref int soft,
            ref int cards,
            bool allowDouble,
            ref bool doubled)
        {
            bool firstDecision = true;

            while (true)
            {
                if (total > 21) return float.NegativeInfinity;
                if (rules.SixCardCharlie == 1 && cards >= 6) return 0;

                byte action;
                // Pair check only on first decision
                if (firstDecision && cards == 2 && ((soft == 2 && total == 22) || (soft == 0 && ((total & 1) == 0) && total >= 4 && total <= 20)))
                {
                    int pairRank = (soft == 2 && total == 22) ? 11 : (total / 2);
                    action = DecidePair(t, pairRank, up);
                }
                else if (soft > 0 && total >= 12)
                {
                    action = DecideSoft(t, total, up);
                }
                else
                {
                    action = DecideHard(t, total, up);
                }

                switch (action)
                {
                    case 0: // Hit
                        AddCardNoBJ(ref total, ref soft, ref cards, ref rng);
                        firstDecision = false;
                        continue;
                    case 1: // Stand
                        return 0;
                    case 2: // Double
                        if (firstDecision && allowDouble)
                        {
                            doubled = true;
                            AddCardNoBJ(ref total, ref soft, ref cards, ref rng);
                            return 0;
                        }
                        // else treat as Hit
                        AddCardNoBJ(ref total, ref soft, ref cards, ref rng);
                        firstDecision = false;
                        continue;
                    case 3: // Split — host handles; here treat as Hit to keep flow coherent for non-split path
                        AddCardNoBJ(ref total, ref soft, ref cards, ref rng);
                        firstDecision = false;
                        continue;
                    default: // N or unknown => Hit
                        AddCardNoBJ(ref total, ref soft, ref cards, ref rng);
                        firstDecision = false;
                        continue;
                }
            }
        }
    }
}
