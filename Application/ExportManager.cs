using System.Text;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    public class ExportManager
    {
        private readonly IncidentRepository _incidentRepository;

        public ExportManager()
        {
            _incidentRepository = new IncidentRepository();
        }

        public ExportManager(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
        }

        public bool ExportDataset(string filePath, FilterSet filterSet)
        {
            try
            {
                if (!ValidateExportLocation(filePath))
                {
                    return false;
                }

                List<Domain.Incident> incidents = _incidentRepository.GetIncidents(filterSet);

                StringBuilder csvBuilder = new();
                csvBuilder.AppendLine("IncidentId,OccurredAt,EquipmentId,ShiftId,SopId");

                foreach (Domain.Incident incident in incidents)
                {
                    csvBuilder.AppendLine(string.Join(",",
                        incident.IncidentId,
                        EscapeCsv(incident.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss")),
                        incident.EquipmentId,
                        incident.ShiftId,
                        incident.SopId?.ToString() ?? string.Empty));
                }

                return WriteCsvFile(filePath, csvBuilder.ToString());
            }
            catch
            {
                return false;
            }
        }

        public bool ExportReport(string filePath, ReportResult reportResult)
        {
            try
            {
                if (reportResult == null || !ValidateExportLocation(filePath))
                {
                    return false;
                }

                StringBuilder csvBuilder = new();
                csvBuilder.AppendLine("PresetName,OutputType,Line,Shift,Equipment,SOP,Date,IncidentCount,Status");

                foreach (ReportRow row in reportResult.Rows)
                {
                    csvBuilder.AppendLine(string.Join(",",
                        EscapeCsv(reportResult.PresetName),
                        EscapeCsv(reportResult.OutputType),
                        EscapeCsv(row.Line),
                        EscapeCsv(row.Shift),
                        EscapeCsv(row.Equipment),
                        EscapeCsv(row.SOP),
                        row.IncidentCount,
                        EscapeCsv(row.Status)));
                }

                return WriteCsvFile(filePath, csvBuilder.ToString());
            }
            catch
            {
                return false;
            }
        }

        public bool WriteCsvFile(string filePath, string csvData)
        {
            try
            {
                File.WriteAllText(filePath, csvData, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateExportLocation(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                string? directory = Path.GetDirectoryName(filePath);

                if (string.IsNullOrWhiteSpace(directory))
                {
                    return false;
                }

                if (!Directory.Exists(directory))
                {
                    return false;
                }

                string extension = Path.GetExtension(filePath);
                return string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value}\"";
            }

            return value;
        }
    }
}