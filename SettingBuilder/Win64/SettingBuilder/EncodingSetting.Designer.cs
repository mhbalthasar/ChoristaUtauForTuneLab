namespace SettingBuilder_win64
{
    partial class EncodingSetting
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
            singerList = new ListBox();
            label2 = new Label();
            txtPath = new TextBox();
            label1 = new Label();
            txtName = new TextBox();
            label3 = new Label();
            txtEncoding = new ComboBox();
            button1 = new Button();
            label4 = new Label();
            overlayName = new TextBox();
            listPhonemizer = new ComboBox();
            label5 = new Label();
            button2 = new Button();
            SuspendLayout();
            // 
            // singerList
            // 
            singerList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            singerList.FormattingEnabled = true;
            singerList.ItemHeight = 17;
            singerList.Location = new Point(12, 12);
            singerList.Name = "singerList";
            singerList.Size = new Size(457, 429);
            singerList.TabIndex = 0;
            singerList.SelectedIndexChanged += singerList_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(475, 12);
            label2.Name = "label2";
            label2.Size = new Size(105, 17);
            label2.TabIndex = 2;
            label2.Text = "Voice bank Path:";
            // 
            // txtPath
            // 
            txtPath.Location = new Point(475, 32);
            txtPath.Multiline = true;
            txtPath.Name = "txtPath";
            txtPath.Size = new Size(313, 143);
            txtPath.TabIndex = 3;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(475, 227);
            label1.Name = "label1";
            label1.Size = new Size(102, 17);
            label1.TabIndex = 2;
            label1.Text = "Detected Name:";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtName.Location = new Point(475, 247);
            txtName.Name = "txtName";
            txtName.Size = new Size(313, 23);
            txtName.TabIndex = 4;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(475, 273);
            label3.Name = "label3";
            label3.Size = new Size(118, 17);
            label3.TabIndex = 2;
            label3.Text = "Selected Encoding:";
            // 
            // txtEncoding
            // 
            txtEncoding.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtEncoding.DropDownStyle = ComboBoxStyle.DropDownList;
            txtEncoding.FormattingEnabled = true;
            txtEncoding.Location = new Point(475, 293);
            txtEncoding.Name = "txtEncoding";
            txtEncoding.Size = new Size(313, 25);
            txtEncoding.TabIndex = 5;
            txtEncoding.SelectedIndexChanged += txtEncoding_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(475, 387);
            button1.Name = "button1";
            button1.Size = new Size(125, 25);
            button1.TabIndex = 6;
            button1.Text = "Save And Reload";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(475, 181);
            label4.Name = "label4";
            label4.Size = new Size(109, 17);
            label4.TabIndex = 2;
            label4.Text = "Overlayed Name:";
            // 
            // overlayName
            // 
            overlayName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            overlayName.Location = new Point(475, 201);
            overlayName.Name = "overlayName";
            overlayName.Size = new Size(313, 23);
            overlayName.TabIndex = 4;
            // 
            // listPhonemizer
            // 
            listPhonemizer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            listPhonemizer.FormattingEnabled = true;
            listPhonemizer.Location = new Point(475, 345);
            listPhonemizer.Name = "listPhonemizer";
            listPhonemizer.Size = new Size(313, 25);
            listPhonemizer.TabIndex = 5;
            listPhonemizer.SelectedIndexChanged += txtEncoding_SelectedIndexChanged;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(475, 326);
            label5.Name = "label5";
            label5.Size = new Size(79, 17);
            label5.TabIndex = 7;
            label5.Text = "Phonemizer:";
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.Location = new Point(606, 387);
            button2.Name = "button2";
            button2.Size = new Size(125, 25);
            button2.TabIndex = 6;
            button2.Text = "Rebuild OtoCache";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // EncodingSetting
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label5);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(listPhonemizer);
            Controls.Add(txtEncoding);
            Controls.Add(overlayName);
            Controls.Add(txtName);
            Controls.Add(txtPath);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(singerList);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MinimizeBox = false;
            Name = "EncodingSetting";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "EncodingSetting";
            Load += EncodingSetting_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox singerList;
        private Label label2;
        private TextBox txtPath;
        private Label label1;
        private TextBox txtName;
        private Label label3;
        private ComboBox txtEncoding;
        private Button button1;
        private Label label4;
        private TextBox overlayName;
        private ComboBox listPhonemizer;
        private Label label5;
        private Button button2;
    }
}