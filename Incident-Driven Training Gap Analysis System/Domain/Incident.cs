/*
 * File: Incident.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents an incident record, including occurrence time, shift, equipment, and optional SOP.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents an incident record with occurrence time, shift, equipment, and optional SOP reference.
    /// </summary>
    public class Incident
    {
        /// <summary>
        /// Gets or sets the unique identifier for the incident.
        /// </summary>
        public int IncidentId { get; set; }

        /// <summary>
        /// Gets or sets when the incident occurred.
        /// </summary>
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Gets or sets the shift identifier for the incident.
        /// </summary>
        public int ShiftId { get; set; }

        /// <summary>
        /// Gets or sets the equipment identifier for the incident.
        /// </summary>
        public int EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the optional SOP identifier.
        /// </summary>
        public int? SopId { get; set; }

        /// <summary>
        /// Determines whether the incident has the required values for creation.
        /// </summary>
        /// <returns>true when occurrence time, equipment, and shift values are valid; otherwise, false.</returns>
        public bool IsCompleteForCreation()
        {
            return OccurredAt <= DateTime.Now
                && EquipmentId > 0
                && ShiftId > 0;
        }
    }
}
