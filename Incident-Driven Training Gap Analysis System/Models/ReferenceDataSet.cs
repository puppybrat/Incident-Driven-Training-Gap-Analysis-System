/*
 * File: ReferenceDataSet.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents a collection of reference data used throughout the application.
 * It groups related domain entities such as lines, shifts, equipment, and SOPs into a single
 * structure for convenient retrieval and use in the user interface and application logic.
 */

using Incident_Driven_Training_Gap_Analysis_System.Domain;

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents a container for reference data entities used to populate UI controls
    /// and support report generation and validation logic.
    /// </summary>
    public class ReferenceDataSet
    {
        /// <summary>
        /// Gets or sets the collection of production lines.
        /// </summary>
        public List<Line> Lines { get; set; } = new List<Line>();

        /// <summary>
        /// Gets or sets the collection of shifts.
        /// </summary>
        public List<Shift> Shifts { get; set; } = new List<Shift>();

        /// <summary>
        /// Gets or sets the collection of equipment.
        /// </summary>
        public List<Equipment> Equipment { get; set; } = new List<Equipment>();

        /// <summary>
        /// Gets or sets the collection of standard operating procedures (SOPs).
        /// </summary>
        public List<SOP> Sops { get; set; } = new List<SOP>();
    }
}