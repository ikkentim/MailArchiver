namespace DecMailBundle
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            labelStatusText = new Label();
            textBoxPath = new TextBox();
            buttonPathBrowse = new Button();
            checkBoxCurrentDate = new CheckBox();
            labelStatusLabel = new Label();
            SuspendLayout();
            // 
            // labelStatusText
            // 
            labelStatusText.AutoSize = true;
            labelStatusText.Location = new Point(60, 28);
            labelStatusText.Name = "labelStatusText";
            labelStatusText.Size = new Size(39, 15);
            labelStatusText.TabIndex = 0;
            labelStatusText.Text = "Ready";
            // 
            // textBoxPath
            // 
            textBoxPath.Location = new Point(12, 93);
            textBoxPath.Name = "textBoxPath";
            textBoxPath.Size = new Size(280, 23);
            textBoxPath.TabIndex = 1;
            // 
            // buttonPathBrowse
            // 
            buttonPathBrowse.Location = new Point(298, 93);
            buttonPathBrowse.Name = "buttonPathBrowse";
            buttonPathBrowse.Size = new Size(75, 23);
            buttonPathBrowse.TabIndex = 2;
            buttonPathBrowse.Text = "Browse...";
            buttonPathBrowse.UseVisualStyleBackColor = true;
            // 
            // checkBoxCurrentDate
            // 
            checkBoxCurrentDate.AutoSize = true;
            checkBoxCurrentDate.Location = new Point(12, 122);
            checkBoxCurrentDate.Name = "checkBoxCurrentDate";
            checkBoxCurrentDate.Size = new Size(219, 19);
            checkBoxCurrentDate.TabIndex = 3;
            checkBoxCurrentDate.Text = "Use current date instead of mail date";
            checkBoxCurrentDate.UseVisualStyleBackColor = true;
            // 
            // labelStatusLabel
            // 
            labelStatusLabel.AutoSize = true;
            labelStatusLabel.Location = new Point(12, 28);
            labelStatusLabel.Name = "labelStatusLabel";
            labelStatusLabel.Size = new Size(42, 15);
            labelStatusLabel.TabIndex = 4;
            labelStatusLabel.Text = "Status:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(650, 338);
            Controls.Add(labelStatusLabel);
            Controls.Add(checkBoxCurrentDate);
            Controls.Add(buttonPathBrowse);
            Controls.Add(textBoxPath);
            Controls.Add(labelStatusText);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Mail Archiver";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelStatusText;
        private TextBox textBoxPath;
        private Button buttonPathBrowse;
        private CheckBox checkBoxCurrentDate;
        private Label labelStatusLabel;
    }
}
