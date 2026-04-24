/*
 * File: ReferenceDataRepository.cs
 * Author: Sarah Portillo
 * Date: 04/18/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class retrieves and seeds reference data used by the application,
 * including shifts, lines, equipment, and SOPs.
 */

using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Provides database access methods for reference data.
    /// </summary>
    public class ReferenceDataRepository
    {
        private readonly DatabaseManager _databaseManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataRepository"/> class.
        /// </summary>
        public ReferenceDataRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataRepository"/> class with a database manager.
        /// </summary>
        /// <param name="databaseManager">The database manager to use.</param>
        public ReferenceDataRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Retrieves shifts, lines, equipment, and SOPs for UI selections and validation.
        /// </summary>
        /// <returns>A populated reference data set.</returns>
        public ReferenceDataSet GetAllReferenceData()
        {
            ReferenceDataSet referenceDataSet = new();

            using SqliteConnection connection = _databaseManager.OpenConnection();

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
        /// Retrieves equipment assigned to a specific production line.
        /// </summary>
        /// <param name="lineId">The production line identifier.</param>
        /// <returns>A list of matching equipment.</returns>
        public List<Equipment> GetEquipmentByLine(int lineId)
        {
            List<Equipment> equipmentList = [];

            using SqliteConnection connection = _databaseManager.OpenConnection();

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
        /// Retrieves SOPs assigned to a specific equipment item.
        /// </summary>
        /// <param name="equipmentId">The equipment identifier.</param>
        /// <returns>A list of matching SOPs.</returns>
        public List<SOP> GetSopsByEquipment(int equipmentId)
        {
            List<SOP> sopList = [];

            using SqliteConnection connection = _databaseManager.OpenConnection();

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
        /// Ensures required reference data exists, inserting default records when the database is empty.
        /// </summary>
        /// <returns>true if reference data exists or is inserted successfully; otherwise, false.</returns>
        public bool SeedReferenceDataIfNeeded()
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                // The Line table is used as the marker for seeded reference data.
                string checkSql = "SELECT COUNT(*) FROM Line;";
                using (SqliteCommand checkCommand = new(checkSql, connection))
                {
                    object? result = checkCommand.ExecuteScalar();
                    long count = result is long value ? value : 0;

                    if (count > 0)
                    {
                        return true;
                    }
                }

                string insertShifts = @"
                INSERT INTO Shift (name) VALUES
                ('1st Shift'),
                ('2nd Shift'),
                ('3rd Shift');";

                string insertLines = @"
                INSERT INTO Line (name) VALUES
                ('Bottle Line'),
                ('NS Bottle Line'),
                ('Tube Line')";

                string insertEquipment = @"
                INSERT INTO Equipment (name, lineId) VALUES
                ('Bottle Labeler', 1),
                ('NS Bottle Labeler', 2),
                ('Bottle Cartoner', 1),
                ('NS Bottle Cartoner', 2),
                ('Tube Cartoner', 3),
                ('Bottle Carton Printer', 1),
                ('Tube Carton Printer', 3),
                ('Bottle Bundler', 1),
                ('NS Bottle Bundler', 2),
                ('Tube Bundler', 3),
                ('Bottle Case Packer', 1),
                ('NS Bottle Case Packer', 2),
                ('Tube Pick and Place Robot', 3),
                ('Tube Pack Off', 3);";

                string insertSop = @"
                INSERT INTO SOP (name, equipmentId) VALUES
                ('Bottle Labeler Operation', 1),
                ('Bottle Labeler Changeover', 1),
                ('NS Bottle Labeler Operation', 2),
                ('NS Bottle Labeler Changeover', 2),
                ('Bottle Cartoner Operation', 3),
                ('Bottle Cartoner Changeover', 3),
                ('NS Bottle Cartoner Operation', 4),
                ('NS Bottle Cartoner Changeover', 4),
                ('Tube Cartoner Operation', 5),
                ('Tube Cartoner Changeover', 5),
                ('Bottle Carton Printer Operation', 6),
                ('Tube Carton Printer Operation', 7),
                ('Bottle Bundler Operation', 8),
                ('Bottle Bundler Changeover', 8),
                ('NS Bottle Bundler Operation', 9),
                ('NS Bottle Bundler Changeover', 9),
                ('Tube Bundler Operation', 10),
                ('Tube Bundler Changeover', 10),
                ('Bottle Case Packer Operation', 11),
                ('Bottle Case Packer Changeover', 11),
                ('NS Bottle Case Packer Operation', 12),
                ('NS Bottle Case Packer Changeover', 12),
                ('Tube Pick and Place Robot Operation', 13),
                ('Tube Pick and Place Robot Changeover', 13),
                ('Tube Pack Off Operation', 14);";

                return _databaseManager.ExecuteTransaction((transactionConnection, transaction) =>
                {
                    using (SqliteCommand cmd = new(insertLines, transactionConnection, transaction))
                        cmd.ExecuteNonQuery();

                    using (SqliteCommand cmd = new(insertShifts, transactionConnection, transaction))
                        cmd.ExecuteNonQuery();

                    using (SqliteCommand cmd = new(insertEquipment, transactionConnection, transaction))
                        cmd.ExecuteNonQuery();

                    using (SqliteCommand cmd = new(insertSop, transactionConnection, transaction))
                        cmd.ExecuteNonQuery();
                });
            }
            catch
            {
                return false;
            }
        }
    }
}
