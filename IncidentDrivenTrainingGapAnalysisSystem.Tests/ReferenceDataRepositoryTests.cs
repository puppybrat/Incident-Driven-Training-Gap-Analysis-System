using Incident_Driven_Training_Gap_Analysis_System.Data;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
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
        public void SeedReferenceDataIfNeeded_MakesReferenceDataAvailable()
        {
            var repository = new ReferenceDataRepository(_databaseManager);

            repository.SeedReferenceDataIfNeeded();
            var result = repository.GetAllReferenceData();

            Assert.That(result.Lines.Count, Is.EqualTo(3));
            Assert.That(result.Shifts.Count, Is.EqualTo(3));
            Assert.That(result.Equipment.Count, Is.GreaterThan(0));
            Assert.That(result.Sops.Count, Is.GreaterThan(0));
        }

        [Test]
        public void SeedReferenceDataIfNeeded_DoesNotDuplicateReferenceData_WhenCalledMultipleTimes()
        {
            var repository = new ReferenceDataRepository(_databaseManager);

            repository.SeedReferenceDataIfNeeded();
            var firstResult = repository.GetAllReferenceData();

            repository.SeedReferenceDataIfNeeded();
            var secondResult = repository.GetAllReferenceData();

            Assert.That(secondResult.Lines.Count, Is.EqualTo(firstResult.Lines.Count));
            Assert.That(secondResult.Shifts.Count, Is.EqualTo(firstResult.Shifts.Count));
            Assert.That(secondResult.Equipment.Count, Is.EqualTo(firstResult.Equipment.Count));
            Assert.That(secondResult.Sops.Count, Is.EqualTo(firstResult.Sops.Count));
        }

        [Test]
        public void GetAllReferenceData_ReturnsReferenceDataSet()
        {
            var repository = new ReferenceDataRepository(_databaseManager);
            var result = repository.GetAllReferenceData();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Lines, Is.Not.Empty);
            Assert.That(result.Shifts, Is.Not.Empty);
            Assert.That(result.Equipment, Is.Not.Empty);
            Assert.That(result.Sops, Is.Not.Empty);
        }

        [Test]
        public void GetAllReferenceData_MaintainsValidRelationships()
        {
            var repository = new ReferenceDataRepository(_databaseManager);

            var data = repository.GetAllReferenceData();

            Assert.That(data, Is.Not.Null);

            var lineIds = data.Lines.Select(l => l.LineId).ToHashSet();
            var equipmentIds = data.Equipment.Select(e => e.EquipmentId).ToHashSet();

            Assert.That(data.Equipment.All(e => lineIds.Contains(e.LineId)), Is.True);
            Assert.That(data.Sops.All(s => equipmentIds.Contains(s.EquipmentId)), Is.True);
        }

        [Test]
        public void GetEquipmentByLine_ReturnsOnlyEquipmentForSelectedLine()
        {
            var repository = new ReferenceDataRepository(_databaseManager);
            var result = repository.GetEquipmentByLine(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(result.All(e => e.LineId == 1), Is.True);
            Assert.That(result.Select(e => e.EquipmentId).Distinct().Count(), Is.EqualTo(result.Count));
        }

        [Test]
        public void GetEquipmentByLine_ReturnsEmpty_WhenLineDoesNotExist()
        {
            var repository = new ReferenceDataRepository(_databaseManager);

            var result = repository.GetEquipmentByLine(999);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetSopsByEquipment_ReturnsOnlySopsForSelectedEquipment()
        {
            var repository = new ReferenceDataRepository(_databaseManager);
            var result = repository.GetSopsByEquipment(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(result.All(s => s.EquipmentId == 1), Is.True);
            Assert.That(result.Select(s => s.SopId).Distinct().Count(), Is.EqualTo(result.Count));
        }

        [Test]
        public void GetSopsByEquipment_ReturnsEmpty_WhenEquipmentDoesNotExist()
        {
            var repository = new ReferenceDataRepository(_databaseManager);

            var result = repository.GetSopsByEquipment(999);

            Assert.That(result, Is.Empty);
        }
    }
}