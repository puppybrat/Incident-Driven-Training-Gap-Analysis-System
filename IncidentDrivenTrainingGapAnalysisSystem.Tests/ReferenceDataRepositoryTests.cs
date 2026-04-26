/*
 * File: ReferenceDataRepositoryTests.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Contains NUnit tests for seeding, retrieving, and filtering line, shift,
 * equipment, and SOP reference data.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    /// <summary>
    /// Tests ReferenceDataRepository behavior for seeded reference data retrieval and filtering.
    /// </summary>
    [TestFixture]
    public class ReferenceDataRepositoryTests
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
        public void SeedReferenceDataIfNeeded_MakesReferenceDataAvailable()
        {
            ReferenceDataRepository repository = new(_databaseManager);

            repository.SeedReferenceDataIfNeeded();
            var result = repository.GetAllReferenceData();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Lines, Has.Count.EqualTo(3));
                Assert.That(result.Shifts, Has.Count.EqualTo(3));
                Assert.That(result.Equipment, Is.Not.Empty);
                Assert.That(result.Sops, Is.Not.Empty);
            }
        }

        [Test]
        public void SeedReferenceDataIfNeeded_DoesNotDuplicateReferenceData_WhenCalledMultipleTimes()
        {
            ReferenceDataRepository repository = new(_databaseManager);

            repository.SeedReferenceDataIfNeeded();
            var firstResult = repository.GetAllReferenceData();

            repository.SeedReferenceDataIfNeeded();
            var secondResult = repository.GetAllReferenceData();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(secondResult.Lines, Has.Count.EqualTo(firstResult.Lines.Count));
                Assert.That(secondResult.Shifts, Has.Count.EqualTo(firstResult.Shifts.Count));
                Assert.That(secondResult.Equipment, Has.Count.EqualTo(firstResult.Equipment.Count));
                Assert.That(secondResult.Sops, Has.Count.EqualTo(firstResult.Sops.Count));
            }
        }

        [Test]
        public void GetAllReferenceData_ReturnsReferenceDataSet()
        {
            ReferenceDataRepository repository = new(_databaseManager);
            var result = repository.GetAllReferenceData();
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
        public void GetAllReferenceData_MaintainsValidRelationships()
        {
            ReferenceDataRepository repository = new(_databaseManager);

            var data = repository.GetAllReferenceData();

            Assert.That(data, Is.Not.Null);

            HashSet<int> lineIds = [.. data.Lines.Select(l => l.LineId)];
            HashSet<int> equipmentIds = [.. data.Equipment.Select(e => e.EquipmentId)];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(data.Equipment.All(e => lineIds.Contains(e.LineId)), Is.True);
                Assert.That(data.Sops.All(s => equipmentIds.Contains(s.EquipmentId)), Is.True);
            }
        }

        [Test]
        public void GetEquipmentByLine_ReturnsOnlyEquipmentForSelectedLine()
        {
            ReferenceDataRepository repository = new(_databaseManager);
            var result = repository.GetEquipmentByLine(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.All(e => e.LineId == 1), Is.True);
                Assert.That(result.Select(e => e.EquipmentId).Distinct().Count(), Is.EqualTo(result.Count));
            }
        }

        [Test]
        public void GetEquipmentByLine_ReturnsEmpty_WhenLineDoesNotExist()
        {
            ReferenceDataRepository repository = new(_databaseManager);

            var result = repository.GetEquipmentByLine(999);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetSopsByEquipment_ReturnsOnlySopsForSelectedEquipment()
        {
            ReferenceDataRepository repository = new(_databaseManager);
            var result = repository.GetSopsByEquipment(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.All(s => s.EquipmentId == 1), Is.True);
                Assert.That(result.Select(s => s.SopId).Distinct().Count(), Is.EqualTo(result.Count));
            }
        }

        [Test]
        public void GetSopsByEquipment_ReturnsEmpty_WhenEquipmentDoesNotExist()
        {
            ReferenceDataRepository repository = new(_databaseManager);

            var result = repository.GetSopsByEquipment(999);

            Assert.That(result, Is.Empty);
        }
    }
}