/*
 * File: ReportForm.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: UI Layer
 * 
 * Purpose:
 * This user control provides the interface for generating reports.
 * It loads presets and filter options, builds report requests, submits them
 * for generation, and displays results as a table or graph.
 */

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Provides the user interface for configuring, generating, and displaying incident reports.
    /// </summary>
    public partial class ReportForm : UserControl
    {
        private readonly ReportGenerator _reportGenerator = new();
        private readonly IncidentManager _incidentManager = new();
        private readonly RuleEvaluator _ruleEvaluator = new();

        private const string AllOption = "All";
        private const string MissingSopOption = "Missing SOP";
        private const string TableOutput = "Table";
        private const string GraphOutput = "Graph";
        private const string ShiftGrouping = "Shift";
        private const string LineGrouping = "Line";
        private const string EquipmentGrouping = "Equipment";
        private const string SopGrouping = "SOP";

        private static readonly string[] GroupingOptions =
        [
            ShiftGrouping,
            LineGrouping,
            EquipmentGrouping,
            SopGrouping
        ];

        private ReportResult? _currentReportResult;

        /// <summary>
        /// Gets the currently displayed report result, if one exists.
        /// </summary>
        public ReportResult? CurrentReportResult => _currentReportResult;

        private bool _isApplyingPreset;

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

        /// <summary>
        /// Represents a combo box item with a stored identifier and display text.
        /// </summary>
        /// <param name="value">The stored identifier.</param>
        /// <param name="text">The display text.</param>
        private sealed class ComboBoxItem(int value, string text)
        {
            /// <summary>
            /// Gets the stored integer value for the item.
            /// </summary>
            public int Value { get; } = value;

            /// <summary>
            /// Gets the display text shown in the combo box.
            /// </summary>
            public string Text { get; } = text;

            /// <summary>
            /// Returns the display text for the combo box item.
            /// </summary>
            /// <returns>The display text.</returns>
            public override string ToString() => Text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportForm"/> class.
        /// </summary>
        public ReportForm()
        {
            InitializeComponent();
            BuildLayout();
            LoadReportPresets();
            LoadFilterOptions();
            ApplyRuleWindowDefaults(_ruleEvaluator.LoadCurrentRuleConfig());
            ApplyPresetToControls();
            ClearResults();
        }

        /// <summary>
        /// Loads predefined report presets into the preset selection control.
        /// </summary>
        public void LoadReportPresets()
        {
            _cboPreset.Items.Clear();

            foreach (string preset in ReportPresetNames.AvailablePresets)
            {
                _cboPreset.Items.Add(preset);
            }

            _cboPreset.SelectedItem = ReportPresetNames.None;
        }

        /// <summary>
        /// Loads grouping, reference data, Missing SOP, and output options into report controls.
        /// </summary>
        public void LoadFilterOptions()
        {
            ReferenceDataSet referenceData = _incidentManager.GetAllReferenceData();

            PopulateComboBoxWithStrings(_cboGroupingType, GroupingOptions);

            _cboLine.Items.Clear();
            _cboShift.Items.Clear();
            _cboEquipment.Items.Clear();
            _cboSop.Items.Clear();

            _cboLine.Items.Add(AllOption);
            _cboShift.Items.Add(AllOption);
            _cboEquipment.Items.Add(AllOption);
            _cboSop.Items.Add(AllOption);
            _cboSop.Items.Add(MissingSopOption);

            AddSortedItems(_cboLine, referenceData.Lines, line => new ComboBoxItem(line.LineId, line.Name));
            AddSortedItems(_cboShift, referenceData.Shifts, shift => new ComboBoxItem(shift.ShiftId, shift.Name));
            AddSortedItems(_cboEquipment, referenceData.Equipment, equipment => new ComboBoxItem(equipment.EquipmentId, equipment.Name));
            AddSortedItems(_cboSop, referenceData.Sops, sop => new ComboBoxItem(sop.SopId, sop.Name));

            _cboLine.SelectedIndex = 0;
            _cboShift.SelectedIndex = 0;
            _cboEquipment.SelectedIndex = 0;
            _cboSop.SelectedIndex = 0;

            PopulateComboBoxWithStrings(_cboOutput, [TableOutput, GraphOutput]);
            _cboOutput.SelectedIndex = 0;
        }

        /// <summary>
        /// Applies the selected report preset by resetting filters and updating preset-controlled options.
        /// </summary>
        public void ApplyPresetToControls()
        {
            if (_cboPreset.SelectedItem == null)
            {
                return;
            }

            string preset = _cboPreset.SelectedItem.ToString() ?? string.Empty;

            if (preset == ReportPresetNames.Custom)
            {
                return;
            }

            _isApplyingPreset = true;

            try
            {
                ResetFiltersOnly();
                ApplyPresetState(preset);
            }
            finally
            {
                _isApplyingPreset = false;
            }
        }

        /// <summary>
        /// Validates the date range, builds a report request, and displays the generated result.
        /// </summary>
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

        /// <summary>
        /// Clears the current report result and resets the output area.
        /// </summary>
        public void ClearResults()
        {
            _currentReportResult = null;

            _gridResults.Visible = true;
            _chartResults.Visible = false;

            _gridResults.DataSource = null;
            _gridResults.Columns.Clear();

            _chartResults.Series = [];
            _chartResults.XAxes = [];
            _chartResults.YAxes = [];

            _lblSummary.Text = "No report generated.";
        }

        /// <summary>
        /// Displays the current report result as either a table or graph.
        /// </summary>
        private void DisplayResults()
        {
            if (_currentReportResult == null)
            {
                ClearResults();
                return;
            }

            if (string.Equals(_currentReportResult.OutputType, GraphOutput, StringComparison.OrdinalIgnoreCase))
            {
                DisplayGraphResults(_currentReportResult);
            }
            else
            {
                DisplayTableResults(_currentReportResult);
            }

            _lblSummary.Text = $"Rows returned: {_currentReportResult.Rows.Count}";
        }

        /// <summary>
        /// Applies grouping, included-field, filter, and output settings for a preset.
        /// </summary>
        /// <param name="preset">The preset name to apply.</param>
        private void ApplyPresetState(string preset)
        {
            switch (preset)
            {
                case ReportPresetNames.None:
                    return;

                case ReportPresetNames.IncidentsPerShiftByLine:
                    _cboGroupingType.SelectedItem = LineGrouping;
                    _chkIncludeLine.Checked = true;
                    _chkIncludeShift.Checked = true;
                    _chkIncludeEquipment.Checked = false;
                    _chkIncludeSop.Checked = false;
                    _cboOutput.SelectedItem = TableOutput;
                    return;

                case ReportPresetNames.IncidentsPerMissingSopByLine:
                    _cboGroupingType.SelectedItem = LineGrouping;
                    _chkIncludeLine.Checked = true;
                    _chkIncludeShift.Checked = false;
                    _chkIncludeEquipment.Checked = false;
                    _chkIncludeSop.Checked = true;
                    SelectMissingSopOption();
                    _cboOutput.SelectedItem = TableOutput;
                    return;

                case ReportPresetNames.IncidentsPerEquipment:
                    _cboGroupingType.SelectedItem = EquipmentGrouping;
                    _chkIncludeLine.Checked = false;
                    _chkIncludeShift.Checked = false;
                    _chkIncludeEquipment.Checked = true;
                    _chkIncludeSop.Checked = false;
                    _cboOutput.SelectedItem = TableOutput;
                    return;

                case ReportPresetNames.IncidentsPerSopReference:
                    _cboGroupingType.SelectedItem = SopGrouping;
                    _chkIncludeLine.Checked = false;
                    _chkIncludeShift.Checked = false;
                    _chkIncludeEquipment.Checked = false;
                    _chkIncludeSop.Checked = true;
                    _cboOutput.SelectedItem = TableOutput;
                    return;
            }
        }

        /// <summary>
        /// Marks the current report configuration as custom when the user manually changes
        /// preset-related controls.
        /// </summary>
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

            if (currentPreset != ReportPresetNames.None && currentPreset != ReportPresetNames.Custom)
            {
                _isApplyingPreset = true;

                try
                {
                    if (!_cboPreset.Items.Contains(ReportPresetNames.Custom))
                    {
                        _cboPreset.Items.Add(ReportPresetNames.Custom);
                    }

                    _cboPreset.SelectedItem = ReportPresetNames.Custom;
                }
                finally
                {
                    _isApplyingPreset = false;
                }
            }
        }

        /// <summary>
        /// Resets filter and output controls while preserving the current preset selection.
        /// </summary>
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
            ApplyRuleWindowDefaults(ruleConfig);
        }

        /// <summary>
        /// Applies the rule configuration grouping type and time window to the report controls.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration providing default report values.</param>
        private void ApplyRuleWindowDefaults(RuleConfig ruleConfig)
        {
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
                "120 days" => 120,
                _ => 30
            };

            _dtpEnd.Value = DateTime.Today;
            _dtpStart.Value = DateTime.Today.AddDays(-daysBack);
        }

        /// <summary>
        /// Selects the item in the SOP combo box that matches the missing SOP option, if it exists.
        /// </summary>
        private void SelectMissingSopOption()
        {
            foreach (object item in _cboSop.Items)
            {
                if (item is string text &&
                    string.Equals(text, MissingSopOption, StringComparison.OrdinalIgnoreCase))
                {
                    _cboSop.SelectedItem = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Builds a report request from the current preset, grouping, output, field, and filter selections.
        /// </summary>
        /// <returns>The populated report request.</returns>
        private ReportRequest BuildReportRequest()
        {
            return new ReportRequest
            {
                PresetName = _cboPreset.SelectedItem?.ToString() ?? string.Empty,
                GroupingType = _cboGroupingType.SelectedItem?.ToString() ?? string.Empty,
                OutputType = _cboOutput.SelectedItem?.ToString() ?? TableOutput,
                IncludeLine = _chkIncludeLine.Checked,
                IncludeShift = _chkIncludeShift.Checked,
                IncludeEquipment = _chkIncludeEquipment.Checked,
                IncludeSop = _chkIncludeSop.Checked,
                Filters = BuildFilterSet()
            };
        }

        /// <summary>
        /// Builds report filters from selected identifiers, date range, and Missing SOP state.
        /// </summary>
        /// <returns>The populated filter set.</returns>
        private FilterSet BuildFilterSet()
        {
            bool requireMissingSop = string.Equals(
                _cboSop.SelectedItem?.ToString(),
                MissingSopOption,
                StringComparison.OrdinalIgnoreCase);

            return new FilterSet
            {
                LineId = GetSelectedId(_cboLine),
                ShiftId = GetSelectedId(_cboShift),
                EquipmentId = GetSelectedId(_cboEquipment),
                SopId = requireMissingSop ? null : GetSelectedId(_cboSop),
                RequireMissingSop = requireMissingSop,
                StartDate = _chkUseDateRange.Checked ? _dtpStart.Value.Date : null,
                EndDate = _chkUseDateRange.Checked ? _dtpEnd.Value.Date : null
            };
        }

        /// <summary>
        /// Displays report rows in the data grid using the enabled output fields.
        /// </summary>
        /// <param name="reportResult">The report result to display.</param>
        private void DisplayTableResults(ReportResult reportResult)
        {
            _chartResults.Visible = false;
            _gridResults.Visible = true;

            _gridResults.AutoGenerateColumns = false;
            _gridResults.Columns.Clear();
            _gridResults.DataSource = null;

            if (reportResult.IncludeLine)
            {
                AddTextColumn("Line", "Line", 14);
            }

            if (reportResult.IncludeShift)
            {
                AddTextColumn("Shift", "Shift", 14);
            }

            if (reportResult.IncludeEquipment)
            {
                AddTextColumn("Equipment", "Equipment", 18);
            }

            if (reportResult.IncludeSop)
            {
                AddTextColumn("SOP", "SOP", 18);
            }

            AddTextColumn("IncidentCount", "Incident Count", 12);
            AddTextColumn("Status", "Status", 10);

            _gridResults.DataSource = reportResult.Rows;
        }

        /// <summary>
        /// Displays report rows as a column chart using group labels and incident counts.
        /// </summary>
        /// <param name="reportResult">The report result to display.</param>
        private void DisplayGraphResults(ReportResult reportResult)
        {
            _gridResults.Visible = false;
            _chartResults.Visible = true;

            string[] labels =
            [
                .. reportResult.Rows.Select(r => r.GroupValue)
            ];

            int[] values =
            [
                .. reportResult.Rows.Select(r => r.IncidentCount)
            ];

            _chartResults.Series =
            [
                new ColumnSeries<int>
                {
                    Values = values,
                    Name = "Incident Count"
                }
            ];

            _chartResults.XAxes =
            [
                new Axis
                {
                    Labels = labels
                }
            ];

            _chartResults.YAxes =
            [
                new Axis
                {
                    Name = "Incident Count"
                }
            ];
        }

        /// <summary>
        /// Retrieves the selected identifier from a combo box containing <see cref="ComboBoxItem"/> entries.
        /// Returns null if the selected value represents "All" or is not a valid item.
        /// </summary>
        /// <param name="comboBox">The combo box containing selectable items.</param>
        /// <returns>The selected identifier, or null if no specific selection is applied.</returns>
        private static int? GetSelectedId(ComboBox comboBox)
        {
            return comboBox.SelectedItem is ComboBoxItem item ? item.Value : null;
        }

        /// <summary>
        /// Adds a text column to the report data grid with the specified binding and display settings.
        /// </summary>
        /// <param name="propertyName">The data property to bind to.</param>
        /// <param name="headerText">The column header text.</param>
        /// <param name="fillWeight">The relative width of the column.</param>
        private void AddTextColumn(string propertyName, string headerText, int fillWeight)
        {
            _gridResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                HeaderText = headerText,
                FillWeight = fillWeight
            });
        }

        /// <summary>
        /// Populates a combo box with a collection of string values.
        /// </summary>
        /// <param name="comboBox">The combo box to populate.</param>
        /// <param name="values">The values to add.</param>
        private static void PopulateComboBoxWithStrings(ComboBox comboBox, IEnumerable<string> values)
        {
            comboBox.Items.Clear();

            foreach (string value in values)
            {
                comboBox.Items.Add(value);
            }
        }

        /// <summary>
        /// Adds items to a combo box after sorting them by display text.
        /// </summary>
        /// <typeparam name="T">The source item type.</typeparam>
        /// <param name="comboBox">The combo box to populate.</param>
        /// <param name="items">The items to add.</param>
        /// <param name="selector">A function that converts items into combo box entries.</param>
        private static void AddSortedItems<T>(
            ComboBox comboBox,
            IEnumerable<T> items,
            Func<T, ComboBoxItem> selector)
            where T : class
        {
            foreach (T item in items.OrderBy(x => selector(x).Text))
            {
                comboBox.Items.Add(selector(item));
            }
        }

        /// <summary>
        /// Builds the report form layout, controls, result area, and event handlers.
        /// </summary>
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
    }
}