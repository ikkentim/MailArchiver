namespace DecMailBundle
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Text = "Mail Archiver";
            AllowDrop = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            // Check if the data is a file
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                if (files.Any(x => x.EndsWith(".eml", true, null)))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }

            e.Effect = DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            // Get the dropped files
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                BringToFrontAndFocus();

                var paths = (string[])e.Data.GetData(DataFormats.FileDrop)!;

                // Handle the files as needed
                foreach (var path in paths)
                {
                    try
                    {
                        if (path.EndsWith(".eml", true, null))
                        {
                            if (HandleFile(path)) return;
                        }
                    }
                    catch (Exception ex)
                    {
                        labelStatusText.Text = ex.ToString();
                    }
                }
            }
        }

        private bool HandleFile(string path)
        {
            if(!Directory.Exists(textBoxPath.Text))
            {
                labelStatusText.Text = $"The folder '{textBoxPath.Text}' does not exist.";
                return true;
            }

            labelStatusText.Text = "Processing...";

            _ = Task.Run(() =>
            {
                string? result;

                try
                {
                    var archiver = new Archiver(textBoxPath.Text, checkBoxCurrentDate.Checked);
                    result = archiver.ConvertEmlFileToPdfInArchive(path);
                }
                catch (Exception ex)
                {
                    result = $"Error processing file '{path}': {ex.Message}";
                }

                labelStatusText.Invoke(() =>
                {
                    labelStatusText.Text = result; // Display the result in a label
                });
            });

            return false;
        }

        public void BringToFrontAndFocus()
        {
            WindowState = FormWindowState.Normal;
            TopMost = true;      // Force it above others
            Activate();          // Try to activate
            BringToFront();      // Bring in Z-order
            Focus();             // Attempt focus
            TopMost = false;     // Remove top-most flag if undesired
        }
    }
}
