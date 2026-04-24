/*
 * File: RuleConfigForm.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: User Interface Layer
 * 
 * Purpose:
 * This user control provides the interface for viewing, editing, saving,
 * and resetting rule configuration settings.
 */

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Provides the user interface for viewing and editing rule configuration settings.
    /// </summary>
    public partial class RuleConfigForm : UserControl
    {
        private NumericUpDown _nudThreshold = null!;
        private ComboBox _cboGroupingType = null!;
        private ComboBox _cboTimeWindow = null!;
        private CheckBox _chkFlagEnabled = null!;
        private Button _btnSave = null!;
        private Button _btnResetDefaults = null!;
        private Label _lblStatus = null!;

        private readonly RuleEvaluator _ruleEvaluator = new();

        private static readonly string[] GroupingOptions = RuleConfig.GroupingOptions;
        private static readonly string[] TimeWindowOptions = RuleConfig.TimeWindowOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleConfigForm"/> class.
        /// </summary>
        public RuleConfigForm()
        {
            InitializeComponent();
            BuildLayout();
            LoadCurrentRules();
        }

        /// <summary>
        /// Loads the current rule configuration and applies it to the form controls.
        /// </summary>
        public void LoadCurrentRules()
        {
            RuleConfig ruleConfig = _ruleEvaluator.LoadCurrentRuleConfig();
            ApplyRuleConfigToControls(ruleConfig);
            SetStatus("Current rule configuration loaded.");
        }

        /// <summary>
        /// Validates the current UI values and attempts to save the rule configuration.
        /// </summary>
        public void ProcessRuleConfiguration()
        {
            RuleConfig ruleConfig = BuildRuleConfigFromControls();
            ValidationResult validationResult = _ruleEvaluator.SaveRuleConfig(ruleConfig);

            if (!validationResult.IsValid)
            {
                ShowValidationErrors(validationResult);
                return;
            }

            SetStatus("Rule configuration saved successfully.");
        }

        /// <summary>
        /// Restores default rule values in the UI and data store.
        /// </summary>
        public void ResetDefaults()
        {
            RuleConfig defaultConfig = _ruleEvaluator.ResetRuleConfigToDefaults();
            ApplyRuleConfigToControls(defaultConfig);
            SetStatus("Default rule values restored.");
        }

        /// <summary>
        /// Builds a rule configuration from the current form values.
        /// </summary>
        /// <returns>The populated rule configuration.</returns>
        private RuleConfig BuildRuleConfigFromControls()
        {
            return new RuleConfig
            {
                ThresholdValue = _nudThreshold.Value,
                GroupingType = _cboGroupingType.SelectedItem?.ToString() ?? string.Empty,
                TimeWindow = _cboTimeWindow.SelectedItem?.ToString() ?? string.Empty,
                FlagEnabled = _chkFlagEnabled.Checked
            };
        }

        /// <summary>
        /// Applies rule configuration values to threshold, grouping, time window, and flagging controls.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to display.</param>
        private void ApplyRuleConfigToControls(RuleConfig ruleConfig)
        {
            _nudThreshold.Value = Math.Min(_nudThreshold.Maximum, Math.Max(_nudThreshold.Minimum, ruleConfig.ThresholdValue));
            SelectComboValue(_cboGroupingType, ruleConfig.GroupingType, GroupingOptions[0]);
            SelectComboValue(_cboTimeWindow, ruleConfig.TimeWindow, TimeWindowOptions[0]);
            _chkFlagEnabled.Checked = ruleConfig.FlagEnabled;
        }

        /// <summary>
        /// Displays rule validation errors to the user.
        /// </summary>
        /// <param name="validationResult">The validation result to display.</param>
        private void ShowValidationErrors(ValidationResult validationResult)
        {
            SetStatus("Rule configuration contains validation errors.");

            if (validationResult.Errors.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, validationResult.Errors));
            }
            else
            {
                MessageBox.Show("Validation failed. Correct the input and try again.");
            }
        }

        /// <summary>
        /// Updates the status label, using the default status text when the message is blank.
        /// </summary>
        /// <param name="message">The status message to display.</param>
        private void SetStatus(string message)
        {
            _lblStatus.Text = string.IsNullOrWhiteSpace(message)
                ? "No changes saved yet."
                : message;
        }

        /// <summary>
        /// Selects a combo box value or falls back to a default value.
        /// </summary>
        /// <param name="comboBox">The combo box to update.</param>
        /// <param name="value">The preferred value.</param>
        /// <param name="fallback">The fallback value.</param>
        private static void SelectComboValue(ComboBox comboBox, string? value, string fallback)
        {
            string resolvedValue = string.IsNullOrWhiteSpace(value) ? fallback : value;

            if (comboBox.Items.Contains(resolvedValue))
            {
                comboBox.SelectedItem = resolvedValue;
            }
            else
            {
                comboBox.SelectedItem = fallback;
            }
        }

        /// <summary>
        /// Builds the rule configuration form layout and wires control events.
        /// </summary>
        private void BuildLayout()
        {
            BackColor = Color.White;

            Label lblTitle = new()
            {
                Text = "Rule Configuration",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(20),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            TableLayoutPanel formLayout = new()
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 6,
                ColumnStyles =
        {
            new ColumnStyle(SizeType.Absolute, 150),
            new ColumnStyle(SizeType.Percent, 100)
        }
            };

            _nudThreshold = new NumericUpDown
            {
                DecimalPlaces = 0,
                Minimum = 0,
                Maximum = 100000,
                Increment = 1,
                Width = 160
            };

            _cboGroupingType = new ComboBox
            {
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboGroupingType.Items.AddRange(GroupingOptions);

            _cboTimeWindow = new ComboBox
            {
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboTimeWindow.Items.AddRange(TimeWindowOptions);

            _chkFlagEnabled = new CheckBox
            {
                Text = "Enable result flagging",
                AutoSize = true
            };

            _btnSave = new Button
            {
                Text = "Save",
                Width = 120,
                Height = 36
            };

            _btnResetDefaults = new Button
            {
                Text = "Reset Defaults",
                Width = 120,
                Height = 36
            };

            FlowLayoutPanel buttonPanel = new()
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                WrapContents = false,
                Margin = new Padding(0)
            };
            buttonPanel.Controls.Add(_btnSave);
            buttonPanel.Controls.Add(_btnResetDefaults);

            _lblStatus = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(520, 0),
                Text = "No changes saved yet.",
                Margin = new Padding(0)
            };

            Panel statusPanel = new()
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(245, 247, 250),
                Margin = new Padding(0)
            };
            statusPanel.Controls.Add(_lblStatus);

            _btnSave.Click += (s, e) => ProcessRuleConfiguration();
            _btnResetDefaults.Click += (s, e) => ResetDefaults();

            formLayout.Controls.Add(new Label { Text = "Threshold Value", AutoSize = true }, 0, 0);
            formLayout.Controls.Add(_nudThreshold, 1, 0);

            formLayout.Controls.Add(new Label { Text = "Grouping Type", AutoSize = true }, 0, 1);
            formLayout.Controls.Add(_cboGroupingType, 1, 1);

            formLayout.Controls.Add(new Label { Text = "Time Window", AutoSize = true }, 0, 2);
            formLayout.Controls.Add(_cboTimeWindow, 1, 2);

            formLayout.Controls.Add(new Label { Text = "Flagging", AutoSize = true }, 0, 3);
            formLayout.Controls.Add(_chkFlagEnabled, 1, 3);

            formLayout.Controls.Add(new Label(), 0, 4);
            formLayout.Controls.Add(buttonPanel, 1, 4);

            formLayout.Controls.Add(new Label { Text = "Status", AutoSize = true }, 0, 5);
            formLayout.Controls.Add(statusPanel, 1, 5);

            Panel container = new()
            {
                Dock = DockStyle.Fill
            };

            container.Controls.Add(formLayout);
            container.Controls.Add(lblTitle);

            lblTitle.Dock = DockStyle.Top;
            formLayout.Dock = DockStyle.Top;

            Controls.Add(container);
        }
    }
}