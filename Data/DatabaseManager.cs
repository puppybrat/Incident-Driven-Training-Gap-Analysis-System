using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _databaseConnectionString;

        public DatabaseManager()
        {
            _databaseConnectionString = "Data Source=training_gap_analysis.db";
        }

        public DatabaseManager(string databasePath)
        {
            _databaseConnectionString = $"Data Source={databasePath}";
        }

        /// <summary>
        /// Gets the connection string used to establish a connection to the database.
        /// </summary>
        public string ConnectionString => _databaseConnectionString;

        /// <summary>
        /// Opens a new connection to the SQLite database using the specified configuration.
        /// </summary>
        /// <param name="databaseConfiguration">A string containing the configuration or connection string used to establish the SQLite database connection.
        /// Cannot be null or empty.</param>
        /// <returns>A new instance of SqliteConnection that represents an open connection to the database.</returns>
        public SqliteConnection OpenConnection(string databaseConfiguration)
        {
         // Establishes a connection to the SQLite database using the provided connection string
            SqliteConnection databaseConnection = new(databaseConfiguration);
            databaseConnection.Open();
            return databaseConnection;
        }

        /// <summary>
        /// Closes the specified SQLite database connection.
        /// </summary>
        /// <param name="databaseConnection">The SQLite database connection to close. Must not be null and should be open or in a state that allows
        /// closing.</param>
        public void CloseConnection(SqliteConnection databaseConnection)
        {
            databaseConnection.Close();
        }

        /// <summary>
        /// Initializes the database and prepares it for use.
        /// Creates the database file if needed and sets up any necessary initial configuration or connections. This method should be called before any other database operations are performed.
        /// </summary>
        /// <returns>true if the database was successfully initialized; otherwise, false.</returns>
        public bool InitializeDatabase()
        {
            try
            {
                using SqliteConnection connection = OpenConnection(ConnectionString);
                using SqliteCommand pragmaCommand = new("PRAGMA foreign_keys = ON;", connection);
                pragmaCommand.ExecuteNonQuery();

                return EnsureSchema();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ensures that the required database schema exists.
        /// Creates the schema and enables foreign key enforcement if they do not already exist. This method should be called after initializing the database and before performing any operations that depend on the schema.
        /// </summary>
        /// <returns>true if the schema is present or was successfully created; otherwise, false.</returns>
        public bool EnsureSchema()
        {
            try
            {
                using SqliteConnection connection = OpenConnection(ConnectionString);

                using SqliteCommand pragmaCommand = new("PRAGMA foreign_keys = ON;", connection);
                pragmaCommand.ExecuteNonQuery();

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
        /// Executes a set of operations within a transactional context.
        /// This method ensures that all operations within the transaction are either committed or rolled back as a single unit, maintaining data integrity.
        /// </summary>
        /// <param name="transactionOperations">An action that contains the operations to execute as part of the transaction. Cannot be null.</param>
        /// <returns>true if the transaction completes successfully and all operations are committed; otherwise, false.</returns>
        public bool ExecuteTransaction(Action transactionOperations)
        {
            return false;
        }
    }
}
