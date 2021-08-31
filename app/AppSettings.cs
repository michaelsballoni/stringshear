using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace StringShear
{
    public class AppSettings
    {
        public double timeSlice { get; set; } = 0.01;
        public double tension { get; set; } = 399565; // a magic number
        public string simulationSpeed { get; set; } = "Fast";

        public bool leftEnabled { get; set; } = true;
        public bool rightEnabled { get; set; } = true;

        public string leftFrequencies { get; set; } = "20";
        public string rightFrequencies { get; set; } = "30";

        public double outOfPhase { get; set; } = 0.5;

        public bool justPulse { get; set; } = false;
        public bool justHalfPulse { get; set; } = false;

        [JsonIgnore]
        public string SettingsText
        {
            get
            {
                var sb = new StringBuilder();
                using (var tw = new StringWriter(sb))
                    sm_serializer.Serialize(tw, this);
                return sb.ToString();
            }
        }

        public static AppSettings LoadSettings(string settingsText)
        {
            AppSettings settings;
            using (var tr = new StringReader(settingsText))
            using (var jr = new JsonTextReader(tr))
                settings = sm_serializer.Deserialize<AppSettings>(jr);
            return settings;
        }

        private static JsonSerializer sm_serializer = new JsonSerializer();
    }
}
