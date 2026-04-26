using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

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
        public void GenerateReport_GroupsMatchingIncidents_WhenGroupingFieldsMatch()
        {
            IncidentRepository repository = new(_databaseManager);

            bool firstInsert = repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            bool secondInsert = repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            bool otherEquipmentInsert = repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 10, 0, 0),
                EquipmentId = 2,
                ShiftId = 1,
                SopId = 3
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(firstInsert, Is.True);
                Assert.That(secondInsert, Is.True);
                Assert.That(otherEquipmentInsert, Is.True);
            }

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                PresetName = ReportPresetNames.IncidentsPerEquipment,
                Filters = new FilterSet
                {
                    EquipmentId = 1,
                    StartDate = new DateTime(2026, 4, 10),
                    EndDate = new DateTime(2026, 4, 10)
                },
                GroupingType = "Equipment",
                OutputType = "Table",
                IncludeEquipment = true
            };

            var result = generator.GenerateReport(request);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.OutputType, Is.EqualTo("Table"));
                Assert.That(result.Rows, Has.Count.EqualTo(1));
            }

            ReportRow row = result.Rows[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(row.Equipment, Is.EqualTo("Bottle Labeler"));
                Assert.That(row.GroupValue, Does.Contain("Bottle Labeler"));
                Assert.That(row.IncidentCount, Is.EqualTo(2));
            }
        }

        [Test]
        public void GenerateReport_ThrowsArgumentNullException_WhenRequestIsNull()
        {
            ReportGenerator generator = new(_databaseManager);

            Assert.Throws<ArgumentNullException>(() => generator.GenerateReport(null!));
        }

        [Test]
        public void GenerateReport_CopiesRequestMetadata_ToReportResult()
        {
            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                PresetName = ReportPresetNames.IncidentsPerShiftByLine,
                Filters = new FilterSet(),
                GroupingType = "Shift",
                OutputType = "Chart",
                IncludeLine = true,
                IncludeShift = true,
                IncludeEquipment = false,
                IncludeSop = true
            };

            var result = generator.GenerateReport(request);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.PresetName, Is.EqualTo(ReportPresetNames.IncidentsPerShiftByLine));
                Assert.That(result.OutputType, Is.EqualTo("Chart"));
                Assert.That(result.IncludeLine, Is.True);
                Assert.That(result.IncludeShift, Is.True);
                Assert.That(result.IncludeEquipment, Is.False);
                Assert.That(result.IncludeSop, Is.True);
            }
        }

        [Test]
        public void GenerateReport_UsesMissingSopLabel_WhenIncidentHasNullSopId()
        {
            IncidentRepository repository = new(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 11, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            });

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                PresetName = ReportPresetNames.None,
                Filters = new FilterSet(),
                GroupingType = "SOP",
                OutputType = "Table",
                IncludeSop = true
            };

            var result = generator.GenerateReport(request);

            Assert.That(result.Rows, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rows[0].SOP, Is.EqualTo("Missing SOP"));
                Assert.That(result.Rows[0].GroupValue, Does.Contain("Missing SOP"));
                Assert.That(result.Rows[0].IncidentCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void GenerateReport_ReturnsOnlyMissingSopIncidents_WhenMissingSopPresetIsUsed()
        {
            IncidentRepository repository = new(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 12, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 12, 9, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                PresetName = ReportPresetNames.IncidentsPerMissingSopByLine,
                Filters = new FilterSet(),
                GroupingType = "Line",
                OutputType = "Table",
                IncludeLine = true,
                IncludeSop = true
            };

            var result = generator.GenerateReport(request);

            Assert.That(result.Rows, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rows[0].SOP, Is.EqualTo("Missing SOP"));
                Assert.That(result.Rows[0].IncidentCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void GenerateReport_ReturnsAllIncidentsGroup_WhenNoFieldsAreIncluded()
        {
            IncidentRepository repository = new(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 13, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 13, 9, 0, 0),
                EquipmentId = 2,
                ShiftId = 2,
                SopId = 3
            });

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                PresetName = ReportPresetNames.None,
                Filters = new FilterSet(),
                GroupingType = "Line",
                OutputType = "Table",
                IncludeLine = false,
                IncludeShift = false,
                IncludeEquipment = false,
                IncludeSop = false
            };

            var result = generator.GenerateReport(request);

            Assert.That(result.Rows, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rows[0].GroupValue, Is.EqualTo("All Incidents"));
                Assert.That(result.Rows[0].IncidentCount, Is.EqualTo(2));
            }
        }

        [Test]
        public void GenerateReport_BuildsGroupValueStartingWithShift_WhenGroupingTypeIsShift()
        {
            IncidentRepository repository = new(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                PresetName = ReportPresetNames.IncidentsPerShiftByLine,
                Filters = new FilterSet(),
                GroupingType = "Shift",
                OutputType = "Table",
                IncludeLine = true,
                IncludeShift = true,
                IncludeEquipment = true,
                IncludeSop = true
            };

            var result = generator.GenerateReport(request);

            Assert.That(result.Rows, Has.Count.EqualTo(1));
            Assert.That(result.Rows[0].GroupValue, Is.EqualTo("1st Shift | Bottle Line | Bottle Labeler | Bottle Labeler Operation"));
        }

        [Test]
        public void GenerateReport_FlagsRow_WhenIncidentCountExceedsThreshold()
        {
            using (var connection = _databaseManager.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
            INSERT OR REPLACE INTO RuleConfig
                (ruleConfigId, thresholdValue, groupingType, timeWindow, flagEnabled)
            VALUES
                (1, 3, 'Equipment', '7 days', 1);";

                command.ExecuteNonQuery();
            }

            IncidentRepository repository = new(_databaseManager);

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
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 10, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 11, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                PresetName = ReportPresetNames.IncidentsPerEquipment,
                Filters = new FilterSet(),
                GroupingType = "Equipment",
                OutputType = "Table",
                IncludeEquipment = true
            };

            var result = generator.GenerateReport(request);

            Assert.That(result.Rows, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Rows[0].Equipment, Is.EqualTo("Bottle Labeler"));
                Assert.That(result.Rows[0].IncidentCount, Is.EqualTo(4));
                Assert.That(result.Rows[0].IsFlagged, Is.True);
            }
        }

        [Test]
        public void ApplyFilters_ReturnsOnlyIncidentsForRequestedLine()
        {
            IncidentRepository repository = new(_databaseManager);

            bool firstInsert = repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            bool secondInsert = repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 2,
                ShiftId = 1,
                SopId = 3
            });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(firstInsert, Is.True);
                Assert.That(secondInsert, Is.True);
            }

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                Filters = new FilterSet { LineId = 1 },
                GroupingType = "Equipment",
                OutputType = "Table"
            };

            var result = generator.ApplyFilters(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].EquipmentId, Is.EqualTo(1));
                Assert.That(result[0].OccurredAt, Is.EqualTo(new DateTime(2026, 4, 10, 8, 0, 0)));
            }
        }

        [Test]
        public void ApplyFilters_ThrowsArgumentNullException_WhenRequestIsNull()
        {
            ReportGenerator generator = new(_databaseManager);

            Assert.Throws<ArgumentNullException>(() => generator.ApplyFilters(null!));
        }

        [Test]
        public void ApplyFilters_UsesEmptyFilterSet_WhenFiltersAreNull()
        {
            IncidentRepository repository = new(_databaseManager);

            repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            ReportGenerator generator = new(_databaseManager);

            ReportRequest request = new()
            {
                Filters = null!,
                GroupingType = "Line",
                OutputType = "Table"
            };

            var result = generator.ApplyFilters(request);

            Assert.That(result, Has.Count.EqualTo(1));
        }

    }
}