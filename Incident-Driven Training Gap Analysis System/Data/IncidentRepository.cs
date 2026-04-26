/*
 * File: IncidentRepository.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class manages persistence for incident records. It inserts,
 * retrieves and maps incident data using the database manager.
 */

using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Provides database access methods for incident records.
    /// </summary>
    public class IncidentRepository
    {
        private readonly DatabaseManager _databaseManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentRepository"/> class.
        /// </summary>
        public IncidentRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentRepository"/> class with a database manager.
        /// </summary>
        /// <param name="databaseManager">The database manager to use.</param>
        public IncidentRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Inserts a single incident record.
        /// </summary>
        /// <param name="incident">The incident to insert.</param>
        /// <returns>true if one incident record is inserted; otherwise, false.</returns>
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
        /// Inserts multiple incident records within one transaction.
        /// </summary>
        /// <param name="incidentCollection">The incident records to insert.</param>
        /// <returns>true if all incident records are inserted; otherwise, false.</returns>
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
        /// Retrieves incidents that match the specified filters.
        /// </summary>
        /// <param name="filterSet">The filters to apply.</param>
        /// <returns>A list of matching incidents, or an empty list if no records are found.</returns>
        public List<Incident> GetIncidents(FilterSet filterSet)
        {
            ArgumentNullException.ThrowIfNull(filterSet);

            List<Incident> incidents = [];

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
                return [];
            }

            return incidents;
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
        /// Adds SQL parameters used by incident filter queries, including the missing SOP filter flag.
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
