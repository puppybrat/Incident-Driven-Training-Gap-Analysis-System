using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    public class ReportGenerator
    {
        private readonly IncidentRepository _incidentRepository;
        private readonly ReferenceDataRepository _referenceDataRepository;

        public ReportGenerator()
        {
            _incidentRepository = new IncidentRepository();
            _referenceDataRepository = new ReferenceDataRepository();
        }

        public ReportGenerator(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
            _referenceDataRepository = new ReferenceDataRepository(databaseManager);
        }

        public ReportResult GenerateReport(ReportRequest reportRequest)
        {
            List<Incident> filteredIncidents = ApplyFilters(reportRequest);

            List<ReportRow> rows = reportRequest.PresetName switch
            {
                "Incidents per Shift by Line" => BuildIncidentsPerShiftByLine(filteredIncidents, reportRequest),
                "Lines by missing SOP" => BuildLinesByMissingSop(filteredIncidents, reportRequest),
                "Incidents per Equipment" => BuildIncidentsPerEquipment(filteredIncidents, reportRequest),
                "Incidents per SOP Reference" => BuildIncidentsPerSopReference(filteredIncidents, reportRequest),
                "None" => BuildGroupedReport(filteredIncidents, reportRequest),
                _ => BuildGroupedReport(filteredIncidents, reportRequest)
            };

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
                    string equipmentName = equipmentLookup.TryGetValue(i.EquipmentId, out Equipment? equipment)
                        ? equipment.Name
                        : $"Equipment {i.EquipmentId}";

                    string lineName = "Unknown Line";
                    if (equipmentLookup.TryGetValue(i.EquipmentId, out Equipment? equipmentForLine) &&
                        lineLookup.TryGetValue(equipmentForLine.LineId, out string? line))
                    {
                        lineName = line;
                    }

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

        public List<Incident> ApplyFilters(ReportRequest reportRequest)
        {
            FilterSet filters = reportRequest.Filters ?? new FilterSet();
            return _incidentRepository.GetIncidents(filters);
        }

        public List<AggregateResult> AggregateIncidents(List<Incident> filteredIncidents)
        {
            ReportRequest defaultRequest = new()
            {
                PresetName = "Incidents per Equipment",
                OutputType = "Table"
            };

            return AggregateIncidents(filteredIncidents, defaultRequest);
        }

        public List<AggregateResult> AggregateIncidents(List<Incident> filteredIncidents, ReportRequest reportRequest)
        {
            List<ReportRow> rows = reportRequest.PresetName switch
            {
                "Incidents per Shift by Line" => BuildIncidentsPerShiftByLine(filteredIncidents, reportRequest),
                "Lines by missing SOP" => BuildLinesByMissingSop(filteredIncidents, reportRequest),
                "Incidents per Equipment" => BuildIncidentsPerEquipment(filteredIncidents, reportRequest),
                "Incidents per SOP Reference" => BuildIncidentsPerSopReference(filteredIncidents, reportRequest),
                "None" => BuildGroupedReport(filteredIncidents, reportRequest),
                _ => BuildGroupedReport(filteredIncidents, reportRequest)
            };

            return rows.Select(r => new AggregateResult
            {
                GroupLabel = r.GroupValue,
                IncidentCount = r.IncidentCount,
                IsFlagged = r.IsFlagged
            }).ToList();
        }

        public List<FormattedResult> FormatResults(List<AggregateResult> aggregateCollection)
        {
            return aggregateCollection
                .Select(a => new FormattedResult
                {
                    DisplayLabel = a.GroupLabel,
                    DisplayValue = a.IncidentCount.ToString()
                })
                .ToList();
        }

        public ReportResult BuildReportResult(List<FormattedResult> formattedResults)
        {
            ReportResult result = new();

            foreach (FormattedResult formatted in formattedResults)
            {
                result.Rows.Add(new ReportRow
                {
                    GroupValue = formatted.DisplayLabel,
                    Line = formatted.DisplayLabel,
                    Shift = string.Empty,
                    Equipment = string.Empty,
                    SOP = string.Empty,
                    IncidentCount = int.TryParse(formatted.DisplayValue, out int count) ? count : 0,
                    IsFlagged = false
                });
            }

            return result;
        }

        private List<ReportRow> BuildIncidentsGroupedByShift(List<Incident> incidents)
        {
            return BuildGroupedReport(incidents, new ReportRequest
            {
                GroupingType = "Shift",
                IncludeLine = false,
                IncludeShift = true,
                IncludeEquipment = false,
                IncludeSop = false
            });
        }

        private List<ReportRow> BuildIncidentsGroupedByLine(List<Incident> incidents)
        {
            return BuildGroupedReport(incidents, new ReportRequest
            {
                GroupingType = "Line",
                IncludeLine = true,
                IncludeShift = false,
                IncludeEquipment = false,
                IncludeSop = false
            });
        }

        private List<ReportRow> BuildIncidentsPerEquipment(List<Incident> incidents, ReportRequest reportRequest)
        {
            return BuildGroupedReport(incidents, reportRequest);
        }

        private List<ReportRow> BuildIncidentsPerSopReference(List<Incident> incidents, ReportRequest reportRequest)
        {
            return BuildGroupedReport(incidents, reportRequest);
        }

        private List<ReportRow> BuildLinesByMissingSop(List<Incident> incidents, ReportRequest reportRequest)
        {
            List<Incident> incidentsWithoutSop = incidents
                .Where(i => !i.SopId.HasValue)
                .ToList();

            return BuildGroupedReport(incidentsWithoutSop, reportRequest);
        }

        private List<ReportRow> BuildIncidentsPerShiftByLine(List<Incident> incidents, ReportRequest reportRequest)
        {
            return BuildGroupedReport(incidents, reportRequest);
        }
    }
}