/*
 * File: IncidentForm.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: UI Layer
 * 
 * Purpose:
 * This user control provides the interface for creating incident records.
 * It loads reference data, handles dependent line-equipment-SOP selections,
 * validates user input, and submits incidents to the application layer.
 */

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Provides the user interface for entering and submitting new incident records.
    /// </summary>
    public partial class IncidentForm : UserControl
    {
        private FlowLayoutPanel _shiftPanel = null!;
        private FlowLayoutPanel _linePanel = null!;
        private FlowLayoutPanel _equipmentPanel = null!;
        private ComboBox _cboSop = null!;

        private DateTimePicker _dtpOccurredAt = null!;
        private Button _btnCreate = null!;
        private Button _btnClear = null!;

        private readonly IncidentManager _incidentManager = new();

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
        }

        /// <summary>
        /// Initializes the incident form, builds the layout, and loads reference data.
        /// </summary>
        public IncidentForm()
        {
            InitializeComponent();
            BuildLayout();
            LoadReferenceData();

            _dtpOccurredAt.MaxDate = DateTime.Now;
        }

        /// <summary>
        /// Loads shift and line reference data into the form and prepares the dependent equipment and SOP controls.
        /// </summary>
        public void LoadReferenceData()
        {
            _shiftPanel.Controls.Clear();
            _linePanel.Controls.Clear();
            _equipmentPanel.Controls.Clear();
            _cboSop.Items.Clear();

            ReferenceDataSet referenceData = _incidentManager.GetAllReferenceData();

            foreach (Shift shift in referenceData.Shifts)
            {
                _shiftPanel.Controls.Add(new RadioButton
                {
                    Text = shift.Name,
                    Tag = shift.ShiftId,
                    AutoSize = true
                });
            }

            foreach (Line line in referenceData.Lines)
            {
                RadioButton lineRadioButton = new()
                {
                    Text = line.Name,
                    Tag = line.LineId,
                    AutoSize = true
                };

                lineRadioButton.CheckedChanged += LineRadioButton_CheckedChanged;
                _linePanel.Controls.Add(lineRadioButton);
            }

            _cboSop.DisplayMember = nameof(ComboBoxItem.Text);
            _cboSop.ValueMember = nameof(ComboBoxItem.Value);
        }

        /// <summary>
        /// Validates the selected form values, builds an incident, and submits it for creation.
        /// </summary>
        public void ProcessIncidentSubmission()
        {
            // Refresh the maximum allowed incident time before validation.
            _dtpOccurredAt.MaxDate = DateTime.Now;

            int? shiftId = GetSelectedRadioButtonValue(_shiftPanel);
            int? lineId = GetSelectedRadioButtonValue(_linePanel);
            int? equipmentId = GetSelectedRadioButtonValue(_equipmentPanel);
            int? sopId = GetSelectedSopId();

            if (!shiftId.HasValue)
            {
                MessageBox.Show("Please select a shift.");
                return;
            }

            if (!lineId.HasValue)
            {
                MessageBox.Show("Please select a line.");
                return;
            }

            if (!equipmentId.HasValue)
            {
                MessageBox.Show("Please select equipment.");
                return;
            }

            Incident incident = new()
            {
                OccurredAt = _dtpOccurredAt.Value,
                ShiftId = shiftId.Value,
                EquipmentId = equipmentId.Value,
                SopId = sopId
            };

            ValidationResult result = _incidentManager.CreateIncident(incident);

            if (!result.IsValid)
            {
                ShowValidationErrors(result);
                return;
            }

            ShowConfirmation();
            ClearForm();
        }

        /// <summary>
        /// Clears incident form selections and restores the default state.
        /// </summary>
        public void ClearForm()
        {
            _dtpOccurredAt.MaxDate = DateTime.Now;
            _dtpOccurredAt.Value = _dtpOccurredAt.MaxDate;

            ClearSelectedRadioButton(_shiftPanel);
            ClearSelectedRadioButton(_linePanel);
            ClearSelectedRadioButton(_equipmentPanel);

            _equipmentPanel.Controls.Clear();
            _cboSop.Items.Clear();
            _cboSop.SelectedIndex = -1;
        }

        /// <summary>
        /// Loads equipment when a production line is selected.
        /// </summary>
        /// <param name="sender">The selected line radio button.</param>
        /// <param name="e">The event data.</param>
        private void LineRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            if (radioButton.Tag is not int lineId)
            {
                return;
            }

            LoadEquipmentByLine(lineId);
        }

        /// <summary>
        /// Loads SOP options when equipment is selected.
        /// </summary>
        /// <param name="sender">The selected equipment radio button.</param>
        /// <param name="e">The event data.</param>
        private void EquipmentRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            if (radioButton.Tag is not int equipmentId)
            {
                return;
            }

            LoadSopsByEquipment(equipmentId);
        }

        /// <summary>
        /// Loads equipment for the selected line and resets dependent SOP selections.
        /// </summary>
        /// <param name="lineId">The selected line identifier.</param>
        private void LoadEquipmentByLine(int lineId)
        {
            _equipmentPanel.Controls.Clear();
            _cboSop.Items.Clear();
            _cboSop.SelectedIndex = -1;

            List<Equipment> equipmentList = _incidentManager.GetEquipmentByLine(lineId);

            foreach (Equipment equipment in equipmentList)
            {
                RadioButton equipmentRadioButton = new()
                {
                    Text = equipment.Name,
                    Tag = equipment.EquipmentId,
                    AutoSize = true
                };

                equipmentRadioButton.CheckedChanged += EquipmentRadioButton_CheckedChanged;
                _equipmentPanel.Controls.Add(equipmentRadioButton);
            }
        }

        /// <summary>
        /// Loads SOP options for the selected equipment.
        /// </summary>
        /// <param name="equipmentId">The selected equipment identifier.</param>
        private void LoadSopsByEquipment(int equipmentId)
        {
            _cboSop.Items.Clear();
            _cboSop.SelectedIndex = -1;

            List<SOP> sopList = _incidentManager.GetSopsByEquipment(equipmentId);

            foreach (SOP sop in sopList)
            {
                _cboSop.Items.Add(new ComboBoxItem(sop.SopId, sop.Name));
            }
        }

        /// <summary>
        /// Displays a confirmation message after an incident is created.
        /// </summary>
        private static void ShowConfirmation()
        {
            MessageBox.Show("Incident created successfully.");
        }

        /// <summary>
        /// Displays incident validation errors to the user.
        /// </summary>
        /// <param name="validationResult">The validation result to display.</param>
        private static void ShowValidationErrors(ValidationResult validationResult)
        {
            if (validationResult.Errors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, validationResult.Errors));
            }
            else
            {
                MessageBox.Show("Validation failed. Correct the input and try again.");
            }
        }

        /// <summary>
        /// Clears the selected radio button in a panel.
        /// </summary>
        /// <param name="panel">The panel containing radio buttons.</param>
        private static void ClearSelectedRadioButton(FlowLayoutPanel panel)
        {
            foreach (Control control in panel.Controls)
            {
                if (control is RadioButton radioButton)
                {
                    radioButton.Checked = false;
                }
            }
        }

        /// <summary>
        /// Gets the integer value stored in the selected radio button.
        /// </summary>
        /// <param name="panel">The panel containing radio buttons.</param>
        /// <returns>The selected radio button value, or null when no radio button is selected.</returns>
        private static int? GetSelectedRadioButtonValue(FlowLayoutPanel panel)
        {
            foreach (Control control in panel.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    if (radioButton.Tag is int selectedValue)
                    {
                        return selectedValue;
                    }
                }
            }

            return null; 
        }

        /// <summary>
        /// Gets the selected SOP identifier from the combo box.
        /// </summary>
        /// <returns>The selected SOP identifier, or null when no SOP is selected.</returns>
        private int? GetSelectedSopId()
        {
            if (_cboSop.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Value;
            }

            return null;
        }

        /// <summary>
        /// Builds the incident entry layout and wires form button events.
        /// </summary>
        private void BuildLayout()
        {
            this.BackColor = Color.White;

            Label lblTitle = new()
            {
                Text = "New Incident Entry",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(20),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            TableLayoutPanel entryPanel = new()
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 8,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 150),
                    new ColumnStyle(SizeType.Percent, 100)
                },
            };

            _dtpOccurredAt = new()
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MM/dd/yyyy HH:mm",
                ShowUpDown = false,
                Width = 160
            };

            _shiftPanel = new()
            {
                AutoSize = true
            };

            _linePanel = new()
            {
                AutoSize = true
            };

            _equipmentPanel = new()
            {
                AutoSize = true
            };

            _cboSop = new()
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _btnCreate = new()
            {
                Text = "Create",
                Width = 120,
                Height = 35
            };

            _btnClear = new()
            {
                Text = "Clear",
                Width = 120,
                Height = 35
            };

            FlowLayoutPanel buttonPanel = new()
            {
                AutoSize = true
            };
            buttonPanel.Controls.Add(_btnCreate);
            buttonPanel.Controls.Add(_btnClear);

            _btnCreate.Click += (s, e) => ProcessIncidentSubmission();
            _btnClear.Click += (s, e) => ClearForm();

            entryPanel.Controls.Add(new Label() { Text = "Occurred At", AutoSize = true }, 0, 0);
            entryPanel.Controls.Add(_dtpOccurredAt, 1, 0);

            entryPanel.Controls.Add(new Label() { Text = "Shift", AutoSize = true }, 0, 1);
            entryPanel.Controls.Add(_shiftPanel, 1, 1);

            entryPanel.Controls.Add(new Label() { Text = "Line", AutoSize = true }, 0, 2);
            entryPanel.Controls.Add(_linePanel, 1, 2);

            entryPanel.Controls.Add(new Label() { Text = "Equipment", AutoSize = true }, 0, 3);
            entryPanel.Controls.Add(_equipmentPanel, 1, 3);

            entryPanel.Controls.Add(new Label() { Text = "SOP", AutoSize = true }, 0, 4);
            entryPanel.Controls.Add(_cboSop, 1, 4);

            entryPanel.Controls.Add(new Label(), 0, 5);
            entryPanel.Controls.Add(buttonPanel, 1, 5);

            Panel container = new()
            {
                Dock = DockStyle.Fill
            };

            container.Controls.Add(entryPanel);
            container.Controls.Add(lblTitle);

            lblTitle.Dock = DockStyle.Top;
            entryPanel.Dock = DockStyle.Top;

            Controls.Add(container);

        }

    }
}
