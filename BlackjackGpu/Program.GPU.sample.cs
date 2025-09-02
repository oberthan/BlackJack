
using System;
using Blackjack.Gpu;

namespace Blackjack.Gpu
{
    public partial class Program
    {
        private static void Main(string[] args)
        {
            bool useGpu = true;
            long rounds = 1_000_000_000;
            int? threads = null;
            int rpt = 2048;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--gpu": useGpu = true; break;
                    case "--rounds":
                        if (i + 1 < args.Length && long.TryParse(args[i + 1], out long r)) { rounds = r; i++; }
                        break;
                    case "--threads":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int t)) { threads = t; i++; }
                        break;
                    case "--rpt":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int v)) { rpt = v; i++; }
                        break;
                }
            }

            if (useGpu)
            {
                Console.WriteLine("Running GPU simulation via ILGPU 1.5.3...");
                GpuRunner.Run(rounds, threads, rpt);
                return;
            }

            Console.WriteLine("Running existing CPU simulation...");
            // Call your current CPU entry point here.
        }
    }
}
