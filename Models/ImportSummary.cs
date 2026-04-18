namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ImportSummary
    {
        public int InsertedCount { get; set; }
        public int RejectedCount { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }
}