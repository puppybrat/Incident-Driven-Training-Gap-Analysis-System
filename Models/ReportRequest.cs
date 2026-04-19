namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReportRequest
    {
        public string PresetName { get; set; } = string.Empty;
        public FilterSet Filters { get; set; } = new();
        public string OutputType { get; set; } = "Table";
    }
}