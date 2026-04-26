/*
 * File: SmokeTests.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Contains smoke tests confirming that the test project is deployed and executable.
 */

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    /// <summary>
    /// Provides basic smoke tests to confirm that the test environment is functioning.
    /// </summary>
    [TestFixture]
    public class SmokeTests
    {
        [Test]
        public void TestEnvironment_IsWorking()
        {
            Assert.Pass("Test project is configured correctly.");
        }
    }
}