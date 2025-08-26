using NUnit.Framework;
using System.Diagnostics;
using static Blackjack.Program;

namespace BlackJackTest
{
    [TestFixture]
    public class PerformanceTest
    {
        [Test]
        public void SingleThreadTest()
        {
            var simulator = new BlackjackSimulator
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
        public void MultiThreadTest()
        {
            var warmUpSimulator = new BlackjackSimulator
            {
                Rounds = 10000
            };
            // Initial run to warm up any JIT compilation
            warmUpSimulator.RunSimulation();


            var threads = Environment.ProcessorCount-2;
            if (threads < 1) threads = 1;
            var simulators = new BlackjackSimulator[threads];
            for (var i = 0; i < threads; i++)
            {
                simulators[i] = new BlackjackSimulator { Rounds = 50_000_000 / threads };
            }

            var sw = Stopwatch.StartNew();
            var tasks = new Task[threads];
            for (int i = 0; i < threads; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => simulators[index].RunSimulation());
            }

            Task.WaitAll(tasks);
            var sum = BlackjackSimulator.Sum(simulators);
            sw.Stop();
            TestContext.Out.WriteLine(
                $"Multi-threaded ({threads} threads): {sum.i:N0} rounds in {sw.Elapsed.TotalSeconds:F2} seconds ({sum.i / sw.Elapsed.TotalSeconds:N0} rounds/sec)");
        }
    }
}
