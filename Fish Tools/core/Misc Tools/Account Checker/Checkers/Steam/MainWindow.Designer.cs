namespace Fish_Tools.core.MiscTools.AccountChecker.Checkers.Steam
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ButtonStart = new Button();
            tabControl1 = new TabControl();
            tabAutomaticCheck = new TabPage();
            button8 = new Button();
            textBoxLocalToExport = new TextBox();
            label11 = new Label();
            groupBox1 = new GroupBox();
            remainingLabel = new Label();
            label16 = new Label();
            checkedLabel = new Label();
            label14 = new Label();
            badLabel = new Label();
            steamGuardLabel = new Label();
            loggableLabel = new Label();
            label10 = new Label();
            label8 = new Label();
            label9 = new Label();
            button6 = new Button();
            button5 = new Button();
            button4 = new Button();
            label7 = new Label();
            textBoxFile = new TextBox();
            label6 = new Label();
            tabSettings = new TabPage();
            CheckboxShowColouredItemsInAccountList = new CheckBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripMenuItem4 = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            whatsHappening = new ListView();
            StatusColumn = new ColumnHeader();
            UsernameColumn = new ColumnHeader();
            PasswordColumn = new ColumnHeader();
            ResultColumn = new ColumnHeader();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            loadingImage = new PictureBox();
            labelStatus = new Label();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            flowLayoutPanel1 = new FlowLayoutPanel();
            tabControl1.SuspendLayout();
            tabAutomaticCheck.SuspendLayout();
            groupBox1.SuspendLayout();
            tabSettings.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)loadingImage).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // ButtonStart
            // 
            ButtonStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ButtonStart.FlatStyle = FlatStyle.System;
            ButtonStart.Location = new Point(714, 448);
            ButtonStart.Margin = new Padding(4, 3, 4, 3);
            ButtonStart.Name = "ButtonStart";
            ButtonStart.Size = new Size(158, 27);
            ButtonStart.TabIndex = 0;
            ButtonStart.Text = "Check for accounts";
            ButtonStart.UseVisualStyleBackColor = true;
            ButtonStart.Click += ButtonStart_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabAutomaticCheck);
            tabControl1.Controls.Add(tabSettings);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Margin = new Padding(4, 3, 4, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 3;
            tabControl1.Size = new Size(860, 430);
            tabControl1.TabIndex = 4;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabAutomaticCheck
            // 
            tabAutomaticCheck.BackColor = Color.White;
            tabAutomaticCheck.Controls.Add(button8);
            tabAutomaticCheck.Controls.Add(textBoxLocalToExport);
            tabAutomaticCheck.Controls.Add(label11);
            tabAutomaticCheck.Controls.Add(groupBox1);
            tabAutomaticCheck.Controls.Add(button6);
            tabAutomaticCheck.Controls.Add(button5);
            tabAutomaticCheck.Controls.Add(button4);
            tabAutomaticCheck.Controls.Add(label7);
            tabAutomaticCheck.Controls.Add(textBoxFile);
            tabAutomaticCheck.Controls.Add(label6);
            tabAutomaticCheck.Location = new Point(4, 24);
            tabAutomaticCheck.Margin = new Padding(4, 3, 4, 3);
            tabAutomaticCheck.Name = "tabAutomaticCheck";
            tabAutomaticCheck.Padding = new Padding(4, 3, 4, 3);
            tabAutomaticCheck.Size = new Size(852, 402);
            tabAutomaticCheck.TabIndex = 1;
            tabAutomaticCheck.Text = "Steam";
            // 
            // button8
            // 
            button8.FlatStyle = FlatStyle.System;
            button8.Location = new Point(391, 84);
            button8.Margin = new Padding(4, 3, 4, 3);
            button8.Name = "button8";
            button8.Size = new Size(35, 25);
            button8.TabIndex = 9;
            button8.Text = "...";
            button8.UseVisualStyleBackColor = true;
            button8.Click += Button8_Click;
            // 
            // textBoxLocalToExport
            // 
            textBoxLocalToExport.AllowDrop = true;
            textBoxLocalToExport.Location = new Point(134, 85);
            textBoxLocalToExport.Margin = new Padding(4, 3, 4, 3);
            textBoxLocalToExport.Name = "textBoxLocalToExport";
            textBoxLocalToExport.Size = new Size(252, 23);
            textBoxLocalToExport.TabIndex = 8;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(21, 88);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(107, 15);
            label11.TabIndex = 7;
            label11.Text = "Location to export:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            groupBox1.Controls.Add(remainingLabel);
            groupBox1.Controls.Add(label16);
            groupBox1.Controls.Add(checkedLabel);
            groupBox1.Controls.Add(label14);
            groupBox1.Controls.Add(badLabel);
            groupBox1.Controls.Add(steamGuardLabel);
            groupBox1.Controls.Add(loggableLabel);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label9);
            groupBox1.Location = new Point(21, 180);
            groupBox1.Margin = new Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 3, 4, 3);
            groupBox1.Size = new Size(400, 140);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Statistics";
            // 
            // remainingLabel
            // 
            remainingLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            remainingLabel.Location = new Point(154, 111);
            remainingLabel.Margin = new Padding(4, 0, 4, 0);
            remainingLabel.Name = "remainingLabel";
            remainingLabel.Size = new Size(47, 15);
            remainingLabel.TabIndex = 8;
            remainingLabel.Text = "0";
            remainingLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label16
            // 
            label16.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label16.Location = new Point(19, 111);
            label16.Margin = new Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new Size(132, 15);
            label16.TabIndex = 8;
            label16.Text = "Remaining:";
            label16.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // checkedLabel
            // 
            checkedLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            checkedLabel.Location = new Point(154, 90);
            checkedLabel.Margin = new Padding(4, 0, 4, 0);
            checkedLabel.Name = "checkedLabel";
            checkedLabel.Size = new Size(47, 15);
            checkedLabel.TabIndex = 8;
            checkedLabel.Text = "0";
            checkedLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label14
            // 
            label14.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label14.Location = new Point(19, 90);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new Size(132, 15);
            label14.TabIndex = 8;
            label14.Text = "Checked:";
            label14.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // badLabel
            // 
            badLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            badLabel.Location = new Point(154, 69);
            badLabel.Margin = new Padding(4, 0, 4, 0);
            badLabel.Name = "badLabel";
            badLabel.Size = new Size(47, 15);
            badLabel.TabIndex = 8;
            badLabel.Text = "0";
            badLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // steamGuardLabel
            // 
            steamGuardLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            steamGuardLabel.Location = new Point(154, 48);
            steamGuardLabel.Margin = new Padding(4, 0, 4, 0);
            steamGuardLabel.Name = "steamGuardLabel";
            steamGuardLabel.Size = new Size(47, 15);
            steamGuardLabel.TabIndex = 8;
            steamGuardLabel.Text = "0";
            steamGuardLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // loggableLabel
            // 
            loggableLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            loggableLabel.Location = new Point(154, 27);
            loggableLabel.Margin = new Padding(4, 0, 4, 0);
            loggableLabel.Name = "loggableLabel";
            loggableLabel.Size = new Size(47, 15);
            loggableLabel.TabIndex = 8;
            loggableLabel.Text = "0";
            loggableLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            label10.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label10.Location = new Point(19, 69);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(132, 15);
            label10.TabIndex = 8;
            label10.Text = "Bad:";
            label10.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            label8.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label8.Location = new Point(19, 47);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(132, 15);
            label8.TabIndex = 8;
            label8.Text = "SteamGuard protected:";
            label8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            label9.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label9.Location = new Point(19, 25);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(132, 15);
            label9.TabIndex = 8;
            label9.Text = "Loggable:";
            label9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button6.FlatStyle = FlatStyle.System;
            button6.Location = new Point(24, 359);
            button6.Margin = new Padding(4, 3, 4, 3);
            button6.Name = "button6";
            button6.Size = new Size(397, 24);
            button6.TabIndex = 7;
            button6.Text = "Export Bad Accounts   ";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button5.FlatStyle = FlatStyle.System;
            button5.Location = new Point(21, 332);
            button5.Margin = new Padding(4, 3, 4, 3);
            button5.Name = "button5";
            button5.Size = new Size(400, 24);
            button5.TabIndex = 7;
            button5.Text = "Export Good Accounts";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button4
            // 
            button4.FlatStyle = FlatStyle.System;
            button4.Location = new Point(391, 54);
            button4.Margin = new Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new Size(35, 25);
            button4.TabIndex = 9;
            button4.Text = "...";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(22, 20);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(238, 15);
            label7.TabIndex = 10;
            label7.Text = "Select a file with multiple accounts to check";
            // 
            // textBoxFile
            // 
            textBoxFile.AllowDrop = true;
            textBoxFile.Location = new Point(55, 55);
            textBoxFile.Margin = new Padding(4, 3, 4, 3);
            textBoxFile.Name = "textBoxFile";
            textBoxFile.Size = new Size(331, 23);
            textBoxFile.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(21, 58);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(28, 15);
            label6.TabIndex = 7;
            label6.Text = "File:";
            // 
            // tabSettings
            // 
            tabSettings.Controls.Add(CheckboxShowColouredItemsInAccountList);
            tabSettings.Location = new Point(4, 24);
            tabSettings.Margin = new Padding(4, 3, 4, 3);
            tabSettings.Name = "tabSettings";
            tabSettings.Padding = new Padding(4, 3, 4, 3);
            tabSettings.Size = new Size(852, 402);
            tabSettings.TabIndex = 3;
            tabSettings.Text = "Settings";
            tabSettings.UseVisualStyleBackColor = true;
            // 
            // CheckboxShowColouredItemsInAccountList
            // 
            CheckboxShowColouredItemsInAccountList.AutoSize = true;
            CheckboxShowColouredItemsInAccountList.Checked = true;
            CheckboxShowColouredItemsInAccountList.CheckState = CheckState.Checked;
            CheckboxShowColouredItemsInAccountList.Location = new Point(19, 18);
            CheckboxShowColouredItemsInAccountList.Margin = new Padding(4, 3, 4, 3);
            CheckboxShowColouredItemsInAccountList.Name = "CheckboxShowColouredItemsInAccountList";
            CheckboxShowColouredItemsInAccountList.Size = new Size(234, 19);
            CheckboxShowColouredItemsInAccountList.TabIndex = 8;
            CheckboxShowColouredItemsInAccountList.Text = "Show coloured items in the account list";
            CheckboxShowColouredItemsInAccountList.UseVisualStyleBackColor = true;
            CheckboxShowColouredItemsInAccountList.CheckedChanged += checkShowPasswordInLogs_CheckedChanged;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem4, toolStripSeparator1, toolStripMenuItem1, toolStripMenuItem2, toolStripMenuItem3 });
            contextMenuStrip1.Name = "contextMSAutomaticAccounts";
            contextMenuStrip1.RenderMode = ToolStripRenderMode.System;
            contextMenuStrip1.Size = new Size(204, 98);
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(203, 22);
            toolStripMenuItem4.Text = "Copy all";
            toolStripMenuItem4.Click += toolStripMenuItem4_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(200, 6);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(203, 22);
            toolStripMenuItem1.Text = "Copy selected";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(203, 22);
            toolStripMenuItem2.Text = "Copy selected username";
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(203, 22);
            toolStripMenuItem3.Text = "Copy selected password";
            toolStripMenuItem3.Click += toolStripMenuItem3_Click;
            // 
            // whatsHappening
            // 
            whatsHappening.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            whatsHappening.BorderStyle = BorderStyle.None;
            whatsHappening.Columns.AddRange(new ColumnHeader[] { StatusColumn, UsernameColumn, PasswordColumn, ResultColumn });
            whatsHappening.ContextMenuStrip = contextMenuStrip1;
            whatsHappening.FullRowSelect = true;
            whatsHappening.GridLines = true;
            whatsHappening.Location = new Point(453, 35);
            whatsHappening.Margin = new Padding(4, 3, 4, 3);
            whatsHappening.Name = "whatsHappening";
            whatsHappening.Size = new Size(416, 403);
            whatsHappening.TabIndex = 5;
            whatsHappening.UseCompatibleStateImageBehavior = false;
            whatsHappening.View = View.Details;
            // 
            // StatusColumn
            // 
            StatusColumn.Name = "StatusColumn";
            StatusColumn.Text = "Status";
            StatusColumn.Width = 45;
            // 
            // UsernameColumn
            // 
            UsernameColumn.Name = "UsernameColumn";
            UsernameColumn.Text = "Username";
            UsernameColumn.Width = 90;
            // 
            // PasswordColumn
            // 
            PasswordColumn.Name = "PasswordColumn";
            PasswordColumn.Text = "Password";
            PasswordColumn.Width = 90;
            // 
            // ResultColumn
            // 
            ResultColumn.Name = "ResultColumn";
            ResultColumn.Text = "Result";
            ResultColumn.Width = 180;
            // 
            // loadingImage
            // 
            loadingImage.Anchor = AnchorStyles.None;
            loadingImage.Location = new Point(4, 3);
            loadingImage.Margin = new Padding(4, 3, 4, 3);
            loadingImage.Name = "loadingImage";
            loadingImage.Size = new Size(26, 27);
            loadingImage.TabIndex = 5;
            loadingImage.TabStop = false;
            // 
            // labelStatus
            // 
            labelStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            labelStatus.AutoEllipsis = true;
            labelStatus.BackColor = SystemColors.Control;
            labelStatus.Location = new Point(38, 2);
            labelStatus.Margin = new Padding(4, 0, 4, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Padding = new Padding(0, 6, 0, 0);
            labelStatus.Size = new Size(637, 28);
            labelStatus.TabIndex = 6;
            // 
            // columnHeader4
            // 
            columnHeader4.Name = "columnHeader4";
            columnHeader4.Text = "Status";
            columnHeader4.Width = 45;
            // 
            // columnHeader5
            // 
            columnHeader5.Name = "columnHeader5";
            columnHeader5.Text = "Username";
            columnHeader5.Width = 90;
            // 
            // columnHeader6
            // 
            columnHeader6.Name = "columnHeader6";
            columnHeader6.Text = "Password";
            columnHeader6.Width = 90;
            // 
            // columnHeader7
            // 
            columnHeader7.Name = "columnHeader7";
            columnHeader7.Text = "Result";
            columnHeader7.Width = 180;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.BackColor = SystemColors.Control;
            flowLayoutPanel1.Controls.Add(loadingImage);
            flowLayoutPanel1.Controls.Add(labelStatus);
            flowLayoutPanel1.Location = new Point(13, 444);
            flowLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(695, 33);
            flowLayoutPanel1.TabIndex = 9;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(884, 486);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(whatsHappening);
            Controls.Add(ButtonStart);
            Controls.Add(tabControl1);
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(900, 525);
            Name = "MainWindow";
            Text = "Account Checker";
            FormClosing += MainWindow_FormClosing;
            Load += MainWindow_Load;
            tabControl1.ResumeLayout(false);
            tabAutomaticCheck.ResumeLayout(false);
            tabAutomaticCheck.PerformLayout();
            groupBox1.ResumeLayout(false);
            tabSettings.ResumeLayout(false);
            tabSettings.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)loadingImage).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TabPage tabAutomaticCheck;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button8;
        public System.Windows.Forms.Button ButtonStart;
        public System.Windows.Forms.Label badLabel;
        public System.Windows.Forms.Label steamGuardLabel;
        public System.Windows.Forms.Label loggableLabel;
        public System.Windows.Forms.TextBox textBoxFile;
        public System.Windows.Forms.Label remainingLabel;
        public System.Windows.Forms.Label checkedLabel;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.CheckBox CheckboxShowColouredItemsInAccountList;
        public System.Windows.Forms.TextBox textBoxLocalToExport;
        public System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        public System.Windows.Forms.ColumnHeader StatusColumn;
        public System.Windows.Forms.ColumnHeader UsernameColumn;
        public System.Windows.Forms.ColumnHeader PasswordColumn;
        public System.Windows.Forms.ColumnHeader ResultColumn;
        public System.Windows.Forms.PictureBox loadingImage;
        public System.Windows.Forms.ListView whatsHappening;
        public System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}