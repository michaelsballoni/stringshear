using System;
using System.Collections.Generic;
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

        static Dictionary<string, double> sm_lastStats = new Dictionary<string, double>();

        static StreamWriter sm_output = 
            new StreamWriter
            (
                File.Open
                (
                    $"output-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.txt", 
                    FileMode.OpenOrCreate, 
                    FileAccess.Write, 
                    FileShare.Read
                )
            );

        static void RunStats()
        {
            while (true)
            {
                Thread.Sleep(5 * 1000);
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
#else
                string summaryStr = GetString(GetSimSummary());
                var newDict = new Dictionary<string, double>();
                foreach (string str in summaryStr.Split('\n'))
                {
                    int idx = str.IndexOf(':');
                    newDict.Add(str.Substring(0, idx), double.Parse(str.Substring(idx + 1).Trim()));
                }

                double time = newDict["time"];
                double cycles = newDict["cycles"];

                string timestamp = DateTime.Now.ToString("yyyy/MM/dd-HH-mm-ss");
                Console.WriteLine("{0}: Time: {1} - Cycles: {2}", timestamp, time, cycles);

                if (newDict.ContainsKey("reset"))
                {
                    Console.WriteLine("\nRESET!!!\n");
                    foreach (string key in sm_lastStats.Keys)
                    {
                        if (key == "time" || key == "reset" || key == "cycles")
                            continue;
                        else
                            sm_lastStats[key] = 0.0;
                    }
                }

                bool anyNewMax = false;
                if (sm_lastStats.Count > 0)
                {
                    foreach (var kvp in newDict)
                    {
                        if (kvp.Key == "time" || kvp.Key == "reset" || kvp.Key == "cycles")
                            continue;

                        if (Math.Abs(kvp.Value) > Math.Abs(sm_lastStats[kvp.Key]))
                        {
                            string newMaxStr = 
                                $"{timestamp} - Old Max: {sm_lastStats[kvp.Key]} - New Max: {kvp.Key}: {kvp.Value}";
                            sm_output.WriteLine(newMaxStr);
                            Console.WriteLine(newMaxStr);
                            anyNewMax = true;
                        }
                    }
                }

                if (anyNewMax)
                {
                    Console.WriteLine(summaryStr);
                    Console.WriteLine();

                    sm_output.WriteLine(summaryStr);
                    sm_output.WriteLine();
                }

                sm_output.Flush();

                sm_lastStats = newDict;
#endif
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
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
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
