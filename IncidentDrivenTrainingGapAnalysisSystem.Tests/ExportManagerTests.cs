using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;
using NUnit.Framework;



namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class ExportManagerTests
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
        public void ValidateExportLocation_ReturnsFailure_WhenPathIsInvalid()
        {
            var manager = new ExportManager(_databaseManager);
            string filePath = "";

            var result = manager.ValidateExportLocation(filePath);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateExportLocation_ReturnsFailure_WhenDirectoryDoesNotExist()
        {
            var manager = new ExportManager(_databaseManager);
            string filePath = Path.Combine("Z:\\definitely_missing_folder", "export-test.csv");

            var result = manager.ValidateExportLocation(filePath);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ExportDataset_WritesFilteredIncidentRows_ToCsvFile()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                IncidentId = 5001,
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                IncidentId = 5002,
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 2,
                ShiftId = 2,
                SopId = 2
            });

            var manager = new ExportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "export-dataset-test.csv");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var filterSet = new FilterSet { LineId = 1 };

            var result = manager.ExportDataset(filePath, filterSet);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(filePath), Is.True);

            var content = File.ReadAllText(filePath);
            Assert.That(content, Does.Contain("IncidentId"));
            Assert.That(content, Does.Contain("5001"));
            Assert.That(content, Does.Not.Contain("5002"));
        }

        [Test]
        public void ExportReport_WritesFormattedResults_ToCsvFile()
        {
            var manager = new ExportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "export-report-test.csv");

            var reportResult = new ReportResult
            {
                OutputType = "Table",
                Results = new List<FormattedResult>
                {
                    new FormattedResult { DisplayLabel = "Line 1", DisplayValue = "3" },
                    new FormattedResult { DisplayLabel = "Line 2", DisplayValue = "1" }
                }
            };

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var result = manager.ExportReport(filePath, reportResult);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(filePath), Is.True);

            var content = File.ReadAllText(filePath);
            Assert.That(content, Does.Contain("Line 1"));
            Assert.That(content, Does.Contain("3"));
            Assert.That(content, Does.Contain("Line 2"));
            Assert.That(content, Does.Contain("1"));
        }

        [Test]
        public void WriteCsvFile_WritesExactCsvContent()
        {
            var manager = new ExportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "writecsv-test.csv");
            string csvData = "A,B\n1,2";

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var result = manager.WriteCsvFile(filePath, csvData);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(filePath), Is.True);
            Assert.That(File.ReadAllText(filePath), Is.EqualTo(csvData));
        }
    }
}