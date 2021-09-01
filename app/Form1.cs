using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace StringShear
{
    public partial class Form1 : Form
    {
        AppSettings m_settings;

        Simulation m_sim;
        Tuning m_tuning;

        BufferedGraphics m_bufferedGraphics;

        int lastFormWidth;

        // Drawing resources.
        Pen outlinePen;
        Brush stringBrush;

        Font nameFont;
        Font valueFont;

        Pen blackPen;
        Pen redPen;
        Pen greenPen;
        Pen bluePen;
        Pen purplePen;
        Pen goldPen;

        Pen endpointPen;
        Pen maxPosPen;
        Pen maxVelPen;
        Pen maxAclPen;
        Pen maxPunchPen;

        public Form1()
        {
            InitializeComponent();

            SetStyle((ControlStyles)(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint), true);

            // Set up double-buffered graphics
            BufferedGraphicsManager.Current.MaximumBuffer = new Size(displayGroup.Width + 1, displayGroup.Height + 1);
            m_bufferedGraphics = BufferedGraphicsManager.Current.Allocate(displayGroup.CreateGraphics(), new Rectangle(0, 0, displayGroup.Width, displayGroup.Height));

            // Capture the initial form width for use when resizing
            lastFormWidth = Width;

            m_sim = new Simulation();

            m_tuning = new Tuning();

            // Create the UI resources
            outlinePen = new Pen(Color.Black);
            stringBrush = new SolidBrush(Color.Black);

            nameFont = new Font("Arial", 14, FontStyle.Bold, GraphicsUnit.Pixel);
            valueFont = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Pixel);

            blackPen = new Pen(Color.Black);

            redPen = new Pen(Color.Red);
            greenPen = new Pen(Color.Green);
            bluePen = new Pen(Color.Blue);
            purplePen = new Pen(Color.Purple);
            goldPen = new Pen(Color.Maroon);

            endpointPen = new Pen(Color.Black, 4);
            maxPosPen = new Pen(Color.Red, 4);
            maxVelPen = new Pen(Color.Green, 4);
            maxAclPen = new Pen(Color.Blue, 4);
            maxPunchPen = new Pen(Color.Purple, 4);

            LoadSettings();

            Form1_SizeChanged(null, null);

            m_sim.Startup();
        }

        string SettingsFilePath
        {
            get
            {
                string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StringShear");
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string filePath = Path.Combine(dirPath, "Settings.json");
                return filePath;
            }
        }

        void SaveSettings()
        {
            try
            {
                double val;

                double.TryParse(timeSliceEdit.Text, out val);
                m_settings.timeSlice = val;

                double.TryParse(tensionEdit.Text, out val);
                m_settings.tension = val;

                m_settings.simulationSpeed = speedComboBox.Text;

                m_settings.leftEnabled = leftEnabledCheck.Checked;
                m_settings.leftFrequencies = leftFrequenciesEdit.Text;

                m_settings.rightEnabled = rightEnabledCheck.Checked;
                m_settings.rightFrequencies = rightFrequenciesEdit.Text;

                double.TryParse(outOfPhaseEdit.Text, out val);
                m_settings.outOfPhase = val;

                m_settings.justPulse = justPulseCheck.Checked;
                m_settings.justHalfPulse = justHalfPulseCheck.Checked;

                // DEBUG
                //MessageBox.Show($"Writing:\n\n{m_settings.SettingsText}\n\nto\n{SettingsFilePath}");
                File.WriteAllText(SettingsFilePath, m_settings.SettingsText);
            }
            catch (Exception exp)
            {
                MessageBox.Show($"Error saving settings: {exp.GetType().FullName}: {exp.Message}");
            }
        }

        void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                    m_settings = AppSettings.LoadSettings(File.ReadAllText(SettingsFilePath));
                else
                    m_settings = new AppSettings();

                timeSliceEdit.Text = m_settings.timeSlice.ToString();
                tensionEdit.Text = m_settings.tension.ToString();
                speedComboBox.Text = m_settings.simulationSpeed;

                leftEnabledCheck.Checked = m_settings.leftEnabled;
                leftFrequenciesEdit.Text = m_settings.leftFrequencies;

                rightEnabledCheck.Checked = m_settings.rightEnabled;
                rightFrequenciesEdit.Text = m_settings.rightFrequencies;

                outOfPhaseEdit.Text = m_settings.outOfPhase.ToString();

                justPulseCheck.Checked = m_settings.justPulse;
                justHalfPulseCheck.Checked = m_settings.justHalfPulse;

                UpdateSettings();
            }
            catch (Exception exp)
            {
                MessageBox.Show($"Error loading settings: {exp.GetType().FullName}: {exp.Message}");
            }
        }

        void UpdateDisplay()
        {
            Graphics g = m_bufferedGraphics.Graphics;

            g.Clear(Color.White);

            int padding = 5;
            int rectCount = 5;
            int width = ClientRectangle.Width - settingsGroup.Width - padding * 2;
            int height = (ClientRectangle.Height - padding * (rectCount + 1)) / rectCount;

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string part in m_sim.ToString().Split('\n'))
            {
                int colon = part.IndexOf(':');
                string name = part.Substring(0, colon);
                string value = part.Substring(colon + 1);
                dict.Add(name, value);
            }

            double time = double.Parse(dict["time"]);
            double elapsedMs = double.Parse(dict["elapsedMs"]);

            Stringy curStringy = Stringy.FromString(dict["string"]);

            Stringy maxPosStringy = Stringy.FromString(dict["maxPosString"]);
            Stringy maxVelStringy = Stringy.FromString(dict["maxValString"]);
            Stringy maxAclStringy = Stringy.FromString(dict["maxAclString"]);
            Stringy maxPunchStringy = Stringy.FromString(dict["maxPunchString"]);

            double maxPosTime = double.Parse(dict["maxPosTime"]);
            double maxVelTime = double.Parse(dict["maxVelTime"]);
            double maxAclTime = double.Parse(dict["maxAclTime"]);
            double maxPunchTime = double.Parse(dict["maxPunchTime"]);

            Rectangle curRect = new Rectangle(padding, padding, width, height);
            DrawStringy("Position",
                        blackPen,
                        g,
                        curRect,
                        curStringy,
                        curStringy.ComputeMaxPos() / Simulation.cOscillatorAmplitude,
                        time,
                        0);

            Rectangle maxPosRect = new Rectangle(padding, padding * 2 + height * 1, width, height);
            DrawStringy("Max Amplitude",
                        redPen,
                        g,
                        maxPosRect,
                        maxPosStringy,
                        maxPosStringy.GetMaxPos() / Simulation.cOscillatorAmplitude,
                        maxPosTime,
                        1);

            Rectangle maxVelRect = new Rectangle(padding, padding * 3 + height * 2, width, height);
            DrawStringy("Max Velocity",
                        greenPen,
                        g,
                        maxVelRect,
                        maxVelStringy,
                        maxVelStringy.GetMaxVel(),
                        maxVelTime,
                        2);

            Rectangle maxAclRect = new Rectangle(padding, padding * 4 + height * 3, width, height);
            DrawStringy("Max Acceleration",
                        bluePen,
                        g,
                        maxAclRect,
                        maxAclStringy,
                        maxAclStringy.GetMaxAcl(),
                        maxAclTime,
                        3);

            Rectangle maxPunchRect = new Rectangle(padding, padding * 5 + height * 4, width, height);
            DrawStringy("Max Punch",
                        purplePen,
                        g,
                        maxPunchRect,
                        maxPunchStringy,
                        maxPunchStringy.GetMaxPunch(),
                        maxPunchTime,
                        4);

            double curTime = Math.Max(time, 0.000001);
            summaryLabel.Text =
                string.Format("start work = {0}/s, max = {1}\n"
                               + "end work = {2}/s, max = {3}\n"
                               + "elapsed = {4} ms",
                               Math.Round(curStringy.GetStartWork() / curTime, 4),
                               Math.Round(curStringy.GetMaxStartWork(), 4),
                               Math.Round(curStringy.GetEndWork() / curTime, 4),
                               Math.Round(curStringy.GetMaxEndWork(), 4),
                               Math.Round(elapsedMs, 4));

            m_bufferedGraphics.Render(Graphics.FromHwnd(displayGroup.Handle));
        }

        void DrawStringy(string strName,
                         Pen linePen,
                         Graphics g,
                         Rectangle rect,
                         Stringy stringy,
                         double maxVal,
                         double time,
                         int whichMaxToDraw)
        {
            // Outline the rectangle.
            g.DrawRectangle(outlinePen, rect);

            // Draw a line through the middle, the x-axis.
            int midY = rect.Top + rect.Height / 2;
            g.DrawLine(outlinePen, rect.Left, midY, rect.Right, midY);

            // Draw a line through the middle, the y-axis.
            int midX = rect.Left + rect.Width / 2;
            g.DrawLine(outlinePen, midX, rect.Top, midX, rect.Bottom);

            // Draw the name of the chart in the top-left corner.
            g.DrawString(strName, nameFont, stringBrush, (float)rect.Left, (float)rect.Top);

            // Draw the maximum value and the current or maximum event time in the right corners.
            int rightTextOffset = -100;
            int bottomTextOffset = -15;
            g.DrawString(string.Format("max = {0}", Math.Round(maxVal, 4)), valueFont, stringBrush, (float)(rect.Right + rightTextOffset), (float)(rect.Top));
            g.DrawString(string.Format("time = {0}s", Math.Round(time, 4)), valueFont, stringBrush, (float)(rect.Right + rightTextOffset), (float)(rect.Bottom + bottomTextOffset));

            // Display the scale of the graph in the bottom-left corner.
            double maxPosVal = Math.Max(Math.Abs(stringy.GetMaxPos()),
                                         Simulation.cOscillatorAmplitude
                                         * (leftFrequenciesEdit.Lines.Length + rightFrequenciesEdit.Lines.Length));
            g.DrawString(string.Format("{0}", Math.Round(maxPosVal / Simulation.cOscillatorAmplitude, 4)),
                          valueFont,
                          stringBrush,
                          (float)(rect.Left),
                          (float)(rect.Bottom + bottomTextOffset));

            // Get the particles from the string.
            Particle[] particles = stringy.Particles;

            // Determine how many particles there are per pixel of output.
            int particlesPerPixel = Math.Max((int)(Math.Round(1.0 * particles.Length / rect.Width)), 1);

            // Walk the width of the rectangle, excluding the first and last positions.
            for (int nGraphColumn = 1; nGraphColumn < rect.Width - 1; ++nGraphColumn)
            {
                // Walk the particles that this pixel represents.
                double maxLocalPosVal = 0.0;
                double totalPosVal = 0.0;
                int particleCount = 0;
                int startIndex = (int)(Math.Round((double)(particles.Length) * (double)(nGraphColumn) / rect.Width));
                int lowIndex = Math.Min(startIndex - particlesPerPixel / 2, startIndex);
                int highIndex = Math.Min(startIndex + particlesPerPixel / 2, startIndex);
                for (int j = lowIndex; j <= highIndex; ++j)
                {
                    int particleIndex = Math.Min(Math.Max(j, 1), particles.Length - 2);
                    double curPosVal = particles[particleIndex].y;
                    if (Math.Abs(curPosVal) > Math.Abs(maxLocalPosVal))
                    {
                        maxLocalPosVal = curPosVal;
                    }
                    totalPosVal += curPosVal;
                    ++particleCount;
                }
                double avgPosVal = particleCount > 0 ? totalPosVal / particleCount : 0.0;

                // Determine the normalized value (-1.0 . 0.0 . 1.0) of this pixel.
                if (maxPosVal == 0.0)
                {
                    maxPosVal = 1.0;
                }
                double avgPosRatio = avgPosVal / maxPosVal;
                double maxPosRatio = maxLocalPosVal / maxPosVal;

                // Compute the y-offset from the center of the rectangle.
                int avgOffset = (int)(Math.Round(avgPosRatio * (rect.Height / 2)));
                int maxOffset = (int)(Math.Round(maxPosRatio * (rect.Height / 2)));

                // Compute the x and y coordinates for the pixel to output.
                int x = rect.Left + nGraphColumn;
                int avgY = rect.Top + rect.Height / 2 - avgOffset;
                int maxY = rect.Top + rect.Height / 2 - maxOffset;

                // Output the pixels as short line segments.
                g.DrawLine(linePen, x - 1, avgY, x + 1, avgY);
                if (maxY != avgY)
                {
                    g.DrawLine(linePen, x, avgY, x, maxY);
                }
            }

            // Draw the start and end particles specially.
            DrawParticle(g, stringy, particles[0], endpointPen, rect, maxPosVal);
            DrawParticle(g, stringy, particles[particles.Length - 1], endpointPen, rect, maxPosVal);

            // Draw the maximum particles of interest for this string.
            switch (whichMaxToDraw)
            {
                case 0:
                    // No special particle for the current string; too distracting.
                    break;
                case 1:
                    DrawParticle(g, stringy, particles[stringy.GetMaxPosIndex()], maxPosPen, rect, maxPosVal);
                    break;
                case 2:
                    DrawParticle(g, stringy, particles[stringy.GetMaxVelIndex()], maxVelPen, rect, maxPosVal);
                    break;
                case 3:
                    DrawParticle(g, stringy, particles[stringy.GetMaxAclIndex()], maxAclPen, rect, maxPosVal);
                    break;
                case 4:
                    DrawParticle(g, stringy, particles[stringy.GetMaxPunchIndex()], maxPunchPen, rect, maxPosVal);
                    break;
            }
        }

        void DrawParticle(Graphics g, Stringy stringy, Particle particle, Pen pen, Rectangle rect, double maxPosVal)
        {
            // NOTE: Assume that the x value for the particle is in the range 0.0 . 1.0.
            int x = rect.Left + (int)(Math.Round((particle.x / stringy.Length) * rect.Width));
            int y = rect.Top + rect.Height / 2
                    - (int)(Math.Round((particle.y / maxPosVal) * (rect.Height / 2)));
            g.DrawEllipse(pen, x - 2, y - 2, 4, 4);
        }

        double[] GetFrequencies(string[] strings)
        {
            double[] retVal = new double[strings.Length];
            for (int nStringIndex = 0; nStringIndex < strings.Length; ++nStringIndex)
            {
                string str = strings[nStringIndex];

                double frequency = 0.0;
                if (!m_tuning.StringToFrequency(str, out frequency))
                {
                    if (!double.TryParse(str, out frequency))
                    {
                        if (str.Trim().Length > 0)
                            MessageBox.Show("Invalid frequency: " + str);
                    }
                }
                retVal[nStringIndex] = frequency;
            }
            return retVal;
        }

        // Update the simulation with the UI settings
        void UpdateSettings()
        {
            string settings = "";

            double timeSliceMs = 1.0;
            double.TryParse(timeSliceEdit.Text, out timeSliceMs);
            settings += "timeSlice:" + timeSliceMs + "\n";

            int delayMs = 0;
            int delayMod = 0;
            if (speedComboBox.Text == "All Out")
            {
                delayMs = -1;
            }
            else if (speedComboBox.Text == "Fast")
            {
                delayMs = 0;
            }
            else if (speedComboBox.Text == "Medium")
            {
                delayMs = 1;
                delayMod = 10;
            }
            else if (speedComboBox.Text == "Slow")
            {
                delayMs = 1;
                delayMod = 2;
            }
            settings += "delayMs:" + delayMs + "\n";
            settings += "delayMod:" + delayMod + "\n";

            double tension = 0.0;
            bool bValidTension = true;
            if (!m_tuning.StringToTension(tensionEdit.Text, Simulation.cStringConstant, out tension))
            {
                if (!double.TryParse(tensionEdit.Text, out tension))
                {
                    MessageBox.Show("Invalid tension value: " + tensionEdit.Text);
                    bValidTension = false;
                }
            }
            if (bValidTension)
                settings += "tension:" + tension + "\n";

            settings += "damping:" + 1 + "\n";

            settings += "rightEnabled:" + rightEnabledCheck.Checked + "\n";
            settings += "rightFrequencies:" +
                string.Join(",", GetFrequencies(rightFrequenciesEdit.Lines).Select(x => x.ToString()));

            settings += "leftEnabled:" + leftEnabledCheck.Checked + "\n";
            settings += "leftFrequencies:" +
                string.Join(",", GetFrequencies(leftFrequenciesEdit.Lines).Select(x => x.ToString()));

            settings += "justPulse:" + justPulseCheck.Checked + "\n";
            settings += "justHalfPulse:" + justHalfPulseCheck.Checked + "\n";

            double outOfPhase = 0.0;
            double.TryParse(outOfPhaseEdit.Text, out outOfPhase);
            settings += "outOfPhase:" + outOfPhase + "\n";

            m_sim.ApplySettings(settings);
        }

        void timer1_Tick(object obj, EventArgs args)
        {
            UpdateDisplay();
        }

        void Form1_FormClosed(object obj, FormClosedEventArgs args)
        {
            m_sim.Shutdown();
            SaveSettings();
        }

        void pauseRunButton_Click(object obj, EventArgs args)
        {
            UpdateSettings();

            bool bPause = pauseRunButton.Text == "Pause";
            pauseRunButton.Text = bPause ? "Run" : "Pause";

            m_sim.ApplySettings("paused:" + bPause);
        }

        void resetButton_Click(object obj, EventArgs args)
        {
            UpdateSettings();

            if (pauseRunButton.Text == "Pause")
                pauseRunButton_Click(null, null);

            m_sim.ApplySettings("reset:true");
        }

        // Keep the settings in the top-right corner as the main form is resized
        void Form1_SizeChanged(object obj, EventArgs args)
        {
            if (m_bufferedGraphics == null)
                return;

            settingsGroup.Left += Width - lastFormWidth;

            displayGroup.Width = settingsGroup.Left;
            displayGroup.Height = Height;

            try // Ignore errors encountered allocating the buffer size...fix for crash on minimize
            {
                BufferedGraphicsManager.Current.MaximumBuffer = new Size(displayGroup.Width + 1, displayGroup.Height + 1);
                m_bufferedGraphics = BufferedGraphicsManager.Current.Allocate(displayGroup.CreateGraphics(), new Rectangle(0, 0, displayGroup.Width, displayGroup.Height));
            }
            catch (Exception exp)
            {
                MessageBox.Show($"Resize error:\n\n{exp}");
            }

            lastFormWidth = Width;

            Invalidate();
        }

        void resetMaxButton_Click(object obj, EventArgs args)
        {
            m_sim.ApplySettings("resetMaxes:true");
        }

        void copyHeadersButton_Click(object obj, EventArgs args)
        {
            string strHeaders =
                "timeslice\t" +
                "tension\t" +
                "out of phase\t" +
                "left frequencies\t" +
                "right frequencies\t" +
                "max position\t" +
                "max velocity\t" +
                "max acceleration\t" +
                "max punch\t" +
                "avg. left work\t" +
                "avg. right work"
                ;

            copyEdit.Text = strHeaders;
            copyEdit.SelectAll();
            copyEdit.Copy();
        }

        void copyStatsButton_Click(object obj, EventArgs args)
        {
            string strLeftFrequencies =
                leftEnabledCheck.Checked
                ? string.Join(", ", leftFrequenciesEdit.Lines)
                : "0.0";

            string strRightFrequencies =
                rightEnabledCheck.Checked
                ? string.Join(", ", rightFrequenciesEdit.Lines)
                : "0.0";

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string part in m_sim.ToString().Split('\n'))
            {
                int colon = part.IndexOf(':');
                string name = part.Substring(0, colon);
                string value = part.Substring(colon + 1);
                dict.Add(name, value);
            }

            double time = double.Parse(dict["time"]);

            Stringy curStringy = Stringy.FromString(dict["string"]);

            Stringy maxPosStringy = Stringy.FromString(dict["maxPosString"]);
            Stringy maxVelStringy = Stringy.FromString(dict["maxValString"]);
            Stringy maxAclStringy = Stringy.FromString(dict["maxAclString"]);
            Stringy maxPunchStringy = Stringy.FromString(dict["maxPunchString"]);

            // Build the tab-delimited string.
            string strStats =
                    timeSliceEdit.Text + "\t" +
                    tensionEdit.Text + "\t" +
                    outOfPhaseEdit.Text + "\t" +
                    strLeftFrequencies + "\t" +
                    strRightFrequencies + "\t" +
                    (maxPosStringy != null ? maxPosStringy.GetMaxPos().ToString() : "0.0") + "\t" +
                    (maxVelStringy != null ? maxVelStringy.GetMaxVel().ToString() : "0.0") + "\t" +
                    (maxAclStringy != null ? maxAclStringy.GetMaxAcl().ToString() : "0.0") + "\t" +
                    (maxPunchStringy != null ? maxPunchStringy.GetMaxPunch().ToString() : "0.0") + "\t" +
                    (curStringy.GetStartWork() / time).ToString() + "\t" +
                    (curStringy.GetEndWork() / time).ToString()
                    ;

            copyEdit.Text = strStats;
            copyEdit.SelectAll();
            copyEdit.Copy();
        }

        void mailLink_LinkClicked(object obj, LinkLabelLinkClickedEventArgs args)
        {
            Process.Start
            (
                new ProcessStartInfo() 
                { 
                    FileName = "https://stringshear.com", 
                    UseShellExecute = true 
                }
            );
        }
    }
}
