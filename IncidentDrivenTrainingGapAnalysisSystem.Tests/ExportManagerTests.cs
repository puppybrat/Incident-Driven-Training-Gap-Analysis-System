using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

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
        public void ExportDataset_ReturnsFailure_WhenPathIsInvalid()
        {
            var manager = new ExportManager(_databaseManager);

            var result = manager.ExportDataset("", new FilterSet());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("invalid"));
        }

        [Test]
        public void ExportDataset_ReturnsFailure_WhenNoIncidentsMatchFilter()
        {
            var manager = new ExportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "empty-export-test.csv");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var result = manager.ExportDataset(filePath, new FilterSet());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("no incident data"));
            Assert.That(File.Exists(filePath), Is.False);
        }

        [Test]
        public void ExportDataset_WritesOnlyFilteredRows_ToCsvFile()
        {
            var repository = new IncidentRepository(_databaseManager);

            bool firstInsert = repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 8, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 1
            });

            Assert.That(firstInsert, Is.True);

            bool secondInsert = repository.InsertIncident(new Incident
            {
                OccurredAt = new DateTime(2026, 4, 10, 9, 0, 0),
                EquipmentId = 2,
                ShiftId = 2,
                SopId = 3
            });

            Assert.That(secondInsert, Is.True);

            var manager = new ExportManager(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "export-dataset-test.csv");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var filterSet = new FilterSet { LineId = 1 };

            var result = manager.ExportDataset(filePath, filterSet);

            Assert.That(result.Success, Is.True);
            Assert.That(File.Exists(filePath), Is.True);

            string[] lines = File.ReadAllLines(filePath);

            Assert.That(lines, Has.Length.EqualTo(2));
            Assert.That(lines[0], Is.EqualTo("OccurredAt,EquipmentId,ShiftId,SopId"));
            Assert.That(lines[1], Is.EqualTo("2026-04-10 08:00:00,1,1,1"));
        }

        [Test]
        public void ExportReport_ReturnsFailure_WhenPathIsInvalid()
        {
            var reportResult = new ReportResult
            {
                IncludeLine = true,
                Rows = new List<ReportRow>
                {
                    new ReportRow
                    {
                        Line = "Line 1",
                        IncidentCount = 1
                    }
                }
            };

            var result = ExportManager.ExportReport("", reportResult);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("invalid"));
        }

        [Test]
        public void ExportReport_WritesReportRows_ToCsvFile()
        {
            string filePath = Path.Combine(Path.GetTempPath(), "export-report-test.csv");

            var reportResult = new ReportResult
            {
                OutputType = "Table",
                IncludeLine = true,
                IncludeShift = true,
                IncludeEquipment = false,
                IncludeSop = false,
                Rows = new List<ReportRow>
        {
            new ReportRow
            {
                Line = "Line 1",
                Shift = "Shift 1",
                IncidentCount = 3,
                IsFlagged = true
            },
            new ReportRow
            {
                Line = "Line 2",
                Shift = "Shift 2",
                IncidentCount = 1,
                IsFlagged = false
            }
        }
            };

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var result = ExportManager.ExportReport(filePath, reportResult);

            Assert.That(result.Success, Is.True);
            Assert.That(File.Exists(filePath), Is.True);

            string[] lines = File.ReadAllLines(filePath);

            Assert.That(lines, Has.Length.EqualTo(3));
            Assert.That(lines[0], Is.EqualTo("Line,Shift,IncidentCount,Status"));
            Assert.That(lines[1], Is.EqualTo("Line 1,Shift 1,3,Flagged"));
            Assert.That(lines[2], Is.EqualTo("Line 2,Shift 2,1,Normal"));
        }
    }
}