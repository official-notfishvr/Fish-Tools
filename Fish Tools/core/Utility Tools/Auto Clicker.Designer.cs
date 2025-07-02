namespace Fish_Tools.core.MiscTools
{
    partial class Auto_Clicker
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
            components = new System.ComponentModel.Container();
            button1 = new Button();
            button2 = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            label1 = new Label();
            IntervalPanel = new Panel();
            OffsetTxtBox = new TextBox();
            IntervalTxtBox = new TextBox();
            label3 = new Label();
            label5 = new Label();
            label4 = new Label();
            PressTypePanel = new Panel();
            KeyTxtBox = new TextBox();
            RightMouseRdBtn = new RadioButton();
            LeftMouseRdBtn = new RadioButton();
            KeyRdBtn = new RadioButton();
            IntervalProgressBar = new ProgressBar();
            timer2 = new System.Windows.Forms.Timer(components);
            toolTip1 = new ToolTip(components);
            label6 = new Label();
            label7 = new Label();
            MaxProgressLbl = new Label();
            IntervalPanel.SuspendLayout();
            PressTypePanel.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(44, 253);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(88, 27);
            button1.TabIndex = 8;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Enabled = false;
            button2.Location = new Point(187, 253);
            button2.Margin = new Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new Size(88, 27);
            button2.TabIndex = 9;
            button2.Text = "Stop";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // timer1
            // 
            timer1.Interval = 11000;
            timer1.Tick += timer1_Tick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 7);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(46, 15);
            label1.TabIndex = 2;
            label1.Text = "Interval";
            // 
            // IntervalPanel
            // 
            IntervalPanel.BorderStyle = BorderStyle.FixedSingle;
            IntervalPanel.Controls.Add(OffsetTxtBox);
            IntervalPanel.Controls.Add(IntervalTxtBox);
            IntervalPanel.Controls.Add(label3);
            IntervalPanel.Controls.Add(label5);
            IntervalPanel.Controls.Add(label4);
            IntervalPanel.Controls.Add(label1);
            IntervalPanel.Location = new Point(14, 14);
            IntervalPanel.Margin = new Padding(4, 3, 4, 3);
            IntervalPanel.Name = "IntervalPanel";
            IntervalPanel.Size = new Size(303, 71);
            IntervalPanel.TabIndex = 3;
            // 
            // OffsetTxtBox
            // 
            OffsetTxtBox.Location = new Point(76, 33);
            OffsetTxtBox.Margin = new Padding(4, 3, 4, 3);
            OffsetTxtBox.Name = "OffsetTxtBox";
            OffsetTxtBox.Size = new Size(82, 23);
            OffsetTxtBox.TabIndex = 2;
            OffsetTxtBox.Text = "0";
            toolTip1.SetToolTip(OffsetTxtBox, "A random offset in milliseconds between 0 and your offset that gets added on to your total interval. Use this in games and applications if you are woried about being detected for botting.");
            // 
            // IntervalTxtBox
            // 
            IntervalTxtBox.Location = new Point(76, 3);
            IntervalTxtBox.Margin = new Padding(4, 3, 4, 3);
            IntervalTxtBox.Name = "IntervalTxtBox";
            IntervalTxtBox.Size = new Size(82, 23);
            IntervalTxtBox.TabIndex = 1;
            IntervalTxtBox.Text = "10000";
            toolTip1.SetToolTip(IntervalTxtBox, "The interval in milliseconds that each key press will take place.");
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, 37);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(59, 15);
            label3.TabIndex = 2;
            label3.Text = "Add Rand";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(163, 37);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(23, 15);
            label5.TabIndex = 2;
            label5.Text = "ms";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(163, 7);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(23, 15);
            label4.TabIndex = 2;
            label4.Text = "ms";
            // 
            // PressTypePanel
            // 
            PressTypePanel.BorderStyle = BorderStyle.FixedSingle;
            PressTypePanel.Controls.Add(KeyTxtBox);
            PressTypePanel.Controls.Add(RightMouseRdBtn);
            PressTypePanel.Controls.Add(LeftMouseRdBtn);
            PressTypePanel.Controls.Add(KeyRdBtn);
            PressTypePanel.Location = new Point(14, 93);
            PressTypePanel.Margin = new Padding(4, 3, 4, 3);
            PressTypePanel.Name = "PressTypePanel";
            PressTypePanel.Size = new Size(303, 90);
            PressTypePanel.TabIndex = 4;
            // 
            // KeyTxtBox
            // 
            KeyTxtBox.Location = new Point(76, 7);
            KeyTxtBox.Margin = new Padding(4, 3, 4, 3);
            KeyTxtBox.Name = "KeyTxtBox";
            KeyTxtBox.RightToLeft = RightToLeft.No;
            KeyTxtBox.Size = new Size(212, 23);
            KeyTxtBox.TabIndex = 7;
            KeyTxtBox.Text = "{ENTER}";
            // 
            // RightMouseRdBtn
            // 
            RightMouseRdBtn.AutoSize = true;
            RightMouseRdBtn.Location = new Point(7, 60);
            RightMouseRdBtn.Margin = new Padding(4, 3, 4, 3);
            RightMouseRdBtn.Name = "RightMouseRdBtn";
            RightMouseRdBtn.Size = new Size(131, 19);
            RightMouseRdBtn.TabIndex = 6;
            RightMouseRdBtn.Text = "Right Mouse Button";
            RightMouseRdBtn.UseVisualStyleBackColor = true;
            RightMouseRdBtn.CheckedChanged += RightMouseRdBtn_CheckedChanged;
            // 
            // LeftMouseRdBtn
            // 
            LeftMouseRdBtn.AutoSize = true;
            LeftMouseRdBtn.Location = new Point(7, 33);
            LeftMouseRdBtn.Margin = new Padding(4, 3, 4, 3);
            LeftMouseRdBtn.Name = "LeftMouseRdBtn";
            LeftMouseRdBtn.Size = new Size(123, 19);
            LeftMouseRdBtn.TabIndex = 5;
            LeftMouseRdBtn.Text = "Left Mouse Button";
            LeftMouseRdBtn.UseVisualStyleBackColor = true;
            LeftMouseRdBtn.CheckedChanged += LeftMouseRdBtn_CheckedChanged;
            // 
            // KeyRdBtn
            // 
            KeyRdBtn.AutoSize = true;
            KeyRdBtn.Checked = true;
            KeyRdBtn.Location = new Point(7, 7);
            KeyRdBtn.Margin = new Padding(4, 3, 4, 3);
            KeyRdBtn.Name = "KeyRdBtn";
            KeyRdBtn.Size = new Size(54, 19);
            KeyRdBtn.TabIndex = 4;
            KeyRdBtn.TabStop = true;
            KeyRdBtn.Text = "Key/s";
            KeyRdBtn.UseVisualStyleBackColor = true;
            KeyRdBtn.CheckedChanged += KeyRdBtn_CheckedChanged;
            // 
            // IntervalProgressBar
            // 
            IntervalProgressBar.Location = new Point(14, 219);
            IntervalProgressBar.Margin = new Padding(4, 3, 4, 3);
            IntervalProgressBar.Name = "IntervalProgressBar";
            IntervalProgressBar.Size = new Size(303, 27);
            IntervalProgressBar.TabIndex = 5;
            // 
            // timer2
            // 
            timer2.Interval = 1000;
            timer2.Tick += timer2_Tick;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            label6.Location = new Point(-2, 286);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(59, 13);
            label6.TabIndex = 6;
            label6.Text = "Hotkey: F6";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(7, 197);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(18, 15);
            label7.TabIndex = 10;
            label7.Text = "0s";
            // 
            // MaxProgressLbl
            // 
            MaxProgressLbl.Anchor = AnchorStyles.Right;
            MaxProgressLbl.AutoEllipsis = true;
            MaxProgressLbl.Location = new Point(270, 197);
            MaxProgressLbl.Margin = new Padding(4, 0, 4, 0);
            MaxProgressLbl.Name = "MaxProgressLbl";
            MaxProgressLbl.RightToLeft = RightToLeft.No;
            MaxProgressLbl.Size = new Size(56, 15);
            MaxProgressLbl.TabIndex = 11;
            MaxProgressLbl.Text = "0";
            MaxProgressLbl.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Auto_Clicker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(331, 301);
            Controls.Add(MaxProgressLbl);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(IntervalProgressBar);
            Controls.Add(PressTypePanel);
            Controls.Add(IntervalPanel);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Text = "Auto Clicker";
            IntervalPanel.ResumeLayout(false);
            IntervalPanel.PerformLayout();
            PressTypePanel.ResumeLayout(false);
            PressTypePanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel IntervalPanel;
        private System.Windows.Forms.TextBox OffsetTxtBox;
        private System.Windows.Forms.TextBox IntervalTxtBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel PressTypePanel;
        private System.Windows.Forms.RadioButton RightMouseRdBtn;
        private System.Windows.Forms.RadioButton LeftMouseRdBtn;
        private System.Windows.Forms.RadioButton KeyRdBtn;
        private System.Windows.Forms.TextBox KeyTxtBox;
        private System.Windows.Forms.ProgressBar IntervalProgressBar;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label MaxProgressLbl;
    }
}

