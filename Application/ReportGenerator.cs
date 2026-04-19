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
                "Incidents per Shift by Line" => BuildIncidentsPerShiftByLine(filteredIncidents),
                "Lines by missing SOP" => BuildLinesByMissingSop(filteredIncidents),
                "Incidents per Equipment" => BuildIncidentsPerEquipment(filteredIncidents),
                "Incidents per SOP Reference" => BuildIncidentsPerSopReference(filteredIncidents),
                _ => new List<ReportRow>()
            };

            RuleEvaluator evaluator = new();
            RuleConfig config = evaluator.LoadCurrentRuleConfig();
            rows = evaluator.EvaluateThresholds(rows, config);

            return new ReportResult
            {
                PresetName = reportRequest.PresetName,
                OutputType = reportRequest.OutputType,
                Rows = rows
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
                "Incidents per Shift by Line" => BuildIncidentsPerShiftByLine(filteredIncidents),
                "Lines by missing SOP" => BuildLinesByMissingSop(filteredIncidents),
                "Incidents per Equipment" => BuildIncidentsPerEquipment(filteredIncidents),
                "Incidents per SOP Reference" => BuildIncidentsPerSopReference(filteredIncidents),
                _ => new List<ReportRow>()
            };

            return rows.Select(r => new AggregateResult
            {
                GroupLabel = string.IsNullOrWhiteSpace(r.GroupSecondary)
                    ? r.GroupPrimary
                    : $"{r.GroupPrimary} - {r.GroupSecondary}",
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
                    GroupPrimary = formatted.DisplayLabel,
                    IncidentCount = int.TryParse(formatted.DisplayValue, out int count) ? count : 0,
                    IsFlagged = false
                });
            }

            return result;
        }

        private List<ReportRow> BuildIncidentsPerEquipment(List<Incident> incidents)
        {
            ReferenceDataSet referenceData = _referenceDataRepository.GetAllReferenceData();

            Dictionary<int, string> equipmentLookup = referenceData.Equipment
                .ToDictionary(e => e.EquipmentId, e => e.Name);

            return incidents
                .GroupBy(i => equipmentLookup.TryGetValue(i.EquipmentId, out string? equipmentName)
                    ? equipmentName
                    : $"Equipment {i.EquipmentId}")
                .Select(g => new ReportRow
                {
                    GroupPrimary = g.Key,
                    IncidentCount = g.Count()
                })
                .OrderByDescending(r => r.IncidentCount)
                .ThenBy(r => r.GroupPrimary)
                .ToList();
        }

        private List<ReportRow> BuildIncidentsPerSopReference(List<Incident> incidents)
        {
            ReferenceDataSet referenceData = _referenceDataRepository.GetAllReferenceData();

            Dictionary<int, string> sopLookup = referenceData.Sops
                .ToDictionary(s => s.SopId, s => s.Name);

            return incidents
                .GroupBy(i =>
                {
                    if (i.SopId.HasValue && sopLookup.TryGetValue(i.SopId.Value, out string? sopName))
                    {
                        return sopName;
                    }

                    return "No SOP";
                })
                .Select(g => new ReportRow
                {
                    GroupPrimary = g.Key,
                    IncidentCount = g.Count()
                })
                .OrderByDescending(r => r.IncidentCount)
                .ThenBy(r => r.GroupPrimary)
                .ToList();
        }

        private List<ReportRow> BuildLinesByMissingSop(List<Incident> incidents)
        {
            ReferenceDataSet referenceData = _referenceDataRepository.GetAllReferenceData();

            Dictionary<int, Equipment> equipmentLookup = referenceData.Equipment
                .ToDictionary(e => e.EquipmentId, e => e);

            Dictionary<int, string> lineLookup = referenceData.Lines
                .ToDictionary(l => l.LineId, l => l.Name);

            return incidents
                .Where(i => !i.SopId.HasValue)
                .GroupBy(i =>
                {
                    if (equipmentLookup.TryGetValue(i.EquipmentId, out Equipment? equipment) &&
                        lineLookup.TryGetValue(equipment.LineId, out string? lineName))
                    {
                        return lineName;
                    }

                    return "Unknown Line";
                })
                .Select(g => new ReportRow
                {
                    GroupPrimary = g.Key,
                    IncidentCount = g.Count()
                })
                .OrderByDescending(r => r.IncidentCount)
                .ThenBy(r => r.GroupPrimary)
                .ToList();
        }

        private List<ReportRow> BuildIncidentsPerShiftByLine(List<Incident> incidents)
        {
            ReferenceDataSet referenceData = _referenceDataRepository.GetAllReferenceData();

            Dictionary<int, string> shiftLookup = referenceData.Shifts
                .ToDictionary(s => s.ShiftId, s => s.Name);

            Dictionary<int, Equipment> equipmentLookup = referenceData.Equipment
                .ToDictionary(e => e.EquipmentId, e => e);

            Dictionary<int, string> lineLookup = referenceData.Lines
                .ToDictionary(l => l.LineId, l => l.Name);

            return incidents
                .GroupBy(i =>
                {
                    string shiftName = shiftLookup.TryGetValue(i.ShiftId, out string? shiftNameValue)
                        ? shiftNameValue
                        : $"Shift {i.ShiftId}";

                    string lineName = "Unknown Line";

                    if (equipmentLookup.TryGetValue(i.EquipmentId, out Equipment? equipment) &&
                        lineLookup.TryGetValue(equipment.LineId, out string? resolvedLine))
                    {
                        lineName = resolvedLine;
                    }

                    return new { Shift = shiftName, Line = lineName };
                })
                .Select(g => new ReportRow
                {
                    GroupPrimary = g.Key.Shift,
                    GroupSecondary = g.Key.Line,
                    IncidentCount = g.Count()
                })
                .OrderBy(r => r.GroupPrimary)
                .ThenBy(r => r.GroupSecondary)
                .ToList();
        }
    }
}