namespace M9Studio.ShadowTalk.Server
{
    partial class FormMain
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
            splitContainer1 = new SplitContainer();
            labelMessageDeleted = new Label();
            labelMessageWaiting = new Label();
            labelMessage = new Label();
            labelUserOnline = new Label();
            labelUser = new Label();
            labelAddress = new Label();
            listBoxLog = new ListBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(labelMessageDeleted);
            splitContainer1.Panel1.Controls.Add(labelMessageWaiting);
            splitContainer1.Panel1.Controls.Add(labelMessage);
            splitContainer1.Panel1.Controls.Add(labelUserOnline);
            splitContainer1.Panel1.Controls.Add(labelUser);
            splitContainer1.Panel1.Controls.Add(labelAddress);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(listBoxLog);
            splitContainer1.Size = new Size(821, 450);
            splitContainer1.SplitterDistance = 273;
            splitContainer1.TabIndex = 0;
            // 
            // labelMessageDeleted
            // 
            labelMessageDeleted.AutoSize = true;
            labelMessageDeleted.Location = new Point(13, 69);
            labelMessageDeleted.Name = "labelMessageDeleted";
            labelMessageDeleted.Size = new Size(118, 15);
            labelMessageDeleted.TabIndex = 11;
            labelMessageDeleted.Text = "labelMessageDeleted";
            // 
            // labelMessageWaiting
            // 
            labelMessageWaiting.AutoSize = true;
            labelMessageWaiting.Location = new Point(12, 54);
            labelMessageWaiting.Name = "labelMessageWaiting";
            labelMessageWaiting.Size = new Size(119, 15);
            labelMessageWaiting.TabIndex = 10;
            labelMessageWaiting.Text = "labelMessageWaiting";
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            labelMessage.Location = new Point(12, 39);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(78, 15);
            labelMessage.TabIndex = 9;
            labelMessage.Text = "labelMessage";
            // 
            // labelUserOnline
            // 
            labelUserOnline.AutoSize = true;
            labelUserOnline.Location = new Point(12, 24);
            labelUserOnline.Name = "labelUserOnline";
            labelUserOnline.Size = new Size(90, 15);
            labelUserOnline.TabIndex = 8;
            labelUserOnline.Text = "labelUserOnline";
            // 
            // labelUser
            // 
            labelUser.AutoSize = true;
            labelUser.Location = new Point(12, 9);
            labelUser.Name = "labelUser";
            labelUser.Size = new Size(55, 15);
            labelUser.TabIndex = 7;
            labelUser.Text = "labelUser";
            // 
            // labelAddress
            // 
            labelAddress.AutoSize = true;
            labelAddress.Location = new Point(13, 118);
            labelAddress.Name = "labelAddress";
            labelAddress.Size = new Size(74, 15);
            labelAddress.TabIndex = 6;
            labelAddress.Text = "labelAddress";
            // 
            // listBoxLog
            // 
            listBoxLog.Dock = DockStyle.Fill;
            listBoxLog.FormattingEnabled = true;
            listBoxLog.IntegralHeight = false;
            listBoxLog.ItemHeight = 15;
            listBoxLog.Location = new Point(0, 0);
            listBoxLog.Name = "listBoxLog";
            listBoxLog.Size = new Size(544, 450);
            listBoxLog.TabIndex = 0;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(821, 450);
            Controls.Add(splitContainer1);
            Name = "FormMain";
            Text = "ShadowTalk Server";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Label labelMessageDeleted;
        private Label labelMessageWaiting;
        private Label labelMessage;
        private Label labelUserOnline;
        private Label labelUser;
        private Label labelAddress;
        private ListBox listBoxLog;
    }
}
