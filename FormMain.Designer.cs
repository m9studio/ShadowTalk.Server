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
            labelAddress = new Label();
            labelUser = new Label();
            labelUserOnline = new Label();
            labelMessage = new Label();
            labelMessageWaiting = new Label();
            labelMessageDeleted = new Label();
            SuspendLayout();
            // 
            // labelAddress
            // 
            labelAddress.AutoSize = true;
            labelAddress.Location = new Point(12, 9);
            labelAddress.Name = "labelAddress";
            labelAddress.Size = new Size(74, 15);
            labelAddress.TabIndex = 0;
            labelAddress.Text = "labelAddress";
            // 
            // labelUser
            // 
            labelUser.AutoSize = true;
            labelUser.Location = new Point(12, 24);
            labelUser.Name = "labelUser";
            labelUser.Size = new Size(55, 15);
            labelUser.TabIndex = 1;
            labelUser.Text = "labelUser";
            // 
            // labelUserOnline
            // 
            labelUserOnline.AutoSize = true;
            labelUserOnline.Location = new Point(12, 39);
            labelUserOnline.Name = "labelUserOnline";
            labelUserOnline.Size = new Size(90, 15);
            labelUserOnline.TabIndex = 2;
            labelUserOnline.Text = "labelUserOnline";
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            labelMessage.Location = new Point(12, 54);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(78, 15);
            labelMessage.TabIndex = 3;
            labelMessage.Text = "labelMessage";
            // 
            // labelMessageWaiting
            // 
            labelMessageWaiting.AutoSize = true;
            labelMessageWaiting.Location = new Point(12, 69);
            labelMessageWaiting.Name = "labelMessageWaiting";
            labelMessageWaiting.Size = new Size(119, 15);
            labelMessageWaiting.TabIndex = 4;
            labelMessageWaiting.Text = "labelMessageWaiting";
            // 
            // labelMessageDeleted
            // 
            labelMessageDeleted.AutoSize = true;
            labelMessageDeleted.Location = new Point(13, 84);
            labelMessageDeleted.Name = "labelMessageDeleted";
            labelMessageDeleted.Size = new Size(118, 15);
            labelMessageDeleted.TabIndex = 5;
            labelMessageDeleted.Text = "labelMessageDeleted";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(labelMessageDeleted);
            Controls.Add(labelMessageWaiting);
            Controls.Add(labelMessage);
            Controls.Add(labelUserOnline);
            Controls.Add(labelUser);
            Controls.Add(labelAddress);
            Name = "FormMain";
            Text = "Server";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelAddress;
        private Label labelUser;
        private Label labelUserOnline;
        private Label labelMessage;
        private Label labelMessageWaiting;
        private Label labelMessageDeleted;
    }
}
