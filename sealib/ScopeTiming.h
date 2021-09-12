#pragma once

#include "CsLocks.h"
#include "Stopwatch.h"

#include <algorithm>
#include <string>
#include <unordered_map>
#include <vector>

/// <summary>
/// Utility class for timing sections of code
/// To do timing:
/// Call Init first to enable timing
/// Call StartTiming at the beginning of the code to time
/// Call RecordScope at the end of the code to time
/// Call Summary to get a summary of the timings performed
/// Call Clear to remove all timings
/// </summary>
class ScopeTiming
{
public:
    /// <summary>
    /// Get the global instance of this class
    /// You can create other instances, it's just convenient to have one available
    /// </summary>
    /// <returns>The ScopeTiming instance</returns>
    static ScopeTiming& GetObj()
    {
        static ScopeTiming obj;
        return obj;
    }

    /// <summary>
    /// Initialize to enable timing
    /// </summary>
    /// <param name="enable">Specify to enable timing</param>
    void Init(bool enable)
    {
        m_enabled = enable;
    }

    /// <summary>
    /// Record the time since StartTiming was called
    /// </summary>
    /// <param name="scope">What area of the code would you call this?</param>
    /// <param name="sw">null if not timing, or Stopwatch started timing</param>
    void RecordScope(const char* scope, Stopwatch& sw)
    {
        if (!m_enabled)
            return;

        auto elapsedMs = sw.GetElapsedMs();

        {
            CSLock lock(m_mutex);

            auto it = m_timings.find(scope);
            if (it == m_timings.end())
            {
                Scope scopeObj;
                scopeObj.ScopeName = scope;
                scopeObj.Hits = 1;
                scopeObj.Allotted = elapsedMs;
                m_timings[scope] = scopeObj;
            }
            else
            {
                Scope& scopeObj = it->second;
                ++scopeObj.Hits;
                scopeObj.Allotted += elapsedMs;
            }
        }

        sw.Start();
    }

    /// <summary>
    /// Get a summary of the recorded timings
    /// </summary>
    std::string GetSummary()
    {
        if (!m_enabled)
            return "";

        std::vector<std::string> sb;
        {
            CSLock lock(m_mutex);
            for (const auto& it : m_timings)
            {
                const Scope& obj = it.second;
                if (obj.Hits == 0)
                    continue;

                std::string summary;
                summary += std::string(obj.ScopeName) + " -> " + std::to_string(obj.Hits) + " hits - ";
                summary += std::to_string(obj.Allotted) + " ms total -> ";
                summary += std::to_string(obj.Allotted / obj.Hits) + " ms avg";
                sb.push_back(summary);
            }
        }

        std::sort(sb.begin(), sb.end());

        std::string output;
        for (const auto& str : sb)
            output += str + "\n";
        return output;
    }

    /// <summary>
    /// Remove all timings
    /// </summary>
    void Clear()
    {
        CSLock lock(m_mutex);
        m_timings.clear();
    }

    bool m_enabled = false;
    CSection m_mutex;

    struct Scope
    {
        const char* ScopeName;
        int Hits;
        double Allotted;
    };
    std::unordered_map<std::string, Scope> m_timings;
};
