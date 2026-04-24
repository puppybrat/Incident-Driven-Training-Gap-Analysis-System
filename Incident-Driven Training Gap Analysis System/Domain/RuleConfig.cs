/*
 * File: RuleConfig.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents rule configuration settings, valid rule options,
 * default values, and normalization helpers used during report evaluation.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents configurable rule settings used during report evaluation.
    /// </summary>
    public class RuleConfig
    {
        /// <summary>
        /// Gets the valid grouping type options for rule configuration.
        /// </summary>
        public static readonly string[] GroupingOptions =
        {
            "Shift",
            "Line",
            "Equipment",
            "SOP"
        };

        /// <summary>
        /// Gets the valid time window options for rule configuration.
        /// </summary>
        public static readonly string[] TimeWindowOptions =
        {
            "7 days",
            "30 days",
            "90 days",
            "120 days"
        };

        /// <summary>
        /// Gets or sets the threshold value used to flag report results.
        /// </summary>
        public decimal ThresholdValue { get; set; }

        /// <summary>
        /// Gets or sets the grouping type used to aggregate incident data.
        /// </summary>
        public string GroupingType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time window used during rule evaluation.
        /// </summary>
        public string TimeWindow { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether report result flagging is enabled.
        /// </summary>
        public bool FlagEnabled { get; set; }

        /// <summary>
        /// Gets or sets the selected preset behavior used at runtime.
        /// This property is not persisted in the database.
        /// </summary>
        public string SelectedPresetBehavior { get; set; } = string.Empty;

        /// <summary>
        /// Creates the default rule configuration used when no saved settings are available.
        /// </summary>
        /// <returns>The default rule configuration.</returns>
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
        /// Applies default values to a rule configuration.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to normalize.</param>
        /// <returns>A normalized rule configuration.</returns>
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
        /// Determines whether a grouping type matches one of the configured grouping options.
        /// </summary>
        /// <param name="groupingType">The grouping type to validate.</param>
        /// <returns>true if the grouping type is valid; otherwise, false.</returns>
        public static bool IsValidGroupingType(string? groupingType)
        {
            return !string.IsNullOrWhiteSpace(groupingType)
                && GroupingOptions.Contains(groupingType);
        }

        /// <summary>
        /// Determines whether a time window matches one of the configured time window options.
        /// </summary>
        /// <param name="timeWindow">The time window to validate.</param>
        /// <returns>true if the time window is valid; otherwise, false.</returns>
        public static bool IsValidTimeWindow(string? timeWindow)
        {
            return !string.IsNullOrWhiteSpace(timeWindow)
                && TimeWindowOptions.Contains(timeWindow);
        }
    }
}
