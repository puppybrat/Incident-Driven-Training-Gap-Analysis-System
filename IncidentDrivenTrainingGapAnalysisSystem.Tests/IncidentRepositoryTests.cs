using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

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
        public void InsertIncident_PersistsSingleIncidentRecord()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            };

            var result = repository.InsertIncident(incident);

            var stored = repository.GetIncidents(new FilterSet())
                .SingleOrDefault(i =>
                    i.OccurredAt == new DateTime(2026, 4, 10, 8, 0, 0)
                    && i.EquipmentId == 1
                    && i.ShiftId == 1
                    && i.SopId == 1);

            Assert.That(result, Is.True);
            Assert.That(stored, Is.Not.Null);
            Assert.That(stored!.IncidentId, Is.GreaterThan(0));
        }

        [Test]
        public void InsertIncident_ReturnsFalse_WhenForeignKeyReferenceIsInvalid()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 9999,
                ShiftId = 1,
                SopId = null
            };

            var result = repository.InsertIncident(incident);

            var stored = repository.GetIncidents(new FilterSet());

            Assert.That(result, Is.False);
            Assert.That(stored, Is.Empty);
        }

        [Test]
        public void InsertIncidents_PersistsAllProvidedIncidentRecords()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incidents = new List<Incident>
            {
                new Incident
                {
                    OccurredAt = new DateTime(2026, 4, 10, 11, 0, 0),
                    EquipmentId = 1,
                    ShiftId = 1,
                    SopId = 1
                },
                new Incident
                {
                    OccurredAt = new DateTime(2026, 4, 10, 12, 0, 0),
                    EquipmentId = 2,
                    ShiftId = 2,
                    SopId = 3
                }
            };

            var result = repository.InsertIncidents(incidents);

            var stored = repository.GetIncidents(new FilterSet());

            Assert.That(result, Is.True);
            Assert.That(stored.Any(i =>
                i.OccurredAt == new DateTime(2026, 4, 10, 11, 0, 0)
                && i.EquipmentId == 1
                && i.ShiftId == 1
                && i.SopId == 1), Is.True);

            Assert.That(stored.Any(i =>
                i.OccurredAt == new DateTime(2026, 4, 10, 12, 0, 0)
                && i.EquipmentId == 2
                && i.ShiftId == 2
                && i.SopId == 3), Is.True);
        }

        [Test]
        public void InsertIncidents_RollsBackAllRecords_WhenOneIncidentFails()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incidents = new List<Incident>
            {
                new Incident
                {
                    OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                    EquipmentId = 1,
                    ShiftId = 1,
                    SopId = 1
                },
                new Incident
                {
                    OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                    EquipmentId = 9999,
                    ShiftId = 1,
                    SopId = null
                }
            };

            var result = repository.InsertIncidents(incidents);

            var stored = repository.GetIncidents(new FilterSet());

            Assert.That(result, Is.False);
            Assert.That(stored, Is.Empty);
        }

        [Test]
        public void GetIncidents_ReturnsInsertedIncidentWithMappedFields()
        {
            var repository = new IncidentRepository(_databaseManager);

            var incident = new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            };

            bool insertResult = repository.InsertIncident(incident);

            var result = repository.GetIncidents(new FilterSet())
                .SingleOrDefault(i =>
                    i.OccurredAt == new DateTime(2026, 4, 10, 9, 0, 0)
                    && i.EquipmentId == 1
                    && i.ShiftId == 1
                    && i.SopId == 1);

            Assert.That(insertResult, Is.True);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.IncidentId, Is.GreaterThan(0));
            Assert.That(result.EquipmentId, Is.EqualTo(1));
            Assert.That(result.ShiftId, Is.EqualTo(1));
            Assert.That(result.SopId, Is.EqualTo(1));
        }

        [Test]
        public void GetIncidents_ReturnsOnlyMatchingIncident_WhenLineFilterIsApplied()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 13, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 14, 0, 0),
                EquipmentId = 2,
                ShiftId = 2,
                SopId = 3
            });

            var filterSet = new FilterSet
            {
                LineId = 1
            };

            var result = repository.GetIncidents(filterSet);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].EquipmentId, Is.EqualTo(1));
            Assert.That(result[0].OccurredAt, Is.EqualTo(new DateTime(2026, 4, 10, 13, 0, 0)));
        }

        [Test]
        public void GetIncidents_ReturnsOnlyMissingSopIncidents_WhenRequireMissingSopIsTrue()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            var filterSet = new FilterSet
            {
                RequireMissingSop = true
            };

            var result = repository.GetIncidents(filterSet);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SopId, Is.Null);
            Assert.That(result[0].OccurredAt, Is.EqualTo(new DateTime(2026, 4, 10, 8, 0, 0)));
        }

        [Test]
        public void GetIncidents_ReturnsOnlyMatchingSopIncidents_WhenSopFilterIsApplied()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 2,
                ShiftId = 1,
                SopId = 3
            });

            var filterSet = new FilterSet
            {
                SopId = 1
            };

            var result = repository.GetIncidents(filterSet);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].SopId, Is.EqualTo(1));
            Assert.That(result[0].EquipmentId, Is.EqualTo(1));
        }

        [Test]
        public void GetIncidents_ReturnsOnlyIncidentsWithinDateRange()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 9, 23, 59, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 12, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 11, 23, 59, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 12, 0, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            var filterSet = new FilterSet
            {
                StartDate = new DateTime(2026, 4, 10),
                EndDate = new DateTime(2026, 4, 11)
            };

            var result = repository.GetIncidents(filterSet);

            Assert.That(result.Select(i => i.OccurredAt), Is.EquivalentTo(new[]
            {
                new DateTime(2026, 4, 10, 12, 0, 0),
                new DateTime(2026, 4, 11, 23, 59, 0)
            }));
        }

        [Test]
        public void GetIncidents_ReturnsOnlyMatchingIncidents_WhenShiftFilterIsApplied()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 1,
                ShiftId = 2,
                SopId = 1
            });

            var result = repository.GetIncidents(new FilterSet
            {
                ShiftId = 1
            });

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].ShiftId, Is.EqualTo(1));
            Assert.That(result[0].OccurredAt, Is.EqualTo(new DateTime(2026, 4, 10, 8, 0, 0)));
        }

        [Test]
        public void GetIncidents_ReturnsOnlyMatchingIncidents_WhenEquipmentFilterIsApplied()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 2,
                ShiftId = 1,
                SopId = 3
            });

            var result = repository.GetIncidents(new FilterSet
            {
                EquipmentId = 1
            });

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].EquipmentId, Is.EqualTo(1));
            Assert.That(result[0].OccurredAt, Is.EqualTo(new DateTime(2026, 4, 10, 8, 0, 0)));
        }

        [Test]
        public void GetIncidents_ThrowsArgumentNullException_WhenFilterSetIsNull()
        {
            var repository = new IncidentRepository(_databaseManager);

            Assert.Throws<ArgumentNullException>(() =>
            {
                repository.GetIncidents(null!);
            });
        }
    }
}