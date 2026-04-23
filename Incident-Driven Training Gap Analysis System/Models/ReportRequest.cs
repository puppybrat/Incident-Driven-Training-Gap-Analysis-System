/*
 * File: ReportRequest.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents the set of parameters used to request report generation.
 * It contains the selected preset, grouping type, filter criteria, output type,
 * and display options that determine how the report will be built.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents the input parameters used to generate a report.
    /// </summary>
    public class ReportRequest
    {
        /// <summary>
        /// Gets or sets the name of the selected report preset.
        /// </summary>
        public string PresetName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the grouping type used when aggregating report results.
        /// </summary>
        public string GroupingType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the filter criteria used to restrict which incidents are included in the report.
        /// </summary>
        public FilterSet Filters { get; set; } = new();

        /// <summary>
        /// Gets or sets the output type of the report, such as table or graph.
        /// </summary>
        public string OutputType { get; set; } = "Table";

        /// <summary>
        /// Gets or sets a value indicating whether line information should be included in the report output.
        /// </summary>
        public bool IncludeLine { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether shift information should be included in the report output.
        /// </summary>
        public bool IncludeShift { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether equipment information should be included in the report output.
        /// </summary>
        public bool IncludeEquipment { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether SOP information should be included in the report output.
        /// </summary>
        public bool IncludeSop { get; set; } = true;
    }
}