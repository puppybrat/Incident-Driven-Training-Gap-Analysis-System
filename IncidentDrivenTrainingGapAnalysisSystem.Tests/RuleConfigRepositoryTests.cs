using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;
using NUnit.Framework;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class RuleConfigRepositoryTests
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
        public void LoadRuleConfig_ReturnsPreviouslySavedConfigurationValues()
        {
            var repository = new RuleConfigRepository(_databaseManager);

            var config = new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "30Days",
                FlagEnabled = true,
                SelectedPresetBehavior = "Default"
            };

            repository.SaveRuleConfig(config);

            var result = repository.LoadRuleConfig();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(3));
            Assert.That(result.GroupingType, Is.EqualTo("Line"));
            Assert.That(result.TimeWindow, Is.EqualTo("30Days"));
            Assert.That(result.FlagEnabled, Is.True);
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo("Default"));
        }

        [Test]
        public void ResetRuleConfigToDefaults_ReplacesSavedCustomValues()
        {
            var repository = new RuleConfigRepository(_databaseManager);

            repository.SaveRuleConfig(new RuleConfig
            {
                ThresholdValue = 7,
                GroupingType = "Equipment",
                TimeWindow = "90Days",
                FlagEnabled = false,
                SelectedPresetBehavior = "Custom"
            });

            var result = repository.ResetRuleConfigToDefaults();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.Not.EqualTo(7));
            Assert.That(result.GroupingType, Is.Not.EqualTo("Equipment"));
            Assert.That(result.TimeWindow, Is.Not.EqualTo("90Days"));
        }
    }
}