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

        /// <summary>
        /// Creates a new incident using the specified incident data.
        /// Validates incoming data, constructs the object model, and saves it to the data store. Returns true if the incident was created successfully; otherwise, false.
        /// </summary>
        /// <param name="incidentData">The incident information to be used for creating the new incident. Cannot be null.</param>
        /// <returns>true if the incident was created successfully; otherwise, false.</returns>
        public bool CreateIncident(Incident incidentData)
        {
            return false;
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
            return new ValidationResult();
        }

        /// <summary>
        /// Saves the specified incident to the underlying data store.
        /// Hands off the incident to the repository layer for persistence. Returns true if the incident was saved successfully; otherwise, false.
        /// </summary>
        /// <param name="incident">The incident to be saved. Cannot be null.</param>
        /// <returns>true if the incident was saved successfully; otherwise, false.</returns>
        public bool SaveIncident(Incident incident)
        {
            return false;
        }
    }
}
