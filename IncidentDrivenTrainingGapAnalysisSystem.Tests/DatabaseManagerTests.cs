/*
 * File: DatabaseManagerTests.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Contains NUnit tests for database schema initialization, SQLite connection behavior,
 * foreign key enforcement, transaction handling, and connection string configuration.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    /// <summary>
    /// Tests DatabaseManager behavior for schema creation, connection setup, foreign key enforcement,
    /// and transaction commit or rollback behavior.
    /// </summary>
    [TestFixture]
    public class DatabaseManagerTests
    {
        private DatabaseManager _databaseManager;

        [SetUp]
        public void SetUp()
        {
            string dbPath = TestPathHelper.GetDatabasePath("training_gap_analysis.db");
            _databaseManager = new DatabaseManager(dbPath);
            _databaseManager.InitializeDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

            string dbPath = TestPathHelper.GetDatabasePath("training_gap_analysis.db");
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        [Test]
        public void InitializeDatabase_CreatesOrPreparesDatabaseForUse()
        {
            var result = _databaseManager.InitializeDatabase();
            Assert.That(result, Is.True);
        }

        [Test]
        public void EnsureSchema_CreatesRequiredTables()
        {
            var result = _databaseManager.EnsureSchema();

            Assert.That(result, Is.True);

            using var connection = _databaseManager.OpenConnection();

            List<string> tables = [];

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            Assert.That(tables, Does.Contain("Line"));
            Assert.That(tables, Does.Contain("Shift"));
            Assert.That(tables, Does.Contain("Equipment"));
            Assert.That(tables, Does.Contain("SOP"));
            Assert.That(tables, Does.Contain("Incident"));
            Assert.That(tables, Does.Contain("RuleConfig"));
        }

        [Test]
        public void EnsureSchema_CreatesIncidentTable_WithExpectedColumns()
        {
            var result = _databaseManager.EnsureSchema();

            Assert.That(result, Is.True);

            using var connection = _databaseManager.OpenConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "PRAGMA table_info(Incident);";

            List<string> columns = [];

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                columns.Add(reader.GetString(1));
            }

            Assert.That(columns, Does.Contain("incidentId"));
            Assert.That(columns, Does.Contain("occurredAt"));
            Assert.That(columns, Does.Contain("equipmentId"));
            Assert.That(columns, Does.Contain("shiftId"));
            Assert.That(columns, Does.Contain("sopId"));
        }

        [Test]
        public void OpenConnection_EnablesForeignKeys()
        {
            using var connection = _databaseManager.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys;";

            var result = command.ExecuteScalar();

            Assert.That(Convert.ToInt32(result), Is.EqualTo(1));
        }

        [Test]
        public void OpenConnection_EnforcesForeignKeys_WhenInvalidReferenceIsInserted()
        {
            using var connection = _databaseManager.OpenConnection();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Equipment (name, lineId)
                VALUES ('Invalid Equipment', 9999);";

            Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() =>
            {
                command.ExecuteNonQuery();
            });
        }

        [Test]
        public void ExecuteTransaction_CommitsChanges_WhenSuccessful()
        {
            var result = _databaseManager.ExecuteTransaction((connection, transaction) =>
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "INSERT INTO Line (name) VALUES ('Test Line');";
                command.ExecuteNonQuery();
            });

            Assert.That(result, Is.True);

            using var connection = _databaseManager.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Line WHERE name = 'Test Line';";

            var count = Convert.ToInt32(command.ExecuteScalar());

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteTransaction_RollsBack_WhenExceptionOccurs()
        {
            var result = _databaseManager.ExecuteTransaction((connection, transaction) =>
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "INSERT INTO Line (name) VALUES ('Rollback Test');";
                command.ExecuteNonQuery();

                throw new Exception("Force rollback");
            });

            Assert.That(result, Is.False);

            using var connection = _databaseManager.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Line WHERE name = 'Rollback Test';";

            var count = Convert.ToInt32(command.ExecuteScalar());

            Assert.That(count, Is.Zero);
        }

        [Test]
        public void ExecuteTransaction_ThrowsArgumentNullException_WhenOperationIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _databaseManager.ExecuteTransaction(null!);
            });
        }

        [Test]
        public void Constructor_SetsConnectionString_FromPath()
        {
            string path = "test.db";
            DatabaseManager manager = new(path);

            Assert.That(manager.ConnectionString, Does.Contain(path));
        }

        [Test]
        public void Constructor_SetsDefaultConnectionString()
        {
            DatabaseManager manager = new();

            Assert.That(manager.ConnectionString, Is.EqualTo("Data Source=training_gap_analysis.db"));
        }


    }
}