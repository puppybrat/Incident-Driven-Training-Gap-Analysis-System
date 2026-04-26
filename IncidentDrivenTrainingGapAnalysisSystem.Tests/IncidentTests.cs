/*
 * File: IncidentTests.cs
 * Author: Sarah Portillo
 * Date: 04/26/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Purpose:
 * Contains NUnit tests for domain-level Incident validation behavior.
 */

using Incident_Driven_Training_Gap_Analysis_System.Domain;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    /// <summary>
    /// Tests Incident domain behavior for determining whether required creation fields are complete.
    /// </summary>
    [TestFixture]
    public class IncidentTests
    {
        [Test]
        public void IsCompleteForCreation_ReturnsFalse_WhenRequiredFieldsAreMissing()
        {
            Incident incident = new()
            {
                OccurredAt = default,
                EquipmentId = 0,
                ShiftId = 0,
                SopId = null
            };

            bool result = incident.IsCompleteForCreation();

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsCompleteForCreation_ReturnsTrue_WhenAllRequiredFieldsArePresent()
        {
            Incident incident = new()
            {
                OccurredAt = DateTime.Now.AddDays(-1),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            bool result = incident.IsCompleteForCreation();

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsCompleteForCreation_ReturnsFalse_WhenOccurredAtIsInFuture()
        {
            Incident incident = new()
            {
                OccurredAt = DateTime.Now.AddDays(1),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            bool result = incident.IsCompleteForCreation();

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsCompleteForCreation_ReturnsFalse_WhenEquipmentIdIsMissing()
        {
            Incident incident = new()
            {
                OccurredAt = DateTime.Now.AddDays(-1),
                EquipmentId = 0,
                ShiftId = 1,
                SopId = null
            };

            bool result = incident.IsCompleteForCreation();

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsCompleteForCreation_ReturnsFalse_WhenShiftIdIsMissing()
        {
            Incident incident = new()
            {
                OccurredAt = DateTime.Now.AddDays(-1),
                EquipmentId = 1,
                ShiftId = 0,
                SopId = null
            };

            bool result = incident.IsCompleteForCreation();

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsCompleteForCreation_ReturnsTrue_WhenSopIdIsMissing()
        {
            Incident incident = new()
            {
                OccurredAt = DateTime.Now.AddDays(-1),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            bool result = incident.IsCompleteForCreation();

            Assert.That(result, Is.True);
        }
    }
}