// Define a system of tuning: equal tempermant, A4 = 440 Hz, A0 = 27.5 Hz
using System;
using System.Collections.Generic;

namespace StringShear
{
    public class Tuning
    {
        Dictionary<char, double> m_noteRootFrequencies = new Dictionary<char, double>();

        public Tuning()
        {
            double rootFrequencyA0 = 27.5;
            m_noteRootFrequencies['A'] = rootFrequencyA0;
            m_noteRootFrequencies['B'] = rootFrequencyA0 * Math.Pow(2, 2.0 / 12);
            m_noteRootFrequencies['C'] = rootFrequencyA0 * Math.Pow(2, 3.0 / 12);
            m_noteRootFrequencies['D'] = rootFrequencyA0 * Math.Pow(2, 5.0 / 12);
            m_noteRootFrequencies['E'] = rootFrequencyA0 * Math.Pow(2, 7.0 / 12);
            m_noteRootFrequencies['F'] = rootFrequencyA0 * Math.Pow(2, 8.0 / 12);
            m_noteRootFrequencies['G'] = rootFrequencyA0 * Math.Pow(2, 10.0 / 12);
        }

        public bool StringToTension(string str, double stringConstant, out double tension)
        {
            tension = 0.0;

            double frequency = 0.0;
            if (!StringToFrequency(str, out frequency))
                return false;

            tension = Math.Pow(frequency / stringConstant, 2);

            return true;
        }

        public bool StringToFrequency(string str, out double frequency)
        {
            frequency = 0.0;

            string strLetters = "";
            string strOctave = "";
            bool bInLetters = true;
            foreach (char c in str)
            {
                if (char.IsDigit(c))
                    bInLetters = false;

                if (bInLetters)
                    strLetters += c;
                else
                    strOctave += c;
            }

            strLetters = strLetters.ToUpper();
            if (strLetters.Length == 0 || strLetters.Length > 2)
                return false;

            if (strLetters[0] < 'A' || strLetters[0] > 'G')
                return false;

            bool bSharp = false;
            bool bFlat = false;
            if (strLetters.Length > 1)
            {
                bSharp = strLetters[1] == '#';
                bFlat = strLetters[1] == 'b';
            }

            int octave = 0;
            if (strOctave.Length > 0)
            {
                if (!int.TryParse(strOctave, out octave))
                    return false;
            }

            frequency = m_noteRootFrequencies[strLetters[0]] * Math.Pow(2, octave);

            if (bSharp)
                frequency *= Math.Pow(2, 1.0 / 12);
            else if (bFlat)
                frequency *= 1.0 / Math.Pow(2, 1.0 / 12);

            return true;
        }
    }
}
