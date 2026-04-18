namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    public class Incident
    {
        public int IncidentId { get; set; }
        public DateTime OccurredAt { get; set; }
        public int EquipmentId { get; set; }
        public int ShiftId { get; set; }
        public int? SopId { get; set; }

        public bool IsCompleteForCreation()
        {
            return false;
        }

        public bool HasSopReference()
        {
            return false;
        }
    }
}
