namespace StringShear
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.speedComboBox = new System.Windows.Forms.ComboBox();
            this.mailLink = new System.Windows.Forms.LinkLabel();
            this.copyHeadersButton = new System.Windows.Forms.Button();
            this.copyEdit = new System.Windows.Forms.TextBox();
            this.copyStatsButton = new System.Windows.Forms.Button();
            this.resetMaxButton = new System.Windows.Forms.Button();
            this.timeSliceEdit = new System.Windows.Forms.TextBox();
            this.rightFrequenciesEdit = new System.Windows.Forms.TextBox();
            this.justHalfPulseCheck = new System.Windows.Forms.CheckBox();
            this.justPulseCheck = new System.Windows.Forms.CheckBox();
            this.tensionEdit = new System.Windows.Forms.TextBox();
            this.speedLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.displayGroup = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.outOfPhaseEdit = new System.Windows.Forms.TextBox();
            this.rightEnabledCheck = new System.Windows.Forms.CheckBox();
            this.leftEnabledCheck = new System.Windows.Forms.CheckBox();
            this.leftFrequenciesEdit = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.resetButton = new System.Windows.Forms.Button();
            this.pauseRunButton = new System.Windows.Forms.Button();
            this.settingsGroup = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.settingsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(122, 55);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 60);
            this.label5.TabIndex = 19;
            this.label5.Text = "Hz\r\n/\r\nNote";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 98);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 20);
            this.label1.TabIndex = 31;
            this.label1.Text = "simulation speed";
            // 
            // speedComboBox
            // 
            this.speedComboBox.FormattingEnabled = true;
            this.speedComboBox.Items.AddRange(new object[] {
            "Fast",
            "Medium",
            "Slow",
            "All Out"});
            this.speedComboBox.Location = new System.Drawing.Point(15, 122);
            this.speedComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.speedComboBox.Name = "speedComboBox";
            this.speedComboBox.Size = new System.Drawing.Size(180, 28);
            this.speedComboBox.TabIndex = 30;
            this.speedComboBox.Text = "Fast";
            // 
            // mailLink
            // 
            this.mailLink.AutoSize = true;
            this.mailLink.Location = new System.Drawing.Point(222, 684);
            this.mailLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mailLink.Name = "mailLink";
            this.mailLink.Size = new System.Drawing.Size(122, 20);
            this.mailLink.TabIndex = 16;
            this.mailLink.TabStop = true;
            this.mailLink.Text = "stringshear.com";
            this.mailLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.mailLink_LinkClicked);
            // 
            // copyHeadersButton
            // 
            this.copyHeadersButton.Location = new System.Drawing.Point(15, 639);
            this.copyHeadersButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.copyHeadersButton.Name = "copyHeadersButton";
            this.copyHeadersButton.Size = new System.Drawing.Size(168, 35);
            this.copyHeadersButton.TabIndex = 14;
            this.copyHeadersButton.Text = "Copy Stats Header";
            this.copyHeadersButton.UseVisualStyleBackColor = true;
            this.copyHeadersButton.Click += new System.EventHandler(this.copyHeadersButton_Click);
            // 
            // copyEdit
            // 
            this.copyEdit.Location = new System.Drawing.Point(15, 684);
            this.copyEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.copyEdit.Name = "copyEdit";
            this.copyEdit.Size = new System.Drawing.Size(111, 26);
            this.copyEdit.TabIndex = 29;
            this.copyEdit.Visible = false;
            // 
            // copyStatsButton
            // 
            this.copyStatsButton.Location = new System.Drawing.Point(192, 639);
            this.copyStatsButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.copyStatsButton.Name = "copyStatsButton";
            this.copyStatsButton.Size = new System.Drawing.Size(153, 35);
            this.copyStatsButton.TabIndex = 15;
            this.copyStatsButton.Text = "Copy Stats Values";
            this.copyStatsButton.UseVisualStyleBackColor = true;
            this.copyStatsButton.Click += new System.EventHandler(this.copyStatsButton_Click);
            // 
            // resetMaxButton
            // 
            this.resetMaxButton.Location = new System.Drawing.Point(15, 594);
            this.resetMaxButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.resetMaxButton.Name = "resetMaxButton";
            this.resetMaxButton.Size = new System.Drawing.Size(330, 35);
            this.resetMaxButton.TabIndex = 11;
            this.resetMaxButton.Text = "Reset Maximums";
            this.resetMaxButton.UseVisualStyleBackColor = true;
            this.resetMaxButton.Click += new System.EventHandler(this.resetMaxButton_Click);
            // 
            // timeSliceEdit
            // 
            this.timeSliceEdit.Location = new System.Drawing.Point(17, 51);
            this.timeSliceEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.timeSliceEdit.Name = "timeSliceEdit";
            this.timeSliceEdit.Size = new System.Drawing.Size(124, 26);
            this.timeSliceEdit.TabIndex = 0;
            this.timeSliceEdit.Text = "0.01";
            this.timeSliceEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // rightFrequenciesEdit
            // 
            this.rightFrequenciesEdit.Location = new System.Drawing.Point(176, 51);
            this.rightFrequenciesEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rightFrequenciesEdit.Multiline = true;
            this.rightFrequenciesEdit.Name = "rightFrequenciesEdit";
            this.rightFrequenciesEdit.Size = new System.Drawing.Size(98, 169);
            this.rightFrequenciesEdit.TabIndex = 5;
            this.rightFrequenciesEdit.Text = "30";
            this.rightFrequenciesEdit.WordWrap = false;
            // 
            // justHalfPulseCheck
            // 
            this.justHalfPulseCheck.AutoSize = true;
            this.justHalfPulseCheck.Location = new System.Drawing.Point(176, 266);
            this.justHalfPulseCheck.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.justHalfPulseCheck.Name = "justHalfPulseCheck";
            this.justHalfPulseCheck.Size = new System.Drawing.Size(132, 24);
            this.justHalfPulseCheck.TabIndex = 8;
            this.justHalfPulseCheck.Text = "just half pulse";
            this.justHalfPulseCheck.UseVisualStyleBackColor = true;
            // 
            // justPulseCheck
            // 
            this.justPulseCheck.AutoSize = true;
            this.justPulseCheck.Location = new System.Drawing.Point(176, 231);
            this.justPulseCheck.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.justPulseCheck.Name = "justPulseCheck";
            this.justPulseCheck.Size = new System.Drawing.Size(102, 24);
            this.justPulseCheck.TabIndex = 7;
            this.justPulseCheck.Text = "just pulse";
            this.justPulseCheck.UseVisualStyleBackColor = true;
            // 
            // tensionEdit
            // 
            this.tensionEdit.Location = new System.Drawing.Point(15, 198);
            this.tensionEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tensionEdit.Name = "tensionEdit";
            this.tensionEdit.Size = new System.Drawing.Size(260, 26);
            this.tensionEdit.TabIndex = 1;
            this.tensionEdit.Text = "399565";
            this.tensionEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(152, 55);
            this.speedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(30, 20);
            this.speedLabel.TabIndex = 19;
            this.speedLabel.Text = "ms";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 171);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 20);
            this.label3.TabIndex = 13;
            this.label3.Text = "tension / root note";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(276, 55);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 60);
            this.label4.TabIndex = 17;
            this.label4.Text = "Hz\r\n/\r\nNote";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 25);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 20);
            this.label7.TabIndex = 18;
            this.label7.Text = "timeSlice";
            // 
            // displayGroup
            // 
            this.displayGroup.Location = new System.Drawing.Point(2, 2);
            this.displayGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.displayGroup.Name = "displayGroup";
            this.displayGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.displayGroup.Size = new System.Drawing.Size(762, 789);
            this.displayGroup.TabIndex = 3;
            this.displayGroup.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.outOfPhaseEdit);
            this.groupBox1.Controls.Add(this.rightEnabledCheck);
            this.groupBox1.Controls.Add(this.leftEnabledCheck);
            this.groupBox1.Controls.Add(this.leftFrequenciesEdit);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.rightFrequenciesEdit);
            this.groupBox1.Controls.Add(this.justHalfPulseCheck);
            this.groupBox1.Controls.Add(this.justPulseCheck);
            this.groupBox1.Location = new System.Drawing.Point(17, 238);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(330, 302);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Oscillators";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 238);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 20);
            this.label2.TabIndex = 25;
            this.label2.Text = "out of phase (-pi)";
            // 
            // outOfPhaseEdit
            // 
            this.outOfPhaseEdit.Location = new System.Drawing.Point(9, 261);
            this.outOfPhaseEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.outOfPhaseEdit.Name = "outOfPhaseEdit";
            this.outOfPhaseEdit.Size = new System.Drawing.Size(122, 26);
            this.outOfPhaseEdit.TabIndex = 6;
            this.outOfPhaseEdit.Text = "0.5";
            this.outOfPhaseEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // rightEnabledCheck
            // 
            this.rightEnabledCheck.AutoSize = true;
            this.rightEnabledCheck.Checked = true;
            this.rightEnabledCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rightEnabledCheck.Location = new System.Drawing.Point(176, 25);
            this.rightEnabledCheck.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rightEnabledCheck.Name = "rightEnabledCheck";
            this.rightEnabledCheck.Size = new System.Drawing.Size(127, 24);
            this.rightEnabledCheck.TabIndex = 4;
            this.rightEnabledCheck.Text = "right enabled";
            this.rightEnabledCheck.UseVisualStyleBackColor = true;
            // 
            // leftEnabledCheck
            // 
            this.leftEnabledCheck.AutoSize = true;
            this.leftEnabledCheck.Checked = true;
            this.leftEnabledCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.leftEnabledCheck.Location = new System.Drawing.Point(9, 25);
            this.leftEnabledCheck.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.leftEnabledCheck.Name = "leftEnabledCheck";
            this.leftEnabledCheck.Size = new System.Drawing.Size(118, 24);
            this.leftEnabledCheck.TabIndex = 2;
            this.leftEnabledCheck.Text = "left enabled";
            this.leftEnabledCheck.UseVisualStyleBackColor = true;
            // 
            // leftFrequenciesEdit
            // 
            this.leftFrequenciesEdit.Location = new System.Drawing.Point(9, 51);
            this.leftFrequenciesEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.leftFrequenciesEdit.Multiline = true;
            this.leftFrequenciesEdit.Name = "leftFrequenciesEdit";
            this.leftFrequenciesEdit.Size = new System.Drawing.Size(103, 169);
            this.leftFrequenciesEdit.TabIndex = 3;
            this.leftFrequenciesEdit.Text = "20";
            this.leftFrequenciesEdit.WordWrap = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // resetButton
            // 
            this.resetButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.resetButton.Location = new System.Drawing.Point(192, 549);
            this.resetButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(153, 35);
            this.resetButton.TabIndex = 10;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // pauseRunButton
            // 
            this.pauseRunButton.Location = new System.Drawing.Point(15, 549);
            this.pauseRunButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pauseRunButton.Name = "pauseRunButton";
            this.pauseRunButton.Size = new System.Drawing.Size(168, 35);
            this.pauseRunButton.TabIndex = 9;
            this.pauseRunButton.Text = "Run";
            this.pauseRunButton.UseVisualStyleBackColor = true;
            this.pauseRunButton.Click += new System.EventHandler(this.pauseRunButton_Click);
            // 
            // settingsGroup
            // 
            this.settingsGroup.BackColor = System.Drawing.SystemColors.Control;
            this.settingsGroup.Controls.Add(this.label1);
            this.settingsGroup.Controls.Add(this.speedComboBox);
            this.settingsGroup.Controls.Add(this.mailLink);
            this.settingsGroup.Controls.Add(this.copyHeadersButton);
            this.settingsGroup.Controls.Add(this.copyEdit);
            this.settingsGroup.Controls.Add(this.copyStatsButton);
            this.settingsGroup.Controls.Add(this.resetMaxButton);
            this.settingsGroup.Controls.Add(this.timeSliceEdit);
            this.settingsGroup.Controls.Add(this.tensionEdit);
            this.settingsGroup.Controls.Add(this.speedLabel);
            this.settingsGroup.Controls.Add(this.label7);
            this.settingsGroup.Controls.Add(this.label3);
            this.settingsGroup.Controls.Add(this.groupBox1);
            this.settingsGroup.Controls.Add(this.resetButton);
            this.settingsGroup.Controls.Add(this.pauseRunButton);
            this.settingsGroup.Location = new System.Drawing.Point(764, 2);
            this.settingsGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.settingsGroup.Name = "settingsGroup";
            this.settingsGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.settingsGroup.Size = new System.Drawing.Size(352, 725);
            this.settingsGroup.TabIndex = 2;
            this.settingsGroup.TabStop = false;
            this.settingsGroup.Text = "Settings";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1131, 731);
            this.Controls.Add(this.displayGroup);
            this.Controls.Add(this.settingsGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StringShear - Simple Harmonic Motion On A String Simulation";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.settingsGroup.ResumeLayout(false);
            this.settingsGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox speedComboBox;
        private System.Windows.Forms.LinkLabel mailLink;
        private System.Windows.Forms.Button copyHeadersButton;
        private System.Windows.Forms.TextBox copyEdit;
        private System.Windows.Forms.Button copyStatsButton;
        private System.Windows.Forms.Button resetMaxButton;
        private System.Windows.Forms.TextBox timeSliceEdit;
        private System.Windows.Forms.TextBox rightFrequenciesEdit;
        private System.Windows.Forms.CheckBox justHalfPulseCheck;
        private System.Windows.Forms.CheckBox justPulseCheck;
        private System.Windows.Forms.TextBox tensionEdit;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox displayGroup;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox outOfPhaseEdit;
        private System.Windows.Forms.CheckBox rightEnabledCheck;
        private System.Windows.Forms.CheckBox leftEnabledCheck;
        private System.Windows.Forms.TextBox leftFrequenciesEdit;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button pauseRunButton;
        private System.Windows.Forms.GroupBox settingsGroup;
    }
}

