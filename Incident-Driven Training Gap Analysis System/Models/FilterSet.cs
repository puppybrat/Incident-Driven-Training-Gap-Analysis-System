/*
 * File: FilterSet.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Model
 * 
 * Purpose:
 * This class represents a set of optional filtering criteria used when retrieving incidents.
 * Each property corresponds to a specific filter condition that can be applied to the dataset.
 * If a property is null or not set, that filter is not applied.
 */

namespace Incident_Driven_Training_Gap_Analysis_System.Models
{
    /// <summary>
    /// Represents a collection of optional filters used to narrow down incident data.
    /// Each property corresponds to a filter condition. If a property is null, that filter is ignored.
    /// </summary>
    public class FilterSet
    {
        /// <summary>
        /// Gets or sets the line identifier to filter incidents by.
        /// If null, incidents from all lines are included.
        /// </summary>
        public int? LineId { get; set; }

        /// <summary>
        /// Gets or sets the shift identifier to filter incidents by.
        /// If null, incidents from all shifts are included.
        /// </summary>
        public int? ShiftId { get; set; }

        /// <summary>
        /// Gets or sets the equipment identifier to filter incidents by.
        /// If null, incidents from all equipment are included.
        /// </summary>
        public int? EquipmentId { get; set; }

        /// <summary>
        /// Gets or sets the SOP identifier to filter incidents by.
        /// If null, incidents with any SOP reference are included.
        /// </summary>
        public int? SopId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only incidents without an SOP reference should be included.
        /// When true, only incidents where SopId is null are returned.
        /// </summary>
        public bool RequireMissingSop { get; set; }

        /// <summary>
        /// Gets or sets the start date for filtering incidents.
        /// If null, no lower bound is applied to the incident date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for filtering incidents.
        /// If null, no upper bound is applied to the incident date.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}