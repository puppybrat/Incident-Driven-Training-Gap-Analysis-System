/*
 * File: Program.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Startup
 * 
 * Purpose:
 * This file serves as the application entry point. It initializes WinForms,
 * prepares database/reference data, launches the main form, and supports
 * optional command-line sample data seeding.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;
using Incident_Driven_Training_Gap_Analysis_System.UI;

using WinFormsApplication = System.Windows.Forms.Application;

namespace Incident_Driven_Training_Gap_Analysis_System
{
    /// <summary>
    /// Provides the application entry point and startup workflow.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Starts the application or runs sample data seeding when requested.
        /// </summary>
        /// <param name="args">Command-line arguments. Use "seed" as the first argument to generate sample incidents.</param>
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
        /// Runs sample data seeding when requested by command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments supplied to the application.</param>
        /// <returns><see langword="true"/> if seeding was run; otherwise, <see langword="false"/>.</returns>
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
        /// Gets the requested sample incident count from command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments supplied to the application.</param>
        /// <returns>The requested count, or 100 when no valid count is provided.</returns>
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
        /// Initializes the application database and required reference data before startup.
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
        /// Seeds the database with randomly generated incident records.
        /// </summary>
        /// <param name="count">The number of sample incidents to generate.</param>
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
        /// Builds a random incident using available shifts, equipment, and optional matching SOP data.
        /// </summary>
        /// <param name="data">The reference data used to build the incident.</param>
        /// <param name="random">The random number generator used for value selection.</param>
        /// <returns>A randomly generated incident.</returns>
        private static Incident BuildRandomIncident(ReferenceDataSet data, Random random)
        {
            Shift shift = data.Shifts[random.Next(data.Shifts.Count)];
            Equipment equipment = data.Equipment[random.Next(data.Equipment.Count)];

            List<SOP> matchingSops = [.. data.Sops.Where(sop => sop.EquipmentId == equipment.EquipmentId)];

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