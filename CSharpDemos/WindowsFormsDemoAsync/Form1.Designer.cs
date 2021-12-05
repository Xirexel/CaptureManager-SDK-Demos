namespace WindowsFormsDemoAsync
{
    partial class WebViewer
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
            this.sourceComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.streamComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.mediaTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.start_stopBtn = new System.Windows.Forms.Button();
            this.mVideoPanel = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.sourceTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.dSCrossbarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sourceComboBox
            // 
            this.sourceComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sourceComboBox.FormattingEnabled = true;
            this.sourceComboBox.Location = new System.Drawing.Point(5, 96);
            this.sourceComboBox.Name = "sourceComboBox";
            this.sourceComboBox.Size = new System.Drawing.Size(171, 28);
            this.sourceComboBox.TabIndex = 0;
            this.sourceComboBox.SelectedIndexChanged += new System.EventHandler(this.sourceComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 29);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sources:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // streamComboBox
            // 
            this.streamComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.streamComboBox.FormattingEnabled = true;
            this.streamComboBox.Location = new System.Drawing.Point(182, 96);
            this.streamComboBox.Name = "streamComboBox";
            this.streamComboBox.Size = new System.Drawing.Size(164, 28);
            this.streamComboBox.TabIndex = 2;
            this.streamComboBox.SelectedIndexChanged += new System.EventHandler(this.streamComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(177, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 29);
            this.label2.TabIndex = 3;
            this.label2.Text = "Streams";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // mediaTypeComboBox
            // 
            this.mediaTypeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mediaTypeComboBox.FormattingEnabled = true;
            this.mediaTypeComboBox.Location = new System.Drawing.Point(352, 96);
            this.mediaTypeComboBox.Name = "mediaTypeComboBox";
            this.mediaTypeComboBox.Size = new System.Drawing.Size(311, 28);
            this.mediaTypeComboBox.TabIndex = 4;
            this.mediaTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.mediaTypeComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(347, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(206, 29);
            this.label3.TabIndex = 5;
            this.label3.Text = "OutputMediaType";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // start_stopBtn
            // 
            this.start_stopBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.start_stopBtn.Location = new System.Drawing.Point(215, 130);
            this.start_stopBtn.Name = "start_stopBtn";
            this.start_stopBtn.Size = new System.Drawing.Size(265, 49);
            this.start_stopBtn.TabIndex = 6;
            this.start_stopBtn.Text = "Start";
            this.start_stopBtn.UseVisualStyleBackColor = true;
            this.start_stopBtn.Click += new System.EventHandler(this.start_stopBtn_Click);
            // 
            // mVideoPanel
            // 
            this.mVideoPanel.Location = new System.Drawing.Point(12, 185);
            this.mVideoPanel.Name = "mVideoPanel";
            this.mVideoPanel.Size = new System.Drawing.Size(658, 216);
            this.mVideoPanel.TabIndex = 7;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sourceTypeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(682, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // sourceTypeToolStripMenuItem
            // 
            this.sourceTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.dSCrossbarToolStripMenuItem});
            this.sourceTypeToolStripMenuItem.Name = "sourceTypeToolStripMenuItem";
            this.sourceTypeToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.sourceTypeToolStripMenuItem.Text = "Source Type";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Checked = true;
            this.toolStripMenuItem1.CheckOnClick = true;
            this.toolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem1.Text = "Webcam";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click_1);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Checked = true;
            this.toolStripMenuItem2.CheckOnClick = true;
            this.toolStripMenuItem2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem2.Text = "Screen";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // dSCrossbarToolStripMenuItem
            // 
            this.dSCrossbarToolStripMenuItem.Checked = true;
            this.dSCrossbarToolStripMenuItem.CheckOnClick = true;
            this.dSCrossbarToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dSCrossbarToolStripMenuItem.Name = "dSCrossbarToolStripMenuItem";
            this.dSCrossbarToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.dSCrossbarToolStripMenuItem.Text = "DS Crossbar";
            this.dSCrossbarToolStripMenuItem.Click += new System.EventHandler(this.dSCrossbarToolStripMenuItem_Click);
            // 
            // WebViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(682, 416);
            this.Controls.Add(this.mVideoPanel);
            this.Controls.Add(this.start_stopBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.mediaTypeComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.streamComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sourceComboBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "WebViewer";
            this.Text = "WebViewer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox sourceComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox streamComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox mediaTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button start_stopBtn;
        private System.Windows.Forms.Panel mVideoPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem sourceTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem dSCrossbarToolStripMenuItem;
    }
}

