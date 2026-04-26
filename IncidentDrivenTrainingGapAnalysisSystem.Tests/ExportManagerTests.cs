/*
 * File: ExportManagerTests.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Contains NUnit tests for dataset export, report export, export path validation,
 * filtered export behavior, and CSV output generation.
 */

using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    /// <summary>
    /// Tests ExportManager behavior for exporting incident datasets and report results to CSV files.
    /// </summary>
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
        public void ExportDataset_ReturnsFailure_WhenPathIsInvalid()
        {
            ExportManager manager = new(_databaseManager);

            var result = manager.ExportDataset("", new FilterSet());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Does.Contain("invalid"));
            }
        }

        [Test]
        public void ExportDataset_ReturnsFailure_WhenNoIncidentsMatchFilter()
        {
            ExportManager manager = new(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "empty-export-test.csv");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var result = manager.ExportDataset(filePath, new FilterSet());

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Does.Contain("no incident data"));
                Assert.That(File.Exists(filePath), Is.False);
            }
        }

        [Test]
        public void ExportDataset_WritesOnlyFilteredRows_ToCsvFile()
        {
            IncidentRepository repository = new(_databaseManager);

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

            ExportManager manager = new(_databaseManager);
            string filePath = Path.Combine(Path.GetTempPath(), "export-dataset-test.csv");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            FilterSet filterSet = new() { LineId = 1 };

            var result = manager.ExportDataset(filePath, filterSet);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Success, Is.True);
                Assert.That(File.Exists(filePath), Is.True);
            }

            string[] lines = File.ReadAllLines(filePath);

            Assert.That(lines, Has.Length.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(lines[0], Is.EqualTo("OccurredAt,EquipmentId,ShiftId,SopId"));
                Assert.That(lines[1], Is.EqualTo("2026-04-10 08:00:00,1,1,1"));
            }
        }

        [Test]
        public void ExportReport_ReturnsFailure_WhenPathIsInvalid()
        {
            ReportResult reportResult = new()
            {
                IncludeLine = true,
                Rows =
                [
                    new ReportRow
                    {
                        Line = "Line 1",
                        IncidentCount = 1
                    }
                ]
            };

            var result = ExportManager.ExportReport("", reportResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Does.Contain("invalid"));
            }
        }

        [Test]
        public void ExportReport_WritesReportRows_ToCsvFile()
        {
            string filePath = Path.Combine(Path.GetTempPath(), "export-report-test.csv");

            ReportResult reportResult = new()
            {
                OutputType = "Table",
                IncludeLine = true,
                IncludeShift = true,
                IncludeEquipment = false,
                IncludeSop = false,
                Rows =
        [
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
        ]
            };

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var result = ExportManager.ExportReport(filePath, reportResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Success, Is.True);
                Assert.That(File.Exists(filePath), Is.True);
            }

            string[] lines = File.ReadAllLines(filePath);

            Assert.That(lines, Has.Length.EqualTo(3));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(lines[0], Is.EqualTo("Line,Shift,IncidentCount,Status"));
                Assert.That(lines[1], Is.EqualTo("Line 1,Shift 1,3,Flagged"));
                Assert.That(lines[2], Is.EqualTo("Line 2,Shift 2,1,Normal"));
            }
        }
    }
}