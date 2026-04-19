using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Provides the user interface for configuring rule thresholds and evaluation behavior.
    /// This control follows the same code-first WinForms pattern used by IncidentForm.
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

        private readonly RuleConfigRepository _ruleConfigRepository = new();

        private static readonly string[] GroupingOptions =
        {
            "Shift",
            "Line",
            "Equipment",
            "SOP"
        };

        private static readonly string[] TimeWindowOptions =
        {
            "7 days",
            "30 days",
            "90 days",
            "120 days"
        };

        public RuleConfigForm()
        {
            InitializeComponent();
            BuildLayout();
            LoadCurrentRules();
        }

        /// <summary>
        /// Loads the persisted rule configuration and applies it to the UI.
        /// If no persisted configuration is available, defaults are loaded.
        /// </summary>
        public void LoadCurrentRules()
        {
            RuleConfig ruleConfig = _ruleConfigRepository.LoadRuleConfig();
            ApplyRuleConfigToControls(EnsureValidSelections(ruleConfig));
            SetStatus("Current rule configuration loaded.");
        }

        /// <summary>
        /// Validates the current UI values and attempts to save the rule configuration.
        /// </summary>
        public void ProcessRuleConfiguration()
        {
            RuleConfig ruleConfig = BuildRuleConfigFromControls();
            ValidationResult validationResult = ValidateRuleConfig(ruleConfig);

            if (!validationResult.IsValid)
            {
                ShowValidationErrors(validationResult);
                return;
            }

            bool saveSucceeded = _ruleConfigRepository.SaveRuleConfig(ruleConfig);

            if (saveSucceeded)
            {
                SetStatus("Rule configuration saved successfully.");
                MessageBox.Show("Rule configuration saved successfully.");
            }
            else
            {
                SetStatus("Unable to save rule configuration.");
                MessageBox.Show("Unable to save rule configuration.");
            }
        }

        /// <summary>
        /// Restores default rule values in both the UI and the data store if supported.
        /// </summary>
        public void ResetDefaults()
        {
            RuleConfig defaultConfig = _ruleConfigRepository.ResetRuleConfigToDefaults();
            defaultConfig = ApplyFallbackDefaults(defaultConfig);
            ApplyRuleConfigToControls(defaultConfig);
            SetStatus("Default rule values restored.");
        }

        private void BuildLayout()
        {
            BackColor = Color.White;

            Label titleLabel = new()
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
                RowCount = 7,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 180),
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
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboGroupingType.Items.AddRange(GroupingOptions);

            _cboTimeWindow = new ComboBox
            {
                Width = 220,
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
                Text = "Save / Apply",
                Width = 140,
                Height = 35
            };

            _btnResetDefaults = new Button
            {
                Text = "Reset Defaults",
                Width = 140,
                Height = 35
            };

            _lblStatus = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(500, 0),
                Text = string.Empty
            };

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
            formLayout.Controls.Add(_btnSave, 1, 4);

            formLayout.Controls.Add(new Label(), 0, 5);
            formLayout.Controls.Add(_btnResetDefaults, 1, 5);

            formLayout.Controls.Add(new Label { Text = "Status", AutoSize = true }, 0, 6);
            formLayout.Controls.Add(_lblStatus, 1, 6);

            Panel container = new()
            {
                Dock = DockStyle.Fill
            };

            container.Controls.Add(formLayout);
            container.Controls.Add(titleLabel);

            Controls.Add(container);
        }

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

        private void ApplyRuleConfigToControls(RuleConfig ruleConfig)
        {
            _nudThreshold.Value = Math.Min(_nudThreshold.Maximum, Math.Max(_nudThreshold.Minimum, ruleConfig.ThresholdValue));
            SelectComboValue(_cboGroupingType, ruleConfig.GroupingType, GroupingOptions[0]);
            SelectComboValue(_cboTimeWindow, ruleConfig.TimeWindow, TimeWindowOptions[0]);
            _chkFlagEnabled.Checked = ruleConfig.FlagEnabled;
        }

        private RuleConfig EnsureValidSelections(RuleConfig ruleConfig)
        {
            return ApplyFallbackDefaults(ruleConfig);
        }

        private RuleConfig ApplyFallbackDefaults(RuleConfig? ruleConfig)
        {
            ruleConfig ??= new RuleConfig();

            if (ruleConfig.ThresholdValue < 0)
            {
                ruleConfig.ThresholdValue = 5;
            }

            if (!GroupingOptions.Contains(ruleConfig.GroupingType))
            {
                ruleConfig.GroupingType = GroupingOptions[0];
            }

            if (!TimeWindowOptions.Contains(ruleConfig.TimeWindow))
            {
                ruleConfig.TimeWindow = TimeWindowOptions[1];
            }

            return ruleConfig;
        }

        private ValidationResult ValidateRuleConfig(RuleConfig ruleConfig)
        {
            ValidationResult validationResult = new()
            {
                IsValid = true
            };

            if (ruleConfig.ThresholdValue < 0)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Threshold must be zero or greater.");
            }

            if (string.IsNullOrWhiteSpace(ruleConfig.GroupingType) || !GroupingOptions.Contains(ruleConfig.GroupingType))
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Please select a valid grouping type.");
            }

            if (string.IsNullOrWhiteSpace(ruleConfig.TimeWindow) || !TimeWindowOptions.Contains(ruleConfig.TimeWindow))
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Please select a valid time window.");
            }

            return validationResult;
        }

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

        private void SetStatus(string message)
        {
            _lblStatus.Text = message;
        }

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
    }
}