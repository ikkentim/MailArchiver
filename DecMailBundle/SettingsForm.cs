namespace DecMailBundle
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                textBoxPath.Text = AppSettings.Default.Path;
                checkBoxCurrentDate.Checked = AppSettings.Default.UseCurrentDate;

                textBoxPath.TextChanged += (sender, e) =>
                {
                    AppSettings.Default.Path = textBoxPath.Text;
                    AppSettings.Default.Save();
                };
                checkBoxCurrentDate.CheckedChanged += (sender, e) =>
                {
                    AppSettings.Default.UseCurrentDate = checkBoxCurrentDate.Checked;
                    AppSettings.Default.Save();
                };

                buttonPathBrowse.Click += ButtonPathBrowse_Click;
            }
        }

        private void ButtonPathBrowse_Click(object? sender, EventArgs e)
        {
            using var folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select the archive folder";
            folderBrowser.SelectedPath = textBoxPath.Text;
            if (folderBrowser.ShowDialog(this) == DialogResult.OK)
            {
                textBoxPath.Text = folderBrowser.SelectedPath;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
