namespace Server_Application_CS408
{
    partial class Server_Application
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server_Application));
            textBox_IP = new TextBox();
            label_IP = new Label();
            label_Port = new Label();
            textBox_Port = new TextBox();
            button_ServerStart = new Button();
            label_IF100 = new Label();
            richTextBox_IF100 = new RichTextBox();
            richTextBox_SPS101 = new RichTextBox();
            label_SPS101 = new Label();
            richTextBox_AllChannels = new RichTextBox();
            label_All = new Label();
            richTextBox_Actions = new RichTextBox();
            label_Actions = new Label();
            label_Subscribe = new Label();
            SuspendLayout();
            // 
            // textBox_IP
            // 
            textBox_IP.Location = new Point(198, 68);
            textBox_IP.Name = "textBox_IP";
            textBox_IP.Size = new Size(173, 31);
            textBox_IP.TabIndex = 0;
            // 
            // label_IP
            // 
            label_IP.AutoSize = true;
            label_IP.Location = new Point(78, 74);
            label_IP.Name = "label_IP";
            label_IP.Size = new Size(31, 25);
            label_IP.TabIndex = 1;
            label_IP.Text = "IP:";
            // 
            // label_Port
            // 
            label_Port.AutoSize = true;
            label_Port.Location = new Point(37, 128);
            label_Port.Name = "label_Port";
            label_Port.Size = new Size(118, 25);
            label_Port.TabIndex = 3;
            label_Port.Text = "Port Number:";
            // 
            // textBox_Port
            // 
            textBox_Port.Location = new Point(198, 122);
            textBox_Port.Name = "textBox_Port";
            textBox_Port.Size = new Size(173, 31);
            textBox_Port.TabIndex = 4;
            // 
            // button_ServerStart
            // 
            button_ServerStart.Location = new Point(433, 81);
            button_ServerStart.Name = "button_ServerStart";
            button_ServerStart.Size = new Size(174, 55);
            button_ServerStart.TabIndex = 5;
            button_ServerStart.Text = "Start To Server";
            button_ServerStart.UseVisualStyleBackColor = true;
            button_ServerStart.Click += button_ServerStart_Click;
            // 
            // label_IF100
            // 
            label_IF100.AutoSize = true;
            label_IF100.Location = new Point(126, 205);
            label_IF100.Name = "label_IF100";
            label_IF100.Size = new Size(56, 25);
            label_IF100.TabIndex = 6;
            label_IF100.Text = "IF100";
            // 
            // richTextBox_IF100
            // 
            richTextBox_IF100.Location = new Point(53, 233);
            richTextBox_IF100.Name = "richTextBox_IF100";
            richTextBox_IF100.ReadOnly = true;
            richTextBox_IF100.Size = new Size(213, 386);
            richTextBox_IF100.TabIndex = 7;
            richTextBox_IF100.Text = "";
            // 
            // richTextBox_SPS101
            // 
            richTextBox_SPS101.Location = new Point(316, 233);
            richTextBox_SPS101.Name = "richTextBox_SPS101";
            richTextBox_SPS101.ReadOnly = true;
            richTextBox_SPS101.Size = new Size(213, 386);
            richTextBox_SPS101.TabIndex = 9;
            richTextBox_SPS101.Text = "";
            // 
            // label_SPS101
            // 
            label_SPS101.AutoSize = true;
            label_SPS101.Location = new Point(389, 205);
            label_SPS101.Name = "label_SPS101";
            label_SPS101.Size = new Size(72, 25);
            label_SPS101.TabIndex = 8;
            label_SPS101.Text = "SPS101";
            // 
            // richTextBox_AllChannels
            // 
            richTextBox_AllChannels.Location = new Point(579, 233);
            richTextBox_AllChannels.Name = "richTextBox_AllChannels";
            richTextBox_AllChannels.ReadOnly = true;
            richTextBox_AllChannels.Size = new Size(213, 386);
            richTextBox_AllChannels.TabIndex = 11;
            richTextBox_AllChannels.Text = "";
            // 
            // label_All
            // 
            label_All.AutoSize = true;
            label_All.Location = new Point(609, 205);
            label_All.Name = "label_All";
            label_All.Size = new Size(145, 25);
            label_All.TabIndex = 10;
            label_All.Text = "Connected Users";
            // 
            // richTextBox_Actions
            // 
            richTextBox_Actions.Location = new Point(912, 108);
            richTextBox_Actions.Name = "richTextBox_Actions";
            richTextBox_Actions.ReadOnly = true;
            richTextBox_Actions.Size = new Size(323, 511);
            richTextBox_Actions.TabIndex = 13;
            richTextBox_Actions.Text = "";
            // 
            // label_Actions
            // 
            label_Actions.AutoSize = true;
            label_Actions.Location = new Point(1042, 80);
            label_Actions.Name = "label_Actions";
            label_Actions.Size = new Size(71, 25);
            label_Actions.TabIndex = 12;
            label_Actions.Text = "Actions";
            // 
            // label_Subscribe
            // 
            label_Subscribe.AutoSize = true;
            label_Subscribe.Location = new Point(207, 180);
            label_Subscribe.Name = "label_Subscribe";
            label_Subscribe.Size = new Size(164, 25);
            label_Subscribe.TabIndex = 14;
            label_Subscribe.Text = "Subscription Status";
            // 
            // Server_Application
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 704);
            Controls.Add(label_Subscribe);
            Controls.Add(richTextBox_Actions);
            Controls.Add(label_Actions);
            Controls.Add(richTextBox_AllChannels);
            Controls.Add(label_All);
            Controls.Add(richTextBox_SPS101);
            Controls.Add(label_SPS101);
            Controls.Add(richTextBox_IF100);
            Controls.Add(label_IF100);
            Controls.Add(button_ServerStart);
            Controls.Add(textBox_Port);
            Controls.Add(label_Port);
            Controls.Add(label_IP);
            Controls.Add(textBox_IP);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Server_Application";
            Text = "Server Application";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox_IP;
        private Label label_IP;
        private Label label_Port;
        private TextBox textBox_Port;
        private Button button_ServerStart;
        private Label label_IF100;
        private RichTextBox richTextBox_IF100;
        private RichTextBox richTextBox_SPS101;
        private Label label_SPS101;
        private RichTextBox richTextBox_AllChannels;
        private Label label_All;
        private RichTextBox richTextBox_Actions;
        private Label label_Actions;
        private Label label_Subscribe;
    }
}