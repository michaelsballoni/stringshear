#pragma once

#include <chrono>

class Stopwatch
{
public:
    Stopwatch()
    {
        Restart();
    }

    void Restart()
    {
        m_start = m_timer.now();
    }

    double ElapsedMs()
    {
        return
            std::chrono::duration_cast<std::chrono::microseconds>
            (m_timer.now() - m_start).count()
            / 1000.0;
    }

private:
    std::chrono::high_resolution_clock m_timer;
    std::chrono::steady_clock::time_point m_start;
};
