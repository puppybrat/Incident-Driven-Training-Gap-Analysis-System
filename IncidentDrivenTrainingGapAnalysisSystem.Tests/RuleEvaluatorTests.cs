using Incident_Driven_Training_Gap_Analysis_System.Application;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

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
        public void ValidateRuleConfig_ReturnsFailure_WhenThresholdIsNegative()
        {
            RuleConfig ruleConfig = new()
            {
                ThresholdValue = -1,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = true
            };

            var result = RuleEvaluator.ValidateRuleConfig(ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ValidateRuleConfig_ReturnsFailure_WhenGroupingTypeIsInvalid()
        {
            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 3,
                GroupingType = "InvalidGrouping",
                TimeWindow = "7 days",
                FlagEnabled = true
            };

            var result = RuleEvaluator.ValidateRuleConfig(ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ValidateRuleConfig_ReturnsFailure_WhenTimeWindowIsInvalid()
        {
            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "InvalidWindow",
                FlagEnabled = true
            };

            var result = RuleEvaluator.ValidateRuleConfig(ruleConfig);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void ValidateRuleConfig_ReturnsFailure_WhenConfigIsNull()
        {
            var result = RuleEvaluator.ValidateRuleConfig(null!);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(result.Errors, Does.Contain("Rule configuration is required."));
            }
        }

        [Test]
        public void ValidateRuleConfig_ReturnsSuccess_WhenConfigIsValid()
        {
            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = true
            };

            var result = RuleEvaluator.ValidateRuleConfig(ruleConfig);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
            }
        }

        [Test]
        public void LoadCurrentRuleConfig_ReturnsSavedConfigurationValues()
        {
            RuleConfigRepository repository = new(_databaseManager);
            repository.SaveRuleConfig(new RuleConfig
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = true
            });

            RuleEvaluator evaluator = new(_databaseManager);
            var result = evaluator.LoadCurrentRuleConfig();

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ThresholdValue, Is.EqualTo(3));
                Assert.That(result.GroupingType, Is.EqualTo("Line"));
                Assert.That(result.TimeWindow, Is.EqualTo("7 days"));
                Assert.That(result.FlagEnabled, Is.True);
            }
        }

        [Test]
        public void EvaluateThresholds_FlagsRows_WhenCountsMeetOrExceedThreshold()
        {
            List<ReportRow> reportData =
            [
                new() {
                    GroupValue = "Line 1",
                    IncidentCount = 3,
                    IsFlagged = false
                }
            ];

            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = true
            };

            var result = RuleEvaluator.EvaluateThresholds(reportData, ruleConfig);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsFlagged, Is.True);
        }

        [Test]
        public void EvaluateThresholds_DoesNotFlagRows_WhenBelowThreshold()
        {
            List<ReportRow> reportData =
            [
                new() {
                    GroupValue = "Line 1",
                    IncidentCount = 2,
                    IsFlagged = false
                }
            ];

            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = true
            };

            var result = RuleEvaluator.EvaluateThresholds(reportData, ruleConfig);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsFlagged, Is.False);
        }

        [Test]
        public void EvaluateThresholds_DoesNotFlag_WhenFlaggingIsDisabled()
        {
            List<ReportRow> reportData =
            [
                new() {
                    GroupValue = "Line 1",
                    IncidentCount = 10,
                    IsFlagged = false
                }
            ];

            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = false
            };

            var result = RuleEvaluator.EvaluateThresholds(reportData, ruleConfig);

            Assert.That(result[0].IsFlagged, Is.False);
        }

        [Test]
        public void EvaluateThresholds_ReturnsEmptyList_WhenInputIsNull()
        {
            var result = RuleEvaluator.EvaluateThresholds(null, null);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void EvaluateThresholds_ClearsExistingFlag_WhenRowIsBelowThreshold()
        {
            List<ReportRow> reportData =
            [
                new() {
                    GroupValue = "Line 1",
                    IncidentCount = 2,
                    IsFlagged = true
                }
            ];

            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 3,
                GroupingType = "Line",
                TimeWindow = "7 days",
                FlagEnabled = true
            };

            var result = RuleEvaluator.EvaluateThresholds(reportData, ruleConfig);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsFlagged, Is.False);
        }

        [Test]
        public void EvaluateThresholds_UsesDefaultConfig_WhenRuleConfigIsNull()
        {
            List<ReportRow> reportData =
            [
                new() {
                    GroupValue = "Line 1",
                    IncidentCount = 5,
                    IsFlagged = false
                }
            ];

            var result = RuleEvaluator.EvaluateThresholds(reportData, null);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsFlagged, Is.True);
        }

        [Test]
        public void SaveRuleConfig_ReturnsFailure_WhenConfigIsInvalid()
        {
            RuleEvaluator evaluator = new(_databaseManager);

            RuleConfig invalidConfig = new()
            {
                ThresholdValue = -1,
                GroupingType = "Invalid",
                TimeWindow = "Invalid",
                FlagEnabled = true
            };

            var result = evaluator.SaveRuleConfig(invalidConfig);

            Assert.That(result.IsValid, Is.False);
        }

        [Test]
        public void SaveRuleConfig_ReturnsSuccess_AndPersistsConfig_WhenConfigIsValid()
        {
            RuleEvaluator evaluator = new(_databaseManager);

            RuleConfig ruleConfig = new()
            {
                ThresholdValue = 4,
                GroupingType = "Equipment",
                TimeWindow = "30 days",
                FlagEnabled = false,
                SelectedPresetBehavior = "RuntimeOnly"
            };

            var result = evaluator.SaveRuleConfig(ruleConfig);
            var saved = evaluator.LoadCurrentRuleConfig();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.Errors, Is.Empty);
                Assert.That(saved.ThresholdValue, Is.EqualTo(4));
                Assert.That(saved.GroupingType, Is.EqualTo("Equipment"));
                Assert.That(saved.TimeWindow, Is.EqualTo("30 days"));
                Assert.That(saved.FlagEnabled, Is.False);
                Assert.That(saved.SelectedPresetBehavior, Is.EqualTo(string.Empty));
            }
        }
    }
}