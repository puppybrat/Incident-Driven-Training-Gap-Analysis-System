/*
 * File: SOP.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents a Standard Operating Procedure (SOP) within the system.
 * It defines the properties associated with an SOP, including its identifier, name,
 * and the equipment to which it applies.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents a Standard Operating Procedure associated with specific equipment.
    /// </summary>
    public class SOP
    {
        /// <summary>
        /// Gets or sets the unique identifier for the SOP.
        /// </summary>
        public int SopId { get; set; }

        /// <summary>
        /// Gets or sets the name of the SOP.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the equipment associated with this SOP.
        /// </summary>
        public int EquipmentId { get; set; }
    }
}