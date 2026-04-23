/*
 * File: ReferenceDataRepository.cs
 * Author: Sarah Portillo
 * Date: 04/18/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Data Persistence Layer
 * 
 * Purpose:
 * This class is responsible for accessing the reference data corresponding to the domain classes of the
 * program: shifts, lines, equipment, and SOPs. It provides methods to retrieve all reference data for
 * populating dropdowns and selection lists in the user interface, as well as methods
 * to retrieve equipment by line and SOPs by equipment for dependent dropdown functionality. Additionally,
 * it includes a method to seed the database with default reference data if it has not already been
 * initialized, ensuring that the application has the necessary data to function properly on startup.
*/

using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System.Data
{
    /// <summary>
    /// Provides methods for retrieving and seeding reference data entities such as shifts, lines, equipment, and
    /// standard operating procedures (SOPs) from the database. Supports UI population and dependent selection scenarios.
    /// </summary>
    public class ReferenceDataRepository
    {
        private readonly DatabaseManager _databaseManager;

        /// <summary>
        /// Default constructor that initializes the ReferenceDataRepository with a new instance of DatabaseManager.
        /// </summary>
        public ReferenceDataRepository()
        {
            _databaseManager = new DatabaseManager();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public ReferenceDataRepository(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// Retrieves all reference data entities, including shifts, lines, equipment, and SOPs, from the database.
        /// Used on startup to populate dropdowns and selection lists in the UI.
        /// </summary>
        /// <returns>A populated ReferenceDataSet containing collections of shifts, lines, equipment, and SOPs.</returns>
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
        /// Retrieves a list of equipment associated with the specified production line.
        /// Supports dependent dropdown functionality in the UI, allowing users to select a production line and view only the equipment assigned to that line. Each equipment item in the returned list will have its LineId property set to the specified lineId.
        /// </summary>
        /// <param name="lineId">The unique identifier of the production line for which to retrieve equipment. Must be a valid line ID.</param>
        /// <returns>A list of <see cref="Equipment"/> objects assigned to the specified production line.</returns>
        public List<Equipment> GetEquipmentByLine(int lineId)
        {
            List<Equipment> equipmentList = new();

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
        /// Retrieves a list of standard operating procedures (SOPs) associated with the specified equipment.
        /// Supports dependent dropdown functionality in the UI, allowing users to select equipment and view only the SOPs assigned to that equipment. Each SOP item in the returned list will have its EquipmentId property set to the specified equipmentId.
        /// </summary>
        /// <param name="equipmentId">The unique identifier of the equipment for which to retrieve SOPs.</param>
        /// <returns>A list of SOP objects associated with the specified equipment.</returns>
        public List<SOP> GetSopsByEquipment(int equipmentId)
        {
            List<SOP> sopList = new();

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
        /// Ensures that reference data exists in the database.
        /// If reference data is already present, the method returns true without inserting anything.
        /// If it is missing, the method inserts the default reference data within a transaction.
        /// </summary>
        /// <returns>true if reference data is available or was successfully seeded; otherwise, false.</returns>
        public bool SeedReferenceDataIfNeeded()
        {
            try
            {
                using SqliteConnection connection = _databaseManager.OpenConnection();

                // Checks whether reference data has already been seeded by looking for records in the Line table.
                // If records exist, the method returns true and skips inserting default reference data.
                string checkSql = "SELECT COUNT(*) FROM Line;";
                using (SqliteCommand checkCommand = new(checkSql, connection))
                {
                    long count = (long)checkCommand.ExecuteScalar();
                    if (count > 0)
                    {
                        return true;
                    }
                }

                // Shifts
                // The shift system is conventional and follows a standard 3-shift model. This is a common industry practice and provides a realistic framework for testing shift-based training gap analysis.
                string insertShifts = @"
                INSERT INTO Shift (name) VALUES
                ('1st Shift'),
                ('2nd Shift'),
                ('3rd Shift');";

                // Lines
                // The lines are a sample of common packaging line configurations. The "NS" prefix denotes "non-serialized" lines, which handle products that do not require serialization and thus have different equipment and associated SOPs.
                string insertLines = @"
                INSERT INTO Line (name) VALUES
                ('Bottle Line'),
                ('NS Bottle Line'),
                ('Tube Line')";

                // Equipment
                // The equipment list includes a variety of common packaging machines. Each equipment name is prefixed with its associated line for clarity and to ensure that the relationships between lines and equipment are well-defined for testing purposes.
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

                // SOP
                // The SOP list includes a sample of standard operating procedures for each piece of equipment. Each SOP name is associated with its respective equipment to ensure that the relationships between equipment and SOPs are well-defined for testing purposes.
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
