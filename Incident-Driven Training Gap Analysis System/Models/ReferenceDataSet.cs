/*
 * File: ReferenceDataSet.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class groups reference data collections used by the UI and application logic.
 */

using Incident_Driven_Training_Gap_Analysis_System.Domain;

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents reference data used for UI selections, validation, and report generation.
    /// </summary>
    public class ReferenceDataSet
    {
        /// <summary>
        /// Gets or sets the collection of production lines.
        /// </summary>
        public List<Line> Lines { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of shifts.
        /// </summary>
        public List<Shift> Shifts { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of equipment.
        /// </summary>
        public List<Equipment> Equipment { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of SOPs.
        /// </summary>
        public List<SOP> Sops { get; set; } = [];
    }
}