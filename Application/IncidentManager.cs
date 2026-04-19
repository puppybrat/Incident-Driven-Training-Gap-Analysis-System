/*
 * File: IncidentManager.cs
 * Author: Sarah Portillo
 * Date: 04/19/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class is responsible for managing the business logic related to incident creation within
 * the Incident-Driven Training Gap Analysis System. It retrieves data from the data persistence layer
 * and returns it for use by the UI layer. It receives incident data from the UI layer, validates it,
 * and then calls supporting methods to save the incident to the data layer. The IncidentManager
 * serves as an intermediary between the UI and data layers for the life cycle of an incident,
 * ensuring that all necessary validations are applied before any data is persisted.
 * 
*/

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Provides methods for managing incidents and retrieving related reference data, such as equipment and standard
    /// operating procedures (SOPs), for use by the UI layer.
    /// </summary>
    public class IncidentManager
    {
        private readonly IncidentRepository _incidentRepository;
        private readonly ReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// Default constructor that initializes the IncidentManager with a new instance of IncidentRepository and ReferenceDataRepository.
        /// </summary>
        public IncidentManager()
        {
            _incidentRepository = new IncidentRepository();
            _referenceDataRepository = new ReferenceDataRepository();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public IncidentManager(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
            _referenceDataRepository = new ReferenceDataRepository(databaseManager);
        }

        /// <summary>
        /// Retrieves all available reference data as a single data set for use by the UI layer.
        /// </summary>
        /// <returns>A <see cref="ReferenceDataSet"/> containing all reference data.</returns>
        public ReferenceDataSet GetAllReferenceData()
        {
            return _referenceDataRepository.GetAllReferenceData();
        }

        /// <summary>
        /// Retrieves a list of equipment associated with the specified production line for use by the UI layer.
        /// </summary>
        /// <param name="lineId">The unique identifier of the production line for which to retrieve equipment.</param>
        /// <returns>A list of equipment assigned to the specified production line.</returns>
        public List<Equipment> GetEquipmentByLine(int lineId)
        {
            return _referenceDataRepository.GetEquipmentByLine(lineId);
        }

        /// <summary>
        /// Retrieves a list of standard operating procedures (SOPs) associated with the specified equipment.
        /// </summary>
        /// <param name="equipmentId">The unique identifier of the equipment for which to retrieve SOPs.</param>
        /// <returns>A list of SOP objects linked to the specified equipment.</returns>
        public List<SOP> GetSopsByEquipment(int equipmentId)
        {
            return _referenceDataRepository.GetSopsByEquipment(equipmentId);
        }

        /// <summary>
        /// Validates and attempts to create a new incident record using the provided incident data.
        /// </summary>
        /// <remarks>If the incident data is invalid or the save operation fails, the returned
        /// ValidationResult will contain error messages describing the issues. The method does not throw exceptions for
        /// validation or save failures; callers should inspect the ValidationResult to determine success.</remarks>
        /// <param name="incidentData">The incident information to be validated and saved. Cannot be null.</param>
        /// <returns>A ValidationResult indicating whether the incident was successfully created. If validation fails or the
        /// incident cannot be saved, the result contains error details.</returns>
        public ValidationResult CreateIncident(Incident incidentData)
        {
            ValidationResult validationResult = ValidateIncident(incidentData);

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            bool saved = SaveIncident(incidentData);

            if (!saved)
            {
                validationResult.IsValid = false;
                validationResult.Errors.Add("Failed to save incident.");
            }

            return validationResult;
        }

        /// <summary>
        /// Validates the specified incident data and returns the result of the validation.
        /// Checks that all required fields are present and contain valid values. Returns a ValidationResult object that indicates whether the incident data is valid and contains any validation errors.
        /// Includes checks for required fields and valid date constraints.
        /// </summary>
        /// <param name="incidentData">The incident data to validate.</param>
        /// <returns>A ValidationResult object that indicates whether the incident data is valid and contains any validation errors.</returns>
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

            return validationResult;
        }

        /// <summary>
        /// Passes the specified incident to the data access layer for saving to the data store. Returns true if the incident was saved successfully; otherwise, false.
        /// </summary>
        /// <param name="incident">The incident to be saved. Cannot be null.</param>
        /// <returns>true if the incident was saved successfully; otherwise, false.</returns>
        private bool SaveIncident(Incident incident)
        {
            return _incidentRepository.InsertIncident(incident);
        }
    }
}
