/*
 * File: FilterSet.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents optional filters used when retrieving incident data.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents optional filters used to narrow incident query results.
    /// </summary>
    public class FilterSet
    {
        /// <summary>
        /// Gets or sets the line identifier filter.
        /// </summary>
        public int? LineId { get; set; }

        /// <summary>
        /// Gets or sets the shift identifier filter.
        /// </summary>
        public int? ShiftId { get; set; }

        /// <summary>
        /// Gets or sets the equipment identifier filter.
        /// </summary>
        public int? EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the SOP identifier filter.
        /// </summary>
        public int? SopId { get; set; }

        /// <summary>
        /// Gets or sets whether only incidents without an SOP are included.
        /// </summary>
        public bool RequireMissingSop { get; set; }

        /// <summary>
        /// Gets or sets the start date filter.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date filter.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}