namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class AggregateResult
    {
        public string GroupLabel { get; set; } = string.Empty;
        public int IncidentCount { get; set; }
        public bool IsFlagged { get; set; }
    }
}