using System;

using System.Threading;

namespace StringShear
{
    class Program
    {
        static void Main(string[] args)
        {
            Simulation sim = new Simulation();

            double totalComputeElapsed = 0.0;
            double totalOutputElapsed = 0.0;

            int statCycles = 0;

            while (true)
            {
                Thread.Sleep(3 * 1000);

                double computeElapsed = 0.0, outputElapsed = 0.0;
                sim.GetElapsed(ref computeElapsed, ref outputElapsed);

                totalComputeElapsed += computeElapsed;
                totalOutputElapsed += outputElapsed;

                ++statCycles;
                Console.WriteLine("{0} compute({1} avg) - {2} output({3} avg))",
                                  computeElapsed, totalComputeElapsed / statCycles,
                                  outputElapsed, totalOutputElapsed / statCycles);
            }
        }
    }
}
