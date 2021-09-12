using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StringShear
{
    /// <summary>
    /// Utility class for timing sections of code
    /// To do timing:
    /// Call Init first to enable timing
    /// Call StartTiming at the beginning of the code to time
    /// Call RecordScope at the end of the code to time
    /// Call Summary to get a summary of the timings performed
    /// Call Clear to remove all timings
    /// </summary>
    public static class ScopeTiming
    {
        /// <summary>
        /// Initialize to enable timing
        /// </summary>
        /// <param name="enable">Specify to enable timing</param>
        public static void Init(bool enable)
        {
            sm_enabled = enable;
        }

        /// <summary>
        /// Get a Stopwatch to start timing
        /// </summary>
        /// <returns>null if not enabled, or else a new Stopwatch</returns>
        public static Stopwatch StartTiming()
        {
            if (!sm_enabled)
                return null;

            return Stopwatch.StartNew();
        }

        /// <summary>
        /// Record the time since StartTiming was called
        /// </summary>
        /// <param name="scope">What area of the code would you call this?</param>
        /// <param name="sw">null if not timing, or Stopwatch started timing</param>
        public static void RecordScope(string scope, Stopwatch sw)
        {
            if (sw == null)
                return;

            var elapsedMs = sw.Elapsed;

            lock (sm_timings)
            {
                Scope scopeObj;
                if (!sm_timings.TryGetValue(scope, out scopeObj))
                {
                    scopeObj = new Scope() { ScopeName = scope, Hits = 1, Allotted = elapsedMs };
                    sm_timings.Add(scope, scopeObj);
                }
                else
                {
                    ++scopeObj.Hits;
                    scopeObj.Allotted += elapsedMs;
                }
            }

            sw.Restart();
        }

        /// <summary>
        /// Get a summary of the recorded timings
        /// </summary>
        public static string Summary
        {
            get
            {
                var sb = new List<string>();
                lock (sm_timings)
                {
                    foreach (Scope obj in sm_timings.Values)
                    {
                        if (obj.Hits == 0)
                            continue;

                        sb.Add
                        (
                            $"{obj.ScopeName} -> {obj.Hits} hits - " +
                            $"{Math.Round(obj.Allotted.TotalMilliseconds)} ms total -> " +
                            $"{Math.Round(obj.Allotted.TotalMilliseconds / obj.Hits, 5)} ms avg"
                        );
                    }
                }
                sb.Sort();
                return string.Join("\n", sb);
            }
        }

        /// <summary>
        /// Remove all timings
        /// </summary>
        public static void Clear()
        {
            lock (sm_timings)
                sm_timings.Clear();
        }

        private static bool sm_enabled;

        private class Scope
        {
            public string ScopeName;
            public int Hits;
            public TimeSpan Allotted;
        }
        private static Dictionary<string, Scope> sm_timings = new Dictionary<string, Scope>();
    }
}