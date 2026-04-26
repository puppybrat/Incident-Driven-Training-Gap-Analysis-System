using Incident_Driven_Training_Gap_Analysis_System.Domain;

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
            var incident = new Incident
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
            var incident = new Incident
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
            var incident = new Incident
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
            var incident = new Incident
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