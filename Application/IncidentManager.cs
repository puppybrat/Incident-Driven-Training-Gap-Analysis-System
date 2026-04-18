using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    public class IncidentManager
    {
        private readonly IncidentRepository _incidentRepository;
        private readonly ReferenceDataRepository _referenceDataRepository;

        public IncidentManager()
        {
            _incidentRepository = new IncidentRepository();
            _referenceDataRepository = new ReferenceDataRepository();
        }

        public IncidentManager(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
            _referenceDataRepository = new ReferenceDataRepository(databaseManager);
        }

        public ReferenceDataSet GetAllReferenceData()
        {
            return _referenceDataRepository.GetAllReferenceData();
        }

        public List<Equipment> GetEquipmentByLine(int lineId)
        {
            return _referenceDataRepository.GetEquipmentByLine(lineId);
        }

        public List<SOP> GetSopsByEquipment(int equipmentId)
        {
            return _referenceDataRepository.GetSopsByEquipment(equipmentId);
        }

        /// <summary>
        /// Creates a new incident using the specified incident data.
        /// Validates incoming data, constructs the object model, and saves it to the data store. Returns true if the incident was created successfully; otherwise, false.
        /// </summary>
        /// <param name="incidentData">The incident information to be used for creating the new incident. Cannot be null.</param>
        /// <returns>true if the incident was created successfully; otherwise, false.</returns>
        public bool CreateIncident(Incident incidentData)
        {
            ValidationResult validationResult = ValidateIncident(incidentData);

            if(!validationResult.IsValid)
            {
                // Display validation error to the user
                return false;
            }

            return SaveIncident(incidentData);
        }

        /// <summary>
        /// Validates the specified incident data and returns the result of the validation.
        /// Checks that all required fields are present and contain valid values, and that the incident ID is unique. Returns a ValidationResult object that indicates whether the incident data is valid and contains any validation errors.
        /// </summary>
        /// <param name="incidentData">The incident data to validate. IncidentId must be unique. Cannot be null.</param>
        /// <returns>A ValidationResult object that indicates whether the incident data is valid and contains any validation
        /// errors.</returns>
        public ValidationResult ValidateIncident(Incident incidentData)
        {
            ValidationResult validationResult = new()
            {
                IsValid = true
            };

            if (incidentData is null)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Incident data is required.");
                return validationResult;
            }

            if (!incidentData.IsCompleteForCreation())
            {
                validationResult.IsValid = false;

                if (incidentData.IncidentId <= 0)
                {
                    validationResult.Errors.Add("Incident ID must be greater than 0.");
                }

                if (incidentData.OccurredAt > DateTime.Now)
                {
                    validationResult.Errors.Add("Occurred At cannot be set past the current time.");
                }

                if (incidentData.ShiftId <= 0)
                {
                    validationResult.Errors.Add("Shift is required.");
                }

                if (incidentData.EquipmentId <= 0)
                {
                    validationResult.Errors.Add("Equipment is required.");
                }
            }

            Incident? existingIncident = _incidentRepository.GetIncidentById(incidentData.IncidentId);
            if (existingIncident is not null)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add($"An incident with ID {incidentData.IncidentId} already exists.");
            }

            return validationResult;
        }

        /// <summary>
        /// Saves the specified incident to the underlying data store.
        /// Hands off the incident to the repository layer for persistence. Returns true if the incident was saved successfully; otherwise, false.
        /// </summary>
        /// <param name="incident">The incident to be saved. Cannot be null.</param>
        /// <returns>true if the incident was saved successfully; otherwise, false.</returns>
        public bool SaveIncident(Incident incident)
        {
            return _incidentRepository.InsertIncident(incident);
        }
    }
}
