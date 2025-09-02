using System;
using ILGPU;
using ILGPU.Algorithms;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Blackjack.Gpu
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceRules
    {
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

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly struct DeviceTables(ArrayView<byte> pairs, ArrayView<byte> soft, ArrayView<byte> hard)
    {
        public readonly ArrayView<byte> Pairs = pairs;
        public readonly ArrayView<byte> Soft = soft;
        public readonly ArrayView<byte> Hard = hard;
    }

    // PRNG: xorshift128+ with fast bounded int
    public struct XorShift128Plus
    {
        private ulong s0, s1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XorShift128Plus(ulong seed, ulong seq)
        {
            ulong z1 = SplitMix64(seed);
            ulong z2 = SplitMix64(seed + 0x9E3779B97F4A7C15UL + seq);
            s0 = z1; s1 = z2;
            if ((s0 | s1) == 0) s1 = 0xBAD5EEDUL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt() => (uint)(NextULong() >> 32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int bound) => (int)(((ulong)NextUInt() * (uint)bound) >> 32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        // NOTE: Removed SpecializedValue<> so everything is plain parameters
        public static void SimKernel(
            Index1D index,
            ArrayView<int> outUnitsTimes2,
            ArrayView<ulong> seeds,
            long roundsPerThread,
            DeviceRules rules,
            DeviceTables t,
            int bjTimes2)
        {
            XorShift128Plus rng = new XorShift128Plus(seeds[index], (ulong)index.X);

            int unitsTimes2 = 0;

            for (int r = 0; r < roundsPerThread; r++)
                unitsTimes2 += SimOneRoundTimes2(ref rng, rules, t, bjTimes2);

            outUnitsTimes2[index] = unitsTimes2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DrawCard(ref XorShift128Plus rng, out bool isAce)
        {
            int rank = rng.NextInt(13) + 1; // 1..13

            if (rank == 1) { isAce = true; return 11; }
            isAce = false;
            if (rank >= 10) return 10;
            return rank;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UpIdx(int up) => up == 11 ? 9 : up - 2; // 2..A -> 0..9
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampHard(int total) => total < 4 ? 0 : (total > 20 ? 16 : total - 4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampSoft(int total) => total < 12 ? 0 : (total > 20 ? 8 : total - 12);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int PairIdx(int rank) => rank - 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddCardPlayer(ref int total, ref int softAces, ref int cards, ref bool bjEligible, ref XorShift128Plus rng)
        {
            int v = DrawCard(ref rng, out var ace);
            cards++;
            if (ace) { softAces++; total += 11; }
            else total += v;
            while (total > 21 && softAces > 0) { total -= 10; softAces--; }
            if (cards > 2) bjEligible = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddCardNoBJ(ref int total, ref int softAces, ref int cards, ref XorShift128Plus rng)
        {
            int v = DrawCard(ref rng, out var ace);
            cards++;
            if (ace) { softAces++; total += 11; }
            else total += v;
            while (total > 21 && softAces > 0) { total -= 10; softAces--; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsBlackjack(int total, bool bjEligible) => bjEligible && total == 21;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte DecidePair(DeviceTables t, int pairRank, int up)
            => t.Pairs[PairIdx(pairRank) * 10 + UpIdx(up)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte DecideHard(DeviceTables t, int total, int up)
            => t.Hard[ClampHard(total) * 10 + UpIdx(up)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte DecideSoft(DeviceTables t, int total, int up)
            => t.Soft[ClampSoft(total) * 10 + UpIdx(up)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DealerPlay(ref XorShift128Plus rng, DeviceRules rules, ref int dTotal, ref int dSoft, ref int dCards)
        {
            while (true)
            {
                if (dTotal > 21) break;
                if (dTotal > 17) break;
                if (dTotal == 17)
                {
                    if (rules.HitSoft17 == 1 && dSoft > 0)
                    { AddCardNoBJ(ref dTotal, ref dSoft, ref dCards, ref rng); continue; }
                    break;
                }
                AddCardNoBJ(ref dTotal, ref dSoft, ref dCards, ref rng);
            }
        }

        private static int SimOneRoundTimes2(ref XorShift128Plus rng, DeviceRules rules, DeviceTables t, int bjTimes2)
        {
            int pTotal = 0, pSoft = 0, pCards = 0;
            int dTotal = 0, dSoft = 0, dCards = 0;
            bool pBJElig = true, dBJElig = true;

            AddCardPlayer(ref pTotal, ref pSoft, ref pCards, ref pBJElig, ref rng); // P1

            int up = DrawCard(ref rng, out var upAce);
            if (upAce) dSoft++;
            dTotal += up; dCards++;

            AddCardPlayer(ref pTotal, ref pSoft, ref pCards, ref pBJElig, ref rng); // P2

            AddCardNoBJ(ref dTotal, ref dSoft, ref dCards, ref rng); // hole

            bool playerBJ = IsBlackjack(pTotal, pBJElig);

            // Peek only on Ace
            if (rules.PeekOnlyOnAce == 1 && up == 11)
            {
                bool dealerBJ = IsBlackjack(dTotal, dBJElig);
                if (dealerBJ) return playerBJ ? 0 : -2;
                if (playerBJ) return bjTimes2; // 3 (in ×2 units) for 3:2
            }
            else if (playerBJ)
            {
                bool dealerBJ = IsBlackjack(dTotal, dBJElig);
                if (dealerBJ) return 0;
                return bjTimes2;
            }

            int unitsTimes2 = 0;

            bool isPair = (pCards == 2) &&
                          ((pSoft == 2 && pTotal == 22) ||
                           (pSoft == 0 && ((pTotal & 1) == 0) && pTotal is >= 4 and <= 20));

            if (rules.AllowSplit == 1 && isPair)
            {
                int pairRank = (pSoft == 2 && pTotal == 22) ? 11 : (pTotal / 2);
                byte pairDecision = DecidePair(t, pairRank, up);
                if (pairDecision == 3) // Split
                {
                    int h1Total = 0, h1Soft = 0, h1Cards = 0; bool h1Bust = false; int h1Stake = 1;
                    int h2Total = 0, h2Soft = 0, h2Cards = 0; bool h2Bust = false; int h2Stake = 1;

                    if (pairRank == 11) // Aces: one card each
                    {
                        h1Cards = 1; h1Soft = 1; h1Total = 11;
                        AddCardNoBJ(ref h1Total, ref h1Soft, ref h1Cards, ref rng);

                        h2Cards = 1; h2Soft = 1; h2Total = 11;
                        AddCardNoBJ(ref h2Total, ref h2Soft, ref h2Cards, ref rng);
                    }
                    else
                    {
                        h1Cards = 1; h1Total = (pairRank == 10) ? 10 : pairRank; h1Soft = 0;
                        h2Cards = 1; h2Total = (pairRank == 10) ? 10 : pairRank; h2Soft = 0;

                        AddCardNoBJ(ref h1Total, ref h1Soft, ref h1Cards, ref rng);
                        AddCardNoBJ(ref h2Total, ref h2Soft, ref h2Cards, ref rng);

                        bool d1 = false, d2 = false;
                        float u1 = PlayPlayerHand(ref rng, rules, t, up, ref h1Total, ref h1Soft, ref h1Cards, allowDouble: rules.DoubleAfterSplitAllowed == 1, ref d1);
                        if (float.IsNegativeInfinity(u1)) h1Bust = true;
                        h1Stake = d1 ? 2 : 1;

                        float u2 = PlayPlayerHand(ref rng, rules, t, up, ref h2Total, ref h2Soft, ref h2Cards, allowDouble: rules.DoubleAfterSplitAllowed == 1, ref d2);
                        if (float.IsNegativeInfinity(u2)) h2Bust = true;
                        h2Stake = d2 ? 2 : 1;

                        if (rules.SixCardCharlie == 1 && !h1Bust && h1Cards >= 6 && h1Total <= 21) unitsTimes2 += h1Stake * 2;
                        if (rules.SixCardCharlie == 1 && !h2Bust && h2Cards >= 6 && h2Total <= 21) unitsTimes2 += h2Stake * 2;
                        if ((rules.SixCardCharlie == 1 && h1Cards >= 6 && h1Total <= 21) &&
                            (rules.SixCardCharlie == 1 && h2Cards >= 6 && h2Total <= 21))
                        {
                            return unitsTimes2;
                        }
                    }

                    DealerPlay(ref rng, rules, ref dTotal, ref dSoft, ref dCards);

                    if (pairRank == 11)
                    {
                        if (dTotal > 21 || h1Total > dTotal) unitsTimes2 += 2;
                        else if (h1Total < dTotal) unitsTimes2 -= 2;

                        if (dTotal > 21 || h2Total > dTotal) unitsTimes2 += 2;
                        else if (h2Total < dTotal) unitsTimes2 -= 2;
                    }
                    else
                    {
                        if (h1Bust) unitsTimes2 -= h1Stake * 2;
                        else if (dTotal > 21 || h1Total > dTotal) unitsTimes2 += h1Stake * 2;
                        else if (h1Total < dTotal) unitsTimes2 -= h1Stake * 2;

                        if (h2Bust) unitsTimes2 -= h2Stake * 2;
                        else if (dTotal > 21 || h2Total > dTotal) unitsTimes2 += h2Stake * 2;
                        else if (h2Total < dTotal) unitsTimes2 -= h2Stake * 2;
                    }

                    return unitsTimes2;
                }
            }

            bool doubled = false;
            float cont = PlayPlayerHand(ref rng, rules, t, up, ref pTotal, ref pSoft, ref pCards, allowDouble: true, ref doubled);
            if (float.IsNegativeInfinity(cont)) return -(doubled ? 4 : 2);
            if (rules.SixCardCharlie == 1 && pCards >= 6 && pTotal <= 21) return (doubled ? 4 : 2);

            DealerPlay(ref rng, rules, ref dTotal, ref dSoft, ref dCards);

            if (dTotal > 21) return (doubled ? 4 : 2);
            if (pTotal > dTotal) return (doubled ? 4 : 2);
            if (pTotal < dTotal) return -(doubled ? 4 : 2);
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                if (firstDecision && cards == 2 && ((soft == 2 && total == 22) || (soft == 0 && ((total & 1) == 0) && total is >= 4 and <= 20)))
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
                        AddCardNoBJ(ref total, ref soft, ref cards, ref rng);
                        firstDecision = false;
                        continue;
                    case 3: // Split (handled by caller) -> treat like Hit here
                        AddCardNoBJ(ref total, ref soft, ref cards, ref rng);

                        firstDecision = true; // TODO: It should still be first decision

                        continue;
                    default: // N/unknown => Hit
                        AddCardNoBJ(ref total, ref soft, ref cards, ref rng);
                        firstDecision = false;
                        continue;
                }
            }
        }
    }
}
