namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    public class RuleConfig
    {
        public decimal ThresholdValue { get; set; }
        public string GroupingType { get; set; } = string.Empty;
        public string TimeWindow { get; set; } = string.Empty;
        public bool FlagEnabled { get; set; }
        public string SelectedPresetBehavior { get; set; } = string.Empty;
    }
}
