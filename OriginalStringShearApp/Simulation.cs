// Define the crucial simulation variables
using System;
using System.Diagnostics;
using System.Threading;

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
        void Run(object obj)
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

        // Set attributes
        // All thread safe
        public void SetPaused(bool bPaused) { lock (this) m_bPaused = bPaused; }
        public void SetDelay(int delayMs) { lock (this) m_delayMs = delayMs; }
        public void SetDelayMod(int delayMod) { lock (this) m_delayMod = delayMod; }
        public void SetTimeSlice(double timeSlice) { lock (this) m_timeSlice = timeSlice; }
        public void SetTension(double tension) { lock (this) m_tension = tension; }
        public void SetDamping(double damping) { lock (this) m_damping = damping; }
        public void SetRightEnabled(bool bEnabled) { lock (this) m_bRightEnabled = bEnabled; }
        public void SetLeftEnabled(bool bEnabled) { lock (this) m_bLeftEnabled = bEnabled; }
        public void SetJustPulse(bool bSetting) { lock (this) m_bJustPulse = bSetting; }
        public void SetJustHalfPulse(bool bSetting) { lock (this) m_bJustHalfPulse = bSetting; }
        public void SetOutOfPhase(double outOfPhase) { lock (this) m_outOfPhase = outOfPhase; }

        public void SetRightFrequencies(double[] frequencies)
        {
            lock (this)
                m_rightFrequencies = (double[])frequencies.Clone();
        }

        public void SetLeftFrequencies(double[] frequencies)
        {
            lock (this)
                m_leftFrequencies = (double[])frequencies.Clone();
        }

        // Get the current time and the time of maximum events
        // All thread safe
        public double GetTime() { lock (this) return m_time; }
        public double GetMaxPosTime() { lock (this) return m_maxPosTime; }
        public double GetMaxVelTime() { lock (this) return m_maxVelTime; }
        public double GetMaxAclTime() { lock (this) return m_maxAclTime; }
        public double GetMaxPunchTime() { lock (this) return m_maxPunchTime; }

        // Get the current string and string instances of maximum events
        // All thread safe
        public Stringy GetCurStringy() { lock (this) return m_string.Clone(); }
        public Stringy GetMaxPosStringy() { lock (this) return m_maxPosString.Clone(); }
        public Stringy GetMaxVelStringy() { lock (this) return m_maxVelString.Clone(); }
        public Stringy GetMaxAclStringy() { lock (this) return m_maxAclString.Clone(); }
        public Stringy GetMaxPunchStringy() { lock (this) return m_maxPunchString.Clone(); }

        // Get the time elapsed during the last update
        public double GetElapsedMs() { lock (this) return m_elapsedMs; }

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

        // Get the current state of affairs for this in one fell-swoop
        // In particular, with one locking operation.
        public void 
            GetState
            (
                out Stringy curStringy,
                ref Stringy maxPosStringy,
                ref Stringy maxVelStringy,
                ref Stringy maxAclStringy,
                ref Stringy maxPunchStringy,
                out double time,
                out double elapsedMs
            )
        {
            curStringy = maxPosStringy = maxVelStringy = maxAclStringy = maxPunchStringy = null;

            time = 0;
            elapsedMs = 0;

            lock (this)
            {
                // Clone the current string out.  This is unavoidable.
                curStringy = m_string.Clone();

                // Clone the max strings out, but only as necessary.
                if (maxPosStringy == null
                    || Math.Abs(maxPosStringy.GetMaxPos()) < Math.Abs(m_maxPosString.GetMaxPos()))
                {
                    maxPosStringy = m_maxPosString.Clone();
                }

                if (maxVelStringy == null
                    || Math.Abs(maxVelStringy.GetMaxVel()) < Math.Abs(m_maxVelString.GetMaxVel()))
                {
                    maxVelStringy = m_maxVelString.Clone();
                }

                if (maxAclStringy == null
                    || Math.Abs(maxAclStringy.GetMaxAcl()) < Math.Abs(m_maxAclString.GetMaxAcl()))
                {
                    maxAclStringy = m_maxAclString.Clone();
                }

                if (maxPunchStringy == null
                    || Math.Abs(maxPunchStringy.GetMaxPunch()) < Math.Abs(m_maxPunchString.GetMaxPunch()))
                {
                    maxPunchStringy = m_maxPunchString.Clone();
                }

                time = m_time;
                elapsedMs = m_elapsedMs;
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
