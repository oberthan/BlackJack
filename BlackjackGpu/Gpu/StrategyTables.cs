
using System;
using ILGPU;
using ILGPU.Runtime;

namespace Blackjack.Gpu
{
    public sealed class StrategyTables
    {
        // Flat arrays
        public byte[] Pairs = new byte[10 * 10]; // ranks 2..A vs up 2..A
        public byte[] Soft  = new byte[9 * 10];  // totals 12..20 vs up 2..A
        public byte[] Hard  = new byte[17 * 10]; // totals 4..20 vs up 2..A

        // Encodings
        // 0=H,1=S,2=D,3=P,4=N
        public const byte H = 0, S = 1, D = 2, P = 3, N = 4;

        public static StrategyTables FromDefaults()
        {
            StrategyTables t = new StrategyTables();

            Array.Fill(t.Pairs, H);
            Array.Fill(t.Soft, H);
            Array.Fill(t.Hard, H);

            // HARD
            for (int up = 2; up <= 11; up++)
            {
                for (int tot = 17; tot <= 20; tot++) t.SetHard(tot, up, S);
                for (int tot = 13; tot <= 16; tot++) t.SetHard(tot, up, (up <= 6 ? S : H));
                t.SetHard(12, up, (up >= 4 && up <= 6) ? S : H);
                if (up >= 3 && up <= 6) t.SetHard(9, up, D);
                if (up >= 2 && up <= 9) t.SetHard(10, up, D);
                if (up >= 2 && up <= 10) t.SetHard(11, up, D);
            }

            // SOFT
            for (int up = 2; up <= 11; up++)
            {
                t.SetSoft(19, up, S);
                t.SetSoft(20, up, S);
                if (up >= 3 && up <= 6) t.SetSoft(18, up, D);
                if (up == 2 || up == 7 || up == 8) t.SetSoft(18, up, S);
                if (up == 9 || up == 10 || up == 11) t.SetSoft(18, up, H);
                t.SetSoft(17, up, (up >= 4 && up <= 6) ? D : H);
                t.SetSoft(16, up, (up >= 4 && up <= 6) ? D : H);
                t.SetSoft(15, up, (up >= 4 && up <= 6) ? D : H);
                t.SetSoft(14, up, (up >= 5 && up <= 6) ? D : H);
                t.SetSoft(13, up, (up >= 5 && up <= 6) ? D : H);
            }

            // PAIRS
            for (int up = 2; up <= 11; up++)
            {
                t.SetPair(11, up, P); // A,A
                t.SetPair(8, up, P);
                t.SetPair(10, up, S);
                if ((up >= 2 && up <= 6) || up == 8 || up == 9) t.SetPair(9, up, P); else t.SetPair(9, up, S);
                t.SetPair(7, up, (up >= 2 && up <= 7) ? P : H);
                t.SetPair(6, up, (up >= 2 && up <= 6) ? P : H);
                t.SetPair(4, up, (up >= 5 && up <= 6) ? P : H);
                t.SetPair(3, up, (up >= 2 && up <= 7) ? P : H);
                t.SetPair(2, up, (up >= 2 && up <= 7) ? P : H);
            }

            return t;
        }

        public void SetPair(int rank, int up, byte v) => Pairs[(rank - 2) * 10 + (up == 11 ? 9 : up - 2)] = v;
        public void SetSoft(int total, int up, byte v) => Soft[(total - 12) * 10 + (up == 11 ? 9 : up - 2)] = v;
        public void SetHard(int total, int up, byte v) => Hard[(total - 8) * 10 + (up == 11 ? 9 : up - 2)] = v;

        public StrategyTablesDevice Upload(Accelerator acc)
        {
            var pairs = acc.Allocate1D<byte>(Pairs.Length);
            var soft  = acc.Allocate1D<byte>(Soft.Length);
            var hard  = acc.Allocate1D<byte>(Hard.Length);

            pairs.CopyFromCPU(Pairs);
            soft.CopyFromCPU(Soft);
            hard.CopyFromCPU(Hard);

            return new StrategyTablesDevice(pairs, soft, hard);
        }
    }

    public sealed class StrategyTablesDevice : System.IDisposable
    {
        public MemoryBuffer1D<byte, Stride1D.Dense> Pairs { get; }
        public MemoryBuffer1D<byte, Stride1D.Dense> Soft { get; }
        public MemoryBuffer1D<byte, Stride1D.Dense> Hard { get; }

        public StrategyTablesDevice(
            MemoryBuffer1D<byte, Stride1D.Dense> pairs,
            MemoryBuffer1D<byte, Stride1D.Dense> soft,
            MemoryBuffer1D<byte, Stride1D.Dense> hard)
        {
            Pairs = pairs; Soft = soft; Hard = hard;
        }

        public DeviceTables Tables => new DeviceTables(Pairs.View, Soft.View, Hard.View);

        public void Dispose()
        {
            Pairs.Dispose();
            Soft.Dispose();
            Hard.Dispose();
        }
    }
}
