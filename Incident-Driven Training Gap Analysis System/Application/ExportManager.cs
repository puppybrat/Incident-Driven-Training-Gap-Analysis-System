/*
 * File: ExportManager.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class is responsible for handling export operations for incident data and report data.
 * It retrieves incident records from the data persistence layer, formats exportable content as CSV,
 * validates export destinations, and writes export files for use outside the application.
 */

using System.Text;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Handles export operations for incident data and generated report data.
    /// </summary>
    public class ExportManager
    {
        private readonly IncidentRepository _incidentRepository;

        /// <summary>
        /// Default constructor that initializes the ExportManager with a new instance of IncidentRepository.
        /// </summary>
        public ExportManager()
        {
            _incidentRepository = new IncidentRepository();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public ExportManager(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
        }

        /// <summary>
        /// Exports incident data that matches the supplied filters to a CSV file.
        /// </summary>
        /// <param name="filePath">The destination CSV file path.</param>
        /// <param name="filterSet">The filters used to select incidents for export.</param>
        /// <returns>An <see cref="ExportSummary"/> describing whether the export succeeded and any resulting message.</returns>
        public ExportSummary ExportDataset(string filePath, FilterSet filterSet)
        {
            ExportSummary summary = new();

            try
            {
                filterSet ??= new FilterSet();

                if (!ValidateExportLocation(filePath))
                {
                    summary.Message = "The selected export location is invalid.";
                    return summary;
                }

                List<Domain.Incident> incidents = _incidentRepository.GetIncidents(filterSet);

                if (incidents.Count == 0)
                {
                    summary.Message = "There is no incident data available to export.";
                    return summary;
                }

                StringBuilder csvBuilder = new();
                csvBuilder.AppendLine("OccurredAt,EquipmentId,ShiftId,SopId");

                foreach (Domain.Incident incident in incidents)
                {
                    List<string> values = new()
            {
                EscapeCsv(incident.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss")),
                incident.EquipmentId.ToString(),
                incident.ShiftId.ToString(),
                EscapeCsv(incident.SopId?.ToString() ?? string.Empty)
            };

                    csvBuilder.AppendLine(string.Join(",", values));
                }

                if (!WriteCsvFile(filePath, csvBuilder.ToString()))
                {
                    summary.Message = "The file could not be written. Ensure it is not open in another application.";
                    return summary;
                }

                summary.Success = true;
                summary.Message = "Incident data exported successfully.";
                return summary;
            }
            catch
            {
                summary.Message = "An unexpected error occurred during export.";
                return summary;
            }
        }

        /// <summary>
        /// Exports the supplied generated report data to a CSV file.
        /// The export uses the report rows and the enabled report columns in <paramref name="reportResult"/>.
        /// </summary>
        /// <param name="filePath">The destination CSV file path.</param>
        /// <param name="reportResult">The report result to export.</param>
        /// <returns>An <see cref="ExportSummary"/> describing whether the export succeeded and any resulting message.</returns>
        public ExportSummary ExportReport(string filePath, ReportResult reportResult)
        {
            ExportSummary summary = new();

            try
            {
                if (reportResult == null)
                {
                    summary.Message = "No report is available to export.";
                    return summary;
                }

                if (!ValidateExportLocation(filePath))
                {
                    summary.Message = "The selected export location is invalid.";
                    return summary;
                }

                if (reportResult.Rows == null || reportResult.Rows.Count == 0)
                {
                    summary.Message = "The report contains no data to export.";
                    return summary;
                }

                List<string> headers = BuildReportHeaders(reportResult);

                if (headers.Count == 0)
                {
                    summary.Message = "The report has no visible columns to export.";
                    return summary;
                }

                StringBuilder csvBuilder = new();
                csvBuilder.AppendLine(string.Join(",", headers));

                foreach (ReportRow row in reportResult.Rows)
                {
                    List<string> values = BuildReportRowValues(reportResult, row);
                    csvBuilder.AppendLine(string.Join(",", values));
                }

                if (!WriteCsvFile(filePath, csvBuilder.ToString()))
                {
                    summary.Message = "The file could not be written. Ensure it is not open in another application.";
                    return summary;
                }

                summary.Success = true;
                summary.Message = "Report data exported successfully.";
                return summary;
            }
            catch
            {
                summary.Message = "An unexpected error occurred during export.";
                return summary;
            }
        }

        /// <summary>
        /// Validates that the export path is usable and has a CSV extension.
        /// </summary>
        /// <param name="filePath">The destination file path to validate.</param>
        /// <returns>True if the location is valid; otherwise, false.</returns>
        private bool ValidateExportLocation(string filePath)
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

        /// <summary>
        /// Writes CSV data to a file.
        /// </summary>
        /// <param name="filePath">The destination path.</param>
        /// <param name="csvData">The CSV data to write.</param>
        /// <returns>True if the write succeeds; otherwise, false.</returns>
        private bool WriteCsvFile(string filePath, string csvData)
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

        /// <summary>
        /// Builds the CSV header row for report export based on the enabled columns in the report result.
        /// </summary>
        /// <param name="reportResult">The report metadata that determines which columns are included.</param>
        /// <returns>A list of header values.</returns>
        private static List<string> BuildReportHeaders(ReportResult reportResult)
        {
            List<string> headers = new();

            if (reportResult.IncludeLine)
            {
                headers.Add("Line");
            }

            if (reportResult.IncludeShift)
            {
                headers.Add("Shift");
            }

            if (reportResult.IncludeEquipment)
            {
                headers.Add("Equipment");
            }

            if (reportResult.IncludeSop)
            {
                headers.Add("SOP");
            }

            headers.Add("IncidentCount");
            headers.Add("Status");

            return headers;
        }

        /// <summary>
        /// Builds a CSV data row for report export based on the enabled columns in the report result.
        /// </summary>
        /// <param name="reportResult">The report metadata that determines which columns are included.</param>
        /// <param name="row">The report row being exported.</param>
        /// <returns>A list of CSV-safe values.</returns>
        private static List<string> BuildReportRowValues(ReportResult reportResult, ReportRow row)
        {
            List<string> values = new();

            if (reportResult.IncludeLine)
            {
                values.Add(EscapeCsv(row.Line));
            }

            if (reportResult.IncludeShift)
            {
                values.Add(EscapeCsv(row.Shift));
            }

            if (reportResult.IncludeEquipment)
            {
                values.Add(EscapeCsv(row.Equipment));
            }

            if (reportResult.IncludeSop)
            {
                values.Add(EscapeCsv(row.SOP));
            }

            values.Add(row.IncidentCount.ToString());
            values.Add(EscapeCsv(row.Status));

            return values;
        }

        /// <summary>
        /// Escapes a value for CSV output.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>A CSV-safe string.</returns>
        private static string EscapeCsv(string? value)
        {
            value ??= string.Empty;

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