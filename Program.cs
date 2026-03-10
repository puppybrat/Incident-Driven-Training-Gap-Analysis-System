using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Incident_Driven_Training_Gap_Analysis_System
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            /// Commented out because Database creation has been tested, pending progress
            /// TODO:
            /// - Set location for creation (Within user files, separate from application)
            /// - Set as initial run only ("If not found, create")
            /// 
            /// CURRENT:
            /// - Delete after each run if further testing is needed
            /// - Location C:\Users\Milo\source\repos\Incident-Driven Training Gap Analysis System\bin\Debug\net10.0-windows
            /// - View with "DB Browser for SQLite"
            /// 
            /// CreateDatabase();

            Application.Run(new Form1());
        }

        static void CreateDatabase()
        {
            using var connection = new SqliteConnection("Data Source=training_gap_analysis.db");

            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE Incident (
                    incidentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    occurredAt TEXT NOT NULL,
                    equipmentId INTEGER REFERENCES Equipment(equipmentId),
                    shiftId INTEGER REFERENCES Shift(shiftId),
                    sopId INTEGER REFERENCES SOP(sopId)
                );

                CREATE TABLE Equipment (
                    equipmentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    lineId INTEGER REFERENCES Line(lineId)
                );

                CREATE TABLE Line (
                    lineId INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL
                );

                CREATE TABLE SOP (
                    sopId INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    equipmentId INTEGER REFERENCES Equipment(equipmentId)
                );

                CREATE TABLE Shift (
                    shiftId INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL
                );

                INSERT INTO Line (name)
                VALUES ('Line 1'),
                       ('Line 2');
            
                INSERT INTO Equipment (name, lineId)
                VALUES ('Bottle-Cartoner', 1),
                       ('Tube-Cartoner', 2);

                INSERT INTO SOP (name, equipmentId)
                VALUES ('Bottle-Cartoner SOP', 1),
                       ('Tube-Cartoner SOP', 2);

                INSERT INTO Shift (name)
                VALUES ('Morning Shift'),
                       ('Evening Shift'),
                       ('Night Shift');

                INSERT INTO Incident (occurredAt, equipmentId, shiftId, sopId)
                VALUES ('2024-01-01 08:00:00', 1, 1, 1);
            """;

            command.ExecuteNonQuery();

            MessageBox.Show("Database created and sample data inserted.");
        }
    }
}