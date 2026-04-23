using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;
using NUnit.Framework;
using System;

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
            var manager = new IncidentManager(_databaseManager);

            var incident = new Incident
            {
                IncidentId = 0,
                OccurredAt = default,
                EquipmentId = 0,
                ShiftId = 0,
                SopId = null
            };

            var result = manager.ValidateIncident(incident);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ValidateIncident_ReturnsFailure_WhenIncidentIdAlreadyExists()
        {
            var repository = new IncidentRepository(_databaseManager);
            repository.InsertIncident(new Incident
            {
                IncidentId = 1001,
                OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            var manager = new IncidentManager(_databaseManager);
            var result = manager.ValidateIncident(new Incident
            {
                IncidentId = 1001,
                OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        }

        [Test]
        public void ValidateIncident_ReturnsFailure_WhenReferencedEntitiesDoNotExist()
        {
            var manager = new IncidentManager(_databaseManager);

            var incident = new Incident
            {
                IncidentId = 1002,
                OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                EquipmentId = 999,
                ShiftId = 999,
                SopId = 999
            };

            var result = manager.ValidateIncident(incident);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        }

        [Test]
        public void CreateIncident_ReturnsFailure_WhenIncidentIsInvalid()
        {
            var manager = new IncidentManager(_databaseManager);

            var incident = new Incident
            {
                IncidentId = 0,
                OccurredAt = default,
                EquipmentId = 0,
                ShiftId = 0,
                SopId = null
            };

            var result = manager.CreateIncident(incident);

            Assert.That(result, Is.False);
        }

        [Test]
        public void CreateIncident_SavesIncident_WhenIncidentDataIsValid()
        {
            var manager = new IncidentManager(_databaseManager);
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                IncidentId = 1003,
                OccurredAt = new DateTime(2026, 4, 1, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            };

            var result = manager.CreateIncident(incident);
            var stored = repository.GetIncidentById(1003);

            Assert.That(result, Is.True);
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.IncidentId, Is.EqualTo(1003));
        }
    }
}