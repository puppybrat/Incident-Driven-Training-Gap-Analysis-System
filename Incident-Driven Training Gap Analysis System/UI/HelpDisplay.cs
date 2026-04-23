using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Help dialog that displays README-based documentation.
    /// Falls back to built-in placeholder content if README is not found.
    /// </summary>
    public partial class HelpDisplay : Form
    {
        private ListBox _sectionList;
        private TextBox _contentBox;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Button _closeButton;
        private Button _openDocsButton;

        private readonly string _readmePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");

        public HelpDisplay()
        {
            InitializeComponents();
            LoadSections();
        }

        private void InitializeComponents()
        {
            this.Text = "Help";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            _titleLabel = new Label
            {
                Text = "Incident-Driven Training Gap Analysis System",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            _subtitleLabel = new Label
            {
                Text = "Help & Documentation",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(12, 40)
            };

            _sectionList = new ListBox
            {
                Location = new Point(10, 80),
                Size = new Size(220, 430)
            };
            _sectionList.SelectedIndexChanged += OnSectionChanged;

            _contentBox = new TextBox
            {
                Location = new Point(240, 80),
                Size = new Size(630, 430),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10),
                BackColor = Color.White
            };

            _openDocsButton = new Button
            {
                Text = "Open Online Documentation",
                Location = new Point(10, 520),
                Size = new Size(220, 30)
            };
            _openDocsButton.Click += OpenDocsClicked;

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(770, 520),
                Size = new Size(100, 30)
            };
            _closeButton.Click += (s, e) => this.Close();

            this.Controls.Add(_titleLabel);
            this.Controls.Add(_subtitleLabel);
            this.Controls.Add(_sectionList);
            this.Controls.Add(_contentBox);
            this.Controls.Add(_openDocsButton);
            this.Controls.Add(_closeButton);
        }

        private void LoadSections()
        {
            _sectionList.Items.Clear();

            _sectionList.Items.AddRange(new object[]
            {
                "Overview",
                "Application Navigation",
                "Create Incident",
                "Configure Rules",
                "Generate Report",
                "Import and Export",
                "Constraints and Notes",
                "Online Documentation"
            });

            _sectionList.SelectedIndex = 0;
        }

        private void OnSectionChanged(object sender, EventArgs e)
        {
            string section = _sectionList.SelectedItem?.ToString() ?? "";
            _contentBox.Text = GetSectionContent(section);
        }

        private string GetSectionContent(string section)
        {
            if (File.Exists(_readmePath))
            {
                return ExtractSectionFromReadme(section);
            }

            return GetFallbackContent(section);
        }

        private string ExtractSectionFromReadme(string section)
        {
            try
            {
                var lines = File.ReadAllLines(_readmePath);
                bool capture = false;
                var result = "";

                foreach (var line in lines)
                {
                    if (line.StartsWith("## ") && line.Contains(section))
                    {
                        capture = true;
                        continue;
                    }

                    if (capture && line.StartsWith("## "))
                        break;

                    if (capture)
                        result += line + Environment.NewLine;
                }

                return string.IsNullOrWhiteSpace(result)
                    ? "Section not found in README."
                    : result;
            }
            catch
            {
                return "Error reading README file.";
            }
        }

        private string GetFallbackContent(string section)
        {
            switch (section)
            {
                case "Overview":
                    return "This system analyzes incident data to identify training gaps and generate actionable reports.";

                case "Application Navigation":
                    return "Use the main menu to access incident creation, rule configuration, and report generation features.";

                case "Create Incident":
                    return "Enter incident details including category, description, and associated attributes.";

                case "Configure Rules":
                    return "Define rules that map incident patterns to training needs.";

                case "Generate Report":
                    return "Generate reports summarizing training gaps identified from incident analysis.";

                case "Import and Export":
                    return "Import incident data or export reports using supported formats.";

                case "Constraints and Notes":
                    return "Ensure data consistency and follow required input formats when entering information.";

                case "Online Documentation":
                    return "Click the button below to open full documentation when available.";

                default:
                    return "";
            }
        }

        private void OpenDocsClicked(object sender, EventArgs e)
        {
            string url = "https://github.com/puppybrat/Incident-Driven-Training-Gap-Analysis-System";

            if (url.Contains("your-documentation-url"))
            {
                MessageBox.Show("Documentation URL not configured.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Unable to open documentation.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}