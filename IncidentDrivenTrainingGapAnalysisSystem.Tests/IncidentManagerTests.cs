/*
 * File: IncidentManagerTests.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Contains NUnit tests for incident validation, incident creation,
 * database-backed persistence, and reference data lookup behavior.
 */

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    /// <summary>
    /// Tests IncidentManager behavior for validating and creating incident records.
    /// </summary>
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

            ReferenceDataRepository referenceDataRepository = new(_databaseManager);
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
            Incident incident = new()
            {
                OccurredAt = default,
                EquipmentId = 0,
                ShiftId = 0,
                SopId = null
            };

            var result = IncidentManager.ValidateIncident(incident);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors, Does.Contain("Shift is required."));
            }
            Assert.That(result.Errors, Does.Contain("Equipment is required."));
        }

        [Test]
        public void ValidateIncident_ReturnsFailure_WhenIncidentIsNull()
        {
            var result = IncidentManager.ValidateIncident(null!);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors, Does.Contain("Incident data is required."));
            }
        }

        [Test]
        public void ValidateIncident_ReturnsFailure_WhenOccurredAtIsInFuture()
        {
            Incident incident = new()
            {
                OccurredAt = DateTime.Now.AddDays(1),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            var result = IncidentManager.ValidateIncident(incident);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors.Any(e => e.Contains("cannot be set past")), Is.True);
            }
        }

        [Test]
        public void ValidateIncident_ReturnsSuccess_WhenRequiredFieldsAreValid()
        {
            Incident incident = new()
            {
                OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            var result = IncidentManager.ValidateIncident(incident);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public void CreateIncident_ReturnsFailure_WhenIncidentIsInvalid()
        {
            IncidentManager manager = new(_databaseManager);

            Incident incident = new()
            {
                OccurredAt = default,
                EquipmentId = 0,
                ShiftId = 0,
                SopId = null
            };

            var result = manager.CreateIncident(incident);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors, Is.Not.Empty);
            }
        }

        [Test]
        public void CreateIncident_SavesIncident_WhenIncidentDataIsValid()
        {
            IncidentManager manager = new(_databaseManager);
            IncidentRepository repository = new(_databaseManager);

            Incident incident = new()
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

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(stored, Is.Not.Null);
            }
        }

        [Test]
        public void CreateIncident_ReturnsFailure_WhenIncidentIsNull()
        {
            IncidentManager manager = new(_databaseManager);

            var result = manager.CreateIncident(null!);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors, Does.Contain("Incident data is required."));
            }
        }

        [Test]
        public void GetAllReferenceData_ReturnsSeededReferenceData()
        {
            IncidentManager manager = new(_databaseManager);

            var result = manager.GetAllReferenceData();

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Lines, Is.Not.Empty);
                Assert.That(result.Shifts, Is.Not.Empty);
                Assert.That(result.Equipment, Is.Not.Empty);
                Assert.That(result.Sops, Is.Not.Empty);
            }
        }

        [Test]
        public void GetEquipmentByLine_ReturnsOnlyEquipmentForSelectedLine()
        {
            IncidentManager manager = new(_databaseManager);
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
            IncidentManager manager = new(_databaseManager);
            var referenceData = manager.GetAllReferenceData();

            int equipmentId = referenceData.Equipment.First().EquipmentId;

            var result = manager.GetSopsByEquipment(equipmentId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.All(s => s.EquipmentId == equipmentId), Is.True);
        }
    }
}