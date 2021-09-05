using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace StringShear
{
    static class Program
    {
        static Simulation g_sim;

        static double g_totalOutputElapsedMs = 0.0;
        static int g_outputsCount = 0;
        static object g_outputCs = new object();

        static double OutputElapsedMs
        {
            get
            {
                lock (g_outputCs)
                    return 
                        g_outputsCount == 0 
                        ? 0.0 
                        : g_totalOutputElapsedMs / g_outputsCount;
            }
            set
            {
                lock (g_outputCs)
                {
                    g_totalOutputElapsedMs += value;
                    ++g_outputsCount;
                }
            }
        }

        static void RunStats()
        {
            double totalComputeElapsed = 0.0;
            int statCycles = 0;

            while (true)
            {
                Thread.Sleep(3 * 1000);

                double computeElapsed = g_sim.ComputeElapsedMs;
                if (computeElapsed == 0.0) // paused
                    continue;
                totalComputeElapsed += computeElapsed;

                ++statCycles;

                Console.WriteLine
                (
                    "compute: {0} - output: {1}",
                    totalComputeElapsed / statCycles,
                    OutputElapsedMs
                );
            }
        }

        static void Main(string[] args)
        {
            Console.Write("Setting up simulation...");
            g_sim = new Simulation();
            Console.WriteLine("done!");

            Console.Write("Setting up server...");
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:9914/");
            listener.Start();
            Console.WriteLine("done!");

            new Thread(RunStats).Start();

            Stopwatch sw = new Stopwatch();
            while (true)
            {
#if DEBUG
                Console.Write("?");
#endif
                var ctxt = listener.GetContext();
#if DEBUG
                Console.Write(".");
#endif
                if (ctxt.Request.HttpMethod == "GET")
                {
                    sw.Restart();
                    string state = g_sim.ToString();
                    using (StreamWriter writer = new StreamWriter(ctxt.Response.OutputStream))
                        writer.Write(state);
                    OutputElapsedMs = sw.Elapsed.TotalMilliseconds;
                }
                else
                {
                    string settings;
                    using (StreamReader reader = new StreamReader(ctxt.Request.InputStream))
                        settings = reader.ReadToEnd();
                    ctxt.Response.OutputStream.Close();
                    g_sim.ApplySettings(settings);
                }
#if DEBUG
                Console.Write("!");
#endif
            }
        }
    }
}
