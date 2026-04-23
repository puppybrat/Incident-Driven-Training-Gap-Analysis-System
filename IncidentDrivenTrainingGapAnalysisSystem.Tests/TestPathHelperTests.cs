using System.IO;
using NUnit.Framework;
using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class TestPathHelperTests
    {
        [Test]
        public void GetCsvPath_ReturnsExistingPath_ForValidIncidentsCsv()
        {
            string path = TestPathHelper.GetCsvPath("valid-incidents.csv");

            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void GetCsvPath_ReturnsExistingPath_ForInvalidHeadersCsv()
        {
            string path = TestPathHelper.GetCsvPath("invalid-headers.csv");

            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void GetCsvPath_ReturnsExistingPath_ForMalformedIncidentsCsv()
        {
            string path = TestPathHelper.GetCsvPath("malformed-incidents.csv");

            Assert.That(File.Exists(path), Is.True);
        }
    }
}