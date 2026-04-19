using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    public partial class ReportForm : UserControl
    {
        private readonly ReportGenerator _reportGenerator = new();
        private readonly IncidentManager _incidentManager = new();

        private ComboBox _cboPreset = null!;
        private ComboBox _cboLine = null!;
        private ComboBox _cboShift = null!;
        private ComboBox _cboEquipment = null!;
        private ComboBox _cboSop = null!;
        private ComboBox _cboOutput = null!;

        private CheckBox _chkUseDateRange = null!;
        private DateTimePicker _dtpStart = null!;
        private DateTimePicker _dtpEnd = null!;

        private Button _btnGenerate = null!;
        private Button _btnClear = null!;

        private DataGridView _gridResults = null!;
        private CartesianChart _chartResults = null!;
        private Label _lblSummary = null!;

        private ReportResult? _currentReportResult;

        private sealed class ComboBoxItem
        {
            public int Value { get; }
            public string Text { get; }

            public ComboBoxItem(int value, string text)
            {
                Value = value;
                Text = text;
            }

            public override string ToString() => Text;
        }

        public ReportForm()
        {
            InitializeComponent();
            BuildLayout();
            LoadReportPresets();
            LoadFilterOptions();
            ApplyPresetToControls();
            ClearResults();
        }

        public void LoadReportPresets()
        {
            _cboPreset.Items.Clear();
            _cboPreset.Items.Add("Incidents per Shift by Line");
            _cboPreset.Items.Add("Lines by missing SOP");
            _cboPreset.Items.Add("Incidents per Equipment");
            _cboPreset.Items.Add("Incidents per SOP Reference");
            _cboPreset.SelectedIndex = 0;
        }

        public void LoadFilterOptions()
        {
            ReferenceDataSet referenceData = _incidentManager.GetAllReferenceData();

            _cboLine.Items.Clear();
            _cboShift.Items.Clear();
            _cboEquipment.Items.Clear();
            _cboSop.Items.Clear();

            _cboLine.Items.Add("All");
            _cboShift.Items.Add("All");
            _cboEquipment.Items.Add("All");
            _cboSop.Items.Add("All");

            foreach (Line line in referenceData.Lines.OrderBy(l => l.Name))
            {
                _cboLine.Items.Add(new ComboBoxItem(line.LineId, line.Name));
            }

            foreach (Shift shift in referenceData.Shifts.OrderBy(s => s.Name))
            {
                _cboShift.Items.Add(new ComboBoxItem(shift.ShiftId, shift.Name));
            }

            foreach (Equipment equipment in referenceData.Equipment.OrderBy(e => e.Name))
            {
                _cboEquipment.Items.Add(new ComboBoxItem(equipment.EquipmentId, equipment.Name));
            }

            foreach (SOP sop in referenceData.Sops.OrderBy(s => s.Name))
            {
                _cboSop.Items.Add(new ComboBoxItem(sop.SopId, sop.Name));
            }

            _cboLine.SelectedIndex = 0;
            _cboShift.SelectedIndex = 0;
            _cboEquipment.SelectedIndex = 0;
            _cboSop.SelectedIndex = 0;

            _cboOutput.Items.Clear();
            _cboOutput.Items.Add("Table");
            _cboOutput.Items.Add("Graph");
            _cboOutput.SelectedIndex = 0;
        }

        public void ApplyPresetToControls()
        {
            string preset = _cboPreset.SelectedItem?.ToString() ?? string.Empty;

            ResetFiltersOnly();

            switch (preset)
            {
                case "Incidents per Shift by Line":
                    _cboOutput.SelectedItem = "Table";
                    break;

                case "Lines by missing SOP":
                    _cboOutput.SelectedItem = "Table";
                    break;

                case "Incidents per Equipment":
                    _cboOutput.SelectedItem = "Table";
                    break;

                case "Incidents per SOP Reference":
                    _cboOutput.SelectedItem = "Table";
                    break;
            }
        }

        public void ProcessReportRequest()
        {
            ReportRequest request = BuildReportRequest();

            if (request.Filters.StartDate.HasValue &&
                request.Filters.EndDate.HasValue &&
                request.Filters.StartDate.Value.Date > request.Filters.EndDate.Value.Date)
            {
                MessageBox.Show(
                    "Start date cannot be later than end date.",
                    "Invalid Date Range",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            _currentReportResult = _reportGenerator.GenerateReport(request);
            DisplayResults();
        }

        public void DisplayResults()
        {
            if (_currentReportResult == null)
            {
                ClearResults();
                return;
            }

            if (string.Equals(_currentReportResult.OutputType, "Graph", StringComparison.OrdinalIgnoreCase))
            {
                DisplayGraphResults(_currentReportResult);
            }
            else
            {
                DisplayTableResults(_currentReportResult);
            }

            _lblSummary.Text = $"Rows returned: {_currentReportResult.Rows.Count}";
        }

        public void ClearResults()
        {
            _currentReportResult = null;

            _gridResults.Visible = true;
            _chartResults.Visible = false;

            _gridResults.DataSource = null;
            _gridResults.Columns.Clear();

            _chartResults.Series = Array.Empty<ISeries>();
            _chartResults.XAxes = Array.Empty<Axis>();
            _chartResults.YAxes = Array.Empty<Axis>();

            _lblSummary.Text = "No report generated.";
        }

        private void BuildLayout()
        {
            BackColor = Color.White;

            Label lblTitle = new()
            {
                Text = "Generate Report",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(20)
            };

            TableLayoutPanel filterPanel = new()
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(20),
                ColumnCount = 2
            };

            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _cboPreset = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboLine = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboShift = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboEquipment = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboSop = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboOutput = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };

            _chkUseDateRange = new CheckBox
            {
                Text = "Use date range",
                AutoSize = true
            };

            _dtpStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today.AddDays(-30)
            };

            _dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today
            };

            FlowLayoutPanel datePanel = new()
            {
                AutoSize = true,
                WrapContents = false
            };
            datePanel.Controls.Add(_chkUseDateRange);
            datePanel.Controls.Add(new Label { Text = "Start", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
            datePanel.Controls.Add(_dtpStart);
            datePanel.Controls.Add(new Label { Text = "End", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
            datePanel.Controls.Add(_dtpEnd);

            _btnGenerate = new Button
            {
                Text = "Generate",
                Width = 120,
                Height = 36
            };

            _btnClear = new Button
            {
                Text = "Clear",
                Width = 120,
                Height = 36
            };

            FlowLayoutPanel buttonPanel = new()
            {
                AutoSize = true
            };
            buttonPanel.Controls.Add(_btnGenerate);
            buttonPanel.Controls.Add(_btnClear);

            filterPanel.Controls.Add(new Label { Text = "Preset", AutoSize = true }, 0, 0);
            filterPanel.Controls.Add(_cboPreset, 1, 0);

            filterPanel.Controls.Add(new Label { Text = "Line", AutoSize = true }, 0, 1);
            filterPanel.Controls.Add(_cboLine, 1, 1);

            filterPanel.Controls.Add(new Label { Text = "Shift", AutoSize = true }, 0, 2);
            filterPanel.Controls.Add(_cboShift, 1, 2);

            filterPanel.Controls.Add(new Label { Text = "Equipment", AutoSize = true }, 0, 3);
            filterPanel.Controls.Add(_cboEquipment, 1, 3);

            filterPanel.Controls.Add(new Label { Text = "SOP", AutoSize = true }, 0, 4);
            filterPanel.Controls.Add(_cboSop, 1, 4);

            filterPanel.Controls.Add(new Label { Text = "Date Range", AutoSize = true }, 0, 5);
            filterPanel.Controls.Add(datePanel, 1, 5);

            filterPanel.Controls.Add(new Label { Text = "Output Mode", AutoSize = true }, 0, 6);
            filterPanel.Controls.Add(_cboOutput, 1, 6);

            filterPanel.Controls.Add(new Label(), 0, 7);
            filterPanel.Controls.Add(buttonPanel, 1, 7);

            _lblSummary = new Label
            {
                Text = "No report generated.",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(20, 10, 20, 10)
            };

            _gridResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            _chartResults = new CartesianChart
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            Panel resultsPanel = new()
            {
                Dock = DockStyle.Fill
            };
            resultsPanel.Controls.Add(_chartResults);
            resultsPanel.Controls.Add(_gridResults);

            Controls.Add(resultsPanel);
            Controls.Add(_lblSummary);
            Controls.Add(filterPanel);
            Controls.Add(lblTitle);

            _cboPreset.SelectedIndexChanged += (s, e) => ApplyPresetToControls();
            _btnGenerate.Click += (s, e) => ProcessReportRequest();
            _btnClear.Click += (s, e) =>
            {
                ResetFiltersOnly();
                ClearResults();
            };
        }

        private ReportRequest BuildReportRequest()
        {
            return new ReportRequest
            {
                PresetName = _cboPreset.SelectedItem?.ToString() ?? string.Empty,
                OutputType = _cboOutput.SelectedItem?.ToString() ?? "Table",
                Filters = new FilterSet
                {
                    LineId = GetSelectedId(_cboLine),
                    ShiftId = GetSelectedId(_cboShift),
                    EquipmentId = GetSelectedId(_cboEquipment),
                    SopId = GetSelectedId(_cboSop),
                    StartDate = _chkUseDateRange.Checked ? _dtpStart.Value.Date : null,
                    EndDate = _chkUseDateRange.Checked ? _dtpEnd.Value.Date : null
                }
            };
        }

        private int? GetSelectedId(ComboBox comboBox)
        {
            return comboBox.SelectedItem is ComboBoxItem item ? item.Value : null;
        }

        private void ResetFiltersOnly()
        {
            if (_cboLine.Items.Count > 0) _cboLine.SelectedIndex = 0;
            if (_cboShift.Items.Count > 0) _cboShift.SelectedIndex = 0;
            if (_cboEquipment.Items.Count > 0) _cboEquipment.SelectedIndex = 0;
            if (_cboSop.Items.Count > 0) _cboSop.SelectedIndex = 0;
            if (_cboOutput.Items.Count > 0) _cboOutput.SelectedIndex = 0;

            _chkUseDateRange.Checked = false;
            _dtpStart.Value = DateTime.Today.AddDays(-30);
            _dtpEnd.Value = DateTime.Today;
        }

        private void DisplayTableResults(ReportResult reportResult)
        {
            _chartResults.Visible = false;
            _gridResults.Visible = true;

            _gridResults.AutoGenerateColumns = false;
            _gridResults.Columns.Clear();

            _gridResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GroupPrimary",
                HeaderText = "Group",
                FillWeight = 35
            });

            _gridResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "GroupSecondary",
                HeaderText = "Subgroup",
                FillWeight = 35
            });

            _gridResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IncidentCount",
                HeaderText = "Incident Count",
                FillWeight = 15
            });

            _gridResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Status",
                FillWeight = 15
            });

            _gridResults.DataSource = reportResult.Rows
                .Select(r => new
                {
                    r.GroupPrimary,
                    r.GroupSecondary,
                    r.IncidentCount,
                    Status = r.IsFlagged ? "Flagged" : "Normal"
                })
                .ToList();
        }

        private void DisplayGraphResults(ReportResult reportResult)
        {
            _gridResults.Visible = false;
            _chartResults.Visible = true;

            string[] labels = reportResult.Rows
                .Select(r => string.IsNullOrWhiteSpace(r.GroupSecondary)
                    ? r.GroupPrimary
                    : $"{r.GroupPrimary} - {r.GroupSecondary}")
                .ToArray();

            int[] values = reportResult.Rows
                .Select(r => r.IncidentCount)
                .ToArray();

            _chartResults.Series = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = values,
                    Name = "Incident Count"
                }
                    };

                    _chartResults.XAxes = new[]
                    {
                new Axis
                {
                    Labels = labels
                }
            };

                    _chartResults.YAxes = new[]
                    {
                new Axis
                {
                    Name = "Incident Count"
                }
            };
        }
    }
}