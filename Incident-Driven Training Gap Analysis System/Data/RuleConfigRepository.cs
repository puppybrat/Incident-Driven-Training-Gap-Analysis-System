/*
 * File: RuleConfigRepository.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class manages persistence for rule configuration settings.
 * It saves, loads, and resets the rule configuration stored in SQLite.
 */

using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Provides database access methods for rule configuration settings.
    /// </summary>
    public class RuleConfigRepository
    {
        private readonly DatabaseManager _databaseManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleConfigRepository"/> class.
        /// </summary>
        public RuleConfigRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleConfigRepository"/> class with a database manager.
        /// </summary>
        /// <param name="databaseManager">The database manager to use.</param>
        public RuleConfigRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Saves the rule configuration to the database.
        /// </summary>
        /// <param name="ruleConfig">The rule configuration to save.</param>
        /// <returns>true if the configuration was saved; otherwise, false.</returns>
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
        /// Loads the saved rule configuration, returning default settings when none are available or loading fails.
        /// </summary>
        /// <returns>The saved or default rule configuration.</returns>
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
                    SelectedPresetBehavior = string.Empty
                };
            }
            catch
            {
                return GetDefaultRuleConfig();
            }
        }

        /// <summary>
        /// Replaces the saved rule configuration with default values.
        /// </summary>
        /// <returns>The default rule configuration.</returns>
        public RuleConfig ResetRuleConfigToDefaults()
        {
            RuleConfig defaultConfig = GetDefaultRuleConfig();
            SaveRuleConfig(defaultConfig);
            return defaultConfig;
        }

        /// <summary>
        /// Creates the default rule configuration used when no saved settings are available.
        /// </summary>
        /// <returns>The default rule configuration.</returns>
        private static RuleConfig GetDefaultRuleConfig()
        {
            return RuleConfig.CreateDefault();
        }
    }
}
