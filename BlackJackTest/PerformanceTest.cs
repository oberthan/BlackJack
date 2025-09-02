using NUnit.Framework;
using System.Diagnostics;
using Blackjack;

namespace BlackJackTest
{
    [TestFixture]
    public class PerformanceTest
    {
        [Test]
        public void SingleThreadTest()
        {
            var simulator = new Program.BlackjackSimulator
            {
                Rounds = 100000
            };
            // Initial run to warm up any JIT compilation
            simulator.RunSimulation();
            

            simulator.Rounds = 10_000_000;
            var sw = Stopwatch.StartNew();
            simulator.RunSimulation();
            sw.Stop();
            // Benchmark: Single-threaded: 10.000.000 rounds in 6,57 seconds (1.521.968 rounds/sec)
            TestContext.Out.WriteLine($"Single-threaded: {simulator.Rounds:N0} rounds in {sw.Elapsed.TotalSeconds:F2} seconds ({simulator.Rounds / sw.Elapsed.TotalSeconds:N0} rounds/sec)");
        }

        [Test]
        public void MultiThreadFixedRoundsTest()
        {
            var warmUpSimulator = new Program.BlackjackSimulator
            {
                Rounds = 10000
            };
            // Initial run to warm up any JIT compilation
            warmUpSimulator.RunSimulation();


            var threads = Environment.ProcessorCount-1;
            if (threads < 1) threads = 1;
            var simulators = new Program.BlackjackSimulator[threads];
            for (var i = 0; i < threads; i++)
            {
                simulators[i] = new Program.BlackjackSimulator { Rounds = (50_000_000+threads-1) / threads };
            }

            var sw = Stopwatch.StartNew();
            var tasks = new Task[threads];
            try
            {


                for (int i = 0; i < threads; i++)
                {
                    int index = i;
                    tasks[i] = Task.Run(() => simulators[index].RunSimulation());
                }

                Task.WaitAll(tasks);
            }
            catch (Exception e)
            {
                TestContext.WriteLine(e);
                throw;
            }
            var sum = Program.BlackjackSimulator.Sum(simulators);

            sw.Stop();
            TestContext.Out.WriteLine(
                $"Multi-threaded ({threads} threads): {sum.rounds:N0} rounds in {sw.Elapsed.TotalSeconds:F2} seconds ({sum.rounds / sw.Elapsed.TotalSeconds:N0} rounds/sec)");
        }

        [Test]
        public void MultiThreadMinRoundsTest()
        {
            var warmUpSimulator = new Program.BlackjackSimulator
            {
                Rounds = 10000
            };
            // Initial run to warm up any JIT compilation
            warmUpSimulator.RunSimulation();

            var minTotalRounds = 50_000_000;



            var threads = Environment.ProcessorCount-1;
            if (threads < 1) threads = 1;

            var simulators = new Program.BlackjackSimulator[threads];
            for (var i = 0; i < threads; i++)
            {
                simulators[i] = new Program.BlackjackSimulator();
            }

            var sw = Stopwatch.StartNew();
            var tasks = new Task[threads];
            using CancellationTokenSource cts = new();
            var cancellationToken = cts.Token;
            for (int i = 0; i < threads; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => simulators[index].RunSimulationUntilCancelled(cancellationToken), cancellationToken);
            }
            var waitHandle = cancellationToken.WaitHandle;
            var terminationTask = Task.Run(() =>
            {
                while (true)
                {
                    if (simulators.Sum(x => x.rounds) >= minTotalRounds)
                    {
                        return;
                    }

                    waitHandle.WaitOne(20);
                }
            }, cancellationToken);
            terminationTask.Wait();
            cts.Cancel();
            Task.WaitAll(tasks);
            
            var sum = Program.BlackjackSimulator.Sum(simulators);
            sw.Stop();
            TestContext.Out.WriteLine(
                $"Multi-threaded ({threads} threads): {sum.rounds:N0} rounds in {sw.Elapsed.TotalSeconds:F2} seconds ({sum.rounds / sw.Elapsed.TotalSeconds:N0} rounds/sec)");
        }
    }
}
