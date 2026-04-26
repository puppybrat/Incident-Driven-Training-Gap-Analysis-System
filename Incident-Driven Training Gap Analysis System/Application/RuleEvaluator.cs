/*
 * File: RuleEvaluator.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class manages rule evaluation and rule configuration operations.
 * It validates, loads, saves, resets, and applies rule settings.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Provides rule evaluation and rule configuration operations.
    /// </summary>
    public class RuleEvaluator
    {
        private readonly RuleConfigRepository _ruleConfigRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleEvaluator"/> class.
        /// </summary>
        public RuleEvaluator()
        {
            _ruleConfigRepository = new RuleConfigRepository();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleEvaluator"/> class with a database manager.
        /// </summary>
        /// <param name="databaseManager">The database manager to use.</param>
        public RuleEvaluator(DatabaseManager databaseManager)
        {
            _ruleConfigRepository = new RuleConfigRepository(databaseManager);
        }

        /// <summary>
        /// Evaluates report rows against the active threshold rule and updates their flagged status.
        /// </summary>
        /// <param name="reportData">The report rows to evaluate, or null to return an empty list.</param>
        /// <param name="ruleConfig">The rule configuration to apply, or null to use default settings.</param>
        /// <returns>The evaluated report rows with updated flagged status.</returns>
        public static List<ReportRow> EvaluateThresholds(List<ReportRow>? reportData, RuleConfig? ruleConfig)
        {
            if (reportData == null)
            {
                return [];
            }

            RuleConfig normalizedConfig = RuleConfig.Normalize(ruleConfig);

            foreach (ReportRow row in reportData)
            {
                row.IsFlagged = normalizedConfig.FlagEnabled
                    && row.IncidentCount >= normalizedConfig.ThresholdValue;
            }

            return reportData;
        }

        /// <summary>
        /// Loads the current rule configuration.
        /// </summary>
        /// <returns>The current rule configuration.</returns>
        public RuleConfig LoadCurrentRuleConfig()
        {
            return RuleConfig.Normalize(_ruleConfigRepository.LoadRuleConfig());
        }

        /// <summary>
        /// Validates threshold, grouping type, and time window values for a rule configuration.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to validate.</param>
        /// <returns>A validation result containing any rule configuration errors.</returns>
        public static ValidationResult ValidateRuleConfig(RuleConfig ruleConfig)
        {
            ValidationResult validationResult = new()
            {
                IsValid = true
            };

            if (ruleConfig == null)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Rule configuration is required.");
                return validationResult;
            }

            if (ruleConfig.ThresholdValue < 0)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Threshold must be zero or greater.");
            }

            if (!RuleConfig.IsValidGroupingType(ruleConfig.GroupingType))
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Please select a valid grouping type.");
            }

            if (!RuleConfig.IsValidTimeWindow(ruleConfig.TimeWindow))
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Please select a valid time window.");
            }

            return validationResult;
        }

        /// <summary>
        /// Validates and saves a rule configuration.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to save.</param>
        /// <returns>A validation result containing success status and error messages.</returns>
        public ValidationResult SaveRuleConfig(RuleConfig ruleConfig)
        {
            ValidationResult validationResult = ValidateRuleConfig(ruleConfig);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            bool saveSucceeded = _ruleConfigRepository.SaveRuleConfig(RuleConfig.Normalize(ruleConfig));

            if (!saveSucceeded)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Unable to save rule configuration.");
            }

            return validationResult;
        }

        /// <summary>
        /// Resets the persisted rule configuration to defaults and returns the result.
        /// </summary>
        /// <returns>A valid default <see cref="RuleConfig"/> instance.</returns>
        public RuleConfig ResetRuleConfigToDefaults()
        {
            RuleConfig defaultConfig = _ruleConfigRepository.ResetRuleConfigToDefaults();
            return RuleConfig.Normalize(defaultConfig);
        }
    }
}
