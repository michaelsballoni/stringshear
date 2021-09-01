using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

        Thread m_thread;

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

        Stopwatch m_stopwatch;
        double m_elapsedMs;

        public Simulation()
        {
            m_stopwatch = new Stopwatch();

            m_string = new Stringy(cParticleCount, cStringLength);
            m_maxPosString = m_string.Clone();
            m_maxVelString = m_string.Clone();
            m_maxAclString = m_string.Clone();
            m_maxPunchString = m_string.Clone();

            m_rightFrequencies = new double[0];
            m_leftFrequencies = new double[0];

            m_thread = new Thread(Run);
        }

        public void Startup()
        {
            m_thread.Start();
        }

        public void Shutdown()
        {
            m_thread.Abort();
        }

        // Implement the worker thread routine to constantly update the situation
        // This sleeps when paused to not burn up the CPU all the time
        private void Run(object obj)
        {
            try
            {
                while (true)
                    Update();
            }
            catch (ThreadAbortException)
            {
                return;
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
            var state = new Dictionary<string, string>();
            lock (this)
            {
                state.Add("time", m_time.ToString());
                state.Add("elapsedMs", m_elapsedMs.ToString());

                state.Add("maxPosTime", m_maxPosTime.ToString());
                state.Add("maxVelTime", m_maxVelTime.ToString());
                state.Add("maxAclTime", m_maxAclTime.ToString());
                state.Add("maxPunchTime", m_maxPunchTime.ToString());

                state.Add("string", m_string.ToString());
                state.Add("maxPosString", m_maxPosString.ToString());
                state.Add("maxVelString", m_maxVelString.ToString());
                state.Add("maxAclString", m_maxAclString.ToString());
                state.Add("maxPunchString", m_maxPunchString.ToString());
            }
            return string.Join("\n", state.Select(kvp => kvp.Key + ":" + kvp.Value));
        }

        public void Update()
        {
            m_simulationCycle++;

            m_stopwatch.Restart();

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
                m_elapsedMs = 0.0;
                return;
            }

            lock (this)
            {
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

                double timeSlice = m_timeSlice >= 0.0 ? m_timeSlice : m_elapsedMs / 1000.0;
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
                m_elapsedMs = m_stopwatch.Elapsed.TotalMilliseconds;
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
