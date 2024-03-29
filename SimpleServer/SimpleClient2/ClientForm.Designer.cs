﻿namespace SimpleClient2
{
    partial class ClientForm
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
            this.chatBox = new System.Windows.Forms.RichTextBox();
            this.InputMessage = new System.Windows.Forms.RichTextBox();
            this.SubmitButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chatBox
            // 
            this.chatBox.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.chatBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.chatBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.chatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.chatBox.ForeColor = System.Drawing.Color.RoyalBlue;
            this.chatBox.Location = new System.Drawing.Point(12, 50);
            this.chatBox.Name = "chatBox";
            this.chatBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.chatBox.Size = new System.Drawing.Size(285, 280);
            this.chatBox.TabIndex = 0;
            this.chatBox.Text = "";
            this.chatBox.TextChanged += new System.EventHandler(this.OutputTextBoxChanged);
            // 
            // InputMessage
            // 
            this.InputMessage.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.InputMessage.Location = new System.Drawing.Point(12, 336);
            this.InputMessage.Name = "InputMessage";
            this.InputMessage.Size = new System.Drawing.Size(285, 23);
            this.InputMessage.TabIndex = 1;
            this.InputMessage.Text = "";
            this.InputMessage.TextChanged += new System.EventHandler(this.InputMessageBoxChanged);
            // 
            // SubmitButton
            // 
            this.SubmitButton.Location = new System.Drawing.Point(303, 336);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(102, 23);
            this.SubmitButton.TabIndex = 2;
            this.SubmitButton.Text = "Send";
            this.SubmitButton.UseVisualStyleBackColor = true;
            this.SubmitButton.Click += new System.EventHandler(this.SendButtonPressed);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(303, 307);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Set-Nickname";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.NickNameButtonPressed);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(304, 50);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(101, 21);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBoxChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(302, 86);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(103, 44);
            this.button2.TabIndex = 5;
            this.button2.Text = "Connect To Server";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ConnectButtonPressed);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(303, 177);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(102, 63);
            this.button3.TabIndex = 6;
            this.button3.Text = "Start Noughts + Crosses";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(418, 397);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.SubmitButton);
            this.Controls.Add(this.InputMessage);
            this.Controls.Add(this.chatBox);
            this.Name = "ClientForm";
            this.Text = "ClientForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClientForm_FormClosed);
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox chatBox;
        private System.Windows.Forms.RichTextBox InputMessage;
        private System.Windows.Forms.Button SubmitButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}