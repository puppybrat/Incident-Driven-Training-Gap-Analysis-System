using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;
using NUnit.Framework;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class ImportManagerTests
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
        public void ValidateFileFormat_ReturnsFailure_WhenFileIsNotCsv()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("not-csv.txt");

            var result = manager.ValidateFileFormat(filePath);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateFileFormat_ReturnsFailure_WhenHeadersDoNotMatchExpectedFormat()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("invalid-headers.csv");

            var result = manager.ValidateFileFormat(filePath);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ParseRows_ReturnsRowCollection_ForValidCsv()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("valid-incidents.csv");

            var rows = manager.ParseRows(filePath);

            Assert.That(rows, Is.Not.Null);
            Assert.That(rows.Count, Is.EqualTo(3));
        }

        [Test]
        public void ProcessRow_ReturnsFailure_WhenRowIsMalformed()
        {
            var manager = new ImportManager(_databaseManager);

            string[] malformedRow =
            {
                "BADROW",
                "not-a-date",
                "abc",
                "1",
                "1"
            };

            var result = manager.ProcessRow(malformedRow);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ProcessRow_ReturnsIncidentWithParsedValues_WhenRowIsValid()
        {
            var manager = new ImportManager(_databaseManager);

            string[] validRow =
            {
                "1001",
                "2026-04-01 08:00:00",
                "1",
                "1",
                "1"
            };

            var result = manager.ProcessRow(validRow);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.IncidentId, Is.EqualTo(1001));
            Assert.That(result.OccurredAt, Is.EqualTo(new DateTime(2026, 4, 1, 8, 0, 0)));
            Assert.That(result.EquipmentId, Is.EqualTo(1));
            Assert.That(result.ShiftId, Is.EqualTo(1));
            Assert.That(result.SopId, Is.EqualTo(1));
        }

        [Test]
        public void InsertValidatedRecords_InsertsProvidedValidIncidentCollection()
        {
            var manager = new ImportManager(_databaseManager);
            var repository = new IncidentRepository(_databaseManager);

            var incidents = new List<Incident>
            {
                new Incident
                {
                    IncidentId = 1001,
                    OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                    EquipmentId = 1,
                    ShiftId = 1,
                    SopId = 1
                },
                new Incident
                {
                    IncidentId = 1002,
                    OccurredAt = new DateTime(2026, 4, 1, 9, 30, 0),
                    EquipmentId = 2,
                    ShiftId = 1,
                    SopId = null
                }
            };

            var result = manager.InsertValidatedRecords(incidents);

            Assert.That(result, Is.True);
            Assert.That(repository.GetIncidentById(1001), Is.Not.Null);
            Assert.That(repository.GetIncidentById(1002), Is.Not.Null);
        }

        [Test]
        public void ImportCsv_ReturnsSummary_WithInsertedAndRejectedCounts()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("valid-incidents.csv");

            var result = manager.ImportCsv(filePath);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.InsertedCount, Is.EqualTo(3));
            Assert.That(result.RejectedCount, Is.EqualTo(0));
        }

        [Test]
        public void ImportCsv_ReturnsMixedSummary_WhenFileContainsValidAndMalformedRows()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("malformed-incidents.csv");

            var result = manager.ImportCsv(filePath);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.InsertedCount, Is.EqualTo(2));
            Assert.That(result.RejectedCount, Is.EqualTo(1));
        }
    }
}