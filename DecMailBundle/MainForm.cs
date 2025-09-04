using System.ComponentModel;
using DecMailBundle.FormComponents;
using System.Diagnostics;
using DecMailBundle.Shell;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace DecMailBundle
{
    public partial class MainForm : Form
    {
        private FileSystemWatcher? _watcher;
        private readonly List<string> _filesCreatedInSession = [];
        private BindingSource _archiveBindingSource = new();
        private string? _selectedRow;
        private bool _isUpdating;

        public static class AppServices
        {
            public static Archiver Archiver => new(AppSettings.Default.Path, AppSettings.Default.UseCurrentDate);
        }

        public MainForm()
        {
            InitializeComponent();

            dataGridView1.DataSource = _archiveBindingSource;
            dataGridView1.Sorted += DataGridView1OnSorted;

            webView21.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = AppDataPath.WebViewData
            };

            webView21.CoreWebView2InitializationCompleted += WebView21OnCoreWebView2InitializationCompleted;
            StartWatcher();
            ReloadArchive();

        }

        private void LoadColumnSorting()
        {
            if (AppSettings.Default.SortColumn is not null)
            {
                var sortColumn = dataGridView1.Columns[AppSettings.Default.SortColumn];
                if (sortColumn != null)
                {
                    dataGridView1.Sort(sortColumn, AppSettings.Default.SortDirection == "Ascending" ? ListSortDirection.Ascending : ListSortDirection.Descending);
                }
            }
        }

        private void DataGridView1OnSorted(object? sender, EventArgs e)
        {
            var sortName = dataGridView1.SortedColumn?.Name;
            var sortDirection = dataGridView1.SortOrder;

            AppSettings.Default.SortColumn = sortName;
            AppSettings.Default.SortDirection = sortDirection == SortOrder.Ascending ? "Ascending" : "Descending";
            AppSettings.Default.Save();
        }

        private void WebView21OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                return;
            }

            webView21.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView21.CoreWebView2.Settings.IsScriptEnabled = false;
            webView21.AllowExternalDrop = false;
            webView21.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView21.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _watcher?.Dispose();
            }
            catch
            {
                //
            }

            base.OnClosed(e);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            // Only allow .eml files to be dragged in
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                if (files.Any(f => f.EndsWith(".eml", StringComparison.OrdinalIgnoreCase)))
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

            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                BringToFrontAndFocus();
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop)!;

                foreach (var path in paths)
                {
                    try
                    {
                        if (path.EndsWith(".eml", StringComparison.OrdinalIgnoreCase))
                        {
                            if (HandleFile(path))
                                return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ReportStatus(ex.Message);
                    }
                }
            }
        }

        private void ReloadArchive()
        {
            if (InvokeRequired)
            {
                Invoke(Work);
            }
            else
            {
                Work();
            }

            return;

            void Work()
            {
                _isUpdating = true;

                Text = $"Mail Archiver - {AppServices.Archiver.PathRoot}";

                var dir = new DirectoryInfo(AppServices.Archiver.GetCurrentYearPath());

                var files = dir.GetFiles("*.pdf").Select(p =>
                {
                    return new FileEntry(PathHelper.NormalizePath(p.FullName), p.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                }).ToList();

                _archiveBindingSource.DataSource = new SortableBindingList<FileEntry>(files);

                LoadColumnSorting();

                _isUpdating = false;

                if (_selectedRow is not null)
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["File"].Value as string == _selectedRow)
                        {
                            row.Selected = true;
                            break;
                        }
                    }
                }
            }
        }

        private bool HandleFile(string path)
        {

            if (!AppServices.Archiver.RootExists())
            {
                ReportStatus($"The folder '{AppServices.Archiver.PathRoot}' does not exist.");
                return true;
            }

            ReportStatus("Processing...");

            _ = Task.Run(async () =>
            {
                var result = await AppServices.Archiver.AddToArchive(path);

                var fileName = Path.GetFileName(result.Path);
                var message = result.Status switch
                {
                    ArchiveResultStatus.Created => $"Created: {fileName}",
                    ArchiveResultStatus.AlreadyExists => $"Already exists: {fileName}",
                    ArchiveResultStatus.Error => $"Error processing '{fileName}': {result.ErrorMessage}",
                    _ => "Unknown result"
                };

                ReportStatus(message);
            });

            return false;
        }

        private void ReportStatus(string txt)
        {
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    labelStatus.Text = txt;
                });
            }
            else
            {
                labelStatus.Text = txt;
            }

        }

        private void BringToFrontAndFocus()
        {
            WindowState = FormWindowState.Normal;
            TopMost = true;      // Force it above others
            Activate();          // Try to activate
            BringToFront();      // Bring in Z-order
            Focus();             // Attempt focus
            TopMost = false;     // Remove top-most flag if undesired
        }

        private void OpenArchiveSettings()
        {
            new SettingsForm().ShowDialog(this);
            ReloadArchive();
            StartWatcher();
        }

        private void StartWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            var archiver = AppServices.Archiver;
            if (archiver.RootExists())
            {
                _watcher = new FileSystemWatcher(archiver.GetCurrentYearPath());

                _watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess |
                                        NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;

                _watcher.Changed += (sender, args) => ReloadArchive();
                _watcher.Created += (sender, args) => ReloadArchive();
                _watcher.Deleted += (sender, args) => ReloadArchive();
                _watcher.Renamed += (sender, args) => ReloadArchive();
                _watcher.Error += (sender, args) => ReloadArchive();
                _watcher.Filter = "*.pdf";
                _watcher.IncludeSubdirectories = true;
                _watcher.EnableRaisingEvents = true;
            }
        }

        private FileEntry? GetEntry(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridView1.RowCount)
            {
                return null;
            }

            var row = dataGridView1.Rows[rowIndex];

            return row.DataBoundItem as FileEntry;
        }

        private void selectArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenArchiveSettings();
        }

        private void openArchiveFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var path = AppServices.Archiver.GetCurrentYearPath();

            if (!Directory.Exists(path))
            {
                OpenArchiveSettings();
                return;
            }

            Process.Start("explorer", [path]);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                var row = dataGridView1.Rows[e.RowIndex];

            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (GetEntry(e.RowIndex) is not { } entry)
            {
                return;
            }

            dataGridView1.Rows[e.RowIndex].Selected = true;

            if (e.Button == MouseButtons.Right)
            {
                var file = new FileInfo(entry.FilePath);
                var cellLocation = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false).Location;
                var pointScreen = dataGridView1.PointToScreen(new Point(e.X + cellLocation.X, e.Y + cellLocation.Y));

                var ctxMnu = new ShellContextMenu();
                ctxMnu.Show(file, pointScreen);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count > 0 && dataGridView1.SelectedRows[0].DataBoundItem is FileEntry entry)
            {
                webView21.Source = new Uri(entry.FilePath);

                if (!_isUpdating)
                {
                    _selectedRow = entry.FileName;
                }
            }
            else
            {
                webView21.Source = new Uri("about:blank");

                if (!_isUpdating)
                {
                    _selectedRow = null;
                }
            }
        }
    }
}
