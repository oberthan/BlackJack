using System;
using System.Diagnostics;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;
using Blackjack;

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

            using Context context = Context.Create(builder =>
                builder.Default().EnableAlgorithms().Optimize(OptimizationLevel.O2));
            Device device = context.GetPreferredDevice(preferCPU: false);
            //device.PrintInformation();
            Accelerator accelerator = device.CreateAccelerator(context);

            using (accelerator)
            {
                if (verbose)
                    Console.WriteLine($"[ILGPU] Using {accelerator.AcceleratorType} accelerator: {accelerator.Name}");

                long threadsLong = explicitThreads ?? device.MaxNumThreads;
                long neededThreads = Math.Max(1, (totalRounds + roundsPerThread - 1) / roundsPerThread);
                if (threadsLong > neededThreads) threadsLong = neededThreads;
                int threads = (int)Math.Clamp(threadsLong, 1, int.MaxValue);
                roundsPerThread = totalRounds/threads;
                long scheduledRounds = (long)threads * roundsPerThread;

                if (verbose)
                {
                    Console.WriteLine($"[ILGPU] threads = {threads:n0}, roundsPerThread = {roundsPerThread:n0}");
                    Console.WriteLine($"[ILGPU] scheduled rounds = {scheduledRounds:n0} (requested {totalRounds:n0})");
                }

                var kernel = accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView<int>,
                    ArrayView<ulong>,
                    long,             // roundsPerThread
                    DeviceRules,     // rules
                    DeviceTables,    // tables
                    int              // bjTimes2
                >(GpuKernel.SimKernel);

                using var outUnitsTimes2 = accelerator.Allocate1D<int>(threads);
                using var seeds = accelerator.Allocate1D<ulong>(threads);

                // Seeds
                ulong[] seedsHost = new ulong[threads];
                for (int i = 0; i < threads; i++)
                    seedsHost[i] = SplitMix64(seed + (ulong)i * 0x9E3779B97F4A7C15UL);
                seeds.CopyFromCPU(seedsHost);

                // Strategy
                //StrategyTables hostTables = Blackjack.Strategy.Instance.ToTables();

                StrategyTables hostTables = StrategyTables.FromDefaults();
                
                using StrategyTablesDevice devTables = hostTables.Upload(accelerator);
                DeviceTables deviceTables = devTables.Tables;

                // Rules
                DeviceRules rules = new DeviceRules
                {
                    HitSoft17 = DeviceRules.B(false),
                    DoubleAfterSplitAllowed = DeviceRules.B(true),
                    DoubleAfterSplitAcesAllowed = DeviceRules.B(false),
                    AllowSplit = DeviceRules.B(true),
                    ResplitAllowed = DeviceRules.B(false),
                    SixCardCharlie = DeviceRules.B(true),
                    PeekOnlyOnAce = DeviceRules.B(true),
                    BlackjackPayoutNumerator = 3,
                    BlackjackPayoutDenominator = 2,
                };

                // Blackjack payout in ×2 units (3 for 3:2)
                int bjTimes2 = (rules.BlackjackPayoutNumerator * 2) / rules.BlackjackPayoutDenominator;

                Stopwatch sw = Stopwatch.StartNew();
                kernel(threads, outUnitsTimes2.View, seeds.View,
                       roundsPerThread, rules, deviceTables, bjTimes2);
                accelerator.Synchronize();
                sw.Stop();

                var hostUnits2 = outUnitsTimes2.GetAsArray1D();
                double totalUnitsTimes2 = 0;
                for (int i = 0; i < hostUnits2.Length; i++) totalUnitsTimes2 += hostUnits2[i];

                double rtp = 1.0 + (totalUnitsTimes2 / 2.0) / scheduledRounds;
                Console.WriteLine($"[ILGPU] GPU time: {sw.Elapsed.TotalSeconds:F3}s");
                Console.WriteLine($"[ILGPU] GPU rounds per second: {scheduledRounds / sw.Elapsed.TotalSeconds:n0}/s");
                Console.WriteLine($"[ILGPU] Units delta = {totalUnitsTimes2/2:n1} over {scheduledRounds:n0} rounds");
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
