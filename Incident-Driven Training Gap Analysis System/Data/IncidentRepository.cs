/*
 * File: IncidentRepository.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class is responsible for managing the persistence of incident records in the database.
 * It provides methods to insert, retrieve, and count incident records, ensuring that
 * incident data is stored and retrieved correctly across application sessions. The repository
 * interacts with the DatabaseManager to perform database operations and handles any exceptions
 * that may arise during these interactions, providing a robust mechanism for maintaining
 * the integrity of the incident data.
*/

using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Provides methods for inserting, retrieving, and counting incident records in the database.
    /// Supports single and batch inserts, retrieval by identifier, and filtered queries for incident data access.
    /// </summary>
    public class IncidentRepository
    {
        private readonly DatabaseManager _databaseManager;

        /// <summary>
        /// Default constructor that initializes the IncidentRepository with a new instance of DatabaseManager.
        /// </summary>
        public IncidentRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public IncidentRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Inserts a single incident record.
        /// </summary>
        /// <param name="incident">The incident to insert. Cannot be null.</param>
        /// <returns>true if exactly one record was inserted; otherwise, false.</returns>
        public bool InsertIncident(Incident incident)
        {
            ArgumentNullException.ThrowIfNull(incident);

            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                const string sql = @"
                    INSERT INTO Incident (occurredAt, equipmentId, shiftId, sopId)
                    VALUES (@occurredAt, @equipmentId, @shiftId, @sopId);";

                using SqliteCommand command = new(sql, connection);
                AddIncidentParameters(command, incident);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts a collection of incident records within a single transaction.
        /// </summary>
        /// <param name="incidentCollection">The incident records to insert.</param>
        /// <returns>true if all records were inserted successfully; otherwise, false.</returns>
        public bool InsertIncidents(List<Incident> incidentCollection)
        {
            ArgumentNullException.ThrowIfNull(incidentCollection);

            const string sql = @"
                INSERT INTO Incident (occurredAt, equipmentId, shiftId, sopId)
                VALUES (@occurredAt, @equipmentId, @shiftId, @sopId);";

            return _databaseManager.ExecuteTransaction((connection, transaction) =>
            {
                foreach (Incident incident in incidentCollection)
                {
                    ArgumentNullException.ThrowIfNull(incident);

                    using SqliteCommand command = new(sql, connection, transaction);
                    AddIncidentParameters(command, incident);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected != 1)
                    {
                        throw new InvalidOperationException("Failed to insert incident record.");
                    }
                }
            });
        }

        /// <summary>
        /// Retrieves an incident by its identifier.
        /// </summary>
        /// <param name="incidentId">The incident identifier.</param>
        /// <returns>The matching incident, or null if no match is found.</returns>
        public Incident? GetIncidentById(int incidentId)
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                const string sql = @"
                    SELECT incidentId, occurredAt, equipmentId, shiftId, sopId
                    FROM Incident
                    WHERE incidentId = @incidentId;";

                using SqliteCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@incidentId", incidentId);

                using SqliteDataReader reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    return null;
                }

                return MapIncident(reader);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves incidents that match the specified filters.
        /// </summary>
        /// <param name="filterSet">The filters to apply.</param>
        /// <returns>A list of matching incidents. Returns an empty list when no matches are found.</returns>
        public List<Incident> GetIncidents(FilterSet filterSet)
        {
            ArgumentNullException.ThrowIfNull(filterSet);

            List<Incident> incidents = new();

            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                const string sql = @"
                    SELECT i.incidentId, i.occurredAt, i.equipmentId, i.shiftId, i.sopId
                    FROM Incident i
                    INNER JOIN Equipment e ON i.equipmentId = e.equipmentId
                    WHERE (@lineId IS NULL OR e.lineId = @lineId)
                      AND (@shiftId IS NULL OR i.shiftId = @shiftId)
                      AND (@equipmentId IS NULL OR i.equipmentId = @equipmentId)
                      AND ((@requireMissingSop = 1 AND i.sopId IS NULL) OR (@requireMissingSop = 0 AND (@sopId IS NULL OR i.sopId = @sopId)))
                      AND (@startDate IS NULL OR i.occurredAt >= @startDate)
                      AND (@endDate IS NULL OR i.occurredAt < @endDate)
                    ORDER BY i.occurredAt;";

                using SqliteCommand command = new(sql, connection);

                AddFilterParameters(command, filterSet);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    incidents.Add(MapIncident(reader));
                }
            }
            catch
            {
                return new List<Incident>();
            }

            return incidents;
        }

        /// <summary>
        /// Counts incidents that match the specified filters.
        /// </summary>
        /// <param name="filterSet">The filters to apply.</param>
        /// <returns>The number of matching incidents.</returns>
        public int CountIncidents(FilterSet filterSet)
        {
            ArgumentNullException.ThrowIfNull(filterSet);

            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                const string sql = @"
                    SELECT COUNT(*)
                    FROM Incident i
                    INNER JOIN Equipment e ON i.equipmentId = e.equipmentId
                    WHERE (@lineId IS NULL OR e.lineId = @lineId)
                      AND (@shiftId IS NULL OR i.shiftId = @shiftId)
                      AND (@equipmentId IS NULL OR i.equipmentId = @equipmentId)
                      AND ((@requireMissingSop = 1 AND i.sopId IS NULL) OR (@requireMissingSop = 0 AND (@sopId IS NULL OR i.sopId = @sopId)))
                      AND (@startDate IS NULL OR i.occurredAt >= @startDate)
                      AND (@endDate IS NULL OR i.occurredAt < @endDate);";

                using SqliteCommand command = new(sql, connection);
                AddFilterParameters(command, filterSet);

                object? result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Adds SQL parameters for an incident insert command.
        /// </summary>
        /// <param name="command">The command to populate.</param>
        /// <param name="incident">The incident whose values are used.</param>
        private static void AddIncidentParameters(SqliteCommand command, Incident incident)
        {
            command.Parameters.AddWithValue("@occurredAt", incident.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@equipmentId", incident.EquipmentId);
            command.Parameters.AddWithValue("@shiftId", incident.ShiftId);
            command.Parameters.AddWithValue("@sopId", incident.SopId.HasValue ? incident.SopId.Value : DBNull.Value);
        }

        /// <summary>
        /// Adds SQL parameters for incident filtering.
        /// </summary>
        /// <param name="command">The command to populate.</param>
        /// <param name="filterSet">The filter values to apply.</param>
        private static void AddFilterParameters(SqliteCommand command, FilterSet filterSet)
        {
            command.Parameters.AddWithValue("@lineId", filterSet.LineId.HasValue ? filterSet.LineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@shiftId", filterSet.ShiftId.HasValue ? filterSet.ShiftId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@equipmentId", filterSet.EquipmentId.HasValue ? filterSet.EquipmentId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@sopId", filterSet.SopId.HasValue ? filterSet.SopId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@requireMissingSop", filterSet.RequireMissingSop ? 1 : 0);
            command.Parameters.AddWithValue("@startDate", filterSet.StartDate.HasValue
                ? filterSet.StartDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss")
                : DBNull.Value);
            command.Parameters.AddWithValue("@endDate", filterSet.EndDate.HasValue
                ? filterSet.EndDate.Value.Date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss")
                : DBNull.Value);
        }

        /// <summary>
        /// Maps the current row in a data reader to an <see cref="Incident"/> instance.
        /// </summary>
        /// <param name="reader">The data reader positioned on a valid record.</param>
        /// <returns>An incident populated from the current row.</returns>
        private static Incident MapIncident(SqliteDataReader reader)
        {
            return new Incident
            {
                IncidentId = reader.GetInt32(0),
                OccurredAt = DateTime.Parse(reader.GetString(1)),
                EquipmentId = reader.GetInt32(2),
                ShiftId = reader.GetInt32(3),
                SopId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
            };
        }

    }
}
