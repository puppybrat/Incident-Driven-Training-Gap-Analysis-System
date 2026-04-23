using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;
using NUnit.Framework;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class RuleEvaluatorTests
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
        public void ValidateRuleConfig_ReturnsFailure_WhenThresholdIsInvalid()
        {
            var evaluator = new RuleEvaluator(_databaseManager);

            var ruleConfig = new RuleConfig
            {
                ThresholdValue = 0,
                GroupingType = "Line",
                TimeWindow = "30Days",
                FlagEnabled = true,
                SelectedPresetBehavior = "Default"
            };

            var result = evaluator.ValidateRuleConfig(ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ValidateRuleConfig_ReturnsFailure_WhenGroupingTypeIsInvalid()
        {
            var evaluator = new RuleEvaluator(_databaseManager);

            var ruleConfig = new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "InvalidGrouping",
                TimeWindow = "30Days",
                FlagEnabled = true,
                SelectedPresetBehavior = "Default"
            };

            var result = evaluator.ValidateRuleConfig(ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ValidateRuleConfig_ReturnsFailure_WhenTimeWindowIsInvalid()
        {
            var evaluator = new RuleEvaluator(_databaseManager);

            var ruleConfig = new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "InvalidWindow",
                FlagEnabled = true,
                SelectedPresetBehavior = "Default"
            };

            var result = evaluator.ValidateRuleConfig(ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void LoadCurrentRuleConfig_ReturnsSavedConfigurationValues()
        {
            var repository = new RuleConfigRepository(_databaseManager);
            repository.SaveRuleConfig(new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "30Days",
                FlagEnabled = true,
                SelectedPresetBehavior = "Default"
            });

            var evaluator = new RuleEvaluator(_databaseManager);
            var result = evaluator.LoadCurrentRuleConfig();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(3));
            Assert.That(result.GroupingType, Is.EqualTo("Line"));
            Assert.That(result.TimeWindow, Is.EqualTo("30Days"));
            Assert.That(result.FlagEnabled, Is.True);
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo("Default"));
        }

        [Test]
        public void EvaluateThresholds_FlagsResults_WhenCountsMeetOrExceedThreshold()
        {
            var evaluator = new RuleEvaluator(_databaseManager);

            var reportData = new List<AggregateResult>
            {
                new AggregateResult
                {
                    GroupLabel = "Line 1",
                    IncidentCount = 3,
                    IsFlagged = false
                }
            };

            var ruleConfig = new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "30Days",
                FlagEnabled = true,
                SelectedPresetBehavior = "Default"
            };

            var result = evaluator.EvaluateThresholds(reportData, ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].IsFlagged, Is.True);
        }

        [Test]
        public void EvaluateThresholds_DoesNotFlagResults_WhenCountsAreBelowThreshold()
        {
            var evaluator = new RuleEvaluator(_databaseManager);

            var reportData = new List<AggregateResult>
            {
                new AggregateResult
                {
                    GroupLabel = "Line 1",
                    IncidentCount = 2,
                    IsFlagged = false
                }
            };

            var ruleConfig = new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "30Days",
                FlagEnabled = true,
                SelectedPresetBehavior = "Default"
            };

            var result = evaluator.EvaluateThresholds(reportData, ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].IsFlagged, Is.False);
        }

        [Test]
        public void FlagResults_ReturnsFlaggedAggregateResults()
        {
            var evaluator = new RuleEvaluator(_databaseManager);

            var reportData = new List<AggregateResult>
            {
                new AggregateResult
                {
                    GroupLabel = "Line 1",
                    IncidentCount = 3,
                    IsFlagged = true
                },
                new AggregateResult
                {
                    GroupLabel = "Line 2",
                    IncidentCount = 1,
                    IsFlagged = false
                }
            };

            var result = evaluator.FlagResults(reportData);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].GroupLabel, Is.EqualTo("Line 1"));
            Assert.That(result[0].IsFlagged, Is.True);
        }
    }
}