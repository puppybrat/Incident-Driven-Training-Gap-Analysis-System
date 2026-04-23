/*
 * File: Program.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Startup
 * 
 * Purpose:
 * This file serves as the application's entry point. It initializes the WinForms
 * environment, ensures the database and reference data are ready for use, launches
 * the main application window, and optionally supports command-line seeding for
 * generating sample incident records.
*/

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Incident_Driven_Training_Gap_Analysis_System.UI;

using WinFormsApplication = System.Windows.Forms.Application;

namespace Incident_Driven_Training_Gap_Analysis_System
{
    /// <summary>
    /// Provides the application entry point and startup orchestration for the
    /// Incident-Driven Training Gap Analysis System.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Initializes the application, optionally runs the sample data seeder when requested
        /// through command-line arguments, and otherwise launches the main application window.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.
        /// When the first argument is seed, the application generates sample incident
        /// records and exits.</param>
        [STAThread]
        private static void Main(string[] args)
        {
            if (TryRunSeeder(args))
            {
                return;
            }

            InitializeApplication();
            EnsureApplicationData();
            RunMainForm();
        }

        /// <summary>
        /// Determines whether the application should run in sample-data seeding mode,
        /// and if so, executes the seeder and prevents the main UI from launching.
        /// </summary>
        /// <param name="args">The command-line arguments supplied to the application.</param>
        /// <returns><see langword="true"/> if seeding mode was requested and executed; otherwise,
        /// <see langword="false"/>.</returns>
        private static bool TryRunSeeder(string[] args)
        {
            if (args.Length == 0 || !args[0].Equals("seed", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int count = GetSeedCount(args);
            RunSeeder(count);
            return true;
        }

        /// <summary>
        /// Determines the number of sample incidents to generate from the supplied
        /// command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments supplied to the application.</param>
        /// <returns>The requested number of records to seed, or the default value of 100 when
        /// no valid count is provided.</returns>
        private static int GetSeedCount(string[] args)
        {
            const int DefaultSeedCount = 100;

            if (args.Length > 1 && int.TryParse(args[1], out int parsedCount))
            {
                return parsedCount;
            }

            return DefaultSeedCount;
        }

        /// <summary>
        /// Initializes the Windows Forms application configuration.
        /// </summary>
        private static void InitializeApplication()
        {
            ApplicationConfiguration.Initialize();
        }

        /// <summary>
        /// Ensures that the application's database and required reference data
        /// are initialized before the main window is displayed.
        /// </summary>
        private static void EnsureApplicationData()
        {
            DatabaseManager databaseManager = new();
            databaseManager.InitializeDatabase();

            ReferenceDataRepository referenceDataRepository = new();
            referenceDataRepository.SeedReferenceDataIfNeeded();
        }

        /// <summary>
        /// Starts the main Windows Forms application shell.
        /// </summary>
        private static void RunMainForm()
        {
            WinFormsApplication.Run(new MainForm());
        }

        /// <summary>
        /// Seeds the database with a specified number of randomly generated incident records.
        /// </summary>
        /// <param name="count">The number of sample incident records to generate.</param>
        private static void RunSeeder(int count)
        {
            DatabaseManager databaseManager = new("training_gap_analysis.db");
            ReferenceDataRepository referenceRepository = new(databaseManager);
            IncidentRepository incidentRepository = new(databaseManager);

            databaseManager.InitializeDatabase();
            referenceRepository.SeedReferenceDataIfNeeded();

            ReferenceDataSet referenceData = referenceRepository.GetAllReferenceData();
            Random random = new();

            int insertedCount = 0;

            for (int i = 0; i < count; i++)
            {
                Incident incident = BuildRandomIncident(referenceData, random);

                if (incidentRepository.InsertIncident(incident))
                {
                    insertedCount++;
                }
            }

            Console.WriteLine($"Inserted {insertedCount} / {count} records.");
        }

        /// <summary>
        /// Builds a randomly generated incident using the available reference data.
        /// A shift and equipment item are always selected, and an SOP is assigned
        /// when one exists for the selected equipment and passes the configured probability check.
        /// </summary>
        /// <param name="data">The reference data used to populate the generated incident.</param>
        /// <param name="random">The random number generator used for value selection.</param>
        /// <returns>A randomly generated <see cref="Incident"/> instance.</returns>
        private static Incident BuildRandomIncident(ReferenceDataSet data, Random random)
        {
            Shift shift = data.Shifts[random.Next(data.Shifts.Count)];
            Equipment equipment = data.Equipment[random.Next(data.Equipment.Count)];

            List<SOP> matchingSops = data.Sops
                .Where(sop => sop.EquipmentId == equipment.EquipmentId)
                .ToList();

            int? sopId = null;

            if (matchingSops.Count > 0 && random.NextDouble() < 0.7)
            {
                sopId = matchingSops[random.Next(matchingSops.Count)].SopId;
            }

            return new Incident
            {
                OccurredAt = DateTime.Now.AddMinutes(-random.Next(0, 60 * 24 * 180)),
                ShiftId = shift.ShiftId,
                EquipmentId = equipment.EquipmentId,
                SopId = sopId
            };
        }
    }
}