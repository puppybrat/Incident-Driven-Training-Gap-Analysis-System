namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReportResult
    {
        public List<FormattedResult> Results { get; set; } = new List<FormattedResult>();
        public string OutputType { get; set; } = string.Empty;
    }
}