
using System;
using System.Diagnostics;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;

namespace Blackjack.Gpu
{
    public static class GpuRunner
    {
        public static void Run(
            long totalRounds,
            int? explicitThreads = null,
            long roundsPerThread = 2048,
            ulong seed = 0xC0FFEE1234567890UL,
            bool verbose = true)
        {
            if (roundsPerThread < 1) roundsPerThread = 1;

            using Context context = Context.Create(builder => builder.Default().EnableAlgorithms());

            Accelerator accelerator = context.GetPreferredDevice(preferCPU: false).CreateAccelerator(context);


            using (accelerator)
            {
                if (verbose)
                    Console.WriteLine($"[ILGPU] Using {accelerator.AcceleratorType} accelerator: {accelerator.Name}");

                long threadsLong = explicitThreads ?? Math.Clamp(Environment.ProcessorCount * 1024, 8_192, 1_048_576);
                long neededThreads = Math.Max(1, (totalRounds + roundsPerThread - 1) / roundsPerThread);
                if (threadsLong > neededThreads) threadsLong = neededThreads;
                int threads = (int)Math.Clamp(threadsLong, 1, int.MaxValue);
                roundsPerThread = totalRounds / threads;
                long scheduledRounds = (long)threads * roundsPerThread;

                if (verbose)
                {
                    Console.WriteLine($"[ILGPU] threads = {threads:n0}, roundsPerThread = {roundsPerThread:n0}");
                    Console.WriteLine($"[ILGPU] scheduled rounds = {scheduledRounds:n0} (requested {totalRounds:n0})");
                }

                var kernel = accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView<long>,
                    ArrayView<ulong>,
                    long,
                    DeviceRules,
                    DeviceTables>(GpuKernel.SimKernel);

                using var outUnits = accelerator.Allocate1D<long>(threads);
                using var seeds = accelerator.Allocate1D<ulong>(threads);

                // Seed array
                ulong[] seedsHost = new ulong[threads];
                for (int i = 0; i < threads; i++)
                    seedsHost[i] = SplitMix64(seed + (ulong)i * 0x9E3779B97F4A7C15UL);
                seeds.CopyFromCPU(seedsHost);

                // Strategy
                StrategyTables hostTables = StrategyTables.FromDefaults();
                using StrategyTablesDevice devTables = hostTables.Upload(accelerator);
                DeviceTables deviceTables = devTables.Tables; // ArrayViews for kernel

                // Rules (Phase 1 defaults per your table)
                DeviceRules rules = new DeviceRules
                {
                    HitSoft17 = DeviceRules.B(false),                  // S17
                    DoubleAfterSplitAllowed = DeviceRules.B(true),     // DAS
                    DoubleAfterSplitAcesAllowed = DeviceRules.B(false),
                    AllowSplit = DeviceRules.B(true),
                    ResplitAllowed = DeviceRules.B(false),
                    SixCardCharlie = DeviceRules.B(true),
                    PeekOnlyOnAce = DeviceRules.B(true),
                    BlackjackPayoutNumerator = 3,
                    BlackjackPayoutDenominator = 2,
                };


                Stopwatch sw = Stopwatch.StartNew();
                kernel(threads, outUnits.View, seeds.View, roundsPerThread, rules, deviceTables);
                accelerator.Synchronize();
                sw.Stop();

                // Reduce on host
                var hostUnits = outUnits.GetAsArray1D();
                long totalUnits = 0;
                for (int i = 0; i < hostUnits.Length; i++) totalUnits += hostUnits[i];

                float rtp = 1 + (float)((float)totalUnits / (float)scheduledRounds);
                Console.WriteLine($"[ILGPU] GPU time: {sw.Elapsed.TotalSeconds:F3}s");
                Console.WriteLine($"[ILGPU] Units delta = {totalUnits:n0} over {scheduledRounds:n0} rounds");
                Console.WriteLine($"[ILGPU] RTP (initial wager) ≈ {rtp:P5}");
            }
        }

        private static ulong SplitMix64(ulong x)
        {
            ulong z = x + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}
