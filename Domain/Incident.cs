namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    public class Incident
    {
        /// <summary>
        /// Gets or sets the unique identifier for the incident.
        /// </summary>
        public int IncidentId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the event occurred.
        /// </summary>
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the shift.
        /// </summary>
        public int ShiftId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the equipment.
        /// </summary>
        public int EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets unique identifier for the Standard Operating Procedure (SOP) associated with the incident, if applicable. This field is nullable, indicating that an incident may not always have an associated SOP reference. If a value is provided, it should be greater than 0 to be considered valid.
        /// </summary>
        public int? SopId { get; set; }

        /// <summary>
        /// Checks the required fields for creating an incident. This method ensures that all necessary information is present and valid before attempting to create a new incident record. It checks that the IncidentId is greater than 0, OccurredAt is not the default value, EquipmentId is greater than 0, and ShiftId is greater than 0. If all these conditions are met, it returns true, indicating that the incident data is complete and ready for creation; otherwise, it returns false.
        /// </summary>
        /// <returns></returns>
        public bool IsCompleteForCreation()
        {
            return IncidentId > 0
                && OccurredAt != default
                && EquipmentId > 0
                && ShiftId > 0;
        }

        /// <summary>
        /// Checks if there is an SOP reference. Accepts that SopId can be null, but if it has a value, it must be greater than 0 to be considered a valid reference.
        /// </summary>
        /// <returns>True if there is a valid SOP reference; otherwise, false.</returns>
        public bool HasSopReference()
        {
            return SopId.HasValue && SopId.Value > 0;
        }
    }
}
