#pragma once

#include "Particle.h"
#include "ScopeTiming.h"
#include "Stopwatch.h"

#include <stdio.h>
#include <math.h>

#include <array>
#include <charconv>
#include <string>
#include <vector>
#include <unordered_map>

class Stringy
{
private:
    std::vector<Particle> m_particles;
    double m_length = 0;

    double m_maxPos = 0;
    double m_maxVel = 0;
    double m_maxAcl = 0;
    double m_maxPunch = 0;

    int m_maxPosIndex = 0;
    int m_maxVelIndex = 0;
    int m_maxAclIndex = 0;
    int m_maxPunchIndex = 0;

    double m_startWork = 0;
    double m_endWork = 0;

    double m_maxStartWork = 0;
    double m_maxEndWork = 0;

    bool m_waveDownAndBackYet = false;

public:
    Stringy() {}

    Stringy(size_t particleCount, double length)
    {
        m_length = length;

        // Initialize the particles, spreading them out across the length
        m_particles.reserve(particleCount);
        m_particles.emplace_back(); // first particle
        for (size_t i = 1; i < particleCount; ++i)
            m_particles.emplace_back(m_length * 1.0 * i / (particleCount - 1));
    }

    Stringy(const std::vector<Particle>& particles, double length)
        : m_length(length)
        , m_particles(particles)
    {
    }

    Stringy(const Stringy& other)
    {
        operator=(other);
    }

    Stringy& operator=(const Stringy& other)
    {
        if (&other == this)
            return *this;

        m_particles = other.m_particles;
        m_length = other.m_length;

        m_maxPos = other.m_maxPos;
        m_maxVel = other.m_maxVel;
        m_maxAcl = other.m_maxAcl;
        m_maxPunch = other.m_maxPunch;

        m_maxPosIndex = other.m_maxPosIndex;
        m_maxVelIndex = other.m_maxVelIndex;
        m_maxAclIndex = other.m_maxAclIndex;
        m_maxPunchIndex = other.m_maxPunchIndex;

        m_startWork = other.m_startWork;
        m_endWork = other.m_endWork;
        m_maxStartWork = other.m_maxStartWork;
        m_maxEndWork = other.m_maxEndWork;

        return *this;
    }

    void AppendToString(std::string& str, std::array<char, 1024 * 1024>& particlesBuffer) const
    {
        Stopwatch sw;
        str += "particles:";
        {
            char* start = particlesBuffer.data();
            char* end = start + particlesBuffer.size() - 1;
            std::to_chars_result result;
            for (const auto& p : m_particles)
            {
                result = std::to_chars(start, end, p.x);
                if (result.ec != std::errc()) throw std::exception("to_chars error");
                *result.ptr = ',';
                start = result.ptr + 1;
                result = std::to_chars(start, end, p.y);
                if (result.ec != std::errc()) throw std::exception("to_chars error");
                *result.ptr = ',';
                start = result.ptr + 1;
                result = std::to_chars(start, end, p.vel);
                if (result.ec != std::errc()) throw std::exception("to_chars error");
                *result.ptr = ',';
                start = result.ptr + 1;
                result = std::to_chars(start, end, p.acl);
                if (result.ec != std::errc()) throw std::exception("to_chars error");
                *result.ptr = ',';
                start = result.ptr + 1;
                result = std::to_chars(start, end, p.punch);
                if (result.ec != std::errc()) throw std::exception("to_chars error");
                *result.ptr = ',';
                start = result.ptr + 1;
                result = std::to_chars(start, end, p.nextNeighborFactor);
                if (result.ec != std::errc()) throw std::exception("to_chars error");
                *result.ptr = '|';
                start = result.ptr + 1;
            }
            *start = '\0';
            str += particlesBuffer.data();
        }
        str += ";";
        ScopeTiming::GetObj().RecordScope("String.AppendToString.Particles", sw);

        str += "length:" + std::to_string(m_length) + ";";

        str += "maxPos:" + std::to_string(m_maxPos) + ";";
        str += "maxVel:" + std::to_string(m_maxVel) + ";";
        str += "maxAcl:" + std::to_string(m_maxAcl) + ";";
        str += "maxPunch:" + std::to_string(m_maxPunch) + ";";

        str += "maxPosIndex:" + std::to_string(m_maxPosIndex) + ";";
        str += "maxVelIndex:" + std::to_string(m_maxVelIndex) + ";";
        str += "maxAclIndex:" + std::to_string(m_maxAclIndex) + ";";
        str += "maxPunchIndex:" + std::to_string(m_maxPunchIndex) + ";";

        str += "startWork:" + std::to_string(m_startWork) + ";";
        str += "endWork:" + std::to_string(m_endWork) + ";";

        str += "maxStartWork:" + std::to_string(m_maxStartWork) + ";";
        str += "maxEndWork:" + std::to_string(m_maxEndWork) + ";";

        ScopeTiming::GetObj().RecordScope("String.AppendToString.TheRest", sw); // not much
    }

    // Get the maximums
    double GetMaxPos() const { return m_maxPos; }
    double GetMaxVel() const { return m_maxVel; }
    double GetMaxAcl() const { return m_maxAcl; }
    double GetMaxPunch() const { return m_maxPunch; }

    // Get the maximum indices
    int GetMaxPosIndex() const { return m_maxPosIndex; }
    int GetMaxVelIndex() const { return m_maxVelIndex; }
    int GetMaxAclIndex() const { return m_maxAclIndex; }
    int GetMaxPunchIndex() const { return m_maxPunchIndex; }

    // Get the end point work
    double GetStartWork() const { return m_startWork; }
    double GetEndWork() const { return m_endWork; }

    double GetMaxStartWork() const { return m_maxStartWork; }
    double GetMaxEndWork() const { return m_maxEndWork; }

    const std::vector<Particle> GetParticles() const { return m_particles; }

    double GetLength() const { return m_length; }

    // Update the particles of this given new endpoints, an elapsed time, and a tension
    // The tension defines how much acceleration the particles receive due to distance from
    // neighboring particles
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

        double newEndWork = m_particles[m_particles.size() - 1].SetPosY(endPosY, elapsedTime, time);
        m_endWork += newEndWork;

        if (newEndWork > m_maxEndWork)
            m_maxEndWork = newEndWork;

        // Compute neighbor factors.
        for (size_t i = 0; i < m_particles.size() - 1; ++i)
        {
            double xGap = m_particles[i].x - m_particles[i + 1].x;
            double yGap = m_particles[i].y - m_particles[i + 1].y;
            double totalGap = sqrt(xGap * xGap + yGap * yGap);
            m_particles[i].nextNeighborFactor = fabs((1.0 / totalGap) * yGap);
        }

        // Compute acceleration using neighbors.
        for (size_t i = 1; i < m_particles.size() - 1; ++i)
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
        for (size_t i = 1; i < m_particles.size() - 1; ++i)
        {
            m_particles[i].vel += m_particles[i].acl * elapsedTime;
            m_particles[i].y += m_particles[i].vel * elapsedTime;
        }

        // Recompute maximums...after a wave has gone down and back.
        if (!m_waveDownAndBackYet)
        {
            // Compute when a wave would propogate all the way down the string and back.
            double waveSpeed = stringConstant * sqrt(tension);
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

    void Reset()
    {
        for (size_t i = 0; i < m_particles.size(); ++i)
            m_particles[i].Reset();

        ResetMaxes();

        m_startWork = 0.0;
        m_endWork = 0.0;

        m_maxStartWork = 0.0;
        m_maxEndWork = 0.0;

        m_waveDownAndBackYet = false;
    }

    void ResetMaxes()
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
    void ComputeMax(double& max, int& maxPosIndex, int valIndex)
    {
        max = 0;
        maxPosIndex = 0;

        for (size_t i = 1; i < m_particles.size() - 1; ++i)
        {
            double cur = m_particles[i].GetVal(valIndex);
            if (fabs(cur) > fabs(max))
            {
                max = cur;
                maxPosIndex = (int)i;
            }
        }
    }

    // Compute the maximum position of this string for display scaling purposes
    double ComputeMaxPos()
    {
        double curMaxPos;
        int curMaxPosIndex;
        ComputeMax(curMaxPos, curMaxPosIndex, 1);
        return curMaxPos;
    }

    void UpdateMaxPos()
    {
        ComputeMax(m_maxPos, m_maxPosIndex, 1);
    }

    void UpdateMaxVel()
    {
        ComputeMax(m_maxVel, m_maxVelIndex, 2);
    }

    void UpdateMaxAcl()
    {
        ComputeMax(m_maxAcl, m_maxAclIndex, 3);
    }

    void UpdateMaxPunch()
    {
        ComputeMax(m_maxPunch, m_maxPunchIndex, 4);
    }
};
