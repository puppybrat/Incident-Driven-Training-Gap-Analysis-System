using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.UI;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

using WinFormsApplication = System.Windows.Forms.Application;

namespace Incident_Driven_Training_Gap_Analysis_System
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // "dotnet run -- seed 100" to implement
            // If "seed" argument is passed, run seeder and exit
            if (args.Length > 0 && args[0].Equals("seed", StringComparison.OrdinalIgnoreCase))
            {
                int count = 100;

                if (args.Length > 1 && int.TryParse(args[1], out int parsed))
                {
                    count = parsed;
                }

                RunSeeder(count);
                return;
            }
            ApplicationConfiguration.Initialize();

            var databaseManager = new DatabaseManager();
            databaseManager.InitializeDatabase();

            var referenceDataRepository = new ReferenceDataRepository();
            referenceDataRepository.SeedReferenceDataIfNeeded();

            WinFormsApplication.Run(new MainForm());
        }

        private static void RunSeeder(int count)
        {
            var databaseManager = new DatabaseManager("training_gap_analysis.db");
            var referenceRepo = new ReferenceDataRepository(databaseManager);
            var incidentRepo = new IncidentRepository(databaseManager);

            databaseManager.InitializeDatabase();
            referenceRepo.SeedReferenceDataIfNeeded();

            var referenceData = referenceRepo.GetAllReferenceData();

            Random random = new();

            int inserted = 0;

            for (int i = 0; i < count; i++)
            {
                var incident = BuildRandomIncident(referenceData, random);

                if (incidentRepo.InsertIncident(incident))
                {
                    inserted++;
                }
            }

            Console.WriteLine($"Inserted {inserted} / {count} records.");
        }

        private static Incident BuildRandomIncident(ReferenceDataSet data, Random random)
        {
            var shift = data.Shifts[random.Next(data.Shifts.Count)];
            var equipment = data.Equipment[random.Next(data.Equipment.Count)];

            var sops = data.Sops
                .Where(s => s.EquipmentId == equipment.EquipmentId)
                .ToList();

            int? sopId = null;

            if (sops.Count > 0 && random.NextDouble() < 0.7)
            {
                sopId = sops[random.Next(sops.Count)].SopId;
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