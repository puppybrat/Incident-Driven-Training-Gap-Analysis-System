namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReportRow
    {
        public string GroupValue { get; set; } = string.Empty;

        public string Line { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string Equipment { get; set; } = string.Empty;
        public string SOP { get; set; } = string.Empty;

        public int IncidentCount { get; set; }
        public bool IsFlagged { get; set; }

        public string Status => IsFlagged ? "Flagged" : "Normal";
    }
}