namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReportResult
    {
        public string PresetName { get; set; } = string.Empty;
        public string OutputType { get; set; } = "Table";

        public bool IncludeLine { get; set; } = true;
        public bool IncludeShift { get; set; } = true;
        public bool IncludeEquipment { get; set; } = true;
        public bool IncludeSop { get; set; } = true;

        public List<ReportRow> Rows { get; set; } = new();
    }
}