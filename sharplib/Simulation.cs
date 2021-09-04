using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;

namespace StringShear
{
    public class Simulation
    {
        public const int cParticleCount = 1000;
        public const double cStringConstant = 0.03164; // magic number
        public const double cStringLength = 1.0; // meter
        public const double cOscillatorAmplitude = 0.001; // mm

        bool m_bPaused = true;

        int m_delayMs; // negative means flat-out
        int m_delayMod;
        long m_simulationCycle;

        double m_timeSlice;
        double m_tension;
        double m_damping;

        double m_time;
        double m_maxPosTime;
        double m_maxVelTime;
        double m_maxAclTime;
        double m_maxPunchTime;

        Stringy m_string;
        Stringy m_maxPosString;
        Stringy m_maxVelString;
        Stringy m_maxAclString;
        Stringy m_maxPunchString;

        double[] m_rightFrequencies;
        double[] m_leftFrequencies;

        bool m_bRightEnabled;
        bool m_bLeftEnabled;
        bool m_bJustPulse;
        bool m_bJustHalfPulse;
        double m_outOfPhase;

        Stopwatch m_computeStopwatch;
        double m_computeElapsedMs;

        Stopwatch m_outputStopwatch;
        double m_outputElapsedMs;

        public Simulation()
        {
            m_computeStopwatch = new Stopwatch();
            m_outputStopwatch = new Stopwatch();

            m_string = new Stringy(cParticleCount, cStringLength);
            m_maxPosString = m_string.Clone();
            m_maxVelString = m_string.Clone();
            m_maxAclString = m_string.Clone();
            m_maxPunchString = m_string.Clone();

            m_rightFrequencies = new double[0];
            m_leftFrequencies = new double[0];

            new Thread(Run).Start();
            new Thread(RunServer).Start();
        }

        private void Run(object obj)
        {
            while (true)
                Update();
        }

        public void RunServer()
        {
            Console.Write("Setting up server...");
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:9914/");
            listener.Start();
            Console.WriteLine("done!");
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
                    string state = ToString();
                    using (StreamWriter writer = new StreamWriter(ctxt.Response.OutputStream))
                        writer.Write(state);
                }
                else
                {
                    string settings;
                    using (StreamReader reader = new StreamReader(ctxt.Request.InputStream))
                        settings = reader.ReadToEnd();
                    ctxt.Response.OutputStream.Close();
                    ApplySettings(settings);
                }
#if DEBUG
                Console.Write("!");
#endif
            }
        }

        public void ApplySettings(string str)
        {
            var settings = new Dictionary<string, string>();
            foreach (string line in str.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                int colon = line.IndexOf(':');
                string name = line.Substring(0, colon);
                string value = line.Substring(colon + 1);
                settings.Add(name, value);
            }

            lock (this)
            {
                foreach (var kvp in settings)
                {
                    switch (kvp.Key)
                    {
                        case "reset": if (bool.Parse(kvp.Value)) Reset(); break;
                        case "resetMaxes": if (bool.Parse(kvp.Value)) ResetMaxes(); break;
                        case "paused": m_bPaused = bool.Parse(kvp.Value); break;
                        case "delayMs": m_delayMs = int.Parse(kvp.Value); break;
                        case "delayMod": m_delayMod = int.Parse(kvp.Value); break;
                        case "timeSlice": m_timeSlice = double.Parse(kvp.Value); break;
                        case "tension": m_tension = double.Parse(kvp.Value); break;
                        case "damping": m_damping = double.Parse(kvp.Value); break;
                        case "rightEnabled": m_bRightEnabled = bool.Parse(kvp.Value); break;
                        case "leftEnabled": m_bLeftEnabled = bool.Parse(kvp.Value); break;
                        case "justPulse": m_bJustPulse = bool.Parse(kvp.Value); break;
                        case "justHalfPulse": m_bJustHalfPulse = bool.Parse(kvp.Value); break;
                        case "outOfPhase": m_outOfPhase = double.Parse(kvp.Value); break;
                        case "rightFrequencies":
                            m_rightFrequencies = kvp.Value.Split(',').Select(x => double.Parse(x)).ToArray();
                            break;
                        case "leftFrequencies":
                            m_leftFrequencies = kvp.Value.Split(',').Select(x => double.Parse(x)).ToArray();
                            break;
                        default: throw new Exception("Unknown setting: " + kvp.Key);
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            lock (this)
            {
                m_outputStopwatch.Restart();

                sb.AppendFormat("time:{0}\n", m_time);
                sb.AppendFormat("elapsedMs:{0}\n", m_computeElapsedMs);

                sb.AppendFormat("maxPosTime:{0}\n", m_maxPosTime);
                sb.AppendFormat("maxVelTime:{0}\n", m_maxVelTime);
                sb.AppendFormat("maxAclTime:{0}\n", m_maxAclTime);
                sb.AppendFormat("maxPunchTime:{0}\n", m_maxPunchTime);

                sb.AppendFormat("string:{0}\n", m_string);
                sb.AppendFormat("maxPosString:{0}\n", m_maxPosString);
                sb.AppendFormat("maxVelString:{0}\n", m_maxVelString);
                sb.AppendFormat("maxAclString:{0}\n", m_maxAclString);
                sb.AppendFormat("maxPunchString:{0}\n", m_maxPunchString);

                m_outputElapsedMs = m_outputStopwatch.Elapsed.TotalMilliseconds;
            }

            string stateStr = sb.ToString();
            return stateStr;
        }

        public void GetElapsed(ref double computeMs, ref double outputMs)
        {
            lock (this)
            {
                computeMs = m_computeElapsedMs;
                outputMs = m_outputElapsedMs;
            }
        }

        public void Update()
        {
            m_simulationCycle++;

            // Delay.  Outside the thread safety lock.
            int delayMs = 0;
            {
                lock (this)
                {
                    delayMs = m_delayMs;

                    if (delayMs > 0)
                    {
                        if (m_delayMod > 0)
                        {
                            if ((m_simulationCycle % m_delayMod) != 0)
                                delayMs = 0;
                        }
                    }
                }
            }

            // Yield to the main application thread to cool off the CPU...
            // ...unless we're running flat out!
            if (delayMs >= 0)
                Thread.Sleep(delayMs);

            // Bail if we're paused, but sleep to keep the processor cool.
            if (m_bPaused)
            {
                Thread.Sleep(200);
                m_computeElapsedMs = 0.0;
                return;
            }

            lock (this)
            {
                m_computeStopwatch.Restart();

                double startPos = 0.0;
                if (m_bLeftEnabled)
                {
                    foreach (double frequency in m_leftFrequencies)
                    {
                        startPos += GetOscillatorPosition(frequency,
                                                            cOscillatorAmplitude,
                                                            m_bJustPulse,
                                                            m_bJustHalfPulse,
                                                            m_outOfPhase,
                                                            m_time);
                    }
                }

                double endPos = 0.0;
                if (m_bRightEnabled)
                {
                    foreach (double frequency in m_rightFrequencies)
                    {
                        endPos += GetOscillatorPosition(frequency,
                                                        cOscillatorAmplitude,
                                                        m_bJustPulse,
                                                        m_bJustHalfPulse,
                                                        /* outOfPhase = */0.0,
                                                        m_time);
                    }
                }

                double timeSlice = m_timeSlice >= 0.0 ? m_timeSlice : m_computeElapsedMs / 1000.0;
                m_string.Update
                (
                    startPos,
                    endPos,
                    timeSlice,
                    m_time,
                    m_tension,
                    m_damping,
                    cStringLength,
                    cStringConstant
                );

                if (Math.Abs(m_string.GetMaxPos()) > Math.Abs(m_maxPosString.GetMaxPos()))
                {
                    m_maxPosString = m_string.Clone();
                    m_maxPosTime = m_time;
                }

                if (Math.Abs(m_string.GetMaxVel()) > Math.Abs(m_maxVelString.GetMaxVel()))
                {
                    m_maxVelString = m_string.Clone();
                    m_maxVelTime = m_time;
                }

                if (Math.Abs(m_string.GetMaxAcl()) > Math.Abs(m_maxAclString.GetMaxAcl()))
                {
                    m_maxAclString = m_string.Clone();
                    m_maxAclTime = m_time;
                }

                if (Math.Abs(m_string.GetMaxPunch()) > Math.Abs(m_maxPunchString.GetMaxPunch()))
                {
                    m_maxPunchString = m_string.Clone();
                    m_maxPunchTime = m_time;
                }

                m_time += timeSlice;
                m_computeElapsedMs = m_computeStopwatch.Elapsed.TotalMilliseconds;
            }
        }

        public void Reset()
        {
            lock (this)
            {
                m_time = 0.0;
                m_string.Reset();
                ResetMaxes();
            }
        }

        public void ResetMaxes()
        {
            lock (this)
            {
                m_string.ResetMaxes();

                m_maxPosTime = 0.0;
                m_maxVelTime = 0.0;
                m_maxAclTime = 0.0;
                m_maxPunchTime = 0.0;

                m_maxPosString.Reset();
                m_maxVelString.Reset();
                m_maxAclString.Reset();
                m_maxPunchString.Reset();
            }
        }

        // Get the position of this at a particular time
        public static double // static to ensure purity of processing
            GetOscillatorPosition
            (
                double frequency,
                double amplitude,
                bool bJustPulse,
                bool bJustHalfPulse,
                double outOfPhase,
                double time
            )
        {
            double radians = 2.0 * Math.PI * frequency * time;

            radians -= outOfPhase * Math.PI;

            if (radians < 0.0)
                radians = 0.0;

            if (bJustPulse && radians > 2.0 * Math.PI)
                radians = 0.0;
            else if (bJustHalfPulse && radians > 1.0 * Math.PI)
                radians = 0.0;

            double retVal = Math.Sin(radians);

            retVal *= amplitude;

            return retVal;
        }
    }
}
