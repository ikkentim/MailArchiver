namespace DecMailBundle
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            checkBoxCurrentDate = new CheckBox();
            buttonPathBrowse = new Button();
            textBoxPath = new TextBox();
            groupBox1 = new GroupBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            buttonClose = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // checkBoxCurrentDate
            // 
            checkBoxCurrentDate.AutoSize = true;
            checkBoxCurrentDate.Location = new Point(6, 22);
            checkBoxCurrentDate.Name = "checkBoxCurrentDate";
            checkBoxCurrentDate.Size = new Size(219, 19);
            checkBoxCurrentDate.TabIndex = 6;
            checkBoxCurrentDate.Text = "Use current date instead of mail date";
            checkBoxCurrentDate.UseVisualStyleBackColor = true;
            // 
            // buttonPathBrowse
            // 
            buttonPathBrowse.Location = new Point(359, 22);
            buttonPathBrowse.Name = "buttonPathBrowse";
            buttonPathBrowse.Size = new Size(75, 23);
            buttonPathBrowse.TabIndex = 5;
            buttonPathBrowse.Text = "&Browse...";
            buttonPathBrowse.UseVisualStyleBackColor = true;
            // 
            // textBoxPath
            // 
            textBoxPath.Location = new Point(63, 22);
            textBoxPath.Name = "textBoxPath";
            textBoxPath.Size = new Size(290, 23);
            textBoxPath.TabIndex = 4;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBoxPath);
            groupBox1.Controls.Add(buttonPathBrowse);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(562, 103);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Archive";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 25);
            label1.Name = "label1";
            label1.Size = new Size(34, 15);
            label1.TabIndex = 6;
            label1.Text = "Path:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(checkBoxCurrentDate);
            groupBox2.Location = new Point(12, 121);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(562, 89);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Options";
            // 
            // buttonClose
            // 
            buttonClose.Location = new Point(499, 216);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(75, 23);
            buttonClose.TabIndex = 9;
            buttonClose.Text = "&Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // SettingsForm
            // 
            AcceptButton = buttonClose;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new Size(586, 251);
            Controls.Add(buttonClose);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SettingsForm";
            Text = "Settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CheckBox checkBoxCurrentDate;
        private Button buttonPathBrowse;
        private TextBox textBoxPath;
        private GroupBox groupBox1;
        private Label label1;
        private GroupBox groupBox2;
        private Button buttonClose;
    }
}