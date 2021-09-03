#pragma once

#include "Stringy.h"
#include "Stopwatch.h"

#include <memory>
#include <mutex>
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

    std::vector<double> m_rightFrequencies;
    std::vector<double> m_leftFrequencies;

    bool m_bRightEnabled;
    bool m_bLeftEnabled;
    bool m_bJustPulse;
    bool m_bJustHalfPulse;
    double m_outOfPhase;

    Stopwatch m_computeStopwatch;
    double m_computeElapsedMs;
    std::shared_ptr<std::thread> m_computeThread;

    Stopwatch m_outputStopwatch;
    double m_outputElapsedMs;
    std::shared_ptr<std::thread> m_networkThread;

    std::recursive_mutex m_mutex;

public:
    Simulation()
        : m_string(cParticleCount, cStringLength)
    {
        m_maxPosString = m_string.Clone();
        m_maxVelString = m_string.Clone();
        m_maxAclString = m_string.Clone();
        m_maxPunchString = m_string.Clone();

        m_computeThread = std::make_shared<std::thread>(&Simulation::Run, this);
        m_networkThread = std::make_shared<std::thread>(&Simulation::RunServer, this);
    }

private:
    void Run()
    {
        while (true)
            Update();
    }

    void RunServer()
    {
        printf("Setting up server...");
        /* FORNOW
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
        */
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

        std::lock_guard<std::recursive_mutex> lock(m_mutex);
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
        std::lock_guard<std::recursive_mutex> lock(m_mutex);
        m_outputStopwatch.Restart();

        std::unordered_map<std::string, std::string> state;
        state["time"] = std::to_string(m_time);
        state["elapsedMs"] = std::to_string(m_computeElapsedMs);

        state["maxPosTime"] = std::to_string(m_maxPosTime);
        state["maxVelTime"] = std::to_string(m_maxVelTime);
        state["maxAclTime"] = std::to_string(m_maxAclTime);
        state["maxPunchTime"] = std::to_string(m_maxPunchTime);

        state["string"] = m_string.ToString();
        state["maxPosString"] = m_maxPosString.ToString();
        state["maxVelString"] = m_maxVelString.ToString();
        state["maxAclString"] = m_maxAclString.ToString();
        state["maxPunchString"] = m_maxPunchString.ToString();

        std::string stateStr;
        for (const auto& it : state)
            stateStr += it.first + ":" + it.second + "\n";

        m_outputElapsedMs = m_outputStopwatch.ElapsedMs();
        
        return stateStr;
    }

    double GetComputeElapsedMs()
    {
        std::lock_guard<std::recursive_mutex> lock(m_mutex);
        return m_computeElapsedMs;
    }

    double GetOutputElapsedMs()
    {
        std::lock_guard<std::recursive_mutex> lock(m_mutex);
        return m_outputElapsedMs;
    }

    void Update()
    {
        m_simulationCycle++;

        // Delay.  Outside the thread safety lock.
        int delayMs = 0;
        {
            std::lock_guard<std::recursive_mutex> lock(m_mutex);
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

        std::lock_guard<std::recursive_mutex> lock(m_mutex);

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
        std::lock_guard<std::recursive_mutex> lock(m_mutex);
        m_time = 0.0;
        m_string.Reset();
        ResetMaxes();
    }

    void ResetMaxes()
    {
        std::lock_guard<std::recursive_mutex> lock(m_mutex);
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
