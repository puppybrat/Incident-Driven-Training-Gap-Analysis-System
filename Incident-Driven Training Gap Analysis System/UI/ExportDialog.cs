/*
 * File: ExportDialog.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: User Interface Layer
 * 
 * Purpose:
 * This form allows the user to export either incident data or the currently generated
 * report data to a CSV file. Incident data exports incident records, while report data
 * exports the underlying report rows regardless of whether the report is currently
 * displayed as a table or graph.
 */

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    public partial class ExportDialog : Form
    {
        private readonly ExportManager _exportManager;
        private readonly ReportResult? _currentReportResult;

        private RadioButton _rdoIncidentData = null!;
        private RadioButton _rdoReportData = null!;
        private TextBox _txtFilePath = null!;
        private Button _btnBrowse = null!;
        private Button _btnExport = null!;
        private Button _btnCancel = null!;
        private Label _lblReportAvailability = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportDialog"/> class.
        /// </summary>
        /// <param name="currentReportResult">
        /// The currently visible generated report result, if one exists.
        /// This is required for report data export.
        /// </param>
        public ExportDialog(ReportResult? currentReportResult = null)
        {
            InitializeComponent();

            _exportManager = new ExportManager();
            _currentReportResult = currentReportResult;

            BuildLayout();
            ConfigureDefaultState();
        }

        /// <summary>
        /// Opens a file save dialog and stores the selected path in the file path text box.
        /// </summary>
        private void SelectSaveLocation()
        {
            using SaveFileDialog saveFileDialog = new()
            {
                Title = "Export CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                DefaultExt = "csv",
                AddExtension = true,
                FileName = GetDefaultFileName()
            };

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                _txtFilePath.Text = saveFileDialog.FileName;
            }
        }

        /// <summary>
        /// Validates the selected export type and destination, then performs the export.
        /// </summary>
        private void ProcessExport()
        {
            string filePath = _txtFilePath.Text.Trim();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                ShowExportResults(new ExportSummary
                {
                    Success = false,
                    Message = "Please select a CSV export location."
                });
                return;
            }

            ExportSummary summary = GetExportSummary(filePath);
            ShowExportResults(summary);

            if (summary.Success)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Cancels the export operation and closes the dialog.
        /// </summary>
        private void CancelExport()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Displays the outcome of an export attempt using the supplied export summary.
        /// </summary>
        /// <param name="summary">The export result to display.</param>
        private void ShowExportResults(ExportSummary summary)
        {
            MessageBoxIcon icon = summary.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
            string caption = summary.Success ? "Export Complete" : "Export Failed";

            MessageBox.Show(
                this,
                summary.Message,
                caption,
                MessageBoxButtons.OK,
                icon);
        }

        private ExportSummary GetExportSummary(string filePath)
        {
            if (_rdoIncidentData.Checked)
            {
                return _exportManager.ExportDataset(filePath, new FilterSet());
            }

            if (_currentReportResult == null)
            {
                return new ExportSummary
                {
                    Success = false,
                    Message = "No generated report is currently available to export."
                };
            }

            return _exportManager.ExportReport(filePath, _currentReportResult);
        }

        /// <summary>
        /// Builds the dialog layout and wires control events.
        /// </summary>
        private void BuildLayout()
        {
            Text = "Export Data";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 250);
            BackColor = Color.White;

            Label lblTitle = new()
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(20, 20),
                Text = "Choose export type and destination"
            };

            GroupBox grpExportType = new()
            {
                Text = "Export Type",
                Location = new Point(20, 55),
                Size = new Size(520, 80)
            };

            _rdoIncidentData = new RadioButton
            {
                AutoSize = true,
                Location = new Point(16, 28),
                Text = "Incident data"
            };

            _rdoReportData = new RadioButton
            {
                AutoSize = true,
                Location = new Point(160, 28),
                Text = "Report data"
            };

            _lblReportAvailability = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(16, 52),
                Text = string.Empty
            };

            grpExportType.Controls.Add(_rdoIncidentData);
            grpExportType.Controls.Add(_rdoReportData);
            grpExportType.Controls.Add(_lblReportAvailability);

            Label lblFilePath = new()
            {
                AutoSize = true,
                Location = new Point(20, 150),
                Text = "Save Location"
            };

            _txtFilePath = new TextBox
            {
                Location = new Point(20, 172),
                Size = new Size(410, 27),
                ReadOnly = true
            };

            _btnBrowse = new Button
            {
                Location = new Point(440, 170),
                Size = new Size(100, 30),
                Text = "Browse..."
            };

            _btnExport = new Button
            {
                Location = new Point(359, 210),
                Size = new Size(85, 30),
                Text = "Export"
            };

            _btnCancel = new Button
            {
                Location = new Point(455, 210),
                Size = new Size(85, 30),
                Text = "Cancel"
            };

            _btnBrowse.Click += (s, e) => SelectSaveLocation();
            _btnExport.Click += (s, e) => ProcessExport();
            _btnCancel.Click += (s, e) => CancelExport();
            _rdoIncidentData.CheckedChanged += (s, e) => UpdateReportAvailabilityState();
            _rdoReportData.CheckedChanged += (s, e) => UpdateReportAvailabilityState();

            Controls.Add(lblTitle);
            Controls.Add(grpExportType);
            Controls.Add(lblFilePath);
            Controls.Add(_txtFilePath);
            Controls.Add(_btnBrowse);
            Controls.Add(_btnExport);
            Controls.Add(_btnCancel);

            AcceptButton = _btnExport;
            CancelButton = _btnCancel;
        }

        /// <summary>
        /// Applies the default state for the dialog controls.
        /// </summary>
        private void ConfigureDefaultState()
        {
            _rdoIncidentData.Checked = true;
            UpdateReportAvailabilityState();
        }

        /// <summary>
        /// Updates the report export message and export button state based on whether
        /// the current report contains exportable row data.
        /// </summary>
        private void UpdateReportAvailabilityState()
        {
            bool reportAvailable = _currentReportResult != null
                && _currentReportResult.Rows != null
                && _currentReportResult.Rows.Count > 0;

            if (_rdoReportData.Checked)
            {
                _lblReportAvailability.Text = reportAvailable
                    ? "The current report data will be exported."
                    : "No exportable report data is currently available.";
            }
            else
            {
                _lblReportAvailability.Text = "Exports incident records as CSV, including OccurredAt.";
            }

            _btnExport.Enabled = _rdoIncidentData.Checked || reportAvailable;
        }

        /// <summary>
        /// Gets the default file name for the selected export type.
        /// </summary>
        /// <returns>A suggested CSV file name.</returns>
        private string GetDefaultFileName()
        {
            return _rdoReportData.Checked
                ? "report_export.csv"
                : "incident_data_export.csv";
        }
    }
}