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
 * prepares database/reference data, and launches the main form.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
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
        /// Starts the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            InitializeApplication();
            EnsureApplicationData();
            RunMainForm();
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
    }
}