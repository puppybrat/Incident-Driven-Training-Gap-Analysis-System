namespace Incident_Driven_Training_Gap_Analysis_System
{
    public partial class MainForm : Form
    {
        private Panel contentPanel = new();

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.Size = new(1000, 1000);
            this.Text = "Incident Driven Training Gap Analysis System";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            MenuStrip mainMenu = new();
            mainMenu.Dock = DockStyle.Top;

            ToolStripMenuItem fileMenu = new("File");
            ToolStripMenuItem settingsMenu = new("Settings");
            ToolStripMenuItem helpMenu = new("Help");

            ToolStripMenuItem fileNewIncident = new("Create Incident");
            ToolStripMenuItem fileGenerateReport = new("Generate Report");
            ToolStripMenuItem fileImportData = new("Import Data");
            ToolStripMenuItem fileExportData = new("Export Data");
            ToolStripMenuItem fileExit = new("Exit");

            fileMenu.DropDownItems.Add(fileNewIncident);
            fileMenu.DropDownItems.Add(fileGenerateReport);
            fileMenu.DropDownItems.Add(fileImportData);
            fileMenu.DropDownItems.Add(fileExportData);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(fileExit);

            ToolStripMenuItem settingsRules = new("Rules");
            ToolStripMenuItem settingsPreferences = new("Preferences");

            settingsMenu.DropDownItems.Add(settingsRules);
            settingsMenu.DropDownItems.Add(settingsPreferences);

            ToolStripMenuItem helpUserGuide = new("User Guide");
            ToolStripMenuItem helpAbout = new("About");

            helpMenu.DropDownItems.Add(helpUserGuide);
            helpMenu.DropDownItems.Add(helpAbout);

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
            btnImport.Text = "Import Data";
            btnImport.Width = 130;
            btnImport.Height = 40;

            Button btnReport = new();
            btnReport.Text = "Generate Report";
            btnReport.Width = 130;
            btnReport.Height = 40;

            Button btnExport = new();
            btnExport.Text = "Export Data";
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

            btnCreate.Click += (s, ev) => ShowScreen(new IncidentEntryControl());
            btnReport.Click += (s, ev) => ShowScreen(new GenerateReportControl());

            ShowScreen(new IncidentEntryControl());
        }

        private void ShowScreen(UserControl screen)
        {
            contentPanel.Controls.Clear();
            screen.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(screen);
        }
    }
}
