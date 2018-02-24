namespace WindowsFormsDemo
{
    partial class Recording
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
            this.label3 = new System.Windows.Forms.Label();
            this.mediaTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.streamComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.sourceComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.encoderComboBox = new System.Windows.Forms.ComboBox();
            this.encoderModeComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.compressedMediaTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.sinkComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.formatComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.mDo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(351, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(206, 29);
            this.label3.TabIndex = 11;
            this.label3.Text = "OutputMediaType";
            // 
            // mediaTypeComboBox
            // 
            this.mediaTypeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mediaTypeComboBox.FormattingEnabled = true;
            this.mediaTypeComboBox.Location = new System.Drawing.Point(356, 40);
            this.mediaTypeComboBox.Name = "mediaTypeComboBox";
            this.mediaTypeComboBox.Size = new System.Drawing.Size(311, 28);
            this.mediaTypeComboBox.TabIndex = 10;
            this.mediaTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.mediaTypeComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(181, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 29);
            this.label2.TabIndex = 9;
            this.label2.Text = "Stream:";
            // 
            // streamComboBox
            // 
            this.streamComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.streamComboBox.FormattingEnabled = true;
            this.streamComboBox.Location = new System.Drawing.Point(186, 40);
            this.streamComboBox.Name = "streamComboBox";
            this.streamComboBox.Size = new System.Drawing.Size(164, 28);
            this.streamComboBox.TabIndex = 8;
            this.streamComboBox.SelectedIndexChanged += new System.EventHandler(this.streamComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 29);
            this.label1.TabIndex = 7;
            this.label1.Text = "Source:";
            // 
            // sourceComboBox
            // 
            this.sourceComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sourceComboBox.FormattingEnabled = true;
            this.sourceComboBox.Location = new System.Drawing.Point(9, 40);
            this.sourceComboBox.Name = "sourceComboBox";
            this.sourceComboBox.Size = new System.Drawing.Size(171, 28);
            this.sourceComboBox.TabIndex = 6;
            this.sourceComboBox.SelectedIndexChanged += new System.EventHandler(this.sourceComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(4, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 29);
            this.label4.TabIndex = 12;
            this.label4.Text = "Encoder:";
            // 
            // encoderComboBox
            // 
            this.encoderComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.encoderComboBox.FormattingEnabled = true;
            this.encoderComboBox.Location = new System.Drawing.Point(9, 123);
            this.encoderComboBox.Name = "encoderComboBox";
            this.encoderComboBox.Size = new System.Drawing.Size(207, 28);
            this.encoderComboBox.TabIndex = 13;
            this.encoderComboBox.SelectedIndexChanged += new System.EventHandler(this.encoderComboBox_SelectedIndexChanged);
            // 
            // encoderModeComboBox
            // 
            this.encoderModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.encoderModeComboBox.FormattingEnabled = true;
            this.encoderModeComboBox.Location = new System.Drawing.Point(222, 123);
            this.encoderModeComboBox.Name = "encoderModeComboBox";
            this.encoderModeComboBox.Size = new System.Drawing.Size(167, 28);
            this.encoderModeComboBox.TabIndex = 14;
            this.encoderModeComboBox.SelectedIndexChanged += new System.EventHandler(this.encoderModeComboBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(217, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(172, 29);
            this.label5.TabIndex = 15;
            this.label5.Text = "EncoderMode:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // compressedMediaTypeComboBox
            // 
            this.compressedMediaTypeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compressedMediaTypeComboBox.FormattingEnabled = true;
            this.compressedMediaTypeComboBox.Location = new System.Drawing.Point(395, 123);
            this.compressedMediaTypeComboBox.Name = "compressedMediaTypeComboBox";
            this.compressedMediaTypeComboBox.Size = new System.Drawing.Size(272, 28);
            this.compressedMediaTypeComboBox.TabIndex = 16;
            this.compressedMediaTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.compressedMediaTypeComboBox_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(390, 91);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(280, 29);
            this.label6.TabIndex = 17;
            this.label6.Text = "CompressedMediaType:";
            // 
            // sinkComboBox
            // 
            this.sinkComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sinkComboBox.FormattingEnabled = true;
            this.sinkComboBox.Location = new System.Drawing.Point(9, 196);
            this.sinkComboBox.Name = "sinkComboBox";
            this.sinkComboBox.Size = new System.Drawing.Size(203, 28);
            this.sinkComboBox.TabIndex = 18;
            this.sinkComboBox.SelectedIndexChanged += new System.EventHandler(this.sinkComboBox_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(4, 164);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 29);
            this.label7.TabIndex = 19;
            this.label7.Text = "Sink:";
            // 
            // formatComboBox
            // 
            this.formatComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.formatComboBox.FormattingEnabled = true;
            this.formatComboBox.Location = new System.Drawing.Point(218, 196);
            this.formatComboBox.Name = "formatComboBox";
            this.formatComboBox.Size = new System.Drawing.Size(171, 28);
            this.formatComboBox.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(213, 164);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 29);
            this.label8.TabIndex = 21;
            this.label8.Text = "Format:";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(395, 196);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(268, 28);
            this.button1.TabIndex = 22;
            this.button1.Text = "Save file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // mDo
            // 
            this.mDo.Enabled = false;
            this.mDo.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mDo.Location = new System.Drawing.Point(218, 256);
            this.mDo.Name = "mDo";
            this.mDo.Size = new System.Drawing.Size(273, 47);
            this.mDo.TabIndex = 23;
            this.mDo.Text = "Start";
            this.mDo.UseVisualStyleBackColor = true;
            this.mDo.Click += new System.EventHandler(this.mDo_Click);
            // 
            // Recording
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 315);
            this.Controls.Add(this.mDo);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.formatComboBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.sinkComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.compressedMediaTypeComboBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.encoderModeComboBox);
            this.Controls.Add(this.encoderComboBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.mediaTypeComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.streamComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sourceComboBox);
            this.Name = "Recording";
            this.Text = "Recording";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox mediaTypeComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox streamComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox sourceComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox encoderComboBox;
        private System.Windows.Forms.ComboBox encoderModeComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox compressedMediaTypeComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox sinkComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox formatComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button mDo;
    }
}