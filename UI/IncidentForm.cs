using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    public partial class IncidentForm : UserControl
    {
        // Initializes the panels and combo box as null, to be populated later in the BuildLayout method
        private FlowLayoutPanel _shiftPanel = null!;
        private FlowLayoutPanel _linePanel = null!;
        private FlowLayoutPanel _equipmentPanel = null!;
        private ComboBox _cboSop = null!;

        private TextBox _txtId = null!;
        private DateTimePicker _dtpOccurredAt = null!;
        private Button _btnCreate = null!;
        private Button _btnClear = null!;

        private readonly IncidentManager _incidentManager = new();

        // Constructor for the IncidentEntryControl, which initializes the component and builds the layout
        public IncidentForm()
        {
            InitializeComponent();
            BuildLayout();
            LoadReferenceData();

            // Guards against creating a time and date in the future, which would be invalid for an incident occurrence time
            _dtpOccurredAt.MaxDate = DateTime.Now;
        }

        // Defines a private class to represent items in the combo box, storing both the value (ID) and text (name) for each item
        private sealed class ComboBoxItem
        {
            public int Value { get; }
            public string Text { get; }

            // Constructor for the ComboBoxItem class, which initializes the Value and Text properties based on the provided parameters
            public ComboBoxItem(int value, string text)
            {
                Value = value;
                Text = text;
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

        private int? GetSelectedSopId()
        {
            if (_cboSop.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Value;
            }

            return null;
        }

        /// <summary>
        /// Initializes and arranges the user interface elements for the incident entry form.
        /// </summary>
        /// <remarks>This method sets up the layout, labels, input controls, and action buttons required
        /// for entering a new incident. It should be called during control initialization to ensure all UI components
        /// are properly configured and displayed. The method is intended for internal use within the control and is not
        /// designed to be called directly by external code.</remarks>
        private void BuildLayout()
        {
            this.BackColor = Color.White;

            Label titleLabel = new()
            {
                Text = "New Incident Entry",
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
                RowCount = 8,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 150),
                    new ColumnStyle(SizeType.Percent, 100)
                },
            };

            _txtId = new TextBox();

            _dtpOccurredAt = new DateTimePicker()
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MM/dd/yyyy HH:mm",
                ShowUpDown = false,
                Width = 160
            };

            _btnCreate = new Button()
            {
                Text = "Create",
                Width = 120,
                Height = 35
            };

            _btnClear = new Button()
            {
                Text = "Clear",
                Width = 120,
                Height = 35
            };

            _shiftPanel = new FlowLayoutPanel()
            {
                AutoSize = true
            };

            _linePanel = new FlowLayoutPanel()
            {
                AutoSize = true
            };

            _equipmentPanel = new FlowLayoutPanel()
            {
                AutoSize = true
            };

            _cboSop = new ComboBox()
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _btnCreate.Click += (s, e) => ProcessIncidentSubmission();
            _btnClear.Click += (s, e) => ClearForm();

            // Adds labels and corresponding controls to the form layout in a structured manner, with labels in the first column and controls in the second column
            formLayout.Controls.Add(new Label() { Text = "ID", AutoSize = true }, 0, 0);
            formLayout.Controls.Add(_txtId, 1, 0);

            formLayout.Controls.Add(new Label() { Text = "Occurred At", AutoSize = true }, 0, 1);
            formLayout.Controls.Add(_dtpOccurredAt, 1, 1);

            formLayout.Controls.Add(new Label() { Text = "Shift", AutoSize = true }, 0, 2);
            formLayout.Controls.Add(_shiftPanel, 1, 2);

            formLayout.Controls.Add(new Label() { Text = "Line", AutoSize = true }, 0, 3);
            formLayout.Controls.Add(_linePanel, 1, 3);

            formLayout.Controls.Add(new Label() { Text = "Equipment", AutoSize = true }, 0, 4);
            formLayout.Controls.Add(_equipmentPanel, 1, 4);

            formLayout.Controls.Add(new Label() { Text = "SOP", AutoSize = true }, 0, 5);
            formLayout.Controls.Add(_cboSop, 1, 5);

            formLayout.Controls.Add(new Label(), 0, 6);
            formLayout.Controls.Add(_btnCreate, 1, 6);

            formLayout.Controls.Add(new Label(), 0, 7);
            formLayout.Controls.Add(_btnClear, 1, 7);

            // Creates a container panel to hold the form layout and the title label, and sets their docking properties to arrange them properly within the IncidentEntryControl
            Panel container = new();
            container.Dock = DockStyle.Fill;
            container.Controls.Add(formLayout);
            container.Controls.Add(titleLabel);

            titleLabel.Dock = DockStyle.Top;
            formLayout.Dock = DockStyle.Top;

            // Adds the container panel to the IncidentEntryControl, which will hold all the form controls and the title label
            this.Controls.Add(container);

        }

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

        private void LineRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            int lineId = (int)radioButton.Tag;
            LoadEquipmentByLine(lineId);
        }

        private void EquipmentRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
            {
                return;
            }

            int equipmentId = (int)radioButton.Tag;
            LoadSopsByEquipment(equipmentId);
        }

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

        public void ProcessIncidentSubmission()
        {
            // Refresh "now" to reflect submit time
            _dtpOccurredAt.MaxDate = DateTime.Now;

            if (!int.TryParse(_txtId.Text, out int incidentId))
            {
                MessageBox.Show("Please enter a valid numeric incident ID.");
                return;
            }

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
                IncidentId = incidentId,
                OccurredAt = _dtpOccurredAt.Value,
                ShiftId = shiftId.Value,
                EquipmentId = equipmentId.Value,
                SopId = sopId
            };

            bool created = _incidentManager.CreateIncident(incident);

            if (created)
            {
                ShowConfirmation();
                ClearForm();
            }
            else
            {
                ValidationResult validationResult = _incidentManager.ValidateIncident(incident);

                if (!validationResult.IsValid && validationResult.Errors.Any())
                {
                    MessageBox.Show(string.Join(Environment.NewLine, validationResult.Errors));
                }
                else
                {
                    MessageBox.Show("Failed to create incident. No validation errors available.");
                }
            }
        }

        /// <summary>
        /// Clears all user input fields and resets the form to its default state.
        /// </summary>
        /// <remarks>Use this method to reset the form before entering new data or after completing an
        /// operation. All text fields, selection controls, and panels are cleared or reset to their initial
        /// values.</remarks>
        public void ClearForm()
        {
            _txtId.Clear();

            _dtpOccurredAt.MaxDate = DateTime.Now;
            _dtpOccurredAt.Value = _dtpOccurredAt.MaxDate;

            ClearSelectedRadioButton(_shiftPanel);
            ClearSelectedRadioButton(_linePanel);
            ClearSelectedRadioButton(_equipmentPanel);

            _equipmentPanel.Controls.Clear();
            _cboSop.Items.Clear();
            _cboSop.SelectedIndex = -1;
        }

        public void ShowConfirmation()
        {
            MessageBox.Show("Incident created successfully.");
        }

        public void ShowValidationErrors()
        {
            MessageBox.Show("Validation failed. Correct the input and try again.");
        }
    }
}
