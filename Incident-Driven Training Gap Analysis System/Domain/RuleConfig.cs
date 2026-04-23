/*
 * File: RuleConfig.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents rule configuration settings used by the Incident-Driven Training Gap Analysis System.
 * It defines the rule properties, valid option sets, default values, and normalization and validation helpers
 * used across the application.
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
        /// Gets the available grouping type options for rule configurations.
        /// </summary>
        public static readonly string[] GroupingOptions =
        {
            "Shift",
            "Line",
            "Equipment",
            "SOP"
        };

        /// <summary>
        /// Gets the available time window options for rule configurations.
        /// </summary>
        public static readonly string[] TimeWindowOptions =
        {
            "7 days",
            "30 days",
            "90 days",
            "120 days"
        };

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

        /// <summary>
        /// Creates a new instance of the RuleConfig class initialized with default values.
        /// </summary>
        /// <returns>A RuleConfig object with default property values set.</returns>
        public static RuleConfig CreateDefault()
        {
            return new RuleConfig
            {
                ThresholdValue = 5,
                GroupingType = GroupingOptions[0],
                TimeWindow = TimeWindowOptions[0],
                FlagEnabled = true,
                SelectedPresetBehavior = string.Empty
            };
        }

        /// <summary>
        /// Normalizes the specified rule configuration by ensuring all required fields have valid values.
        /// Invalid or missing values are replaced with defaults.
        /// If a non-null instance is provided, that instance may be updated and returned.
        /// If null is provided, a new default configuration is created and returned.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to normalize. If null, a default configuration is created.</param>
        /// <returns>A normalized <see cref="RuleConfig"/> instance with all fields set to valid values.</returns>
        public static RuleConfig Normalize(RuleConfig? ruleConfig)
        {
            RuleConfig normalized = ruleConfig ?? CreateDefault();

            if (normalized.ThresholdValue < 0)
            {
                normalized.ThresholdValue = 5;
            }

            if (!GroupingOptions.Contains(normalized.GroupingType))
            {
                normalized.GroupingType = GroupingOptions[0];
            }

            if (!TimeWindowOptions.Contains(normalized.TimeWindow))
            {
                normalized.TimeWindow = TimeWindowOptions[0];
            }

            normalized.SelectedPresetBehavior ??= string.Empty;

            return normalized;
        }

        /// <summary>
        /// Determines whether the specified grouping type is valid.
        /// </summary>
        /// <param name="groupingType">The grouping type to validate. Can be null or whitespace.</param>
        /// <returns>true if the specified grouping type is recognized and not null or whitespace; otherwise, false.</returns>
        public static bool IsValidGroupingType(string? groupingType)
        {
            return !string.IsNullOrWhiteSpace(groupingType)
                && GroupingOptions.Contains(groupingType);
        }

        /// <summary>
        /// Determines whether the specified time window string is valid.
        /// </summary>
        /// <param name="timeWindow">The time window string to validate. May be null or empty.</param>
        /// <returns>true if the time window is not null, not whitespace, and is contained in the set of valid time window
        /// options; otherwise, false.</returns>
        public static bool IsValidTimeWindow(string? timeWindow)
        {
            return !string.IsNullOrWhiteSpace(timeWindow)
                && TimeWindowOptions.Contains(timeWindow);
        }
    }
}
