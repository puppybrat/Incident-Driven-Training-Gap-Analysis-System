/*
 * File: Equipment.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents a piece of equipment within the system.
 * It defines the properties associated with equipment, including its identifier,
 * name, and the production line to which it belongs.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents equipment used in the production environment, including its identity
    /// and associated production line.
    /// </summary>
    public class Equipment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the equipment.
        /// </summary>
        public int EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the equipment.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the production line to which this equipment belongs.
        /// </summary>
        public int LineId { get; set; }
    }
}