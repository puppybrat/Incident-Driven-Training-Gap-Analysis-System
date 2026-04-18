using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    public partial class IncidentForm : UserControl
    {
        // Initializes the panels and combo box as null, to be populated later in the BuildLayout method
        private FlowLayoutPanel _linePanel = null!;
        private FlowLayoutPanel _shiftPanel = null!;
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

        // Builds the layout of the IncidentEntryControl by creating and arranging various controls such as labels, text boxes, date pickers, panels for radio buttons, and a combo box for SOP selection
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

            _linePanel = new FlowLayoutPanel()
            {
                AutoSize = true
            };

            _shiftPanel = new FlowLayoutPanel()
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

            formLayout.Controls.Add(new Label() { Text = "Line", AutoSize = true }, 0, 2);
            formLayout.Controls.Add(_linePanel, 1, 2);

            formLayout.Controls.Add(new Label() { Text = "Shift", AutoSize = true }, 0, 3);
            formLayout.Controls.Add(_shiftPanel, 1, 3);

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
            _linePanel.Controls.Clear();
            _shiftPanel.Controls.Clear();
            _equipmentPanel.Controls.Clear();
            _cboSop.Items.Clear();

            ReferenceDataSet referenceData = _incidentManager.GetAllReferenceData();

            foreach (Line line in referenceData.Lines)
            {
                _linePanel.Controls.Add(new RadioButton
                {
                    Text = line.Name,
                    Tag = line.LineId,
                    AutoSize = true
                });
            }

            foreach (Shift shift in referenceData.Shifts)
            {
                _shiftPanel.Controls.Add(new RadioButton
                {
                    Text = shift.Name,
                    Tag = shift.ShiftId,
                    AutoSize = true
                });
            }

            foreach (Equipment equipment in referenceData.Equipment)
            {
                _equipmentPanel.Controls.Add(new RadioButton
                {
                    Text = equipment.Name,
                    Tag = equipment.EquipmentId,
                    AutoSize = true
                });
            }

            foreach (SOP sop in referenceData.Sops)
            {
                _cboSop.Items.Add(new ComboBoxItem(sop.SopId, sop.Name));
            }

            _cboSop.DisplayMember = nameof(ComboBoxItem.Text);
            _cboSop.ValueMember = nameof(ComboBoxItem.Value);
        }

        public void ProcessIncidentSubmission()
        {
            MessageBox.Show("Incident submission stub called.");
        }

        public void ClearForm()
        {
            _txtId.Clear();
            _dtpOccurredAt.Value = DateTime.Now;
            _cboSop.SelectedIndex = -1;

            ClearSelectedRadioButton(_linePanel);
            ClearSelectedRadioButton(_shiftPanel);
            ClearSelectedRadioButton(_equipmentPanel);
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
