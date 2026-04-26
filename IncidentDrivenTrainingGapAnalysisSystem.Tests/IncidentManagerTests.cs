using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class IncidentManagerTests
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
        public void ValidateIncident_ReturnsFailure_WhenRequiredFieldsAreMissing()
        {
            var incident = new Incident
            {
                OccurredAt = default,
                EquipmentId = 0,
                ShiftId = 0,
                SopId = null
            };

            var result = IncidentManager.ValidateIncident(incident);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("Shift is required."));
            Assert.That(result.Errors, Does.Contain("Equipment is required."));
        }

        [Test]
        public void ValidateIncident_ReturnsFailure_WhenIncidentIsNull()
        {
            var result = IncidentManager.ValidateIncident(null!);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("Incident data is required."));
        }

        [Test]
        public void ValidateIncident_ReturnsFailure_WhenOccurredAtIsInFuture()
        {
            var incident = new Incident
            {
                OccurredAt = DateTime.Now.AddDays(1),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            var result = IncidentManager.ValidateIncident(incident);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e => e.Contains("cannot be set past")), Is.True);
        }

        [Test]
        public void ValidateIncident_ReturnsSuccess_WhenRequiredFieldsAreValid()
        {
            var incident = new Incident
            {
                OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            var result = IncidentManager.ValidateIncident(incident);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Is.Empty);
        }

        [Test]
        public void CreateIncident_ReturnsFailure_WhenIncidentIsInvalid()
        {
            var manager = new IncidentManager(_databaseManager);

            var incident = new Incident
            {
                OccurredAt = default,
                EquipmentId = 0,
                ShiftId = 0,
                SopId = null
            };

            var result = manager.CreateIncident(incident);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        }

        [Test]
        public void CreateIncident_SavesIncident_WhenIncidentDataIsValid()
        {
            var manager = new IncidentManager(_databaseManager);
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            };

            var result = manager.CreateIncident(incident);
            var stored = repository.GetIncidents(new FilterSet())
                .SingleOrDefault(i =>
                    i.OccurredAt == new DateTime(2026, 4, 1, 8, 0, 0)
                    && i.EquipmentId == 1
                    && i.ShiftId == 1
                    && i.SopId == 1);

            Assert.That(result.IsValid, Is.True);
            Assert.That(stored, Is.Not.Null);
        }

        [Test]
        public void CreateIncident_ReturnsFailure_WhenIncidentIsNull()
        {
            var manager = new IncidentManager(_databaseManager);

            var result = manager.CreateIncident(null!);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Does.Contain("Incident data is required."));
        }

        [Test]
        public void GetAllReferenceData_ReturnsSeededReferenceData()
        {
            var manager = new IncidentManager(_databaseManager);

            var result = manager.GetAllReferenceData();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Lines, Is.Not.Empty);
            Assert.That(result.Shifts, Is.Not.Empty);
            Assert.That(result.Equipment, Is.Not.Empty);
            Assert.That(result.Sops, Is.Not.Empty);
        }

        [Test]
        public void GetEquipmentByLine_ReturnsOnlyEquipmentForSelectedLine()
        {
            var manager = new IncidentManager(_databaseManager);
            var referenceData = manager.GetAllReferenceData();

            int lineId = referenceData.Lines.First().LineId;

            var result = manager.GetEquipmentByLine(lineId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.All(e => e.LineId == lineId), Is.True);
        }

        [Test]
        public void GetSopsByEquipment_ReturnsOnlySopsForSelectedEquipment()
        {
            var manager = new IncidentManager(_databaseManager);
            var referenceData = manager.GetAllReferenceData();

            int equipmentId = referenceData.Equipment.First().EquipmentId;

            var result = manager.GetSopsByEquipment(equipmentId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.All(s => s.EquipmentId == equipmentId), Is.True);
        }
    }
}