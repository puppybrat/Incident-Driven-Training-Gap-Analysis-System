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
        private readonly RuleEvaluator _ruleEvaluator = new();

        private ComboBox _cboPreset = null!;
        private ComboBox _cboGroupingType = null!;
        private ComboBox _cboLine = null!;
        private ComboBox _cboShift = null!;
        private ComboBox _cboEquipment = null!;
        private ComboBox _cboSop = null!;
        private ComboBox _cboOutput = null!;

        private CheckBox _chkIncludeLine = null!;
        private CheckBox _chkIncludeShift = null!;
        private CheckBox _chkIncludeEquipment = null!;
        private CheckBox _chkIncludeSop = null!;

        private CheckBox _chkUseDateRange = null!;
        private DateTimePicker _dtpStart = null!;
        private DateTimePicker _dtpEnd = null!;

        private Button _btnGenerate = null!;
        private Button _btnClear = null!;

        private DataGridView _gridResults = null!;
        private CartesianChart _chartResults = null!;
        private Label _lblSummary = null!;

        private ReportResult? _currentReportResult;
        private bool _isApplyingPreset;

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
            ApplyRuleConfigDefaults();
            ApplyPresetToControls();
            ClearResults();
        }

        public void LoadReportPresets()
        {
            _cboPreset.Items.Clear();
            _cboPreset.Items.Add("None");
            _cboPreset.Items.Add("Incidents per Shift by Line");
            _cboPreset.Items.Add("Lines by missing SOP");
            _cboPreset.Items.Add("Incidents per Equipment");
            _cboPreset.Items.Add("Incidents per SOP Reference");
            _cboPreset.SelectedItem = "None";
        }

        public void LoadFilterOptions()
        {
            ReferenceDataSet referenceData = _incidentManager.GetAllReferenceData();

            _cboGroupingType.Items.Clear();
            _cboGroupingType.Items.Add("Shift");
            _cboGroupingType.Items.Add("Line");
            _cboGroupingType.Items.Add("Equipment");
            _cboGroupingType.Items.Add("SOP");

            _cboLine.Items.Clear();
            _cboShift.Items.Clear();
            _cboEquipment.Items.Clear();
            _cboSop.Items.Clear();

            _cboLine.Items.Add("All");
            _cboShift.Items.Add("All");
            _cboEquipment.Items.Add("All");
            _cboSop.Items.Add("All");
            _cboSop.Items.Add("Missing SOP");

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

        private void SelectMissingSopOption()
        {
            for (int i = 0; i < _cboSop.Items.Count; i++)
            {
                if (string.Equals(_cboSop.Items[i]?.ToString(), "Missing SOP", StringComparison.OrdinalIgnoreCase))
                {
                    _cboSop.SelectedIndex = i;
                    return;
                }
            }
        }

        private void ApplyRuleConfigDefaults()
        {
            RuleConfig ruleConfig = _ruleEvaluator.LoadCurrentRuleConfig();

            if (_cboGroupingType.Items.Contains(ruleConfig.GroupingType))
            {
                _cboGroupingType.SelectedItem = ruleConfig.GroupingType;
            }
            else if (_cboGroupingType.Items.Count > 0)
            {
                _cboGroupingType.SelectedIndex = 0;
            }

            _chkUseDateRange.Checked = true;

            int daysBack = ruleConfig.TimeWindow switch
            {
                "7 days" => 7,
                "30 days" => 30,
                "90 days" => 90,
                _ => 30
            };

            _dtpEnd.Value = DateTime.Today;
            _dtpStart.Value = DateTime.Today.AddDays(-daysBack);
        }

        public void ApplyPresetToControls()
        {
            if (_cboPreset.SelectedItem == null)
            {
                return;
            }

            string preset = _cboPreset.SelectedItem.ToString() ?? string.Empty;

            if (preset == "Custom")
            {
                return;
            }

            _isApplyingPreset = true;

            try
            {
                ResetFiltersOnly();

                switch (preset)
                {
                    case "None":
                        break;

                    case "Incidents per Shift by Line":
                        _cboGroupingType.SelectedItem = "Line";
                        _chkIncludeLine.Checked = true;
                        _chkIncludeShift.Checked = true;
                        _chkIncludeEquipment.Checked = false;
                        _chkIncludeSop.Checked = false;
                        _cboOutput.SelectedItem = "Table";
                        break;

                    case "Lines by missing SOP":
                        _cboGroupingType.SelectedItem = "Line";
                        _chkIncludeLine.Checked = true;
                        _chkIncludeShift.Checked = false;
                        _chkIncludeEquipment.Checked = false;
                        _chkIncludeSop.Checked = true;
                        SelectMissingSopOption();
                        _cboOutput.SelectedItem = "Table";
                        break;

                    case "Incidents per Equipment":
                        _cboGroupingType.SelectedItem = "Equipment";
                        _chkIncludeLine.Checked = false;
                        _chkIncludeShift.Checked = false;
                        _chkIncludeEquipment.Checked = true;
                        _chkIncludeSop.Checked = false;
                        _cboOutput.SelectedItem = "Table";
                        break;

                    case "Incidents per SOP Reference":
                        _cboGroupingType.SelectedItem = "SOP";
                        _chkIncludeLine.Checked = false;
                        _chkIncludeShift.Checked = false;
                        _chkIncludeEquipment.Checked = false;
                        _chkIncludeSop.Checked = true;
                        _cboOutput.SelectedItem = "Table";
                        break;
                }
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }

        private void MarkPresetAsCustom()
        {
            if (_isApplyingPreset)
            {
                return;
            }

            if (_cboPreset.SelectedItem == null)
            {
                return;
            }

            string currentPreset = _cboPreset.SelectedItem.ToString() ?? string.Empty;

            if (currentPreset != "None" && currentPreset != "Custom")
            {
                _isApplyingPreset = true;

                try
                {
                    if (!_cboPreset.Items.Contains("Custom"))
                    {
                        _cboPreset.Items.Add("Custom");
                    }

                    _cboPreset.SelectedItem = "Custom";
                }
                finally
                {
                    _isApplyingPreset = false;
                }
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
                Padding = new Padding(20),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
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
            _cboGroupingType = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboLine = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboShift = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboEquipment = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboSop = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboOutput = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };

            _chkIncludeLine = new CheckBox
            {
                Text = "Include Line",
                AutoSize = true,
                Checked = true
            };

            _chkIncludeShift = new CheckBox
            {
                Text = "Include Shift",
                AutoSize = true,
                Checked = true
            };

            _chkIncludeEquipment = new CheckBox
            {
                Text = "Include Equipment",
                AutoSize = true,
                Checked = true
            };

            _chkIncludeSop = new CheckBox
            {
                Text = "Include SOP",
                AutoSize = true,
                Checked = true
            };

            FlowLayoutPanel groupingFieldsPanel = new()
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };
            groupingFieldsPanel.Controls.Add(_chkIncludeLine);
            groupingFieldsPanel.Controls.Add(_chkIncludeShift);
            groupingFieldsPanel.Controls.Add(_chkIncludeEquipment);
            groupingFieldsPanel.Controls.Add(_chkIncludeSop);

            _chkUseDateRange = new CheckBox
            {
                Text = "Use date range",
                AutoSize = true,
                Checked = true
            };

            _dtpStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today.AddDays(-7)
            };

            _dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Value = DateTime.Today
            };

            TableLayoutPanel datePanel = new()
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0)
            };

            datePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            datePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            datePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            datePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            FlowLayoutPanel dateRangeCheckRow = new()
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                WrapContents = false,
                Margin = new Padding(0)
            };
            dateRangeCheckRow.Controls.Add(_chkUseDateRange);

            FlowLayoutPanel startRow = new()
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                WrapContents = false,
                Margin = new Padding(0)
            };
            startRow.Controls.Add(new Label
            {
                Text = "Start",
                AutoSize = true,
                Width = 45,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 6, 0)
            });
            _dtpStart.Dock = DockStyle.None;
            startRow.Controls.Add(_dtpStart);

            FlowLayoutPanel endRow = new()
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                WrapContents = false,
                Margin = new Padding(0)
            };
            endRow.Controls.Add(new Label
            {
                Text = "End",
                AutoSize = true,
                Width = 45,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 6, 6, 0)
            });
            _dtpEnd.Dock = DockStyle.None;
            endRow.Controls.Add(_dtpEnd);

            datePanel.Controls.Add(dateRangeCheckRow, 0, 0);
            datePanel.Controls.Add(startRow, 0, 1);
            datePanel.Controls.Add(endRow, 0, 2);

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

            filterPanel.Controls.Add(new Label { Text = "Grouping Type", AutoSize = true }, 0, 1);
            filterPanel.Controls.Add(_cboGroupingType, 1, 1);

            filterPanel.Controls.Add(new Label { Text = "Included Fields", AutoSize = true }, 0, 2);
            filterPanel.Controls.Add(groupingFieldsPanel, 1, 2);

            filterPanel.Controls.Add(new Label { Text = "Line", AutoSize = true }, 0, 3);
            filterPanel.Controls.Add(_cboLine, 1, 3);

            filterPanel.Controls.Add(new Label { Text = "Shift", AutoSize = true }, 0, 4);
            filterPanel.Controls.Add(_cboShift, 1, 4);

            filterPanel.Controls.Add(new Label { Text = "Equipment", AutoSize = true }, 0, 5);
            filterPanel.Controls.Add(_cboEquipment, 1, 5);

            filterPanel.Controls.Add(new Label { Text = "SOP", AutoSize = true }, 0, 6);
            filterPanel.Controls.Add(_cboSop, 1, 6);

            filterPanel.Controls.Add(new Label { Text = "Date Range", AutoSize = true }, 0, 7);
            filterPanel.Controls.Add(datePanel, 1, 7);

            filterPanel.Controls.Add(new Label { Text = "Output Mode", AutoSize = true }, 0, 8);
            filterPanel.Controls.Add(_cboOutput, 1, 8);

            filterPanel.Controls.Add(new Label(), 0, 9);
            filterPanel.Controls.Add(buttonPanel, 1, 9);

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

            Panel container = new()
            {
                Dock = DockStyle.Fill
            };

            container.Controls.Add(resultsPanel);
            container.Controls.Add(_lblSummary);
            container.Controls.Add(filterPanel);
            container.Controls.Add(lblTitle);

            lblTitle.Dock = DockStyle.Top;
            filterPanel.Dock = DockStyle.Top;
            _lblSummary.Dock = DockStyle.Top;
            resultsPanel.Dock = DockStyle.Fill;

            Controls.Add(container);

            _cboPreset.SelectedIndexChanged += (s, e) => ApplyPresetToControls();

            _cboGroupingType.SelectedIndexChanged += (s, e) => MarkPresetAsCustom();
            _cboLine.SelectedIndexChanged += (s, e) => MarkPresetAsCustom();
            _cboShift.SelectedIndexChanged += (s, e) => MarkPresetAsCustom();
            _cboEquipment.SelectedIndexChanged += (s, e) => MarkPresetAsCustom();
            _cboSop.SelectedIndexChanged += (s, e) => MarkPresetAsCustom();
            _cboOutput.SelectedIndexChanged += (s, e) => MarkPresetAsCustom();
            _chkIncludeLine.CheckedChanged += (s, e) => MarkPresetAsCustom();
            _chkIncludeShift.CheckedChanged += (s, e) => MarkPresetAsCustom();
            _chkIncludeEquipment.CheckedChanged += (s, e) => MarkPresetAsCustom();
            _chkIncludeSop.CheckedChanged += (s, e) => MarkPresetAsCustom();
            _chkUseDateRange.CheckedChanged += (s, e) => MarkPresetAsCustom();
            _dtpStart.ValueChanged += (s, e) => MarkPresetAsCustom();
            _dtpEnd.ValueChanged += (s, e) => MarkPresetAsCustom();

            _btnGenerate.Click += (s, e) => ProcessReportRequest();
            _btnClear.Click += (s, e) =>
            {
                ResetFiltersOnly();
                ClearResults();
            };
        }

        private ReportRequest BuildReportRequest()
        {
            bool requireMissingSop = string.Equals(
                _cboSop.SelectedItem?.ToString(),
                "Missing SOP",
                StringComparison.OrdinalIgnoreCase);

            return new ReportRequest
            {
                PresetName = _cboPreset.SelectedItem?.ToString() ?? string.Empty,
                GroupingType = _cboGroupingType.SelectedItem?.ToString() ?? string.Empty,
                OutputType = _cboOutput.SelectedItem?.ToString() ?? "Table",
                IncludeLine = _chkIncludeLine.Checked,
                IncludeShift = _chkIncludeShift.Checked,
                IncludeEquipment = _chkIncludeEquipment.Checked,
                IncludeSop = _chkIncludeSop.Checked,
                Filters = new FilterSet
                {
                    LineId = GetSelectedId(_cboLine),
                    ShiftId = GetSelectedId(_cboShift),
                    EquipmentId = GetSelectedId(_cboEquipment),
                    SopId = requireMissingSop ? null : GetSelectedId(_cboSop),
                    RequireMissingSop = requireMissingSop,
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

            _chkIncludeLine.Checked = true;
            _chkIncludeShift.Checked = true;
            _chkIncludeEquipment.Checked = true;
            _chkIncludeSop.Checked = true;

            _chkUseDateRange.Checked = true;

            RuleConfig ruleConfig = _ruleEvaluator.LoadCurrentRuleConfig();

            if (_cboGroupingType.Items.Contains(ruleConfig.GroupingType))
            {
                _cboGroupingType.SelectedItem = ruleConfig.GroupingType;
            }
            else if (_cboGroupingType.Items.Count > 0)
            {
                _cboGroupingType.SelectedIndex = 0;
            }

            int daysBack = ruleConfig.TimeWindow switch
            {
                "7 days" => 7,
                "30 days" => 30,
                "90 days" => 90,
                _ => 30
            };

            _dtpEnd.Value = DateTime.Today;
            _dtpStart.Value = DateTime.Today.AddDays(-daysBack);
        }

        private void DisplayTableResults(ReportResult reportResult)
        {
            _chartResults.Visible = false;
            _gridResults.Visible = true;

            _gridResults.AutoGenerateColumns = false;
            _gridResults.Columns.Clear();
            _gridResults.DataSource = null;

            if (reportResult.IncludeLine)
            {
                _gridResults.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Line",
                    HeaderText = "Line",
                    FillWeight = 14
                });
            }

            if (reportResult.IncludeShift)
            {
                _gridResults.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Shift",
                    HeaderText = "Shift",
                    FillWeight = 14
                });
            }

            if (reportResult.IncludeEquipment)
            {
                _gridResults.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Equipment",
                    HeaderText = "Equipment",
                    FillWeight = 18
                });
            }

            if (reportResult.IncludeSop)
            {
                _gridResults.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "SOP",
                    HeaderText = "SOP",
                    FillWeight = 18
                });
            }

            _gridResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IncidentCount",
                HeaderText = "Incident Count",
                FillWeight = 12
            });

            _gridResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Status",
                FillWeight = 10
            });

            _gridResults.DataSource = reportResult.Rows;
        }

        private void DisplayGraphResults(ReportResult reportResult)
        {
            _gridResults.Visible = false;
            _chartResults.Visible = true;

            string[] labels = reportResult.Rows
                .Select(r => r.GroupValue)
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