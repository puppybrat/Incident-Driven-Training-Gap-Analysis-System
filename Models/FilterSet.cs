namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class FilterSet
    {
        public int? LineId { get; set; }
        public int? ShiftId { get; set; }
        public int? EquipmentId { get; set; }
        public int? SopId { get; set; }
        public bool RequireMissingSop { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}