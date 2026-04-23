using Incident_Driven_Training_Gap_Analysis_System.Data;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;
using NUnit.Framework;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class DatabaseAndReferenceDataTests
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
        public void InitializeDatabase_CreatesOrPreparesDatabaseForUse()
        {
            var result = _databaseManager.InitializeDatabase();
            Assert.That(result, Is.True);
        }

        [Test]
        public void EnsureSchema_CreatesRequiredTablesAndEnablesForeignKeys()
        {
            var result = _databaseManager.EnsureSchema();
            Assert.That(result, Is.True);
        }

        [Test]
        public void SeedReferenceDataIfNeeded_MakesReferenceDataAvailable()
        {
            var repository = new ReferenceDataRepository(_databaseManager);

            repository.SeedReferenceDataIfNeeded();
            var result = repository.GetAllReferenceData();

            Assert.That(result.Lines, Is.Not.Empty);
            Assert.That(result.Shifts, Is.Not.Empty);
            Assert.That(result.Equipment, Is.Not.Empty);
            Assert.That(result.Sops, Is.Not.Empty);
        }

        [Test]
        public void GetAllReferenceData_ReturnsReferenceDataSet()
        {
            var repository = new ReferenceDataRepository(_databaseManager);
            var result = repository.GetAllReferenceData();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Lines, Is.Not.Null);
            Assert.That(result.Shifts, Is.Not.Null);
            Assert.That(result.Equipment, Is.Not.Null);
            Assert.That(result.Sops, Is.Not.Null);
        }

        [Test]
        public void GetEquipmentByLine_ReturnsOnlyEquipmentForSelectedLine()
        {
            var repository = new ReferenceDataRepository(_databaseManager);
            var result = repository.GetEquipmentByLine(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(result.All(e => e.LineId == 1), Is.True);
        }

        [Test]
        public void GetSopsByEquipment_ReturnsOnlySopsForSelectedEquipment()
        {
            var repository = new ReferenceDataRepository(_databaseManager);
            var result = repository.GetSopsByEquipment(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(result.All(s => s.EquipmentId == 1), Is.True);
        }
    }
}