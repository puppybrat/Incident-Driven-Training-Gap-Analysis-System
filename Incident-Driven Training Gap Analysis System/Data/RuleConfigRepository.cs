/*
 * File: RuleConfigRepository.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class is responsible for managing the persistence of rule configuration settings in the database.
 * It provides methods to save, load, and reset rule configurations, ensuring that user-defined settings
 * are stored and retrieved correctly across application sessions. The repository interacts with the
 * DatabaseManager to perform database operations and handles any exceptions that may arise during these
 * interactions, providing a robust mechanism for maintaining the integrity of the rule configuration data.
*/

using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Provides methods for persisting, retrieving, and resetting rule configuration settings in the application's
    /// database.
    /// </summary>
    public class RuleConfigRepository
    {
        private readonly DatabaseManager _databaseManager;

        /// <summary>
        /// Default constructor that initializes the RuleConfigRepository with a new instance of DatabaseManager.
        /// </summary>
        public RuleConfigRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public RuleConfigRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Persists the provided rule configuration settings to the database. If a configuration already exists, it updates the existing record; otherwise, it inserts a new record.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to save. Cannot be null.</param>
        /// <returns>true if the configuration was saved successfully; otherwise, false.</returns>
        public bool SaveRuleConfig(RuleConfig ruleConfig)
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                string sql = @"
                INSERT INTO RuleConfig
                    (ruleConfigId, thresholdValue, groupingType, timeWindow, flagEnabled)
                VALUES
                    (1, @thresholdValue, @groupingType, @timeWindow, @flagEnabled)
                ON CONFLICT(ruleConfigId) DO UPDATE SET
                    thresholdValue = excluded.thresholdValue,
                    groupingType = excluded.groupingType,
                    timeWindow = excluded.timeWindow,
                    flagEnabled = excluded.flagEnabled;";

                using SqliteCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@thresholdValue", ruleConfig.ThresholdValue);
                command.Parameters.AddWithValue("@groupingType", ruleConfig.GroupingType);
                command.Parameters.AddWithValue("@timeWindow", ruleConfig.TimeWindow);
                command.Parameters.AddWithValue("@flagEnabled", ruleConfig.FlagEnabled ? 1 : 0);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the saved rule configuration for the application.
        /// If no saved configuration exists or loading fails, returns a default configuration.
        /// </summary>
        /// <returns>A new instance of the <see cref="RuleConfig"/> class containing the configuration settings.</returns>
        public RuleConfig LoadRuleConfig()
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                string sql = @"
                SELECT thresholdValue, groupingType, timeWindow, flagEnabled
                FROM RuleConfig
                WHERE ruleConfigId = 1;";

                using SqliteCommand command = new(sql, connection);
                using SqliteDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return GetDefaultRuleConfig();
                }

                return new RuleConfig
                {
                    ThresholdValue = reader.GetDecimal(0),
                    GroupingType = reader.GetString(1),
                    TimeWindow = reader.GetString(2),
                    FlagEnabled = reader.GetInt32(3) == 1,
                    SelectedPresetBehavior = string.Empty // Preset behavior is not saved in the database, so we return an empty string.
                };
            }
            catch
            {
                return GetDefaultRuleConfig();
            }
        }

        /// <summary>
        /// Overwrites any saved configuration with default values.
        /// </summary>
        /// <returns>A new instance of the RuleConfig class initialized with default settings.</returns>
        public RuleConfig ResetRuleConfigToDefaults()
        {
            RuleConfig defaultConfig = GetDefaultRuleConfig();
            SaveRuleConfig(defaultConfig);
            return defaultConfig;
        }

        /// <summary>
        /// Creates a new instance of the default rule configuration with predefined settings.
        /// </summary>
        /// <returns>A <see cref="RuleConfig"/> object initialized with default values for threshold, grouping type, time window, flag status, and preset behavior.</returns>
        private static RuleConfig GetDefaultRuleConfig()
        {
            return RuleConfig.CreateDefault();
        }
    }
}
