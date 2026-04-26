/*
 * File: ImportDialog.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: User Interface Layer
 * 
 * Purpose:
 * This form allows the user to import incident data from a CSV file into the database.
 * The import accepts incident CSV data and displays a summary of inserted and rejected rows.
 * Report data is not supported for import.
 */

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Provides the dialog used to select and import incident data from a CSV file.
    /// </summary>
    public partial class ImportDialog : Form
    {
        private readonly ImportManager _importManager;

        private TextBox _txtFilePath = null!;
        private Button _btnBrowse = null!;
        private Button _btnImport = null!;
        private Button _btnCancel = null!;
        private Label _lblInstructions = null!;

        private ImportSummary? _lastImportSummary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDialog"/> class.
        /// </summary>
        public ImportDialog()
        {
            InitializeComponent();

            _importManager = new ImportManager();

            BuildLayout();
        }

        /// <summary>
        /// Opens a file browser for CSV selection.
        /// </summary>
        private void BrowseForFile()
        {
            using OpenFileDialog openFileDialog = new()
            {
                Title = "Import Incident Data",
                Filter = "CSV Files (*.csv)|*.csv",
                DefaultExt = "csv",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _txtFilePath.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Validates the selected file and executes the import workflow.
        /// </summary>
        private void ProcessImport()
        {
            string filePath = _txtFilePath.Text.Trim();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show(
                    this,
                    "Please select a CSV file to import.",
                    "Import Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _lastImportSummary = _importManager.ImportCsv(filePath);
            ShowImportResults();

            if (_lastImportSummary.HasInsertedRecords)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Cancels the import operation and closes the dialog.
        /// </summary>
        private void CancelImport()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Displays the most recent import result, including inserted rows, rejected rows, and detail messages.
        /// </summary>
        private void ShowImportResults()
        {
            if (_lastImportSummary == null)
            {
                return;
            }

            List<string> lines =
            [
                $"Inserted: {_lastImportSummary.InsertedCount}",
                $"Rejected: {_lastImportSummary.RejectedCount}"
            ];

            if (_lastImportSummary.Messages.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("Details:");

                foreach (string message in _lastImportSummary.Messages)
                {
                    lines.Add($"- {message}");
                }
            }

            string caption;
            MessageBoxIcon icon;

            if (_lastImportSummary.IsCompleteSuccess)
            {
                caption = "Import Complete";
                icon = MessageBoxIcon.Information;
            }
            else if (_lastImportSummary.IsPartialSuccess)
            {
                caption = "Import Complete with Rejections";
                icon = MessageBoxIcon.Warning;
            }
            else
            {
                caption = "Import Failed";
                icon = MessageBoxIcon.Warning;
            }

            MessageBox.Show(
                this,
                string.Join(Environment.NewLine, lines),
                caption,
                MessageBoxButtons.OK,
                icon);
        }

        /// <summary>
        /// Builds the dialog layout and wires control events.
        /// </summary>
        private void BuildLayout()
        {
            Text = "Import Incident Data";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(580, 235);
            BackColor = Color.White;

            Label lblTitle = new()
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(20, 20),
                Text = "Select an incident CSV file to import"
            };

            _lblInstructions = new Label
            {
                AutoSize = false,
                Location = new Point(20, 55),
                Size = new Size(540, 50),
                ForeColor = Color.DimGray,
                Text = "Supported input is incident data only. Imported records receive new " +
                       "database-generated incident identifiers."
            };

            Label lblFilePath = new()
            {
                AutoSize = true,
                Location = new Point(20, 120),
                Text = "CSV File"
            };

            _txtFilePath = new TextBox
            {
                Location = new Point(20, 142),
                Size = new Size(420, 27),
                ReadOnly = true
            };

            _btnBrowse = new Button
            {
                Location = new Point(450, 140),
                Size = new Size(100, 30),
                Text = "Browse..."
            };

            _btnImport = new Button
            {
                Location = new Point(369, 190),
                Size = new Size(85, 30),
                Text = "Import"
            };

            _btnCancel = new Button
            {
                Location = new Point(465, 190),
                Size = new Size(85, 30),
                Text = "Cancel"
            };

            _btnBrowse.Click += (s, e) => BrowseForFile();
            _btnImport.Click += (s, e) => ProcessImport();
            _btnCancel.Click += (s, e) => CancelImport();

            Controls.Add(lblTitle);
            Controls.Add(_lblInstructions);
            Controls.Add(lblFilePath);
            Controls.Add(_txtFilePath);
            Controls.Add(_btnBrowse);
            Controls.Add(_btnImport);
            Controls.Add(_btnCancel);

            AcceptButton = _btnImport;
            CancelButton = _btnCancel;
        }
    }
}