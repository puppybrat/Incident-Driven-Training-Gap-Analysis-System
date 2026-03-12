using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System
{
    public partial class IncidentEntryControl : UserControl
    {
        // Opens the connection to the SQLite database file
        private readonly string _databaseConnectionString =
            "Data Source=training_gap_analysis.db";

        // Initializes the panels and combo box as null, to be populated later in the BuildLayout method
        private FlowLayoutPanel _linePanel = null!;
        private FlowLayoutPanel _shiftPanel = null!;
        private FlowLayoutPanel _equipmentPanel = null!;
        private ComboBox _cboSop = null!;

        // Constructor for the IncidentEntryControl, which initializes the component and builds the layout
        public IncidentEntryControl()
        {
            InitializeComponent();
            BuildLayout();
        }

        // Loads radio buttons into the specified panel based on data from the specified database table and columns
        private void LoadRadioButtonsFromTable(
            FlowLayoutPanel panel,
            string tableName,
            string idColumn,
            string textColumn)
        {
            // Clears any existing controls from the panel before loading new radio buttons
            panel.Controls.Clear();

            // Establishes a connection to the SQLite database using the provided connection string
            using SqliteConnection connection = new(_databaseConnectionString);
            connection.Open();

            // Constructs a SQL query to select the ID and text columns from the specified table, ordered by the text column
            string sql = $"SELECT {idColumn}, {textColumn} FROM {tableName} ORDER BY {textColumn}";

            using SqliteCommand command = new(sql, connection);
            using SqliteDataReader reader = command.ExecuteReader();

            // Iterates through the results of the query, creating a new radio button for each record and adding it to the panel
            while (reader.Read())
            {
                RadioButton radioButton = new()
                {
                    Text = reader.GetString(1),
                    Tag = reader.GetInt32(0),
                    AutoSize = true
                };

                panel.Controls.Add(radioButton);
            }
        }

        // Loads the SOP combo box with items from the SOP table in the database, displaying the name and storing the ID as the value
        private void LoadSopComboBox()
        {
            _cboSop = new ComboBox();

            // Establishes a connection to the SQLite database using the provided connection string
            using SqliteConnection connection = new(_databaseConnectionString);
            connection.Open();

            // Constructs a SQL query to select the SOP ID and name from the SOP table, ordered by the name
            string sql = "SELECT sopId, name FROM SOP ORDER BY name";

            using SqliteCommand command = new(sql, connection);
            using SqliteDataReader reader = command.ExecuteReader();

            // Iterates through the results of the query, creating a new ComboBoxItem for each record and adding it to the SOP combo box
            while (reader.Read())
            {
                _cboSop.Items.Add(new ComboBoxItem(
                    reader.GetInt32(0),
                    reader.GetString(1)));
            }

            // Sets the display member and value member of the combo box to show the text and store the value of the ComboBoxItem objects
            _cboSop.DisplayMember = nameof(ComboBoxItem.Text);
            _cboSop.ValueMember = nameof(ComboBoxItem.Value);
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

        // Builds the layout of the IncidentEntryControl by creating and arranging various controls such as labels, text boxes, date pickers, panels for radio buttons, and a combo box for SOP selection
        private void BuildLayout()
        {
            this.BackColor = Color.White;

            Label titleLabel = new()
            {
                Text = "New Incident Entry",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(10),
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
                    new ColumnStyle(SizeType.Absolute, 150),
                    new ColumnStyle(SizeType.Percent, 100)
                },
            };

            TextBox txtId = new();
            DateTimePicker dtpOccurredAt = new();


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

            Button btnCreate = new()
            {
                Text = "Create",
                Width = 120,
                Height = 35
            };

            // Adds labels and corresponding controls to the form layout in a structured manner, with labels in the first column and controls in the second column
            formLayout.Controls.Add(new Label() { Text = "ID", AutoSize = true }, 0, 0);
            formLayout.Controls.Add(txtId, 1, 0);

            formLayout.Controls.Add(new Label() { Text = "Occurred At", AutoSize = true }, 0, 1);
            formLayout.Controls.Add(dtpOccurredAt, 1, 1);

            formLayout.Controls.Add(new Label() { Text = "Line", AutoSize = true }, 0, 2);
            formLayout.Controls.Add(_linePanel, 1, 2);

            formLayout.Controls.Add(new Label() { Text = "Shift", AutoSize = true }, 0, 3);
            formLayout.Controls.Add(_shiftPanel, 1, 3);

            formLayout.Controls.Add(new Label() { Text = "Equipment", AutoSize = true }, 0, 4);
            formLayout.Controls.Add(_equipmentPanel, 1, 4);

            formLayout.Controls.Add(new Label() { Text = "SOP", AutoSize = true }, 0, 5);
            formLayout.Controls.Add(_cboSop, 1, 5);

            formLayout.Controls.Add(new Label(), 0, 6);
            formLayout.Controls.Add(btnCreate, 1, 6);

            // Creates a container panel to hold the form layout and the title label, and sets their docking properties to arrange them properly within the IncidentEntryControl
            Panel container = new();
            container.Dock = DockStyle.Fill;
            container.Controls.Add(formLayout);
            container.Controls.Add(titleLabel);

            titleLabel.Dock = DockStyle.Top;
            formLayout.Dock = DockStyle.Top;

            // Adds the container panel to the IncidentEntryControl, which will hold all the form controls and the title label
            this.Controls.Add(container);

            // Loads the radio buttons for Line, Shift, and Equipment from their respective tables in the database, and loads the SOP combo box with items from the SOP table
            LoadRadioButtonsFromTable(_linePanel, "Line", "lineId", "name");
            LoadRadioButtonsFromTable(_shiftPanel, "Shift", "shiftId", "name");
            LoadRadioButtonsFromTable(_equipmentPanel, "Equipment", "equipmentId", "name");
            LoadSopComboBox();

        }
    }
}
