using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace StringShear
{
    static class Program
    {
        static void RunStats()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                Console.WriteLine(ScopeTiming.Summary);
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            ScopeTiming.Init(true);

            Console.Write("Setting up simulation...");
            Simulation sim = new Simulation();
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
                    string state = sim.ToString();
                    ScopeTiming.RecordScope("Output.ToString", sw);

                    sw.Restart();
                    using (StreamWriter writer = new StreamWriter(ctxt.Response.OutputStream))
                        writer.Write(state);
                    ScopeTiming.RecordScope("Output.StreamWriter", sw);
                }
                else
                {
                    string settings;
                    using (StreamReader reader = new StreamReader(ctxt.Request.InputStream))
                        settings = reader.ReadToEnd();
                    ctxt.Response.OutputStream.Close();
                    sim.ApplySettings(settings);
                }
#if DEBUG
                Console.Write("!");
#endif
            }
        }
    }
}
