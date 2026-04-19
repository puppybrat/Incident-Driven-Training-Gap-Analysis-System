using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    public class IncidentRepository
    {
        private readonly DatabaseManager _databaseManager;

        public IncidentRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        public IncidentRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Inserts a new incident record into the data store.
        /// Accepts a single incident object and attempts to persist it. The method returns a boolean value indicating the success of the operation.
        /// </summary>
        /// <param name="incident">The incident to insert. Cannot be null.</param>
        /// <returns>true if the incident was successfully inserted; otherwise, false.</returns>
        public bool InsertIncident(Incident incident)
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

                string sql = @"
                INSERT INTO Incident (occurredAt, equipmentId, shiftId, sopId)
                VALUES (@occurredAt, @equipmentId, @shiftId, @sopId);";

                using SqliteCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@occurredAt", incident.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@equipmentId", incident.EquipmentId);
                command.Parameters.AddWithValue("@shiftId", incident.ShiftId);

                if (incident.SopId.HasValue)
                {
                    command.Parameters.AddWithValue("@sopId", incident.SopId.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@sopId", DBNull.Value);
                }

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts a collection of incidents into the data store.
        /// Accepts a list of incident objects and attempts to persist them. The method returns a boolean value indicating the success of the operation.
        /// </summary>
        /// <param name="incidentCollection">The collection of incidents to insert. Cannot be null or contain null elements.</param>
        /// <returns>true if all incidents are successfully inserted; otherwise, false.</returns>
        public bool InsertIncidents(List<Incident> incidentCollection)
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);
                using SqliteTransaction transaction = connection.BeginTransaction();

                string sql = @"
                INSERT INTO Incident (occurredAt, equipmentId, shiftId, sopId)
                VALUES (@occurredAt, @equipmentId, @shiftId, @sopId);";

                foreach (Incident incident in incidentCollection)
                {
                    using SqliteCommand command = new(sql, connection, transaction);
                    command.Parameters.AddWithValue("@occurredAt", incident.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@equipmentId", incident.EquipmentId);
                    command.Parameters.AddWithValue("@shiftId", incident.ShiftId);

                    if (incident.SopId.HasValue)
                    {
                        command.Parameters.AddWithValue("@sopId", incident.SopId.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@sopId", DBNull.Value);
                    }

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the incident with the specified unique identifier.
        /// Accepts an incident ID and attempts to find the corresponding incident in the data store. The method returns the incident if found, or null if no matching incident exists.
        /// </summary>
        /// <param name="incidentId">The unique identifier of the incident to retrieve. Must be a positive integer.</param>
        /// <returns>The incident that matches the specified identifier, or null if no such incident exists.</returns>
        public Incident? GetIncidentById(int incidentId)
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

                string sql = @"
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

                return new Incident
                {
                    IncidentId = reader.GetInt32(0),
                    OccurredAt = DateTime.Parse(reader.GetString(1)),
                    EquipmentId = reader.GetInt32(2),
                    ShiftId = reader.GetInt32(3),
                    SopId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves a list of incidents that match the specified filter criteria.
        /// Accepts a set of filters and attempts to find all incidents that satisfy the criteria. The method returns a list of matching incidents, or an empty list if no incidents match.
        /// </summary>
        /// <param name="filterSet">The set of filters to apply when retrieving incidents. Cannot be null.</param>
        /// <returns>A list of incidents that satisfy the filter criteria. The list will be empty if no incidents match.</returns>
        public List<Incident> GetIncidents(FilterSet filterSet)
        {
            List<Incident> incidents = new();

            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

                string sql = @"
                SELECT i.incidentId, i.occurredAt, i.equipmentId, i.shiftId, i.sopId
                FROM Incident i
                INNER JOIN Equipment e ON i.equipmentId = e.equipmentId
                WHERE (@lineId IS NULL OR e.lineId = @lineId)
                  AND (@shiftId IS NULL OR i.shiftId = @shiftId)
                  AND (@equipmentId IS NULL OR i.equipmentId = @equipmentId)
                  AND (@sopId IS NULL OR i.sopId = @sopId)
                  AND (@startDate IS NULL OR i.occurredAt >= @startDate)
                  AND (@endDate IS NULL OR i.occurredAt < @endDate)
                ORDER BY i.occurredAt;";

                using SqliteCommand command = new(sql, connection);

                command.Parameters.AddWithValue("@lineId", filterSet.LineId.HasValue ? filterSet.LineId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@shiftId", filterSet.ShiftId.HasValue ? filterSet.ShiftId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@equipmentId", filterSet.EquipmentId.HasValue ? filterSet.EquipmentId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@sopId", filterSet.SopId.HasValue ? filterSet.SopId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@startDate", filterSet.StartDate.HasValue
                    ? filterSet.StartDate.Value.Date.ToString("yyyy-MM-dd HH:mm:ss")
                    : DBNull.Value);
                command.Parameters.AddWithValue("@endDate", filterSet.EndDate.HasValue
                    ? filterSet.EndDate.Value.Date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss")
                    : DBNull.Value);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    incidents.Add(new Incident
                    {
                        IncidentId = reader.GetInt32(0),
                        OccurredAt = DateTime.Parse(reader.GetString(1)),
                        EquipmentId = reader.GetInt32(2),
                        ShiftId = reader.GetInt32(3),
                        SopId = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                    });
                }
            }
            catch
            {
                return new List<Incident>();
            }

            return incidents;
        }

        /// <summary>
        /// Retrieves a list of aggregated incident results based on the specified filters and grouping criteria.
        /// Accepts a set of filters and a grouping type, and attempts to find all incidents that satisfy the criteria and group them accordingly.
        /// The method returns a list of aggregated results, or an empty list if no incidents match.
        /// </summary>
        /// <param name="filterSet">The set of filters to apply when selecting incidents to aggregate. Cannot be null.</param>
        /// <param name="groupingType">The type of grouping to apply to the aggregated results. For example, group by severity, status, or date.
        /// Cannot be null or empty.</param>
        /// <returns>A list of aggregated incident results matching the specified filters and grouping. The list will be empty if
        /// no incidents match the criteria.</returns>
        public List<AggregateResult> GetAggregatedIncidents(FilterSet filterSet, string groupingType)
        {
            List<AggregateResult> results = new();

            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

                if (groupingType != "Equipment")
                {
                    return results;
                }

                string sql = @"
                SELECT CAST(i.equipmentId AS TEXT) AS GroupLabel, COUNT(*) AS IncidentCount
                FROM Incident i
                GROUP BY i.equipmentId
                ORDER BY i.equipmentId;";

                using SqliteCommand command = new(sql, connection);
                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new AggregateResult
                    {
                        GroupLabel = reader.GetString(0),
                        IncidentCount = reader.GetInt32(1),
                        IsFlagged = false
                    });
                }
            }
            catch
            {
                return new List<AggregateResult>();
            }

            return results;
        }

        /// <summary>
        /// Returns the number of incidents that match the specified filter criteria.
        /// Does not retrieve the actual incident records, but instead returns a count of how many incidents satisfy the provided filters. This can be useful for pagination or when only the total number of matching incidents is needed.
        /// </summary>
        /// <param name="filterset">The set of filters to apply when counting incidents. Cannot be null.</param>
        /// <returns>The number of incidents that satisfy the provided filter criteria.</returns>
        public int CountIncidents(FilterSet filterSet)
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

                string sql = "SELECT COUNT(*) FROM Incident;";
                using SqliteCommand command = new(sql, connection);

                object? result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch
            {
                return 0;
            }
        }
    }
}
