namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReportRequest
    {
        public string PresetName { get; set; } = string.Empty;
        public FilterSet Filters { get; set; } = new FilterSet();
        public string GroupingType { get; set; } = string.Empty;
        public string OutputType { get; set; } = string.Empty;
    }
}