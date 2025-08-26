using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
