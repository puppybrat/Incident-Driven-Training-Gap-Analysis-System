/*
 * File: IncidentManager.cs
 * Author: Sarah Portillo
 * Date: 04/19/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class manages incident creation and related reference data access.
 * It validates new incident records before saving them through the data layer.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Provides incident creation and related reference data operations.
    /// </summary>
    public class IncidentManager
    {
        private readonly IncidentRepository _incidentRepository;
        private readonly ReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// Initializes incident services using the default incident and reference data repositories.
        /// </summary>
        public IncidentManager()
        {
            _incidentRepository = new IncidentRepository();
            _referenceDataRepository = new ReferenceDataRepository();
        }

        /// <summary>
        /// Initializes incident services with a database manager, primarily for controlled database access in tests.
        /// </summary>
        /// <param name="databaseManager">The database manager used to create incident repositories.</param>
        public IncidentManager(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
            _referenceDataRepository = new ReferenceDataRepository(databaseManager);
        }

        /// <summary>
        /// Retrieves reference data needed for incident entry and related UI selections.
        /// </summary>
        /// <returns>A data set containing lines, shifts, equipment, and SOPs.</returns>
        public ReferenceDataSet GetAllReferenceData()
        {
            return _referenceDataRepository.GetAllReferenceData();
        }

        /// <summary>
        /// Retrieves equipment assigned to a production line.
        /// </summary>
        /// <param name="lineId">The production line identifier.</param>
        /// <returns>A list of matching equipment.</returns>
        public List<Equipment> GetEquipmentByLine(int lineId)
        {
            return _referenceDataRepository.GetEquipmentByLine(lineId);
        }

        /// <summary>
        /// Retrieves SOPs assigned to an equipment item.
        /// </summary>
        /// <param name="equipmentId">The equipment identifier.</param>
        /// <returns>A list of matching SOPs.</returns>
        public List<SOP> GetSopsByEquipment(int equipmentId)
        {
            return _referenceDataRepository.GetSopsByEquipment(equipmentId);
        }

        /// <summary>
        /// Validates and saves a new incident record.
        /// </summary>
        /// <param name="incidentData">The incident data to create.</param>
        /// <returns>A validation result describing success or failure.</returns>
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
        /// Validates required incident creation fields and returns any validation errors.
        /// </summary>
        /// <param name="incidentData">The incident data to validate.</param>
        /// <returns>A validation result containing success status and error messages.</returns>
        public static ValidationResult ValidateIncident(Incident incidentData)
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
        /// Saves an incident through the repository.
        /// </summary>
        /// <param name="incident">The incident to save.</param>
        /// <returns>true if the incident was saved; otherwise, false.</returns>
        private bool SaveIncident(Incident incident)
        {
            return _incidentRepository.InsertIncident(incident);
        }
    }
}
