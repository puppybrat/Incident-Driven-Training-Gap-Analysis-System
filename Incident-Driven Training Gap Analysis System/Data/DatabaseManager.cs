/*
 * File: DatabaseManager.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class manages SQLite connection creation, schema initialization,
 * and transaction execution for the application.
 */

using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Manages SQLite connections, schema creation, and transaction execution.
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _databaseConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseManager"/> class.
        /// </summary>
        public DatabaseManager()
        {
            _databaseConnectionString = "Data Source=training_gap_analysis.db";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseManager"/> class with a database path.
        /// </summary>
        /// <param name="databasePath">The SQLite database file path.</param>
        public DatabaseManager(string databasePath)
        {
            _databaseConnectionString = $"Data Source={databasePath}";
        }

        /// <summary>
        /// Gets the connection string used to establish a connection to the database.
        /// </summary>
        public string ConnectionString => _databaseConnectionString;

        /// <summary>
        /// Opens a SQLite connection and enables foreign key enforcement for that connection.
        /// </summary>
        /// <returns>An open SQLite connection.</returns>
        public SqliteConnection OpenConnection()
        {
            SqliteConnection databaseConnection = new(_databaseConnectionString);
            databaseConnection.Open();

            using SqliteCommand pragmaCommand = new("PRAGMA foreign_keys = ON;", databaseConnection);
            pragmaCommand.ExecuteNonQuery();

            return databaseConnection;
        }

        /// <summary>
        /// Prepares the database for application use by ensuring the required schema exists.
        /// </summary>
        /// <returns>true if the database is ready; otherwise, false.</returns>
        public bool InitializeDatabase()
        {
            try
            {
                return EnsureSchema();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates required database tables when they do not already exist.
        /// </summary>
        /// <returns>true if schema setup succeeds; otherwise, false.</returns>
        public bool EnsureSchema()
        {
            try
            {
                using SqliteConnection connection = OpenConnection();

                string createLineTable = @"
                    CREATE TABLE IF NOT EXISTS Line (
                        lineId INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL
                    );";

                string createShiftTable = @"
                    CREATE TABLE IF NOT EXISTS Shift (
                        shiftId INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL
                    );";

                string createEquipmentTable = @"
                    CREATE TABLE IF NOT EXISTS Equipment (
                        equipmentId INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        lineId INTEGER REFERENCES Line(lineId)
                    );";

                string createSopTable = @"
                    CREATE TABLE IF NOT EXISTS SOP (
                        sopId INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        equipmentId INTEGER REFERENCES Equipment(equipmentId)
                    );";

                string createIncidentTable = @"
                    CREATE TABLE IF NOT EXISTS Incident (
                        incidentId INTEGER PRIMARY KEY AUTOINCREMENT,
                        occurredAt TEXT NOT NULL,
                        equipmentId INTEGER REFERENCES Equipment(equipmentId),
                        shiftId INTEGER REFERENCES Shift(shiftId),
                        sopId INTEGER REFERENCES SOP(sopId)
                    );";

                string createRuleConfigTable = @"
                    CREATE TABLE IF NOT EXISTS RuleConfig (
                        ruleConfigId INTEGER PRIMARY KEY CHECK (ruleConfigId = 1),
                        thresholdValue REAL NOT NULL,
                        groupingType TEXT NOT NULL,
                        timeWindow TEXT NOT NULL,
                        flagEnabled INTEGER NOT NULL
                    );";

                using (SqliteCommand command = new(createLineTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqliteCommand command = new(createShiftTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqliteCommand command = new(createEquipmentTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqliteCommand command = new(createSopTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqliteCommand command = new(createIncidentTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqliteCommand command = new(createRuleConfigTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes database operations inside one transaction, committing on success and rolling back on failure.
        /// </summary>
        /// <param name="transactionOperations">The operations to execute with the active connection and transaction.</param>
        /// <returns>true if the transaction commits successfully; otherwise, false.</returns>
        public bool ExecuteTransaction(Action<SqliteConnection, SqliteTransaction> transactionOperations)
        {
            ArgumentNullException.ThrowIfNull(transactionOperations);

            try
            {
                using SqliteConnection connection = OpenConnection();
                using SqliteTransaction transaction = connection.BeginTransaction();

                transactionOperations(connection, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
