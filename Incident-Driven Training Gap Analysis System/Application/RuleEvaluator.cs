/*
 * File: RuleEvaluator.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class is responsible for applying rule-related business logic within the system.
 * It validates rule configuration settings, loads the current configuration for application use,
 * resets configuration values to defaults when needed, and coordinates persistence of rule settings
 * through the data persistence layer.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Applies rule-related business logic, including validation, configuration loading,
    /// default restoration, and rule-setting persistence coordination.
    /// </summary>
    public class RuleEvaluator
    {
        private readonly RuleConfigRepository _ruleConfigRepository;

        /// <summary>
        /// Default constructor that initializes the RuleEvaluator with a new instance of RuleConfigRepository.
        /// </summary>
        public RuleEvaluator()
        {
            _ruleConfigRepository = new RuleConfigRepository();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public RuleEvaluator(DatabaseManager databaseManager)
        {
            _ruleConfigRepository = new RuleConfigRepository(databaseManager);
        }

        /// <summary>
        /// Evaluates the specified report rows against the provided rule configuration and updates their flagged status
        /// based on the configured threshold rules.
        /// </summary>
        /// <param name="reportData">A list of report rows to evaluate. If null, an empty list is returned.</param>
        /// <param name="ruleConfig">The rule configuration that defines the threshold and flagging behavior. If null, default rule settings are applied.</param>
        /// <returns>The evaluated report rows with their flagged status updated according to the effective rule configuration.</returns>
        public List<ReportRow> EvaluateThresholds(List<ReportRow>? reportData, RuleConfig? ruleConfig)
        {
            if (reportData == null)
            {
                return new List<ReportRow>();
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
        /// Retrieves the active rule configuration from the data store or configuration source, ensuring that the latest settings are applied during evaluation.
        /// </summary>
        /// <returns>A <see cref="RuleConfig"/> instance representing the current rule configuration.</returns>
        public RuleConfig LoadCurrentRuleConfig()
        {
            return RuleConfig.Normalize(_ruleConfigRepository.LoadRuleConfig());
        }

        /// <summary>
        /// Validates the specified rule configuration and returns the result of the validation.
        /// </summary>
        /// <param name="ruleConfig">
        /// The rule configuration to validate. If null, validation fails.
        /// The threshold value must be zero or greater, and the grouping type and time window
        /// must be valid predefined options.
        /// </param>
        /// <returns>A ValidationResult indicating whether the configuration is valid. If invalid, the result contains error
        /// messages describing the validation failures.</returns>
        public ValidationResult ValidateRuleConfig(RuleConfig ruleConfig)
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
        /// Validates and saves the specified rule configuration.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to validate and save.</param>
        /// <returns>A ValidationResult indicating whether the save operation succeeded.</returns>
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
