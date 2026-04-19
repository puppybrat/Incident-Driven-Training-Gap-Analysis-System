namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReportResult
    {
        public string PresetName { get; set; } = string.Empty;
        public string OutputType { get; set; } = "Table";
        public List<ReportRow> Rows { get; set; } = new();
    }
}