using Incident_Driven_Training_Gap_Analysis_System.Domain;
using NUnit.Framework;
using System;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
    [TestFixture]
    public class IncidentTests
    {
        [Test]
        public void IsCompleteForCreation_ReturnsFalse_WhenRequiredFieldsAreMissing()
        {
            var incident = new Incident
            {
                IncidentId = 0,
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
            var incident = new Incident
            {
                IncidentId = 1001,
                OccurredAt = new DateTime(2026, 4, 1, 8, 30, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            bool result = incident.IsCompleteForCreation();

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasSopReference_ReturnsTrue_WhenSopIdHasValue()
        {
            var incident = new Incident
            {
                IncidentId = 1002,
                OccurredAt = new DateTime(2026, 4, 1, 9, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = 5
            };

            bool result = incident.HasSopReference();

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasSopReference_ReturnsFalse_WhenSopIdIsNull()
        {
            var incident = new Incident
            {
                IncidentId = 1003,
                OccurredAt = new DateTime(2026, 4, 1, 10, 0, 0),
                EquipmentId = 1,
                ShiftId = 1,
                SopId = null
            };

            bool result = incident.HasSopReference();

            Assert.That(result, Is.False);
        }
    }
}