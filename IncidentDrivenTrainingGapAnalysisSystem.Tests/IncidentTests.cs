using Incident_Driven_Training_Gap_Analysis_System.Domain;

namespace IncidentDrivenTrainingGapAnalysisSystem.Tests
{
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