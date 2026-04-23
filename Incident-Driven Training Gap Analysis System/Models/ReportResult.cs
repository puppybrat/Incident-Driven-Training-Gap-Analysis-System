/*
 * File: ReportResult.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents the result of a generated report.
 * It contains the report metadata, display options, and the collection of rows
 * produced by the report generation process.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents the output of a generated report, including metadata and report rows.
    /// </summary>
    public class ReportResult
    {
        /// <summary>
        /// Gets or sets the name of the preset used to generate the report.
        /// </summary>
        public string PresetName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output type of the generated report, such as table or graph.
        /// </summary>
        public string OutputType { get; set; } = "Table";

        /// <summary>
        /// Gets or sets a value indicating whether line information is included in the report output.
        /// </summary>
        public bool IncludeLine { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether shift information is included in the report output.
        /// </summary>
        public bool IncludeShift { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether equipment information is included in the report output.
        /// </summary>
        public bool IncludeEquipment { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether SOP information is included in the report output.
        /// </summary>
        public bool IncludeSop { get; set; } = true;

        /// <summary>
        /// Gets or sets the collection of rows that make up the report output.
        /// </summary>
        public List<ReportRow> Rows { get; set; } = new();
    }
}