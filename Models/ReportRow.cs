namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReportRow
    {
        public string GroupPrimary { get; set; } = string.Empty;
        public string? GroupSecondary { get; set; }
        public int IncidentCount { get; set; }
        public bool IsFlagged { get; set; }
        public string Status => IsFlagged ? "Flagged" : "Normal";
    }
}