/*
 * File: ReportRow.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents a single row in generated report output.
 * It stores the grouped display value, included reference values, incident count,
 * and flagging status used for report presentation.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents a single row of report output, including grouped values, reference data, and incident count.
    /// </summary>
    public class ReportRow
    {
        /// <summary>
        /// Gets or sets the primary grouped display value for the report row.
        /// </summary>
        public string GroupValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the line value associated with the report row.
        /// </summary>
        public string Line { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shift value associated with the report row.
        /// </summary>
        public string Shift { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the equipment value associated with the report row.
        /// </summary>
        public string Equipment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SOP value associated with the report row.
        /// </summary>
        public string SOP { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of incidents represented by this report row.
        /// </summary>
        public int IncidentCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this report row has been flagged by rule evaluation.
        /// </summary>
        public bool IsFlagged { get; set; }

        /// <summary>
        /// Gets the display status of the report row based on whether it is flagged.
        /// </summary>
        public string Status => IsFlagged ? "Flagged" : "Normal";
    }
}