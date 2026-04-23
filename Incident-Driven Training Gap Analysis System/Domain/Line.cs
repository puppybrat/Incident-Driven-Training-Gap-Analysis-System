/*
 * File: Line.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Domain
 * 
 * Purpose:
 * This class represents a production line within the system.
 * It defines the properties associated with a line, including its identifier and name.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Domain
{
    /// <summary>
    /// Represents a production line used to group equipment and incidents.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Gets or sets the unique identifier for the production line.
        /// </summary>
        public int LineId { get; set; }

        /// <summary>
        /// Gets or sets the name of the production line.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}