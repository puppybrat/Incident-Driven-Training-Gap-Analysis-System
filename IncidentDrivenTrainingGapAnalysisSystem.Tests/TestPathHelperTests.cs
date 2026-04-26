/*
 * File: TestPathHelperTests.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Contains NUnit tests for resolving test data, CSV file, and test database paths.
 */

using IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    /// <summary>
    /// Tests TestPathHelper behavior for resolving test data and file paths.
    /// </summary>
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