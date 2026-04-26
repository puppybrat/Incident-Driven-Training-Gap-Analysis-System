using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

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
                TimeWindow = "7 days",
                FlagEnabled = true,
                SelectedPresetBehavior = string.Empty
            };

            bool saveResult = repository.SaveRuleConfig(config);

            var result = repository.LoadRuleConfig();

            Assert.That(saveResult, Is.True);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(3));
            Assert.That(result.GroupingType, Is.EqualTo("Line"));
            Assert.That(result.TimeWindow, Is.EqualTo("7 days"));
            Assert.That(result.FlagEnabled, Is.True);
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ResetRuleConfigToDefaults_ReplacesSavedCustomValues()
        {
            var repository = new RuleConfigRepository(_databaseManager);

            repository.SaveRuleConfig(new RuleConfig
            {
                ThresholdValue = 7,
                GroupingType = "Equipment",
                TimeWindow = "90 days",
                FlagEnabled = false,
                SelectedPresetBehavior = "Custom"
            });

            var result = repository.ResetRuleConfigToDefaults();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(RuleConfig.CreateDefault().ThresholdValue));
            Assert.That(result.GroupingType, Is.EqualTo(RuleConfig.CreateDefault().GroupingType));
            Assert.That(result.TimeWindow, Is.EqualTo(RuleConfig.CreateDefault().TimeWindow));
            Assert.That(result.FlagEnabled, Is.EqualTo(RuleConfig.CreateDefault().FlagEnabled));
        }

        [Test]
        public void LoadRuleConfig_ReturnsDefaultConfiguration_WhenNoSavedConfigurationExists()
        {
            var repository = new RuleConfigRepository(_databaseManager);

            var result = repository.LoadRuleConfig();
            var expected = RuleConfig.CreateDefault();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(expected.ThresholdValue));
            Assert.That(result.GroupingType, Is.EqualTo(expected.GroupingType));
            Assert.That(result.TimeWindow, Is.EqualTo(expected.TimeWindow));
            Assert.That(result.FlagEnabled, Is.EqualTo(expected.FlagEnabled));
        }

        [Test]
        public void SaveRuleConfig_UpdatesExistingConfiguration_WhenConfigurationAlreadyExists()
        {
            var repository = new RuleConfigRepository(_databaseManager);

            repository.SaveRuleConfig(new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = true,
                SelectedPresetBehavior = string.Empty
            });

            bool saveResult = repository.SaveRuleConfig(new RuleConfig
            {
                ThresholdValue = 9,
                GroupingType = "Equipment",
                TimeWindow = "90 days",
                FlagEnabled = false,
                SelectedPresetBehavior = "RuntimeOnly"
            });

            var result = repository.LoadRuleConfig();

            Assert.That(saveResult, Is.True);
            Assert.That(result.ThresholdValue, Is.EqualTo(9));
            Assert.That(result.GroupingType, Is.EqualTo("Equipment"));
            Assert.That(result.TimeWindow, Is.EqualTo("90 days"));
            Assert.That(result.FlagEnabled, Is.False);
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo(string.Empty));
        }
    }
}