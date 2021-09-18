using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StringShear
{
    static class Program
    {
        [DllImport("sealib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void StartSimulation();

        [DllImport("sealib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void ApplySimSettings(string settings);

        [DllImport("sealib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr GetSimState();

        [DllImport("sealib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr GetTimingSummary();

        [DllImport("sealib.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr GetSimSummary();

        static string GetString(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }

        static void RunStats()
        {
            while (true)
            {
                Thread.Sleep(2 * 1000);
#if TIMING
                string seaTiming = GetString(GetTimingSummary());
                string svrTiming = ScopeTiming.Summary;

                string timing = "";
                if (seaTiming != "")
                    timing += seaTiming;

                if (svrTiming != "")
                {
                    if (timing != "")
                        timing += '\n';
                    timing += svrTiming;
                }

                if (timing != "")
                    timing += '\n';

                Console.WriteLine(timing);
#endif
                Console.WriteLine(GetString(GetSimSummary()));
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
#if TIMING
            ScopeTiming.Init(true);
#endif
            Console.Write("Setting up simulation...");
            StartSimulation();
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
#if TIMING
                    sw.Restart();
#endif
                    string state = GetString(GetSimState());
#if TIMING
                    ScopeTiming.RecordScope("Output.ToString", sw);
#endif
                    using (StreamWriter writer = new StreamWriter(ctxt.Response.OutputStream))
                        writer.Write(state);
#if TIMING
                    ScopeTiming.RecordScope("Output.StreamWriter", sw);
#endif
                }
                else
                {
#if TIMING
                    sw.Restart();
#endif
                    string settings;
                    using (StreamReader reader = new StreamReader(ctxt.Request.InputStream))
                        settings = reader.ReadToEnd();
#if TIMING
                    ScopeTiming.RecordScope("Settings.StreamReader", sw);
#endif
                    ApplySimSettings(settings);
#if TIMING
                    ScopeTiming.RecordScope("Settings.Apply", sw);
#endif
                }
                ctxt.Response.OutputStream.Close();
#if DEBUG
                Console.Write("!");
#endif
            }
        }
    }
}
