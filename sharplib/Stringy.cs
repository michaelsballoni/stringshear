// Define a string as a list of particles,
// and track the maximum position, velocity, acceleration, and punch of this,
// and the work done to move the end points, and the max work required to move the end points
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace StringShear
{
    public class Stringy
    {
        Particle[] m_particles;
        double m_length;

        double m_maxPos;
        double m_maxVel;
        double m_maxAcl;
        double m_maxPunch;

        int m_maxPosIndex;
        int m_maxVelIndex;
        int m_maxAclIndex;
        int m_maxPunchIndex;

        double m_startWork;
        double m_endWork;

        double m_maxStartWork;
        double m_maxEndWork;

        bool m_waveDownAndBackYet;

        public Stringy(Particle[] particles, double length)
        {
            m_length = length;
            m_particles = particles;
        }

        public Stringy(int particleCount, double length)
        {
            m_length = length;

            m_particles = new Particle[particleCount];

            // Initialize the particles, spreading them out across the length
            // NOTE: Leave the starting particle alone
            for (int i = 1; i < particleCount; ++i)
                m_particles[i] = new Particle(m_length * 1.0 * i / (particleCount - 1));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("particles:" + string.Join("|", m_particles.Select(p => p.ToString())) + ";");
            sb.Append("length:" + m_length + ";");

            sb.Append("maxPos:" + m_maxPos + ";");
            sb.Append("maxVel:" + m_maxVel + ";");
            sb.Append("maxAcl:" + m_maxAcl + ";");
            sb.Append("maxPunch:" + m_maxPunch + ";");

            sb.Append("maxPosIndex:" + m_maxPosIndex + ";");
            sb.Append("maxVelIndex:" + m_maxVelIndex + ";");
            sb.Append("maxAclIndex:" + m_maxAclIndex + ";");
            sb.Append("maxPunchIndex:" + m_maxPunchIndex + ";");

            sb.Append("startWork:" + m_startWork + ";");
            sb.Append("endWork:" + m_endWork + ";");

            sb.Append("maxStartWork:" + m_maxStartWork + ";");
            sb.Append("maxEndWork:" + m_maxEndWork + ";");

            return sb.ToString();
        }

        public static Stringy FromString(string str)
        {
            var dict = new Dictionary<string, string>();
            foreach (string line in str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int colon = line.IndexOf(':');
                string name = line.Substring(0, colon);
                string value = line.Substring(colon + 1);
                dict.Add(name, value);
            }

            double length = double.Parse(dict["length"]);

            Particle[] particles = 
                dict["particles"]
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Particle.FromString(s))
                .ToArray();

            Stringy stringy = new Stringy(particles, length);
            
            stringy.m_maxPos = double.Parse(dict["maxPos"]);
            stringy.m_maxVel = double.Parse(dict["maxVel"]);
            stringy.m_maxAcl = double.Parse(dict["maxAcl"]);
            stringy.m_maxPunch = double.Parse(dict["maxPunch"]);

            stringy.m_maxPosIndex = int.Parse(dict["maxPosIndex"]);
            stringy.m_maxVelIndex = int.Parse(dict["maxVelIndex"]);
            stringy.m_maxAclIndex = int.Parse(dict["maxAclIndex"]);
            stringy.m_maxPunchIndex = int.Parse(dict["maxPunchIndex"]);

            stringy.m_startWork = double.Parse(dict["startWork"]);
            stringy.m_endWork = double.Parse(dict["endWork"]);

            stringy.m_maxStartWork = double.Parse(dict["maxStartWork"]);
            stringy.m_maxEndWork = double.Parse(dict["maxEndWork"]);

            return stringy;
        }

        public Stringy Clone()
        {
            Stringy ret = new Stringy(m_particles.Length, m_length);

            for (int i = 0; i < m_particles.Length; ++i)
                ret.m_particles[i] = m_particles[i];

            ret.m_maxPos = m_maxPos;
            ret.m_maxVel = m_maxVel;
            ret.m_maxAcl = m_maxAcl;
            ret.m_maxPunch = m_maxPunch;

            ret.m_maxPosIndex = m_maxPosIndex;
            ret.m_maxVelIndex = m_maxVelIndex;
            ret.m_maxAclIndex = m_maxAclIndex;
            ret.m_maxPunchIndex = m_maxPunchIndex;

            ret.m_startWork = m_startWork;
            ret.m_endWork = m_endWork;
            ret.m_maxStartWork = m_maxStartWork;
            ret.m_maxEndWork = m_maxEndWork;

            return ret;
        }

        // Get the maximums
        public double GetMaxPos() { return m_maxPos; }
        public double GetMaxVel() { return m_maxVel; }
        public double GetMaxAcl() { return m_maxAcl; }
        public double GetMaxPunch() { return m_maxPunch; }

        // Get the maximum indices
        public int GetMaxPosIndex() { return m_maxPosIndex; }
        public int GetMaxVelIndex() { return m_maxVelIndex; }
        public int GetMaxAclIndex() { return m_maxAclIndex; }
        public int GetMaxPunchIndex() { return m_maxPunchIndex; }

        // Get the end point work
        public double GetStartWork() { return m_startWork; }
        public double GetEndWork() { return m_endWork; }

        public double GetMaxStartWork() { return m_maxStartWork; }
        public double GetMaxEndWork() { return m_maxEndWork; }

        public Particle[] Particles => m_particles;

        public double Length => m_length;

        // Update the particles of this given new endpoints, an elapsed time, and a tension
        // The tension defines how much acceleration the particles receive due to distance from
        // neighboring particles
        public
            void Update
            (
                double startPosY,
                double endPosY,
                double elapsedTime,
                double time,
                double tension,
                double damping,
                double stringLength,
                double stringConstant
            )
        {
            // Set the endpoints and add to the work we've performed for them
            // and track the max work we've seen for the endpoints.
            double newStartWork = m_particles[0].SetPosY(startPosY, elapsedTime, time);
            m_startWork += newStartWork;

            if (newStartWork > m_maxStartWork)
                m_maxStartWork = newStartWork;

            double newEndWork = m_particles[m_particles.Length - 1].SetPosY(endPosY, elapsedTime, time);
            m_endWork += newEndWork;

            if (newEndWork > m_maxEndWork)
                m_maxEndWork = newEndWork;

            // Compute neighbor factors.
            for (int i = 0; i < m_particles.Length - 1; ++i)
            {
                double xGap = m_particles[i].x - m_particles[i + 1].x;
                double yGap = m_particles[i].y - m_particles[i + 1].y;
                double totalGap = Math.Sqrt(xGap * xGap + yGap * yGap);
                m_particles[i].nextNeighborFactor = Math.Abs((1.0 / totalGap) * yGap);
            }

            // Compute acceleration using neighbors.
            for (int i = 1; i < m_particles.Length - 1; ++i)
            {
                double prevComponent = m_particles[i - 1].nextNeighborFactor;
                if (m_particles[i - 1].y < m_particles[i].y)
                    prevComponent = -prevComponent;

                double nextComponent = m_particles[i].nextNeighborFactor;
                if (m_particles[i + 1].y < m_particles[i].y)
                    nextComponent = -nextComponent;

                double newAcl = tension * (prevComponent + nextComponent)
                                - damping * m_particles[i].vel;

                m_particles[i].punch = newAcl - m_particles[i].acl;

                m_particles[i].acl = newAcl;
            }

            // Update velocity and position.
            for (int i = 1; i < m_particles.Length - 1; ++i)
            {
                m_particles[i].vel += m_particles[i].acl * elapsedTime;
                m_particles[i].y += m_particles[i].vel * elapsedTime;
            }

            // Recompute maximums...after a wave has gone down and back.
            if (!m_waveDownAndBackYet)
            {
                // Compute when a wave would propogate all the way down the string and back.
                double waveSpeed = stringConstant * Math.Sqrt(tension);
                double timeTilWaveDownAndBack = stringLength * 2.0 / waveSpeed;
                m_waveDownAndBackYet = time > timeTilWaveDownAndBack;
            }

            if (m_waveDownAndBackYet)
            {
                UpdateMaxPos();
                UpdateMaxVel();
                UpdateMaxAcl();
                UpdateMaxPunch();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < m_particles.Length; ++i)
                m_particles[i].Reset();

            ResetMaxes();

            m_startWork = 0.0;
            m_endWork = 0.0;

            m_maxStartWork = 0.0;
            m_maxEndWork = 0.0;

            m_waveDownAndBackYet = false;
        }

        public void ResetMaxes()
        {
            m_maxPos = 0.0;
            m_maxVel = 0.0;
            m_maxAcl = 0.0;
            m_maxPunch = 0.0;

            m_maxPosIndex = 0;
            m_maxVelIndex = 0;
            m_maxAclIndex = 0;
            m_maxPunchIndex = 0;
        }

        // Compute the maximum of a value for the particle
        public void ComputeMax(out double max, out int maxPosIndex, int valIndex)
        {
            max = 0;
            maxPosIndex = 0;

            for (int i = 1; i < m_particles.Length - 1; ++i)
            {
                double cur = m_particles[i].GetVal(valIndex);
                if (Math.Abs(cur) > Math.Abs(max))
                {
                    max = cur;
                    maxPosIndex = i;
                }
            }
        }

        // Compute the maximum position of this string for display scaling purposes
        public double ComputeMaxPos()
        {
            double curMaxPos;
            int curMaxPosIndex;
            ComputeMax(out curMaxPos, out curMaxPosIndex, 1);
            return curMaxPos;
        }

        public void UpdateMaxPos()
        {
            ComputeMax(out m_maxPos, out m_maxPosIndex, 1);
        }

        public void UpdateMaxVel()
        {
            ComputeMax(out m_maxVel, out m_maxVelIndex, 2);
        }

        public void UpdateMaxAcl()
        {
            ComputeMax(out m_maxAcl, out m_maxAclIndex, 3);
        }

        public void UpdateMaxPunch()
        {
            ComputeMax(out m_maxPunch, out m_maxPunchIndex, 4);
        }
    }
}
