/*
 * File: ExportSummary.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents the outcome of an export operation.
 * It stores whether the export succeeded and the message associated
 * with the result of the export attempt.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents the outcome of an export operation.
    /// </summary>
    public class ExportSummary
    {
        /// <summary>
        /// Gets or sets a value indicating whether the export completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message describing the export result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}