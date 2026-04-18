using Incident_Driven_Training_Gap_Analysis_System.Domain;

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    public class ReferenceDataSet
    {
        public List<Line> Lines { get; set; } = new List<Line>();
        public List<Shift> Shifts { get; set; } = new List<Shift>();
        public List<Equipment> Equipment { get; set; } = new List<Equipment>();
        public List<SOP> Sops { get; set; } = new List<SOP>();
    }
}