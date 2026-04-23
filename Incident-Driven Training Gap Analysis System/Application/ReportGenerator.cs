/*
 * File: ReportGenerator.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class is responsible for managing the business logic related to report generation within
 * the Incident-Driven Training Gap Analysis System. It retrieves data from the data persistence layer
 * and returns it for use by the UI layer. It receives report requests from the UI layer, validates them,
 * and then calls supporting methods to generate the report. The ReportGenerator
 * serves as an intermediary between the UI and data layers for the life cycle of a report
 * coordinating the report generation process and ensuring that data is retrieved, 
 * grouped, and transformed according to the specified request.
*/

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Provides methods for generating incident reports based on configurable filters, grouping options, and report
    /// presets. Supports building report data from incident and reference data repositories, applying business rules,
    /// and returning structured report results.
    /// </summary>
    public class ReportGenerator
    {
        private readonly IncidentRepository _incidentRepository;
        private readonly ReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// Default constructor that initializes the ReportGenerator with a new instance of IncidentRepository and ReferenceDataRepository.
        /// </summary>
        public ReportGenerator()
        {
            _incidentRepository = new IncidentRepository();
            _referenceDataRepository = new ReferenceDataRepository();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public ReportGenerator(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
            _referenceDataRepository = new ReferenceDataRepository(databaseManager);
        }

        /// <summary>
        /// Coordinates the full report generation workflow.
        /// Applies filters, builds grouped report rows based on the selected preset and grouping options,
        /// evaluates rule thresholds, and returns a populated ReportResult.
        /// </summary> 
        /// <param name="reportRequest">The parameters that define the report to generate, including filters, output type, and included fields.
        /// Cannot be null.</param>
        /// <returns>A ReportResult object containing the generated report data and metadata as specified by the request.</returns>
        /// <exception cref="ArgumentNullException">Thrown if reportRequest is null.</exception>
        public ReportResult GenerateReport(ReportRequest reportRequest)
        {
            if (reportRequest == null)
            {
                throw new ArgumentNullException(nameof(reportRequest));
            }

            List<Incident> filteredIncidents = ApplyFilters(reportRequest);
            List<ReportRow> rows = BuildRows(filteredIncidents, reportRequest);

            RuleEvaluator evaluator = new();
            RuleConfig config = evaluator.LoadCurrentRuleConfig();
            rows = evaluator.EvaluateThresholds(rows, config);

            return new ReportResult
            {
                PresetName = reportRequest.PresetName,
                OutputType = reportRequest.OutputType,
                IncludeLine = reportRequest.IncludeLine,
                IncludeShift = reportRequest.IncludeShift,
                IncludeEquipment = reportRequest.IncludeEquipment,
                IncludeSop = reportRequest.IncludeSop,
                Rows = rows
            };
        }

        /// <summary>
        /// Applies the filter set from the report request and retrieves matching incidents from the repository.
        /// </summary>
        /// <param name="reportRequest">The report request containing the filter criteria to apply. Cannot be null.</param>
        /// <returns>A list of incidents that satisfy the specified filters. The list is empty if no incidents match the criteria.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reportRequest"/> is null.</exception>
        public List<Incident> ApplyFilters(ReportRequest reportRequest)
        {
            if (reportRequest == null)
            {
                throw new ArgumentNullException(nameof(reportRequest));
            }

            FilterSet filters = reportRequest.Filters ?? new FilterSet();
            return _incidentRepository.GetIncidents(filters);
        }

        /// <summary>
        /// Selects the appropriate report-building strategy based on the selected preset.
        /// Routes to preset-specific logic or the general grouped report builder.
        /// </summary>
        /// <param name="incidents">The collection of incidents to include in the report. Cannot be null.</param>
        /// <param name="reportRequest">The report request specifying the preset and additional parameters for report generation. Cannot be null.</param>
        /// <returns>A list of report rows generated according to the selected report preset and the provided incidents. The list
        /// may be empty if no rows are generated.</returns>
        private List<ReportRow> BuildRows(List<Incident> incidents, ReportRequest reportRequest)
        {
            return reportRequest.PresetName switch
            {
                ReportPresetNames.IncidentsPerMissingSopByLine => BuildLinesByMissingSop(incidents, reportRequest),
                ReportPresetNames.IncidentsPerShiftByLine => BuildGroupedReport(incidents, reportRequest),
                ReportPresetNames.IncidentsPerEquipment => BuildGroupedReport(incidents, reportRequest),
                ReportPresetNames.IncidentsPerSopReference => BuildGroupedReport(incidents, reportRequest),
                ReportPresetNames.None => BuildGroupedReport(incidents, reportRequest),
                _ => BuildGroupedReport(incidents, reportRequest)
            };
        }

        /// <summary>
        /// Filters incidents to those without an associated SOP and builds grouped report rows.
        /// Used for the "Incidents per missing SOP by Line" preset.
        /// </summary>
        /// <param name="incidents">The collection of incidents to evaluate for missing SOP associations. Cannot be null.</param>
        /// <param name="reportRequest">The report request parameters that define how the report rows are generated. Cannot be null.</param>
        /// <returns>A list of report rows representing incidents without an associated SOP. The list will be empty if no such
        /// incidents are found.</returns>
        private List<ReportRow> BuildLinesByMissingSop(List<Incident> incidents, ReportRequest reportRequest)
        {
            List<Incident> incidentsWithoutSop = incidents
                .Where(i => !i.SopId.HasValue)
                .ToList();

            return BuildGroupedReport(incidentsWithoutSop, reportRequest);
        }

        /// <summary>
        /// Builds grouped report rows by:
        /// - Resolving reference data (line, shift, equipment, SOP)
        /// - Applying include flags from the report request
        /// - Grouping incidents by selected fields
        /// - Counting incidents per group
        /// - Sorting results for display
        /// </summary>
        /// <param name="incidents">The list of incidents to include in the report. Each incident is grouped according to the criteria specified
        /// in the report request.</param>
        /// <param name="reportRequest">The report request specifying which grouping options to apply, such as including line, shift, equipment, or
        /// SOP information.</param>
        /// <returns>A list of report rows, where each row represents a group of incidents and includes group values and the
        /// count of incidents in that group.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an incident references an equipment or line that does not exist in the reference data.</exception>
        private List<ReportRow> BuildGroupedReport(List<Incident> incidents, ReportRequest reportRequest)
        {
            ReferenceDataSet referenceData = _referenceDataRepository.GetAllReferenceData();

            Dictionary<int, string> shiftLookup = referenceData.Shifts.ToDictionary(s => s.ShiftId, s => s.Name);
            Dictionary<int, Equipment> equipmentLookup = referenceData.Equipment.ToDictionary(e => e.EquipmentId, e => e);
            Dictionary<int, string> lineLookup = referenceData.Lines.ToDictionary(l => l.LineId, l => l.Name);
            Dictionary<int, string> sopLookup = referenceData.Sops.ToDictionary(s => s.SopId, s => s.Name);

            List<ReportRow> rows = incidents
                .Select(i =>
                {
                    string shiftName = shiftLookup.TryGetValue(i.ShiftId, out string? shift) ? shift : $"Shift {i.ShiftId}";

                    if (!equipmentLookup.TryGetValue(i.EquipmentId, out Equipment? equipment))
                    {
                        throw new InvalidOperationException(
                            $"Incident {i.IncidentId} references missing EquipmentId {i.EquipmentId}.");
                    }

                    if (!lineLookup.TryGetValue(equipment.LineId, out string? lineName))
                    {
                        throw new InvalidOperationException(
                            $"Equipment {equipment.EquipmentId} references missing LineId {equipment.LineId}.");
                    }

                    string equipmentName = equipment.Name;

                    string sopName = "Missing SOP";
                    if (i.SopId.HasValue && sopLookup.TryGetValue(i.SopId.Value, out string? sop))
                    {
                        sopName = sop;
                    }

                    string groupedLine = reportRequest.IncludeLine ? lineName : string.Empty;
                    string groupedShift = reportRequest.IncludeShift ? shiftName : string.Empty;
                    string groupedEquipment = reportRequest.IncludeEquipment ? equipmentName : string.Empty;
                    string groupedSop = reportRequest.IncludeSop ? sopName : string.Empty;

                    return new
                    {
                        Line = groupedLine,
                        Shift = groupedShift,
                        Equipment = groupedEquipment,
                        SOP = groupedSop
                    };
                })
                .GroupBy(x => new
                {
                    x.Line,
                    x.Shift,
                    x.Equipment,
                    x.SOP
                })
                .Select(g => new ReportRow
                {
                    GroupValue = BuildGroupValue(
                        g.Key.Line,
                        g.Key.Shift,
                        g.Key.Equipment,
                        g.Key.SOP,
                        reportRequest.GroupingType),
                    Line = g.Key.Line,
                    Shift = g.Key.Shift,
                    Equipment = g.Key.Equipment,
                    SOP = g.Key.SOP,
                    IncidentCount = g.Count()
                })
                .ToList();

            return rows
                .OrderBy(r => GetPrimaryGroupSortValue(r, reportRequest.GroupingType))
                .ThenByDescending(r => r.IncidentCount)
                .ThenByDescending(r => r.IsFlagged)
                .ThenBy(r => r.GroupValue)
                .ThenBy(r => r.Line)
                .ThenBy(r => r.Shift)
                .ThenBy(r => r.Equipment)
                .ThenBy(r => r.SOP)
                .ToList();
        }

        /// <summary>
        /// Constructs the display label for a grouped report row based on the selected grouping type.
        /// Combines relevant fields in priority order to form a readable group identifier.
        /// </summary>
        /// <param name="line">The line identifier to include in the group value. Can be null or whitespace if not applicable.</param>
        /// <param name="shift">The shift identifier to include in the group value. Can be null or whitespace if not applicable.</param>
        /// <param name="equipment">The equipment identifier to include in the group value. Can be null or whitespace if not applicable.</param>
        /// <param name="sop">The SOP (Standard Operating Procedure) identifier to include in the group value. Can be null or whitespace
        /// if not applicable.</param>
        /// <param name="groupingType">The grouping type that determines the order in which the values are combined. Supported values are "Shift",
        /// "Equipment", "SOP", and "Line". If an unsupported value is provided, "Line" is used as the default.</param>
        /// <returns>A string representing the combined group value based on the specified grouping type and non-empty input
        /// values. Returns "All Incidents" if all input values are null or whitespace.</returns>
        private string BuildGroupValue(string line, string shift, string equipment, string sop, string groupingType)
        {
            List<string> parts = new();

            void AddIfPresent(string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    parts.Add(value);
                }
            }

            switch (groupingType)
            {
                case "Shift":
                    AddIfPresent(shift);
                    AddIfPresent(line);
                    AddIfPresent(equipment);
                    AddIfPresent(sop);
                    break;

                case "Equipment":
                    AddIfPresent(equipment);
                    AddIfPresent(line);
                    AddIfPresent(shift);
                    AddIfPresent(sop);
                    break;

                case "SOP":
                    AddIfPresent(sop);
                    AddIfPresent(line);
                    AddIfPresent(shift);
                    AddIfPresent(equipment);
                    break;

                case "Line":
                default:
                    AddIfPresent(line);
                    AddIfPresent(shift);
                    AddIfPresent(equipment);
                    AddIfPresent(sop);
                    break;
            }

            return parts.Count > 0
                ? string.Join(" | ", parts)
                : "All Incidents";
        }

        /// <summary>
        /// Returns the primary field used for sorting report rows based on the selected grouping type.
        /// </summary>
        /// <param name="row">The report row from which to extract the grouping value.</param>
        /// <param name="groupingType">The type of grouping to use when selecting the sort value. Supported values include "Shift", "Equipment",
        /// "SOP", and "Line". If an unrecognized value is provided, the default group value is used.</param>
        /// <returns>A string representing the primary sort value for the specified grouping type.</returns>
        private string GetPrimaryGroupSortValue(ReportRow row, string groupingType)
        {
            return groupingType switch
            {
                "Shift" => row.Shift,
                "Equipment" => row.Equipment,
                "SOP" => row.SOP,
                "Line" => row.Line,
                _ => row.GroupValue
            };
        }
    }
}