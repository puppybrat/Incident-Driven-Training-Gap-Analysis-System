/*
 * File: TestPathHelper.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Provides helper methods for resolving test data, CSV, and database paths
 * during NUnit test execution.
 */

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers
{
    /// <summary>
    /// Provides helper methods for resolving test data paths used by the NUnit test suite.
    /// </summary>
    public static class TestPathHelper
    {
        /// <summary>
        /// Gets the root TestData folder path for the executing test project.
        /// </summary>
        /// <returns>The full path to the TestData folder.</returns>
        public static string GetTestDataPath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "TestData");
        }

        /// <summary>
        /// Gets the full path to a CSV test data file.
        /// </summary>
        /// <param name="fileName">The CSV file name.</param>
        /// <returns>The full path to the requested CSV file.</returns>
        public static string GetCsvPath(string fileName)
        {
            return Path.Combine(GetTestDataPath(), "Csv", fileName);
        }

        /// <summary>
        /// Gets the full path to a SQLite test database file.
        /// </summary>
        /// <param name="fileName">The database file name.</param>
        /// <returns>The full path to the requested database file.</returns>
        public static string GetDatabasePath(string fileName)
        {
            return Path.Combine(GetTestDataPath(), "Database", fileName);
        }
    }
}