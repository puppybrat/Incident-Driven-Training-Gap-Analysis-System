/*
 * File: ReportPresetNames.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This static class defines the available report preset names used throughout the application.
 * These constants provide a centralized and consistent source for preset labels used in the
 * user interface and report generation logic.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Defines the available report preset names used for report configuration and generation.
    /// </summary>
    public static class ReportPresetNames
    {
        /// <summary>
        /// Represents the absence of a selected report preset.
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// Represents a custom report configuration created by modifying preset values.
        /// </summary>
        public const string Custom = "Custom";

        /// <summary>
        /// Represents the preset for incidents per shift grouped by line.
        /// </summary>
        public const string IncidentsPerShiftByLine = "Incidents per Shift by Line";

        /// <summary>
        /// Represents the preset for incidents per missing SOP grouped by line.
        /// </summary>
        public const string IncidentsPerMissingSopByLine = "Incidents per missing SOP by Line";

        /// <summary>
        /// Represents the preset for incidents per equipment.
        /// </summary>
        public const string IncidentsPerEquipment = "Incidents per Equipment";

        /// <summary>
        /// Represents the preset for incidents per SOP reference.
        /// </summary>
        public const string IncidentsPerSopReference = "Incidents per SOP Reference";

        /// <summary>
        /// Gets the list of preset names available for user selection in the application.
        /// </summary>
        public static readonly string[] AvailablePresets =
        {
            None,
            IncidentsPerShiftByLine,
            IncidentsPerMissingSopByLine,
            IncidentsPerEquipment,
            IncidentsPerSopReference
        };
    }
}