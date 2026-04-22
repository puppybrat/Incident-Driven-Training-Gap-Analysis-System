namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    public partial class MainForm : Form
    {
        private Panel contentPanel = new();

        public MainForm()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.Size = new Size(900, 700);
            this.Text = "Incident Driven Training Gap Analysis System";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            MenuStrip mainMenu = new();
            mainMenu.Dock = DockStyle.Top;

            ToolStripMenuItem fileMenu = new("File");
            ToolStripMenuItem settingsMenu = new("Configuration");
            ToolStripMenuItem helpMenu = new("Help");

            ToolStripMenuItem fileNewIncident = new("Create Incident");
            ToolStripMenuItem fileGenerateReport = new("Generate Report");
            ToolStripMenuItem fileImportData = new("Import");
            ToolStripMenuItem fileExportData = new("Export");
            ToolStripMenuItem fileExit = new("Exit");

            fileMenu.DropDownItems.Add(fileNewIncident);
            fileMenu.DropDownItems.Add(fileGenerateReport);
            fileMenu.DropDownItems.Add(fileImportData);
            fileMenu.DropDownItems.Add(fileExportData);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(fileExit);

            ToolStripMenuItem settingsRules = new("Configure Rules");

            settingsMenu.DropDownItems.Add(settingsRules);

            ToolStripMenuItem helpUserGuide = new("View Help / README");

            helpMenu.DropDownItems.Add(helpUserGuide);

            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(settingsMenu);
            mainMenu.Items.Add(helpMenu);

            this.MainMenuStrip = mainMenu;

            Panel navigationPanel = new();
            navigationPanel.Dock = DockStyle.Left;
            navigationPanel.Width = 250;
            navigationPanel.BackColor = Color.LightGray;
            navigationPanel.BorderStyle = BorderStyle.FixedSingle;

            FlowLayoutPanel navigationLayout = new();
            navigationLayout.Dock = DockStyle.Fill;
            navigationLayout.FlowDirection = FlowDirection.TopDown;
            navigationLayout.WrapContents = false;
            navigationLayout.Padding = new(10);

            Button btnCreate = new();
            btnCreate.Text = "Create Incident";
            btnCreate.Width = 130;
            btnCreate.Height = 40;

            Button btnImport = new();
            btnImport.Text = "Import";
            btnImport.Width = 130;
            btnImport.Height = 40;

            Button btnReport = new();
            btnReport.Text = "Generate Report";
            btnReport.Width = 130;
            btnReport.Height = 40;

            Button btnExport = new();
            btnExport.Text = "Export";
            btnExport.Width = 130;
            btnExport.Height = 40;

            navigationLayout.Controls.Add(btnCreate);
            navigationLayout.Controls.Add(btnImport);
            navigationLayout.Controls.Add(btnReport);
            navigationLayout.Controls.Add(btnExport);

            navigationPanel.Controls.Add(navigationLayout);

            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.White;

            this.Controls.Add(contentPanel);
            this.Controls.Add(navigationPanel);


            this.Controls.Add(mainMenu);

            btnCreate.Click += (s, ev) => OpenIncidentForm();
            btnImport.Click += (s, ev) => OpenImportDialog();
            btnReport.Click += (s, ev) => OpenReportForm();
            btnExport.Click += (s, ev) => OpenExportDialog();

            fileNewIncident.Click += (s, ev) => OpenIncidentForm();
            fileGenerateReport.Click += (s, ev) => OpenReportForm();
            fileImportData.Click += (s, ev) => OpenImportDialog();
            fileExportData.Click += (s, ev) => OpenExportDialog();
            fileExit.Click += (s, ev) => ExitApplication();

            settingsRules.Click += (s, ev) => OpenRuleConfigForm();
            helpUserGuide.Click += (s, ev) => OpenHelpDisplay();

            OpenIncidentForm();
        }

        private void ShowScreen(UserControl screen)
        {
            contentPanel.Controls.Clear();
            screen.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(screen);
        }

        private void OpenIncidentForm()
        {
            ShowScreen(new IncidentForm());
        }

        private void OpenReportForm()
        {
            ShowScreen(new ReportForm());
        }

        private void OpenRuleConfigForm()
        {
            ShowScreen(new RuleConfigForm());
        }

        private void OpenImportDialog()
        { 
            using ImportDialog dialog = new();
            dialog.ShowDialog();
        }

        private void OpenExportDialog()
        {
            using ExportDialog dialog = new();
            dialog.ShowDialog();
        }

        private void OpenHelpDisplay()
        {
            using HelpDisplay display = new();
            display.ShowDialog();
        }

        private void ExitApplication()
        {
            this.Close();
        }
    }
}
