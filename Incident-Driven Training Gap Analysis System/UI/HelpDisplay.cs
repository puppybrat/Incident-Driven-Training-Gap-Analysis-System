/*
 * File: HelpDisplay.cs
 * Author: Sarah Portillo
 * Date: 04/24/2026
 * Project: Incident-Driven Training Gap Analysis System
 *
 * Layer: User Interface Layer
 *
 * Purpose:
 * This form displays README-based help documentation by section.
 * Built-in fallback text is shown when README.md is unavailable.
 */

using System.Diagnostics;

namespace Incident_Driven_Training_Gap_Analysis_System.UI
{
    /// <summary>
    /// Displays README-based help documentation with built-in fallback content.
    /// </summary>
    public partial class HelpDisplay : Form
    {
        private ListBox _sectionList = null!;
        private TextBox _contentBox = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private Button _closeButton = null!;
        private Button _openDocsButton = null!;

        private readonly string _readmePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "README.md");

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpDisplay"/> class.
        /// </summary>
        public HelpDisplay()
        {
            InitializeComponents();
            LoadSections();
        }

        /// <summary>
        /// Builds the help display controls and wires their events.
        /// </summary>
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
                Text = "Open Project Repository",
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

        /// <summary>
        /// Loads the available help section names into the section list.
        /// </summary>
        private void LoadSections()
        {
            _sectionList.Items.Clear();

            _sectionList.Items.AddRange(
            [
                "Overview",
                "Purpose",
                "System Type",
                "Application Navigation",
                "First-Time Use",
                "Creating an Incident",
                "Configuring Rules",
                "Generating Reports",
                "Importing CSV Data",
                "Exporting Data",
                "Data Storage",
                "Constraints and Notes",
                "Recommended Workflow",
                "CSV Example",
                "Troubleshooting"
            ]);

            _sectionList.SelectedIndex = 0;
        }

        /// <summary>
        /// Updates the displayed help text when the selected section changes.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OnSectionChanged(object? sender, EventArgs e)
        {
            string section = _sectionList.SelectedItem?.ToString() ?? "";
            _contentBox.Text = GetSectionContent(section);
        }

        /// <summary>
        /// Gets README content for the selected section or fallback text when needed.
        /// </summary>
        /// <param name="section">The help section name.</param>
        /// <returns>The help text for the selected section.</returns>
        private string GetSectionContent(string section)
        {
            if (File.Exists(_readmePath))
            {
                return ExtractSectionFromReadme(section);
            }

            return GetFallbackContent(section);
        }

        /// <summary>
        /// Extracts a named section from README.md.
        /// </summary>
        /// <param name="section">The README section heading to find.</param>
        /// <returns>The extracted section text, or an empty string if it is not found.</returns>
        private string ExtractSectionFromReadme(string section)
        {
            try
            {
                var lines = File.ReadAllLines(_readmePath);
                bool capture = false;
                var result = "";

                foreach (var line in lines)
                {
                    if (line.StartsWith("## ") && line[3..].Trim().Equals(section, StringComparison.OrdinalIgnoreCase))
                    {
                        capture = true;
                        continue;
                    }

                    if (capture && line.StartsWith("## "))
                        break;

                    if (capture)
                        result += FormatReadmeLine(line) + Environment.NewLine;
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

        /// <summary>
        /// Converts selected README markdown syntax into readable plain text for display.
        /// </summary>
        /// <param name="line">The README line to format.</param>
        /// <returns>A plain-text version of the README line.</returns>
        private static string FormatReadmeLine(string line)
        {
            if (line.StartsWith("### "))
            {
                string heading = line[4..].Trim().ToUpperInvariant();
                return $"{heading}{Environment.NewLine}{new string('-', heading.Length)}";
            }

            if (line.StartsWith("- "))
            {
                return "• " + line[2..];
            }

            return line;
        }

        /// <summary>
        /// Gets built-in help text for a section when README content is unavailable.
        /// </summary>
        /// <param name="section">The help section name.</param>
        /// <returns>Fallback help text for the selected section.</returns>
        private static string GetFallbackContent(string section)
        {
            return section switch
            {
                "Overview" => "This system stores incident records and generates rule-based reports to identify repeated incident patterns for review.",
                "Purpose" => "The system supports incident creation, CSV import, rule configuration, report generation, Missing SOP review, and CSV export.",
                "System Type" => "This is a local, offline, single-user Windows desktop application that stores data in SQLite.",
                "Application Navigation" => "Use the main menu to access incident creation, rule configuration, report generation, import, export, and help features.",
                "First-Time Use" => "On first use, the system initializes the local database and required reference data.",
                "Creating an Incident" => "Create incident records by selecting occurrence date and time, shift, line, equipment, and an optional SOP reference.",
                "Configuring Rules" => "Configure threshold, grouping type, time window, and flagging settings used during report evaluation.",
                "Generating Reports" => "Generate table or graph reports using presets, filters, grouping options, included fields, and optional date ranges.",
                "Importing CSV Data" => "Import incident records from CSV files using the required header: OccurredAt,EquipmentId,ShiftId,SopId.",
                "Exporting Data" => "Export incident datasets or generated report results to CSV.",
                "Data Storage" => "The system stores incident records, reference data, and rule configuration values locally using SQLite.",
                "Constraints and Notes" => "The system supports local, single-user incident tracking and rule-based reporting. Reports are decision-support outputs, not automatic corrective actions.",
                "Recommended Workflow" => "Create or import incidents, configure rules, generate a report, review results, and export if needed.",
                "CSV Example" => "The README provides an example CSV format for importing incident data.",
                "Troubleshooting" => "The README includes common troubleshooting notes for CSV import, report generation, Missing SOP reports, and export issues.",
                _ => ""
            };
        }

        /// <summary>
        /// Opens the project repository link in the default browser.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void OpenDocsClicked(object? sender, EventArgs e)
        {
            string url = "https://github.com/puppybrat/Incident-Driven-Training-Gap-Analysis-System";

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