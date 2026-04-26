using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class TestPathHelperTests
    {
        [Test]
        public void GetCsvPath_ReturnsExistingFixturePath_ForValidIncidentsCsv()
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

        [Test]
        public void GetCsvPath_ReturnsExistingPath_ForNonCsvTextFile()
        {
            string path = TestPathHelper.GetCsvPath("not-csv.txt");

            Assert.That(File.Exists(path), Is.True);
        }

        [Test]
        public void GetDatabasePath_ReturnsPathInsideDatabaseFolder()
        {
            string path = TestPathHelper.GetDatabasePath("training_gap_analysis.db");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(path, Does.Contain("Database"));
                Assert.That(Path.GetFileName(path), Is.EqualTo("training_gap_analysis.db"));
            }
        }
    }
}