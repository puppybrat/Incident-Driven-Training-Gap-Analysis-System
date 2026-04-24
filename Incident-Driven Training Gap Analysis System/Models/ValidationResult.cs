/*
 * File: ValidationResult.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents the result of a validation operation.
 * It indicates whether the validation succeeded and provides a collection
 * of error messages when validation fails.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents the result of a validation operation, including success status and error messages.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the collection of error messages generated during validation.
        /// This list is empty when validation succeeds.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}