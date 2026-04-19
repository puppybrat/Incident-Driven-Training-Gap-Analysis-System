/*
 * File: Incident.cs
 * Author: Sarah Portillo
 * Date: 04/19/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Domain Class
 * 
 * Purpose:
 * This class represents the domain class for an incident within the Incident-Driven Training Gap Analysis
 * System. It encapsulates the properties and behaviors associated with an incident, including its unique
 * identifier, occurrence date and time, associated shift and equipment, and an optional reference to a
 * Standard Operating Procedure (SOP). The class includes methods to validate the completeness of the
 * incident data for creation and to check for the presence of a valid SOP reference. This domain class
 * serves as a fundamental building block for representing incidents in the system and is used by the 
 * application layer to manage incident-related business logic.
*/

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents an incident record, including details such as the time of occurrence, associated shift, equipment,
    /// and an optional SOP reference.
    /// </summary>
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
        /// Checks the required fields for creating an incident. This method ensures that all necessary information is present and valid before attempting to create a new incident record. It checks that OccurredAt is not set in the future, EquipmentId is greater than 0, and ShiftId is greater than 0. If all these conditions are met, it returns true, indicating that the incident data is complete and ready for creation; otherwise, it returns false.
        /// </summary>
        /// <returns>True if the incident data is complete and valid for creation; otherwise, false.</returns>
        public bool IsCompleteForCreation()
        {
            return OccurredAt <= DateTime.Now
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
