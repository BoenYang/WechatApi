﻿namespace WindowsFormsApplication1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.start_btn = new System.Windows.Forms.Button();
            this.qrcode_img = new System.Windows.Forms.PictureBox();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.info_display = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.qrcode_img)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // start_btn
            // 
            this.start_btn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.start_btn.Location = new System.Drawing.Point(137, 312);
            this.start_btn.MinimumSize = new System.Drawing.Size(109, 44);
            this.start_btn.Name = "start_btn";
            this.start_btn.Size = new System.Drawing.Size(109, 44);
            this.start_btn.TabIndex = 0;
            this.start_btn.Text = "开始";
            this.start_btn.UseVisualStyleBackColor = true;
            this.start_btn.Click += new System.EventHandler(this.startbtn_Click);
            // 
            // qrcode_img
            // 
            this.qrcode_img.Location = new System.Drawing.Point(12, 3);
            this.qrcode_img.Name = "qrcode_img";
            this.qrcode_img.Size = new System.Drawing.Size(223, 235);
            this.qrcode_img.TabIndex = 1;
            this.qrcode_img.TabStop = false;
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // info_display
            // 
            this.info_display.Location = new System.Drawing.Point(12, 373);
            this.info_display.MinimumSize = new System.Drawing.Size(301, 65);
            this.info_display.Name = "info_display";
            this.info_display.Size = new System.Drawing.Size(336, 124);
            this.info_display.TabIndex = 2;
            this.info_display.Text = "点击开始按钮查找谁删除了你";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 533);
            this.Controls.Add(this.info_display);
            this.Controls.Add(this.start_btn);
            this.Controls.Add(this.qrcode_img);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(332, 461);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "微信清粉";
            ((System.ComponentModel.ISupportInitialize)(this.qrcode_img)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button start_btn;
        private System.Windows.Forms.PictureBox qrcode_img;
        private System.Diagnostics.EventLog eventLog1;
        private System.Windows.Forms.RichTextBox info_display;
       
    }
}

