using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;
using NUnit.Framework;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class IncidentRepositoryTests
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
        public void InsertIncident_PersistsSingleIncidentRecord()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                IncidentId = 2001,
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            };

            var result = repository.InsertIncident(incident);
            var stored = repository.GetIncidentById(2001);

            Assert.That(result, Is.True);
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.IncidentId, Is.EqualTo(2001));
        }

        [Test]
        public void InsertIncidents_PersistsAllProvidedIncidentRecords()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incidents = new List<Incident>
            {
                new Incident
                {
                    IncidentId = 2004,
                    OccurredAt = new DateTime(2026, 4, 10, 11, 0, 0),
                    EquipmentId = 1,
                    ShiftId = 1,
                    SopId = 1
                },
                new Incident
                {
                    IncidentId = 2005,
                    OccurredAt = new DateTime(2026, 4, 10, 12, 0, 0),
                    EquipmentId = 2,
                    ShiftId = 2,
                    SopId = 2
                }
            };

            var result = repository.InsertIncidents(incidents);

            Assert.That(result, Is.True);
            Assert.That(repository.GetIncidentById(2004), Is.Not.Null);
            Assert.That(repository.GetIncidentById(2005), Is.Not.Null);
        }

        [Test]
        public void GetIncidentById_ReturnsMatchingIncident_WhenIdExists()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                IncidentId = 2002,
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            };

            repository.InsertIncident(incident);

            var result = repository.GetIncidentById(2002);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.IncidentId, Is.EqualTo(2002));
            Assert.That(result.EquipmentId, Is.EqualTo(1));
            Assert.That(result.ShiftId, Is.EqualTo(1));
            Assert.That(result.SopId, Is.EqualTo(1));
        }

        [Test]
        public void GetIncidents_ReturnsOnlyMatchingIncidentIds_WhenLineFilterIsApplied()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                IncidentId = 2006,
                OccurredAt = new DateTime(2026, 4, 10, 13, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                IncidentId = 2007,
                OccurredAt = new DateTime(2026, 4, 10, 14, 0, 0),
                EquipmentId = 2,
                ShiftId = 2,
                SopId = 2
            });

            var filterSet = new FilterSet
            {
                LineId = 1
            };

            var result = repository.GetIncidents(filterSet);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Select(i => i.IncidentId), Is.EquivalentTo(new[] { 2006 }));
        }

        [Test]
        public void GetAggregatedIncidents_ReturnsExactGroupedCount_ForSelectedGroupingType()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                IncidentId = 2008,
                OccurredAt = new DateTime(2026, 4, 10, 15, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                IncidentId = 2009,
                OccurredAt = new DateTime(2026, 4, 10, 16, 0, 0),
                EquipmentId = 1,
                ShiftId = 2,
                SopId = 1
            });

            var result = repository.GetAggregatedIncidents(new FilterSet(), "Equipment");

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].GroupLabel, Is.EqualTo("1"));
            Assert.That(result[0].IncidentCount, Is.EqualTo(2));
        }

        [Test]
        public void CountIncidents_ReturnsMatchingIncidentCount()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                IncidentId = 2003,
                OccurredAt = new DateTime(2026, 4, 10, 10, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            };

            repository.InsertIncident(incident);

            var filterSet = new FilterSet();

            var result = repository.CountIncidents(filterSet);

            Assert.That(result, Is.GreaterThan(0));
        }
    }
}