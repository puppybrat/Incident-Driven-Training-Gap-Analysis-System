/*
 * File: ReportGenerator.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class generates reports from incident data. It applies filters,
 * groups matching incidents, builds report rows, and returns the final
 * report result for UI display or export.
 */

using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Generates incident reports using filters, grouping options, presets, and rule evaluation.
    /// </summary>
    public class ReportGenerator
    {
        private readonly DatabaseManager _databaseManager;
        private readonly IncidentRepository _incidentRepository;
        private readonly ReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// Initializes report generation using the default incident and reference data repositories.
        /// </summary>
        public ReportGenerator()
        {
            _databaseManager = new DatabaseManager();
            _incidentRepository = new IncidentRepository();
            _referenceDataRepository = new ReferenceDataRepository();
        }

        /// <summary>
        /// Initializes report generation with a database manager, primarily for controlled database access in tests.
        /// </summary>
        /// <param name="databaseManager">The database manager used to create report repositories.</param>
        public ReportGenerator(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
            _incidentRepository = new IncidentRepository(databaseManager);
            _referenceDataRepository = new ReferenceDataRepository(databaseManager);
        }

        /// <summary>
        /// Generates a report by applying filters, building grouped rows, and evaluating rule thresholds.
        /// </summary>
        /// <param name="reportRequest">The filters, grouping, output type, and included fields for the report.</param>
        /// <returns>The generated report result with rows and output metadata.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reportRequest"/> is null.</exception>
        public ReportResult GenerateReport(ReportRequest reportRequest)
        {
            ArgumentNullException.ThrowIfNull(reportRequest);

            List<Incident> filteredIncidents = ApplyFilters(reportRequest);
            List<ReportRow> rows = BuildRows(filteredIncidents, reportRequest);

            RuleEvaluator evaluator = new(_databaseManager);
            RuleConfig config = evaluator.LoadCurrentRuleConfig();
            rows = RuleEvaluator.EvaluateThresholds(rows, config);

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
        /// Retrieves incidents that match the report request filters.
        /// </summary>
        /// <param name="reportRequest">The report request containing the filters.</param>
        /// <returns>A list of matching incidents.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reportRequest"/> is null.</exception>
        public List<Incident> ApplyFilters(ReportRequest reportRequest)
        {
            ArgumentNullException.ThrowIfNull(reportRequest);

            FilterSet filters = reportRequest.Filters ?? new FilterSet();
            return _incidentRepository.GetIncidents(filters);
        }

        /// <summary>
        /// Selects preset-specific report logic or the general grouped report builder.
        /// </summary>
        /// <param name="incidents">The incidents to include in the report.</param>
        /// <param name="reportRequest">The report request used to choose the row-building strategy.</param>
        /// <returns>The generated report rows.</returns>
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
        /// Builds grouped report rows for incidents that do not have an SOP reference.
        /// </summary>
        /// <param name="incidents">The incidents to check for missing SOP references.</param>
        /// <param name="reportRequest">The report request used to build the grouped rows.</param>
        /// <returns>Report rows for incidents without SOP references.</returns>
        private List<ReportRow> BuildLinesByMissingSop(List<Incident> incidents, ReportRequest reportRequest)
        {
            List<Incident> incidentsWithoutSop =
            [
                .. incidents.Where(i => !i.SopId.HasValue)
            ];

            return BuildGroupedReport(incidentsWithoutSop, reportRequest);
        }

        /// <summary>
        /// Builds grouped report rows from matching incidents and selected report fields.
        /// </summary>
        /// <param name="incidents">The incidents to include.</param>
        /// <param name="reportRequest">The report configuration to apply.</param>
        /// <returns>A list of grouped report rows.</returns>
        private List<ReportRow> BuildGroupedReport(List<Incident> incidents, ReportRequest reportRequest)
        {
            ReferenceDataSet referenceData = _referenceDataRepository.GetAllReferenceData();

            Dictionary<int, string> shiftLookup = referenceData.Shifts.ToDictionary(s => s.ShiftId, s => s.Name);
            Dictionary<int, Equipment> equipmentLookup = referenceData.Equipment.ToDictionary(e => e.EquipmentId, e => e);
            Dictionary<int, string> lineLookup = referenceData.Lines.ToDictionary(l => l.LineId, l => l.Name);
            Dictionary<int, string> sopLookup = referenceData.Sops.ToDictionary(s => s.SopId, s => s.Name);

            List<ReportRow> rows =
            [
                .. incidents
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
            ];

            return
            [
                .. rows
                    .OrderBy(r => GetPrimaryGroupSortValue(r, reportRequest.GroupingType))
                    .ThenByDescending(r => r.IncidentCount)
                    .ThenByDescending(r => r.IsFlagged)
                    .ThenBy(r => r.GroupValue)
                    .ThenBy(r => r.Line)
                    .ThenBy(r => r.Shift)
                    .ThenBy(r => r.Equipment)
                    .ThenBy(r => r.SOP)
            ];
        }

        /// <summary>
        /// Builds the display label for a grouped report row.
        /// </summary>
        /// <param name="line">The line value.</param>
        /// <param name="shift">The shift value.</param>
        /// <param name="equipment">The equipment value.</param>
        /// <param name="sop">The SOP value.</param>
        /// <param name="groupingType">The selected grouping type.</param>
        /// <returns>The formatted group label.</returns>
        private static string BuildGroupValue(string line, string shift, string equipment, string sop, string groupingType)
        {
            List<string> parts = [];

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
        /// Gets the row value used for primary sorting based on the selected grouping type.
        /// </summary>
        /// <param name="row">The report row to inspect.</param>
        /// <param name="groupingType">The grouping type used to choose the sort field.</param>
        /// <returns>The row value used for sorting.</returns>
        private static string GetPrimaryGroupSortValue(ReportRow row, string groupingType)
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