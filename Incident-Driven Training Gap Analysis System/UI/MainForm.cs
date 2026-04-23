/*
 * File: MainForm.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: UI Layer
 * 
 * Purpose:
 * This form provides the primary application shell for the system.
 * It configures the main window, builds the shared navigation structure,
 * routes users to the major application screens, and opens supporting
 * dialogs such as import, export, and help.
*/

using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Represents the main application window for the Incident-Driven Training Gap Analysis System.
    /// This form provides the top-level menu, left-side navigation, and central content area used to
    /// display the primary user controls for incident entry, report generation, and rule configuration.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly Panel _contentPanel = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class,
        /// configures the main application shell, builds the layout,
        /// and displays the default startup screen.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            ConfigureForm();
            BuildLayout();
            OpenIncidentForm();
        }

        /// <summary>
        /// Applies the main form's top-level visual configuration and window behavior.
        /// </summary>
        public void ConfigureForm()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ForeColor = Color.Black;
            Size = new Size(900, 700);
            Text = "Incident Driven Training Gap Analysis System";
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// Handles requests to display the incident creation screen.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void CreateIncident_Click(object? sender, EventArgs e)
        {
            OpenIncidentForm();
        }

        /// <summary>
        /// Handles requests to display the report generation screen.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void GenerateReport_Click(object? sender, EventArgs e)
        {
            OpenReportForm();
        }

        /// <summary>
        /// Handles requests to open the import dialog.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Import_Click(object? sender, EventArgs e)
        {
            OpenImportDialog();
        }

        /// <summary>
        /// Handles requests to open the export dialog.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Export_Click(object? sender, EventArgs e)
        {
            OpenExportDialog();
        }

        /// <summary>
        /// Handles requests to display the rule configuration screen.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void ConfigureRules_Click(object? sender, EventArgs e)
        {
            OpenRuleConfigForm();
        }

        /// <summary>
        /// Handles requests to open the help display dialog.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void ViewHelp_Click(object? sender, EventArgs e)
        {
            OpenHelpDisplay();
        }

        /// <summary>
        /// Handles requests to close the application.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Exit_Click(object? sender, EventArgs e)
        {
            ExitApplication();
        }

        /// <summary>
        /// Displays the specified user control in the main content region,
        /// replacing any previously displayed screen.
        /// </summary>
        /// <param name="screen">The user control to display.</param>
        private void ShowScreen(UserControl screen)
        {
            _contentPanel.Controls.Clear();
            screen.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(screen);
        }

        /// <summary>
        /// Displays the incident entry screen in the main content region.
        /// </summary>
        private void OpenIncidentForm()
        {
            ShowScreen(new IncidentForm());
        }

        /// <summary>
        /// Displays the report generation screen in the main content region.
        /// </summary>
        private void OpenReportForm()
        {
            ShowScreen(new ReportForm());
        }

        /// <summary>
        /// Displays the rule configuration screen in the main content region.
        /// </summary>
        private void OpenRuleConfigForm()
        {
            ShowScreen(new RuleConfigForm());
        }

        /// <summary>
        /// Opens the import dialog as a modal window.
        /// </summary>
        private void OpenImportDialog()
        {
            using ImportDialog dialog = new();
            dialog.ShowDialog(this);
        }

        /// <summary>
        /// Opens the export dialog as a modal window.
        /// If the current screen is a report form, the active report result is passed to the dialog.
        /// </summary>
        private void OpenExportDialog()
        {
            ReportResult? currentReportResult = null;

            if (_contentPanel.Controls.OfType<ReportForm>().FirstOrDefault() is ReportForm reportForm)
            {
                currentReportResult = reportForm.CurrentReportResult;
            }

            using ExportDialog dialog = new(currentReportResult);
            dialog.ShowDialog(this);
        }

        /// <summary>
        /// Opens the help display dialog as a modal window.
        /// </summary>
        private void OpenHelpDisplay()
        {
            using HelpDisplay display = new();
            display.ShowDialog(this);
        }

        /// <summary>
        /// Closes the main form and exits the application.
        /// </summary>
        private void ExitApplication()
        {
            Close();
        }

        /// <summary>
        /// Builds and returns the application's main menu strip,
        /// including File, Configuration, and Help menus.
        /// </summary>
        /// <returns>A fully configured <see cref="MenuStrip"/> instance.</returns>
        private MenuStrip BuildMainMenu()
        {
            MenuStrip mainMenu = new()
            {
                Dock = DockStyle.Top
            };

            ToolStripMenuItem fileMenu = new("File");
            ToolStripMenuItem settingsMenu = new("Configuration");
            ToolStripMenuItem helpMenu = new("Help");

            ToolStripMenuItem fileNewIncident = new("Create Incident");
            ToolStripMenuItem fileGenerateReport = new("Generate Report");
            ToolStripMenuItem fileImportData = new("Import");
            ToolStripMenuItem fileExportData = new("Export");
            ToolStripMenuItem fileExit = new("Exit");

            ToolStripMenuItem settingsRules = new("Configure Rules");
            ToolStripMenuItem helpUserGuide = new("View Help / README");

            fileNewIncident.Click += CreateIncident_Click;
            fileGenerateReport.Click += GenerateReport_Click;
            fileImportData.Click += Import_Click;
            fileExportData.Click += Export_Click;
            fileExit.Click += Exit_Click;

            settingsRules.Click += ConfigureRules_Click;
            helpUserGuide.Click += ViewHelp_Click;

            fileMenu.DropDownItems.Add(fileNewIncident);
            fileMenu.DropDownItems.Add(fileGenerateReport);
            fileMenu.DropDownItems.Add(fileImportData);
            fileMenu.DropDownItems.Add(fileExportData);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(fileExit);

            settingsMenu.DropDownItems.Add(settingsRules);
            helpMenu.DropDownItems.Add(helpUserGuide);

            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(settingsMenu);
            mainMenu.Items.Add(helpMenu);

            return mainMenu;
        }

        /// <summary>
        /// Builds and returns the left-side navigation panel used to open
        /// the application's primary screens and dialogs.
        /// </summary>
        /// <returns>A fully configured navigation <see cref="Panel"/>.</returns>
        private Panel BuildNavigationPanel()
        {
            Panel navigationPanel = new()
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            FlowLayoutPanel navigationLayout = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };

            Button btnCreate = CreateNavigationButton("Create Incident");
            Button btnImport = CreateNavigationButton("Import");
            Button btnReport = CreateNavigationButton("Generate Report");
            Button btnExport = CreateNavigationButton("Export");

            btnCreate.Click += CreateIncident_Click;
            btnImport.Click += Import_Click;
            btnReport.Click += GenerateReport_Click;
            btnExport.Click += Export_Click;

            navigationLayout.Controls.Add(btnCreate);
            navigationLayout.Controls.Add(btnImport);
            navigationLayout.Controls.Add(btnReport);
            navigationLayout.Controls.Add(btnExport);

            navigationPanel.Controls.Add(navigationLayout);

            return navigationPanel;
        }

        /// <summary>
        /// Creates a standardized navigation button for the left-side menu.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <returns>A configured navigation <see cref="Button"/>.</returns>
        private static Button CreateNavigationButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 130,
                Height = 40
            };
        }

        /// <summary>
        /// Builds the main form layout, including the menu strip,
        /// navigation panel, and central content area.
        /// </summary>
        private void BuildLayout()
        {
            SuspendLayout();

            MenuStrip mainMenu = BuildMainMenu();
            Panel navigationPanel = BuildNavigationPanel();

            MainMenuStrip = mainMenu;

            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.BackColor = Color.White;

            Controls.Add(_contentPanel);
            Controls.Add(navigationPanel);
            Controls.Add(mainMenu);

            ResumeLayout(true);
        }
    }
}