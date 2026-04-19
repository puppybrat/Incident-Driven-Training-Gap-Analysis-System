/*
 * File: RuleConfig.cs
 * Author: Sarah Portillo
 * Date: 04/19/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Domain Class
 * 
 * Purpose:
 * This class represents the domain class for a rule configuration within the Incident-Driven Training Gap Analysis
 * System. It encapsulates the properties and behaviors associated with a rule configuration, including its threshold value,
 * grouping type, time window, flagging options, and selected preset behavior. This domain class serves as a fundamental
 * building block for representing rule configurations in the system and is used by the application layer to manage
 * rule-related business logic.
*/

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents the configuration settings for a rule, including threshold values, grouping options, time window, and
    /// optional behavior presets.
    /// </summary>
    public class RuleConfig
    {
        /// <summary>
        /// Gets or sets the threshold value used to determine when a result should be flagged.
        /// </summary>
        public decimal ThresholdValue { get; set; }

        /// <summary>
        /// Gets or sets the grouping type used for aggregating incident data (ie. by shift, by equipment, etc.).
        /// </summary>
        public string GroupingType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time window over which incident data is evaluated (ie. last 7 days, last 30 days, etc.).
        /// </summary>
        public string TimeWindow { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether result flagging is enabled.
        /// </summary>
        public bool FlagEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the currently selected preset behavior.
        /// This property is not persisted in the database and is only used at runtime.
        /// </summary>
        public string SelectedPresetBehavior { get; set; } = string.Empty;
    }
}
