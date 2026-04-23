/*
 * File: Shift.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents a work shift within the system.
 * It defines the properties associated with a shift, including its identifier and name.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents a work shift used to categorize incidents by time period.
    /// </summary>
    public class Shift
    {
        /// <summary>
        /// Gets or sets the unique identifier for the shift.
        /// </summary>
        public int ShiftId { get; set; }

        /// <summary>
        /// Gets or sets the name of the shift.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}