/*
 * File: IncidentForm.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: UI Layer
 * 
 * Purpose:
 * This user control provides the interface for creating new incidents within the system.
 * It dynamically loads and displays reference data (shifts, lines, equipment, and SOPs),
 * supports dependent selections (line → equipment → SOP), performs UI-level validation,
 * constructs an Incident domain object, and passes it to the application layer for processing
 * and persistence.
*/

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Represents a user control for entering and submitting new incident reports, providing a structured form with
    /// fields for shift, line, equipment, SOP, and occurrence time. Supports validation, user feedback, and dynamic
    /// population of form controls based on user selections.
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
        /// Helper class to represent items in the SOP combo box, allowing for easy retrieval of the selected SOP's unique identifier (SopId) when processing the incident submission. Each ComboBoxItem instance holds both the display text (the SOP name) and the corresponding value (the SopId) for use in the UI and backend logic.
        /// </summary>
        private sealed class ComboBoxItem
        {
            /// <summary>
            /// Gets the value represented by this instance.
            /// </summary>
            public int Value { get; }

            /// <summary>
            /// Gets the text content associated with this instance.
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// Initializes a new instance of the ComboBoxItem class with the specified value and display text.
            /// </summary>
            /// <param name="value">The integer value associated with the item. Typically used as the item's underlying data or identifier.</param>
            /// <param name="text">The display text shown for the item in the combo box.</param>
            public ComboBoxItem(int value, string text)
            {
                Value = value;
                Text = text;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentForm"/> class, builds the layout, loads reference data, and sets the maximum allowed date/time to now.
        /// </summary>
        public IncidentForm()
        {
            InitializeComponent();
            BuildLayout();
            LoadReferenceData();

            // Guards against creating a time and date in the future, which would be invalid for an incident occurrence time
            _dtpOccurredAt.MaxDate = DateTime.Now;
        }

        /// <summary>
        /// Clears all controls before loading reference data. Populates for shift and line for the default form state.
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
        /// Processes incident submission starting with UI-level validation before constructing the Incident object and passing it to the application layer. If validation checks are passed, the method attempts to create the incident and provides user feedback based on the success or failure of the operation, including displaying any validation errors returned from the application layer if creation fails.
        /// </summary>
        public void ProcessIncidentSubmission()
        {
            // Refresh "now" to reflect submit time
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
        /// Clears all user input fields and resets the form to its default state.
        /// </summary>
        /// <remarks>Use this method to reset the form before entering new data or after completing an
        /// operation. All text fields, selection controls, and panels are cleared or reset to their initial
        /// values.</remarks>
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
        /// Handles the CheckedChanged event for a line selection RadioButton, updating the displayed equipment based on
        /// the selected line.
        /// </summary>
        /// <remarks>This handler assumes that the sender is a RadioButton with its Tag property set to an
        /// integer line identifier. Only processes the event when the RadioButton is checked.</remarks>
        /// <param name="sender">The RadioButton control that triggered the event. Expected to have its Tag property set to the line
        /// identifier.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void LineRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            int lineId = (int)radioButton.Tag;
            LoadEquipmentByLine(lineId);
        }

        /// <summary>
        /// Handles the CheckedChanged event for an equipment selection RadioButton, updating the displayed SOPs based on
        /// the selected equipment.
        /// </summary>
        /// <remarks>This handler assumes that the sender is a RadioButton with its Tag property set to an
        /// integer equipment identifier. Only processes the event when the RadioButton is checked.</remarks>
        /// <param name="sender">The RadioButton control that triggered the event. Expected to have its Tag property set to the equipment
        /// identifier.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        private void EquipmentRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            int equipmentId = (int)radioButton.Tag;
            LoadSopsByEquipment(equipmentId);
        }

        /// <summary>
        /// Populates the equipment panel with equipment associated with the specified line.
        /// </summary>
        /// <remarks>Clears any existing controls before loading new equipment. This method
        /// updates the UI to reflect the equipment available for the selected line.</remarks>
        /// <param name="lineId">The unique identifier of the line for which to load equipment.</param>
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
        /// Populates the SOP selection control with SOPs associated with the specified
        /// equipment.
        /// </summary>
        /// <remarks>Clears any existing items in the SOP selection control before loading new entries.
        /// This method updates the UI to reflect the SOPs available for the selected equipment.</remarks>
        /// <param name="equipmentId">The unique identifier of the equipment for which to load associated SOPs.</param>
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
        /// Displays a confirmation message indicating that an incident has been created successfully.
        /// </summary>
        private void ShowConfirmation()
        {
            MessageBox.Show("Incident created successfully.");
        }

        /// <summary>
        /// Displays validation error messages to the user in a message box.
        /// </summary>
        /// <remarks>If the validation result contains errors, each error message is shown in a single
        /// message box separated by line breaks. If there are no specific errors, a generic validation failure message
        /// is displayed. This method is intended for use in interactive applications where user feedback is
        /// required.</remarks>
        /// <param name="validationResult">The result of a validation operation containing any validation errors to display. Cannot be null.</param>
        private void ShowValidationErrors(ValidationResult validationResult)
        {
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
        /// Accepts a parameter to identify the panel containing the radio buttons and iterates through its controls to uncheck any selected radio buttons, effectively clearing the user's selection for that category.
        /// </summary>
        /// <param name="panel">The FlowLayoutPanel containing the radio buttons to be cleared.</param>
        private void ClearSelectedRadioButton(FlowLayoutPanel panel)
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
        /// Retrieves the integer value associated with the selected radio button in the specified flow layout panel.
        /// </summary>
        /// <param name="panel">The FlowLayoutPanel containing the radio buttons to search.</param>
        /// <returns>The integer value stored in the Tag property of the selected radio button, or null if no radio button is selected.</returns>
        private int? GetSelectedRadioButtonValue(FlowLayoutPanel panel)
        {
            foreach (Control control in panel.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    return (int)radioButton.Tag;
                }
            }

            return null; 
        }

        /// <summary>
        /// Retrieves the identifier of the currently selected SOP item from the combo box.
        /// </summary>
        /// <returns>The identifier of the selected SOP item if an item is selected; otherwise, null.</returns>
        private int? GetSelectedSopId()
        {
            if (_cboSop.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Value;
            }

            return null;
        }

        /// <summary>
        /// Constructs the layout of the incident entry form by creating and configuring various controls such as labels, date/time picker, radio button panels, combo box, and buttons. The controls are arranged using a TableLayoutPanel for a structured and organized appearance. Event handlers are attached to the buttons to handle user interactions for creating an incident and clearing the form.
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
