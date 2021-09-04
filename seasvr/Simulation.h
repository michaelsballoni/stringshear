#pragma once

#include "Stringy.h"
#include "Stopwatch.h"
#include "CsLocks.h"

#include <memory>
#include <thread>

class Simulation
{
public:
    const int cParticleCount = 1000;
    const double cStringConstant = 0.03164; // magic number
    const double cStringLength = 1.0; // meter
    const double cOscillatorAmplitude = 0.001; // mm

    // https://stackoverflow.com/questions/1727881/how-to-use-the-pi-constant-in-c
    const double PI = 3.141592653589793238462643383279502884L;

private:
    bool m_bPaused = true;

    int m_delayMs = 0; // negative means flat-out
    int m_delayMod = 0;
    long m_simulationCycle = 0;

    double m_timeSlice = 0.0;
    double m_tension = 0.0;
    double m_damping = 0.0;

    double m_time = 0.0;
    double m_maxPosTime = 0.0;
    double m_maxVelTime = 0.0;
    double m_maxAclTime = 0.0;
    double m_maxPunchTime = 0.0;

    Stringy m_string;
    Stringy m_maxPosString;
    Stringy m_maxVelString;
    Stringy m_maxAclString;
    Stringy m_maxPunchString;

    std::vector<double> m_rightFrequencies;
    std::vector<double> m_leftFrequencies;

    bool m_bRightEnabled = false;
    bool m_bLeftEnabled = false;
    bool m_bJustPulse = false;
    bool m_bJustHalfPulse = false;
    double m_outOfPhase = 0.0;

    Stopwatch m_computeStopwatch;
    double m_computeElapsedMs = 0.0;
    std::shared_ptr<std::thread> m_computeThread;

    Stopwatch m_outputStopwatch;
    double m_outputElapsedMs = 0.0;

    CSection m_mutex;

public:
    Simulation()
        : m_string(cParticleCount, cStringLength)
    {
        m_maxPosString = m_string.Clone();
        m_maxVelString = m_string.Clone();
        m_maxAclString = m_string.Clone();
        m_maxPunchString = m_string.Clone();

        m_computeThread = std::make_shared<std::thread>(&Simulation::Run, this);
    }

    void ApplySettings(const std::string& str)
    {
        std::unordered_map<std::string, std::string> settings;
        for (const auto& line : Split(str, '\n'))
        {
            if (line.empty())
                continue;

            int colon = line.find(':', 0);
            std::string name = line.substr(0, colon);
            std::string value = line.substr(colon + 1);
            settings[name] = value;
        }

        CSLock lock(m_mutex);
        for (const auto& it : settings)
        {
            std::string key = it.first;
            std::string value = it.second;

            if (key == "reset")
                Reset();
            else if (key == "resetMaxes")
                ResetMaxes();
            else if (key == "paused")
                m_bPaused = stricmp(value.c_str(), "true") == 0;
            else if (key == "delayMs")
                m_delayMs = atoi(value.c_str());
            else if (key == "delayMod")
                m_delayMod = atoi(value.c_str());
            else if (key == "timeSlice")
                m_timeSlice = atof(value.c_str());
            else if (key == "tension")
                m_tension = atof(value.c_str());
            else if (key == "damping")
                m_damping = atof(value.c_str());
            else if (key == "rightEnabled")
                m_bRightEnabled = stricmp(value.c_str(), "true") == 0;
            else if (key == "leftEnabled")
                m_bLeftEnabled = stricmp(value.c_str(), "true") == 0;
            else if (key == "justPulse")
                m_bJustPulse = stricmp(value.c_str(), "true") == 0;
            else if (key == "justHalfPulse")
                m_bJustHalfPulse = stricmp(value.c_str(), "true") == 0;
            else if (key == "outOfPhase")
                m_outOfPhase = atof(value.c_str());
            else if (key == "rightFrequencies")
                m_rightFrequencies = StrToDoubleArray(value);
            else if (key == "leftFrequencies")
                m_leftFrequencies = StrToDoubleArray(value);
            else
                throw std::exception("Simulation::ApplySettings fails");
        }
    }

    std::string ToString()
    {
        CSLock lock(m_mutex);
        m_outputStopwatch.Restart();

        std::string str;
        str += "time:" + std::to_string(m_time) + ";";
        str += "elapsedMs:" + std::to_string(m_computeElapsedMs) + ";";

        str += "maxPosTime:" + std::to_string(m_maxPosTime) + ";";
        str += "maxVelTime:" + std::to_string(m_maxVelTime) + ";";
        str += "maxAclTime:" + std::to_string(m_maxAclTime) + ";";
        str += "maxPunchTime:" + std::to_string(m_maxPunchTime) + ";";

        str += "string:" + m_string.ToString() + ";";
        str += "maxPosString:" + m_maxPosString.ToString() + ";";
        str += "maxVelString:" + m_maxVelString.ToString() + ";";
        str += "maxAclString:" + m_maxAclString.ToString() + ";";
        str += "maxPunchString:" + m_maxPunchString.ToString() + ";";

        m_outputElapsedMs = m_outputStopwatch.ElapsedMs();
        return str;
    }

    void GetElapsed(double& computeMs, double& outputMs)
    {
        CSLock lock(m_mutex);
        computeMs = m_computeElapsedMs;
        outputMs = m_outputElapsedMs;
    }

private:
    void Run()
    {
        while (true)
            Update();
    }

    // https://stackoverflow.com/questions/14265581/parse-split-a-string-in-c-using-string-delimiter-standard-c
    static std::vector<std::string> Split(const std::string& s, const char delimiter) 
    {
        size_t pos_start = 0, pos_end, delim_len = 1;
        std::string token;
        std::vector<std::string> res;

        while ((pos_end = s.find(delimiter, pos_start)) != std::string::npos) 
        {
            token = s.substr(pos_start, pos_end - pos_start);
            pos_start = pos_end + delim_len;
            res.push_back(token);
        }

        res.push_back(s.substr(pos_start));
        return res;
    }

    static std::vector<double> StrToDoubleArray(const std::string& str)
    {
        std::vector<double> retVal;
        for (const auto& cur : Split(str, ','))
            retVal.emplace_back(atof(cur.c_str()));
        return retVal;
    }

    void Update()
    {
        m_simulationCycle++;

        // Delay.  Outside the thread safety lock.
        int delayMs = 0;
        {
            CSLock lock(m_mutex);
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

        // Yield to the main application thread to cool off the CPU...
        // ...unless we're running flat out!
        if (delayMs >= 0)
            std::this_thread::sleep_for(std::chrono::milliseconds(delayMs));

        // Bail if we're paused, but sleep to keep the processor cool.
        if (m_bPaused)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(200));
            m_computeElapsedMs = 0.0;
            return;
        }

        CSLock lock(m_mutex);

        m_computeStopwatch.Restart();

        double startPos = 0.0;
        if (m_bLeftEnabled)
        {
            for (double frequency : m_leftFrequencies)
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
            for (double frequency : m_rightFrequencies)
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

        if (fabs(m_string.GetMaxPos()) > fabs(m_maxPosString.GetMaxPos()))
        {
            m_maxPosString = m_string.Clone();
            m_maxPosTime = m_time;
        }

        if (fabs(m_string.GetMaxVel()) > fabs(m_maxVelString.GetMaxVel()))
        {
            m_maxVelString = m_string.Clone();
            m_maxVelTime = m_time;
        }

        if (fabs(m_string.GetMaxAcl()) > fabs(m_maxAclString.GetMaxAcl()))
        {
            m_maxAclString = m_string.Clone();
            m_maxAclTime = m_time;
        }

        if (fabs(m_string.GetMaxPunch()) > fabs(m_maxPunchString.GetMaxPunch()))
        {
            m_maxPunchString = m_string.Clone();
            m_maxPunchTime = m_time;
        }

        m_time += timeSlice;
        m_computeElapsedMs = m_computeStopwatch.ElapsedMs();
    }

    void Reset()
    {
        CSLock lock(m_mutex);
        m_time = 0.0;
        m_string.Reset();
        ResetMaxes();
    }

    void ResetMaxes()
    {
        CSLock lock(m_mutex);
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

    // Get the position of this at a particular time
    double
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
        double radians = 2.0 * PI * frequency * time;

        radians -= outOfPhase * PI;

        if (radians < 0.0)
            radians = 0.0;

        if (bJustPulse && radians > 2.0 * PI)
            radians = 0.0;
        else if (bJustHalfPulse && radians > 1.0 * PI)
            radians = 0.0;

        double retVal = sin(radians);

        retVal *= amplitude;

        return retVal;
    }
};
