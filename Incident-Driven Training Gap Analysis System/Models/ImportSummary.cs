/*
 * File: ImportSummary.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents the outcome of an import operation.
 * It stores the number of inserted and rejected rows, along with
 * any messages describing validation issues or the final import result.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents the outcome of a CSV import operation, including record counts and detail messages.
    /// </summary>
    public class ImportSummary
    {
        /// <summary>
        /// Gets or sets the number of rows successfully inserted.
        /// </summary>
        public int InsertedCount { get; set; }

        /// <summary>
        /// Gets or sets the number of rows rejected during import.
        /// </summary>
        public int RejectedCount { get; set; }

        /// <summary>
        /// Gets or sets the messages describing validation issues and overall import results.
        /// </summary>
        public List<string> Messages { get; set; } = new();

        /// <summary>
        /// Gets a value indicating whether at least one row was imported.
        /// </summary>
        public bool HasInsertedRecords => InsertedCount > 0;

        /// <summary>
        /// Gets a value indicating whether all imported rows succeeded without rejection.
        /// </summary>
        public bool IsCompleteSuccess => InsertedCount > 0 && RejectedCount == 0;

        /// <summary>
        /// Gets a value indicating whether some rows were imported and some were rejected.
        /// </summary>
        public bool IsPartialSuccess => InsertedCount > 0 && RejectedCount > 0;

        /// <summary>
        /// Gets a value indicating whether no rows were imported.
        /// </summary>
        public bool Failed => InsertedCount == 0;
    }
}