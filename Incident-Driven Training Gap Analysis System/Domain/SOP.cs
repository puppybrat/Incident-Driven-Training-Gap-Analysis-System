/*
 * File: SOP.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents a standard operating procedure (SOP) and its associated equipment.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents a standard operating procedure associated with equipment.
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
        /// Gets or sets the equipment identifier this SOP applies to.
        /// </summary>
        public int EquipmentId { get; set; }
    }
}