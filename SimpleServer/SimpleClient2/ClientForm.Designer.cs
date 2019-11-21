namespace SimpleClient2
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
            this.Chat = new System.Windows.Forms.RichTextBox();
            this.InputMessage = new System.Windows.Forms.RichTextBox();
            this.SubmitButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Chat
            // 
            this.Chat.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Chat.Location = new System.Drawing.Point(87, 12);
            this.Chat.Name = "Chat";
            this.Chat.Size = new System.Drawing.Size(461, 274);
            this.Chat.TabIndex = 0;
            this.Chat.Text = "";
            this.Chat.TextChanged += new System.EventHandler(this.RichTextBox1_TextChanged);
            // 
            // InputMessage
            // 
            this.InputMessage.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.InputMessage.Location = new System.Drawing.Point(87, 345);
            this.InputMessage.Name = "InputMessage";
            this.InputMessage.Size = new System.Drawing.Size(461, 44);
            this.InputMessage.TabIndex = 1;
            this.InputMessage.Text = "";
            // 
            // SubmitButton
            // 
            this.SubmitButton.Location = new System.Drawing.Point(572, 366);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(75, 23);
            this.SubmitButton.TabIndex = 2;
            this.SubmitButton.Text = "Send";
            this.SubmitButton.UseVisualStyleBackColor = true;
            this.SubmitButton.Click += new System.EventHandler(this.Button1_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.SubmitButton);
            this.Controls.Add(this.InputMessage);
            this.Controls.Add(this.Chat);
            this.Name = "ClientForm";
            this.Text = "ClientForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClientForm_FormClosed);
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox Chat;
        private System.Windows.Forms.RichTextBox InputMessage;
        private System.Windows.Forms.Button SubmitButton;
    }
}