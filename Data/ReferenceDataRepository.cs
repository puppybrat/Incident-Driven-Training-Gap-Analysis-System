using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    public class ReferenceDataRepository
    {
        private readonly DatabaseManager _databaseManager;

        public ReferenceDataRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        public ReferenceDataRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Retrieves all reference data entities, including lines, shifts, equipment, and SOPs, from the database.
        /// Used on startup to populate dropdowns and selection lists in the UI. Each collection in the returned ReferenceDataSet will be empty if no corresponding records exist in the database.
        /// </summary>
        /// <returns>A populated ReferenceDataSet containing collections of lines, shifts, equipment, and SOPs. Each collection
        /// will be empty if no corresponding records exist in the database.</returns>
        public ReferenceDataSet GetAllReferenceData()
        {
            ReferenceDataSet referenceDataSet = new();

            using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

            string lineSql = "SELECT lineId, name FROM Line ORDER BY name";
            using (SqliteCommand command = new(lineSql, connection))
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    referenceDataSet.Lines.Add(new Line
                    {
                        LineId = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }

            string shiftSql = "SELECT shiftId, name FROM Shift ORDER BY name";
            using (SqliteCommand command = new(shiftSql, connection))
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    referenceDataSet.Shifts.Add(new Shift
                    {
                        ShiftId = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }

            string equipmentSql = "SELECT equipmentId, name, lineId FROM Equipment ORDER BY name";
            using (SqliteCommand command = new(equipmentSql, connection))
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    referenceDataSet.Equipment.Add(new Equipment
                    {
                        EquipmentId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        LineId = reader.GetInt32(2)
                    });
                }
            }

            string sopSql = "SELECT sopId, name, equipmentId FROM SOP ORDER BY name";
            using (SqliteCommand command = new(sopSql, connection))
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    referenceDataSet.Sops.Add(new SOP
                    {
                        SopId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        EquipmentId = reader.GetInt32(2)
                    });
                }
            }

            return referenceDataSet;
        }

        /// <summary>
        /// Retrieves a list of equipment associated with the specified production line.
        /// Supports dependent dropdown functionality in the UI, allowing users to select a production line and view only the equipment assigned to that line. Each equipment item in the returned list will have its LineId property set to the specified lineId. The list will be empty if no equipment is associated with the line or if the lineId does not exist in the database.
        /// </summary>
        /// <param name="lineId">The unique identifier of the production line for which to retrieve equipment. Must be a valid line ID.</param>
        /// <returns>A list of <see cref="Equipment"/> objects assigned to the specified production line. The list is empty if no
        /// equipment is associated with the line.</returns>
        public List<Equipment> GetEquipmentByLine(int lineId)
        {
            List<Equipment> equipmentList = new();

            using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

            string sql = "SELECT equipmentId, name, lineId FROM Equipment WHERE lineId = @lineId ORDER BY name";

            using SqliteCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@lineId", lineId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipmentList.Add(new Equipment
                {
                    EquipmentId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    LineId = reader.GetInt32(2)
                });
            }

            return equipmentList;
        }

        /// <summary>
        /// Retrieves a list of standard operating procedures (SOPs) associated with the specified equipment.
        /// Supports dependent dropdown functionality in the UI, allowing users to select equipment and view only the SOPs assigned to that equipment. Each SOP item in the returned list will have its EquipmentId property set to the specified equipmentId. The list will be empty if no SOPs are associated with the equipment or if the equipmentId does not exist in the database.
        /// </summary>
        /// <param name="equipmentId">The unique identifier of the equipment for which to retrieve SOPs.</param>
        /// <returns>A list of SOP objects associated with the specified equipment. The list is empty if no SOPs are found.</returns>
        public List<SOP> GetSopsByEquipment(int equipmentId)
        {
            List<SOP> sopList = new();

            using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

            string sql = "SELECT sopId, name, equipmentId FROM SOP WHERE equipmentId = @equipmentId ORDER BY name";

            using SqliteCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@equipmentId", equipmentId);

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                sopList.Add(new SOP
                {
                    SopId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    EquipmentId = reader.GetInt32(2)
                });
            }

            return sopList;
        }

        /// <summary>
        /// Ensures that reference data is seeded if it has not already been initialized.
        /// Used on startup to populate the database with default reference data if it is empty. This method checks for the presence of reference data and inserts default records if necessary. Returns true if reference data was seeded during this call, or false if the database already contained reference data and no seeding was performed.
        /// </summary>
        /// <returns>true if reference data was seeded during this call; otherwise, false.</returns>
        public bool SeedReferenceDataIfNeeded()
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection(_databaseManager.ConnectionString);

                // Check if data already exists
                string checkSql = "SELECT COUNT(*) FROM Line;";
                using (SqliteCommand checkCommand = new(checkSql, connection))
                {
                    long count = (long)checkCommand.ExecuteScalar();
                    if (count > 0)
                    {
                        return true; // already seeded
                    }
                }

                using SqliteTransaction transaction = connection.BeginTransaction();

                // Lines
                string insertLines = @"
                INSERT INTO Line (name) VALUES
                ('Line 1'),
                ('Line 2');";

                // Shifts
                string insertShifts = @"
                INSERT INTO Shift (name) VALUES
                ('Shift A'),
                ('Shift B'),
                ('Shift C');";

                // Equipment
                string insertEquipment = @"
                INSERT INTO Equipment (name, lineId) VALUES
                ('Machine 1', 1),
                ('Machine 2', 2);";

                // SOP
                string insertSop = @"
                INSERT INTO SOP (name, equipmentId) VALUES
                ('SOP 1', 1),
                ('SOP 2', 2);";

                using (SqliteCommand cmd = new(insertLines, connection, transaction))
                    cmd.ExecuteNonQuery();

                using (SqliteCommand cmd = new(insertShifts, connection, transaction))
                    cmd.ExecuteNonQuery();

                using (SqliteCommand cmd = new(insertEquipment, connection, transaction))
                    cmd.ExecuteNonQuery();

                using (SqliteCommand cmd = new(insertSop, connection, transaction))
                    cmd.ExecuteNonQuery();

                transaction.Commit();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
