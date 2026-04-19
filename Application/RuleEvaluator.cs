/*
 * File: RuleEvaluator.cs
 * Author: Sarah Portillo
 * Date: 04/19/2026
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

        private static readonly string[] GroupingOptions =
        {
            "Shift",
            "Line",
            "Equipment",
            "SOP"
        };

        private static readonly string[] TimeWindowOptions =
        {
            "7 days",
            "30 days",
            "90 days",
            "120 days"
        };

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
        /// Evaluates the specified report data against the provided rule configuration and returns the results that meet the defined thresholds.
        /// </summary>
        /// <param name="reportData">A list of aggregate results to be evaluated. Cannot be null.</param>
        /// <param name="ruleConfig">The rule configuration that defines the thresholds to apply during evaluation. Cannot be null.</param>
        /// <returns>A list of aggregate results that satisfy the thresholds specified in the rule configuration. The list will
        /// be empty if no results meet the criteria.</returns>
        public List<ReportRow> EvaluateThresholds(List<ReportRow> reportData, RuleConfig ruleConfig)
        {
            if (reportData == null)
            {
                return new List<ReportRow>();
            }

            if (ruleConfig == null)
            {
                return reportData;
            }

            decimal threshold = ruleConfig.ThresholdValue;
            bool flagEnabled = ruleConfig.FlagEnabled;

            foreach (ReportRow row in reportData)
            {
                row.IsFlagged = flagEnabled && row.IncidentCount >= threshold;
            }

            return reportData;
        }

        /// <summary>
        /// Flags or annotates the provided aggregate results based on specific criteria.
        /// Marks individual report entries as flagged if they meet certain conditions defined within the method. The criteria for flagging can be based on various factors such as incident count, group label, or other relevant attributes of the aggregate results.
        /// </summary>
        /// <param name="reportData">A list of aggregate results to be evaluated and flagged. Cannot be null.</param>
        /// <returns>A new list of aggregate results with flags or annotations applied according to the evaluation criteria. The
        /// returned list will be empty if no results are flagged.</returns>
        public List<AggregateResult> FlagResults(List<AggregateResult> reportData)
        {
            return new List<AggregateResult>();
        }

        /// <summary>
        /// Loads the current rule configuration.
        /// Retrieves the active rule configuration from the data store or configuration source, ensuring that the latest settings are applied during evaluation.
        /// </summary>
        /// <returns>A <see cref="RuleConfig"/> instance representing the current rule configuration.</returns>
        public RuleConfig LoadCurrentRuleConfig()
        {
            RuleConfig ruleConfig = _ruleConfigRepository.LoadRuleConfig();
            return ApplyFallbackDefaults(ruleConfig);
        }

        /// <summary>
        /// Validates the specified rule configuration and returns the result of the validation.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to validate. Must not be null. The configuration's threshold value must be zero or
        /// greater, and its grouping type and time window must be valid options.</param>
        /// <returns>A ValidationResult indicating whether the configuration is valid. If invalid, the result contains error
        /// messages describing the validation failures.</returns>
        public ValidationResult ValidateRuleConfig(RuleConfig ruleConfig)
        {
            ValidationResult validationResult = new()
            {
                IsValid = true
            };

            if (ruleConfig.ThresholdValue < 0)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Threshold must be zero or greater.");
            }

            if (string.IsNullOrWhiteSpace(ruleConfig.GroupingType) || !GroupingOptions.Contains(ruleConfig.GroupingType))
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Please select a valid grouping type.");
            }

            if (string.IsNullOrWhiteSpace(ruleConfig.TimeWindow) || !TimeWindowOptions.Contains(ruleConfig.TimeWindow))
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

            bool saveSucceeded = _ruleConfigRepository.SaveRuleConfig(ruleConfig);

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
            return ApplyFallbackDefaults(defaultConfig);
        }

        /// <summary>
        /// Applies fallback defaults to invalid or missing rule configuration values.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to normalize. If null, a new instance is created.</param>
        /// <returns>A rule configuration with valid selections.</returns>
        private RuleConfig ApplyFallbackDefaults(RuleConfig? ruleConfig)
        {
            ruleConfig ??= new RuleConfig();

            if (ruleConfig.ThresholdValue < 0)
            {
                ruleConfig.ThresholdValue = 5;
            }

            if (!GroupingOptions.Contains(ruleConfig.GroupingType))
            {
                ruleConfig.GroupingType = GroupingOptions[0];
            }

            if (!TimeWindowOptions.Contains(ruleConfig.TimeWindow))
            {
                ruleConfig.TimeWindow = TimeWindowOptions[1];
            }

            return ruleConfig;
        }
    }
}
