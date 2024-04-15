namespace QuantGate.WindowsFormsExample
{
    partial class Form1
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
            lvSearch = new ListView();
            hdrSymbol = new ColumnHeader();
            hdrName = new ColumnHeader();
            hdrProgress = new ColumnHeader();
            txtSearch = new TextBox();
            btnConnect = new Button();
            btnDisconnect = new Button();
            label1 = new Label();
            label2 = new Label();
            txtPassword = new TextBox();
            txtUsername = new TextBox();
            SuspendLayout();
            // 
            // lvSearch
            // 
            lvSearch.Columns.AddRange(new ColumnHeader[] { hdrSymbol, hdrName, hdrProgress });
            lvSearch.Location = new Point(550, 80);
            lvSearch.Margin = new Padding(4, 3, 4, 3);
            lvSearch.Name = "lvSearch";
            lvSearch.Size = new Size(328, 354);
            lvSearch.TabIndex = 0;
            lvSearch.UseCompatibleStateImageBehavior = false;
            lvSearch.View = View.Details;
            // 
            // hdrSymbol
            // 
            hdrSymbol.Text = "Symbol";
            // 
            // hdrName
            // 
            hdrName.Text = "Name";
            // 
            // hdrProgress
            // 
            hdrProgress.Text = "Progress";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(550, 50);
            txtSearch.Margin = new Padding(4, 3, 4, 3);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(328, 23);
            txtSearch.TabIndex = 1;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(257, 102);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(75, 23);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += HandleConnectClicked;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new Point(348, 102);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(75, 23);
            btnDisconnect.TabIndex = 3;
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += HandleDisconnectClicked;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(61, 50);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 4;
            label1.Text = "Username";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(255, 50);
            label2.Name = "label2";
            label2.Size = new Size(57, 15);
            label2.TabIndex = 5;
            label2.Text = "Password";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(257, 68);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(166, 23);
            txtPassword.TabIndex = 6;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(61, 68);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(190, 23);
            txtUsername.TabIndex = 7;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(933, 519);
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnDisconnect);
            Controls.Add(btnConnect);
            Controls.Add(txtSearch);
            Controls.Add(lvSearch);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView lvSearch;
        private System.Windows.Forms.ColumnHeader hdrSymbol;
        private System.Windows.Forms.ColumnHeader hdrName;
        private System.Windows.Forms.ColumnHeader hdrProgress;
        private System.Windows.Forms.TextBox txtSearch;
        private Button btnConnect;
        private Button btnDisconnect;
        private Label label1;
        private Label label2;
        private TextBox txtPassword;
        private TextBox txtUsername;
    }
}