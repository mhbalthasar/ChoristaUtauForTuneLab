namespace SettingBuilder
{
    partial class MainForm
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
            searchDirs = new ListBox();
            label1 = new Label();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            label2 = new Label();
            button4 = new Button();
            lab_size = new Label();
            SuspendLayout();
            // 
            // searchDirs
            // 
            searchDirs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            searchDirs.FormattingEnabled = true;
            searchDirs.ItemHeight = 17;
            searchDirs.Location = new Point(12, 29);
            searchDirs.Name = "searchDirs";
            searchDirs.Size = new Size(776, 310);
            searchDirs.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(162, 17);
            label1.TabIndex = 1;
            label1.Text = "VoiceBank Search Folders:";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button1.Location = new Point(12, 341);
            button1.Name = "button1";
            button1.Size = new Size(102, 28);
            button1.TabIndex = 2;
            button1.Text = "Add Folder";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button2.Location = new Point(120, 341);
            button2.Name = "button2";
            button2.Size = new Size(114, 28);
            button2.TabIndex = 2;
            button2.Text = "Remove Folder";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button3.Location = new Point(240, 341);
            button3.Name = "button3";
            button3.Size = new Size(114, 28);
            button3.TabIndex = 2;
            button3.Text = "Refresh";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(12, 386);
            label2.Name = "label2";
            label2.Size = new Size(149, 17);
            label2.TabIndex = 1;
            label2.Text = "Render Cache Manager:";
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button4.Location = new Point(12, 406);
            button4.Name = "button4";
            button4.Size = new Size(102, 28);
            button4.TabIndex = 2;
            button4.Text = "Clean All";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // lab_size
            // 
            lab_size.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lab_size.AutoSize = true;
            lab_size.Location = new Point(167, 386);
            lab_size.Name = "lab_size";
            lab_size.Size = new Size(77, 17);
            lab_size.TabIndex = 1;
            lab_size.Text = "Size Calcing";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 447);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button4);
            Controls.Add(button1);
            Controls.Add(lab_size);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(searchDirs);
            Name = "MainForm";
            Text = "ChoristaUtau Settings (Windows)";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox searchDirs;
        private Label label1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Label label2;
        private Button button4;
        private Label lab_size;
    }
}
