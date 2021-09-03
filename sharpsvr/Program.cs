using System;

using System.Threading;

namespace StringShear
{
    class Program
    {
        static void Main(string[] args)
        {
            Simulation sim = new Simulation();
            while (true)
            {
                Thread.Sleep(3 * 1000);
                Console.WriteLine
                (
                    "compute: " + sim.ComputeElapsedMs 
                    + " - " 
                    + "output: " + sim.OutputElapsedMs
                );
            }
        }
    }
}
