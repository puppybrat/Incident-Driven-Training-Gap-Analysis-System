using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

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

            var referenceDataRepository = new ReferenceDataRepository(_databaseManager);
            referenceDataRepository.SeedReferenceDataIfNeeded();
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
        public void ImportCsv_ReturnsError_WhenFileIsNotCsv()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("not-csv.txt");

            var result = manager.ImportCsv(filePath);

            Assert.That(result.InsertedCount, Is.EqualTo(0));
            Assert.That(result.Messages.Any(m => m.Contains("CSV")), Is.True);
        }

        [Test]
        public void ImportCsv_ReturnsError_WhenHeaderIsInvalid()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("invalid-headers.csv");

            var result = manager.ImportCsv(filePath);

            Assert.That(result.InsertedCount, Is.EqualTo(0));
            Assert.That(result.Messages.Any(m => m.Contains("header")), Is.True);
        }

        [Test]
        public void ImportCsv_ReturnsError_WhenFileIsEmpty()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("empty.csv");

            var result = manager.ImportCsv(filePath);

            Assert.That(result.InsertedCount, Is.EqualTo(0));
            Assert.That(result.RejectedCount, Is.EqualTo(0));
            Assert.That(result.Messages.Any(m => m.Contains("empty")), Is.True);
        }

        [Test]
        public void ImportCsv_ImportsAllRows_WhenFileIsValid()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("valid-incidents.csv");

            var result = manager.ImportCsv(filePath);

            var repository = new IncidentRepository(_databaseManager);
            var incidents = repository.GetIncidents(new Incident_Driven_Training_Gap_Analysis_System.Models.FilterSet());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.InsertedCount, Is.EqualTo(3));
            Assert.That(result.RejectedCount, Is.EqualTo(0));
            Assert.That(result.Messages.Any(m => m.Contains("successfully")), Is.True);
            Assert.That(incidents, Has.Count.EqualTo(3));
        }

        [Test]
        public void ImportCsv_ImportsValidRows_AndRejectsInvalidRows()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("malformed-incidents.csv");

            var result = manager.ImportCsv(filePath);

            var repository = new IncidentRepository(_databaseManager);
            var incidents = repository.GetIncidents(new Incident_Driven_Training_Gap_Analysis_System.Models.FilterSet());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.InsertedCount, Is.EqualTo(2));
            Assert.That(result.RejectedCount, Is.EqualTo(1));
            Assert.That(result.Messages.Any(m => m.Contains("rejected")), Is.True);
            Assert.That(incidents, Has.Count.EqualTo(2));
        }

        [Test]
        public void ImportCsv_RejectsRows_WithFutureOccurredAt()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("future-date.csv");

            var result = manager.ImportCsv(filePath);

            Assert.That(result.InsertedCount, Is.EqualTo(0));
            Assert.That(result.RejectedCount, Is.EqualTo(1));
            Assert.That(result.Messages.Any(m => m.Contains("future")), Is.True);
        }

        [Test]
        public void ImportCsv_RejectsRows_WithInvalidReferenceData()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = TestPathHelper.GetCsvPath("invalid-reference.csv");

            var result = manager.ImportCsv(filePath);

            Assert.That(result.InsertedCount, Is.EqualTo(0));
            Assert.That(result.RejectedCount, Is.GreaterThan(0));
        }

        [Test]
        public void ImportCsv_AllowsBlankSopId()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "blank-sop-import-test.csv");

            File.WriteAllText(
                filePath,
                "OccurredAt,EquipmentId,ShiftId,SopId" + Environment.NewLine +
                "2025-04-10 08:00:00,1,1,");

            var result = manager.ImportCsv(filePath);

            var repository = new IncidentRepository(_databaseManager);
            var incidents = repository.GetIncidents(new Incident_Driven_Training_Gap_Analysis_System.Models.FilterSet());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.InsertedCount, Is.EqualTo(1));
            Assert.That(result.RejectedCount, Is.EqualTo(0));
            Assert.That(incidents, Has.Count.EqualTo(1));
            Assert.That(incidents[0].SopId, Is.Null);
        }

        [Test]
        public void ImportCsv_RejectsRows_WhenSopDoesNotBelongToEquipment()
        {
            var referenceDataRepository = new ReferenceDataRepository(_databaseManager);
            var referenceData = referenceDataRepository.GetAllReferenceData();

            var sop = referenceData.Sops.First();
            var differentEquipment = referenceData.Equipment.First(e => e.EquipmentId != sop.EquipmentId);

            var manager = new ImportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "sop-equipment-mismatch-import-test.csv");

            File.WriteAllText(
                filePath,
                "OccurredAt,EquipmentId,ShiftId,SopId" + Environment.NewLine +
                $"2025-04-10 08:00:00,{differentEquipment.EquipmentId},1,{sop.SopId}");

            var result = manager.ImportCsv(filePath);

            var repository = new IncidentRepository(_databaseManager);
            var incidents = repository.GetIncidents(new Incident_Driven_Training_Gap_Analysis_System.Models.FilterSet());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.InsertedCount, Is.EqualTo(0));
            Assert.That(result.RejectedCount, Is.EqualTo(1));
            Assert.That(result.Messages.Any(m => m.Contains("not associated")), Is.True);
            Assert.That(incidents, Has.Count.EqualTo(0));
        }

        [Test]
        public void ImportCsv_RejectsRows_WithWrongColumnCount()
        {
            var manager = new ImportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "wrong-column-count-import-test.csv");

            File.WriteAllText(
                filePath,
                "OccurredAt,EquipmentId,ShiftId,SopId" + Environment.NewLine +
                "2025-04-10 08:00:00,1,1,1,ExtraColumn");

            var result = manager.ImportCsv(filePath);

            var repository = new IncidentRepository(_databaseManager);
            var incidents = repository.GetIncidents(new FilterSet());

            Assert.That(result, Is.Not.Null);
            Assert.That(result.InsertedCount, Is.EqualTo(0));
            Assert.That(result.RejectedCount, Is.EqualTo(1));
            Assert.That(result.Messages.Any(m => m.Contains("expected number of columns")), Is.True);
            Assert.That(incidents, Has.Count.EqualTo(0));
        }
    }
}