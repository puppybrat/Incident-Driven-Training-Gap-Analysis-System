using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.UI;
using WinFormsApplication = System.Windows.Forms.Application;

namespace Incident_Driven_Training_Gap_Analysis_System
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var databaseManager = new DatabaseManager();
            databaseManager.InitializeDatabase();

            var referenceDataRepository = new ReferenceDataRepository();
            referenceDataRepository.SeedReferenceDataIfNeeded();

            WinFormsApplication.Run(new MainForm());
        }
    }
}