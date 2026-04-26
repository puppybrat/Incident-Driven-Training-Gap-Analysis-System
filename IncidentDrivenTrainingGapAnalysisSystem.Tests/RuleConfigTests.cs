using Incident_Driven_Training_Gap_Analysis_System.Domain;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class RuleConfigTests
    {
        [Test]
        public void CreateDefault_ReturnsExpectedDefaultValues()
        {
            var result = RuleConfig.CreateDefault();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(5));
            Assert.That(result.GroupingType, Is.EqualTo("Shift"));
            Assert.That(result.TimeWindow, Is.EqualTo("7 days"));
            Assert.That(result.FlagEnabled, Is.True);
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Normalize_ReturnsDefaultConfig_WhenInputIsNull()
        {
            var result = RuleConfig.Normalize(null);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(5));
            Assert.That(result.GroupingType, Is.EqualTo("Shift"));
            Assert.That(result.TimeWindow, Is.EqualTo("7 days"));
            Assert.That(result.FlagEnabled, Is.True);
        }

        [Test]
        public void Normalize_ReturnsDefault_WhenConfigIsNull()
        {
            var result = RuleConfig.Normalize(null);

            var expected = RuleConfig.CreateDefault();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThresholdValue, Is.EqualTo(expected.ThresholdValue));
            Assert.That(result.GroupingType, Is.EqualTo(expected.GroupingType));
            Assert.That(result.TimeWindow, Is.EqualTo(expected.TimeWindow));
            Assert.That(result.FlagEnabled, Is.EqualTo(expected.FlagEnabled));
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo(expected.SelectedPresetBehavior));
        }

        [Test]
        public void Normalize_ReplacesInvalidValues_WithDefaults()
        {
            var input = new RuleConfig
            {
                ThresholdValue = -10,
                GroupingType = "Invalid",
                TimeWindow = "Invalid",
                FlagEnabled = true,
                SelectedPresetBehavior = null!
            };

            var result = RuleConfig.Normalize(input);

            Assert.That(result.ThresholdValue, Is.EqualTo(5));
            Assert.That(result.GroupingType, Is.EqualTo("Shift"));
            Assert.That(result.TimeWindow, Is.EqualTo("7 days"));
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Normalize_KeepsValidValues_WhenInputIsValid()
        {
            var input = new RuleConfig
            {
                ThresholdValue = 10,
                GroupingType = "Line",
                TimeWindow = "30 days",
                FlagEnabled = false,
                SelectedPresetBehavior = "Custom"
            };

            var result = RuleConfig.Normalize(input);

            Assert.That(result.ThresholdValue, Is.EqualTo(10));
            Assert.That(result.GroupingType, Is.EqualTo("Line"));
            Assert.That(result.TimeWindow, Is.EqualTo("30 days"));
            Assert.That(result.FlagEnabled, Is.False);
            Assert.That(result.SelectedPresetBehavior, Is.EqualTo("Custom"));
        }

        [Test]
        public void IsValidGroupingType_ReturnsTrue_ForValidOption()
        {
            var result = RuleConfig.IsValidGroupingType("Equipment");

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValidGroupingType_ReturnsFalse_ForInvalidOption()
        {
            var result = RuleConfig.IsValidGroupingType("Invalid");

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidGroupingType_ReturnsFalse_WhenNullOrEmpty()
        {
            Assert.That(RuleConfig.IsValidGroupingType(null), Is.False);
            Assert.That(RuleConfig.IsValidGroupingType(string.Empty), Is.False);
            Assert.That(RuleConfig.IsValidGroupingType(" "), Is.False);
        }

        [Test]
        public void IsValidTimeWindow_ReturnsTrue_ForValidOption()
        {
            var result = RuleConfig.IsValidTimeWindow("30 days");

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValidTimeWindow_ReturnsFalse_ForInvalidOption()
        {
            var result = RuleConfig.IsValidTimeWindow("Invalid");

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidTimeWindow_ReturnsFalse_WhenNullOrEmpty()
        {
            Assert.That(RuleConfig.IsValidTimeWindow(null), Is.False);
            Assert.That(RuleConfig.IsValidTimeWindow(string.Empty), Is.False);
            Assert.That(RuleConfig.IsValidTimeWindow(" "), Is.False);
        }
    }
}