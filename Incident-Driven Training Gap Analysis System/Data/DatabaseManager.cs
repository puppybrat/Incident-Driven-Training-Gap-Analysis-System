/*
 * File: DatabaseManager.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class manages database connectivity and schema initialization for the application.
 * It provides functionality for opening database connections, ensuring that required tables exist,
 * and executing database operations within transactions. It serves as the central point for
 * database setup and connection management across the system.
 */

using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Manages database connectivity, schema initialization,
    /// and transactional execution for the application.
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _databaseConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseManager"/> class
        /// using the default database file location.
        /// </summary>
        public DatabaseManager()
        {
            _databaseConnectionString = "Data Source=training_gap_analysis.db";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseManager"/> class
        /// using a specified database file path.
        /// </summary>
        /// <param name="databasePath">The file path to the SQLite database file.</param>
        public DatabaseManager(string databasePath)
        {
            _databaseConnectionString = $"Data Source={databasePath}";
        }

        /// <summary>
        /// Gets the connection string used to establish a connection to the database.
        /// </summary>
        public string ConnectionString => _databaseConnectionString;

        /// <summary>
        /// Opens a new SQLite connection using the DatabaseManager's configured connection string.
        /// Foreign key enforcement is enabled for every opened connection.
        /// </summary>
        /// <returns>An open <see cref="SqliteConnection"/>.</returns>
        public SqliteConnection OpenConnection()
        {
            SqliteConnection databaseConnection = new(_databaseConnectionString);
            databaseConnection.Open();

            using SqliteCommand pragmaCommand = new("PRAGMA foreign_keys = ON;", databaseConnection);
            pragmaCommand.ExecuteNonQuery();

            return databaseConnection;
        }

        /// <summary>
        /// Initializes the database for use by ensuring that the required schema exists.
        /// This method should be called before performing any database operations.
        /// </summary>
        /// <returns>true if the database is successfully initialized; otherwise, false.</returns>
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
        /// Ensures that all required database tables and schema objects exist.
        /// Creates any missing tables required by the application.
        /// </summary>
        /// <returns>true if the schema exists or was successfully created; otherwise, false.</returns>
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
        /// Executes the specified database operations within a single SQLite transaction.
        /// If all operations complete successfully, the transaction is committed; otherwise, it is rolled back.
        /// </summary>
        /// <param name="transactionOperations">The operations to execute using the provided connection and transaction. Cannot be null.</param>
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
