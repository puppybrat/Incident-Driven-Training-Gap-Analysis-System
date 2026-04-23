using System;
using System.IO;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests.Helpers
{
    public static class TestPathHelper
    {
        public static string GetTestDataPath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "TestData");
        }

        public static string GetCsvPath(string fileName)
        {
            return Path.Combine(GetTestDataPath(), "Csv", fileName);
        }

        public static string GetDatabasePath(string fileName)
        {
            return Path.Combine(GetTestDataPath(), "Database", fileName);
        }
    }
}