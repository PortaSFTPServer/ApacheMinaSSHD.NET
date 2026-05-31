namespace SimpleSSHDSever
{
    partial class frmMain
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
            btnStart = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            richTextBox1 = new RichTextBox();
            listView1 = new ListView();
            label1 = new Label();
            checkBox1 = new CheckBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStart.Location = new Point(893, 15);
            btnStart.Margin = new Padding(4, 2, 4, 2);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(209, 51);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Location = new Point(15, 124);
            tabControl1.Margin = new Padding(4, 2, 4, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1086, 448);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(richTextBox1);
            tabPage1.Controls.Add(listView1);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Margin = new Padding(4, 2, 4, 2);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 2, 4, 2);
            tabPage1.Size = new Size(1078, 410);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Logs";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(4, 2);
            richTextBox1.Margin = new Padding(4, 5, 4, 5);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(1070, 406);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            // 
            // listView1
            // 
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(4, 2);
            listView1.Margin = new Padding(4, 5, 4, 5);
            listView1.Name = "listView1";
            listView1.Size = new Size(1070, 406);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(20, 28);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(335, 38);
            label1.TabIndex = 2;
            label1.Text = "Simple Mina SSHD Server";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(676, 27);
            checkBox1.Margin = new Padding(4, 2, 4, 2);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(157, 29);
            checkBox1.TabIndex = 3;
            checkBox1.Text = "FIPS Algorithm";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1116, 592);
            Controls.Add(checkBox1);
            Controls.Add(label1);
            Controls.Add(tabControl1);
            Controls.Add(btnStart);
            Margin = new Padding(4, 2, 4, 2);
            Name = "frmMain";
            Text = "Simple Apache Mina SSHD Server";
            Load += frmMain_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button btnStart;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private Label label1;
        private CheckBox checkBox1;
        private ListView listView1;
        private RichTextBox richTextBox1;
    }
}
