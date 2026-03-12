using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Incident_Driven_Training_Gap_Analysis_System
{
    public partial class GenerateReportControl : UserControl
    {
        public GenerateReportControl()
        {
            InitializeComponent();

            Label lbl = new()
            {
                Text = "Generate Report",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            this.Controls.Add(lbl);
        }
    }
}
