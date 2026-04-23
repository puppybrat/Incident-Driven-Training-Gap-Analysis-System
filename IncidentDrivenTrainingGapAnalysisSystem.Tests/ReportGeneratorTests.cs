using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;


using NUnit.Framework;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class ReportGeneratorTests
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
        public void GenerateReport_ReturnsFormattedGroupedResults_ForRequestedLineAndOutputType()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                IncidentId = 7001,
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1, // Line 1
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                IncidentId = 7002,
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 1, // Line 1
                ShiftId = 2,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                IncidentId = 7003,
                OccurredAt = new DateTime(2026, 4, 10, 10, 0, 0),
                EquipmentId = 2, // Line 2
                ShiftId = 1,
                SopId = 2
            });

            var generator = new ReportGenerator(_databaseManager);

            var request = new ReportRequest
            {
                PresetName = "Incidents per Equipment",
                Filters = new FilterSet { LineId = 1 },
                GroupingType = "Equipment",
                OutputType = "Table"
            };

            var result = generator.GenerateReport(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.OutputType, Is.EqualTo("Table"));
            Assert.That(result.Results, Has.Count.EqualTo(1));
            Assert.That(result.Results[0].DisplayLabel, Does.Contain("1"));
            Assert.That(result.Results[0].DisplayValue, Is.EqualTo("2"));
        }

        [Test]
        public void ApplyFilters_ReturnsOnlyIncidentsForRequestedLine()
        {
            var repository = new IncidentRepository(_databaseManager);

            repository.InsertIncident(new Incident
            {
                IncidentId = 6001,
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1, // seeded under Line 1
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                IncidentId = 6002,
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 2, // seeded under Line 2
                ShiftId = 1,
                SopId = 2
            });

            var generator = new ReportGenerator(_databaseManager);

            var request = new ReportRequest
            {
                Filters = new FilterSet { LineId = 1 },
                GroupingType = "Equipment",
                OutputType = "Table"
            };

            var result = generator.ApplyFilters(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Select(i => i.IncidentId), Is.EquivalentTo(new[] { 6001 }));
        }

        [Test]
        public void AggregateIncidents_ReturnsSeparateGroups_WhenEquipmentIdsDiffer()
        {
            var generator = new ReportGenerator(_databaseManager);

            var incidents = new List<Incident>
            {
                new Incident
                {
                    IncidentId = 3003,
                    EquipmentId = 1,
                    ShiftId = 1,
                    OccurredAt = new DateTime(2026, 4, 10)
                },
                new Incident
                {
                    IncidentId = 3004,
                    EquipmentId = 2,
                    ShiftId = 1,
                    OccurredAt = new DateTime(2026, 4, 10)
                }
            };

            var result = generator.AggregateIncidents(incidents);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Any(r => r.GroupLabel == "1" && r.IncidentCount == 1), Is.True);
            Assert.That(result.Any(r => r.GroupLabel == "2" && r.IncidentCount == 1), Is.True);
        }

        [Test]
        public void AggregateIncidents_ReturnsSingleEquipmentGroup_WithCorrectCount()
        {
            var generator = new ReportGenerator(_databaseManager);

            var incidents = new List<Incident>
            {
                new Incident
                {
                    IncidentId = 3001,
                    EquipmentId = 1,
                    ShiftId = 1,
                    OccurredAt = new DateTime(2026, 4, 10)
                },
                new Incident
                {
                    IncidentId = 3002,
                    EquipmentId = 1,
                    ShiftId = 2,
                    OccurredAt = new DateTime(2026, 4, 10)
                }
            };

            var result = generator.AggregateIncidents(incidents);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].GroupLabel, Is.EqualTo("1"));
            Assert.That(result[0].IncidentCount, Is.EqualTo(2));
            Assert.That(result[0].IsFlagged, Is.False);
        }

        [Test]
        public void FormatResults_ReturnsFormattedResults_FromAggregateCollection()
        {
            var generator = new ReportGenerator(_databaseManager);

            var aggregateCollection = new List<AggregateResult>
            {
                new AggregateResult
                {
                    GroupLabel = "Equipment 1",
                    IncidentCount = 2,
                    IsFlagged = false
                },
                new AggregateResult
                {
                    GroupLabel = "Equipment 2",
                    IncidentCount = 1,
                    IsFlagged = false
                }
            };

            var result = generator.FormatResults(aggregateCollection);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].DisplayLabel, Is.EqualTo("Equipment 1"));
            Assert.That(result[0].DisplayValue, Is.EqualTo("2"));
        }

        [Test]
        public void BuildReportResult_ReturnsFinalOutputObject_WithFormattedResults()
        {
            var generator = new ReportGenerator(_databaseManager);

            var formattedResults = new List<FormattedResult>
            {
                new FormattedResult
                {
                    DisplayLabel = "Equipment 1",
                    DisplayValue = "2"
                },
                new FormattedResult
                {
                    DisplayLabel = "Equipment 2",
                    DisplayValue = "1"
                }
            };

            var result = generator.BuildReportResult(formattedResults);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Count, Is.EqualTo(2));
        }
    }
}