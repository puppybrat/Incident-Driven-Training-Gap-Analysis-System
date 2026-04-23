/*
 * File: ImportManager.cs
 * Author: Sarah Portillo
 * Date: 04/23/2026
 * Project: Incident-Driven Training Gap Analysis System
 * 
 * Layer: Application Layer
 * 
 * Purpose:
 * This class is responsible for managing the import of incident data from CSV files within the
 * Incident-Driven Training Gap Analysis System. It validates the format of the selected file, parses its contents, and
 * processes each row to ensure it meets the required criteria for an incident record. Valid records are
 * then inserted into the data store, while any errors encountered during processing are collected and
 * returned in an import summary. The ImportManager serves as an intermediary between the UI layer and
 * the data layer for handling bulk imports of incident data, ensuring that all necessary validations
 * are applied before any data is persisted.
 */

using System.Globalization;
using System.Text;
using Incident_Driven_Training_Gap_Analysis_System.Data;
using Incident_Driven_Training_Gap_Analysis_System.Domain;
using Incident_Driven_Training_Gap_Analysis_System.Models;

namespace Incident_Driven_Training_Gap_Analysis_System.Application
{
    /// <summary>
    /// Handles import operations for incident CSV files.
    /// </summary>
    public class ImportManager
    {
        private const int ColumnCount = 4;

        private readonly IncidentRepository _incidentRepository;
        private readonly ReferenceDataRepository _referenceDataRepository;

        private ReferenceDataSet _referenceDataSet = new();
        private string _lastRowError = string.Empty;

        /// <summary>
        /// Default constructor that initializes the ImportManager with a new instance of IncidentRepository and ReferenceDataRepository.
        /// </summary>
        public ImportManager()
        {
            _incidentRepository = new IncidentRepository();
            _referenceDataRepository = new ReferenceDataRepository();
        }

        /// <summary>
        /// Parameterized constructor for the test suite, allowing control over the database path for unit testing purposes.
        /// </summary>
        /// <param name="databaseManager">The DatabaseManager instance to use for database operations. Cannot be null.</param>
        public ImportManager(DatabaseManager databaseManager)
        {
            _incidentRepository = new IncidentRepository(databaseManager);
            _referenceDataRepository = new ReferenceDataRepository(databaseManager);
        }

        /// <summary>
        /// Imports records from a CSV file and returns a summary of the import operation.
        /// </summary>
        /// <param name="filePath">The path to the CSV file to import.</param>
        /// <returns>An import summary describing inserted and rejected rows.</returns>
        public ImportSummary ImportCsv(string filePath)
        {
            ImportSummary summary = new();

            try
            {
                if (!ValidateFileFormat(filePath))
                {
                    summary.Messages.Add("The selected file must be an existing CSV file.");
                    return summary;
                }

                List<string[]> rows;

                try
                {
                    rows = ParseRows(filePath);
                }
                catch (IOException)
                {
                    summary.Messages.Add("The CSV file could not be read. Make sure it is closed before importing.");
                    return summary;
                }
                catch
                {
                    summary.Messages.Add("The CSV file could not be parsed.");
                    return summary;
                }

                if (rows.Count == 0)
                {
                    summary.Messages.Add("The CSV file is empty.");
                    return summary;
                }

                string[] headerRow = rows[0];

                if (!IsSupportedHeader(headerRow))
                {
                    summary.Messages.Add(
                        "The CSV header is invalid. Expected format is " +
                        "'OccurredAt,EquipmentId,ShiftId,SopId'.");
                    return summary;
                }

                _referenceDataSet = _referenceDataRepository.GetAllReferenceData();

                List<Incident> validIncidents = new();

                for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
                {
                    string[] rowData = rows[rowIndex];

                    if (IsBlankRow(rowData))
                    {
                        continue;
                    }

                    Incident? incident = ProcessRow(rowData);

                    if (incident == null)
                    {
                        summary.RejectedCount++;
                        summary.Messages.Add($"Row {rowIndex + 1}: {_lastRowError}");
                        continue;
                    }

                    validIncidents.Add(incident);
                }

                if (validIncidents.Count == 0)
                {
                    if (summary.Messages.Count == 0)
                    {
                        summary.Messages.Add("No valid incident rows were found to import.");
                    }

                    return summary;
                }

                if (!InsertValidatedRecords(validIncidents))
                {
                    summary.RejectedCount += validIncidents.Count;
                    summary.Messages.Add("The import could not be completed because the validated records failed to insert.");
                    return summary;
                }

                summary.InsertedCount = validIncidents.Count;

                if (summary.RejectedCount == 0)
                {
                    summary.Messages.Add("All incident records were imported successfully.");
                }
                else
                {
                    summary.Messages.Add("Import completed with some rejected rows.");
                }

                return summary;
            }
            catch
            {
                summary.Messages.Add("An unexpected error occurred while importing the CSV file.");
                return summary;
            }
        }

        /// <summary>
        /// Validates whether the specified file is in a supported format.
        /// </summary>
        /// <param name="filePath">The full path to the file to validate.</param>
        /// <returns>true if the file format is supported; otherwise, false.</returns>
        private bool ValidateFileFormat(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                if (!File.Exists(filePath))
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
        /// Parses the specified CSV file and returns its non-blank rows as arrays of column values.
        /// </summary>
        /// <param name="filePath">The path to the file to parse.</param>
        /// <returns>A list of parsed rows.</returns>
        private List<string[]> ParseRows(string filePath)
        {
            List<string[]> rows = new();

            foreach (string line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                rows.Add(ParseCsvLine(line).ToArray());
            }

            return rows;
        }

        /// <summary>
        /// Processes a four-column incident CSV row and converts it to an <see cref="Incident"/> when all values are valid.
        /// The expected column order is OccurredAt, EquipmentId, ShiftId, and SopId.
        /// Reference data must already be loaded before calling this method so the identifier checks can be performed.
        /// </summary>
        /// <param name="rowData">The CSV row values to validate and convert.</param>
        /// <returns>The created <see cref="Incident"/> when the row is valid; otherwise, <see langword="null"/>.</returns>
        /// 
        private Incident? ProcessRow(string[] rowData)
        {
            _lastRowError = string.Empty;

            if (rowData == null || rowData.Length != ColumnCount)
            {
                _lastRowError = "The row does not contain the expected number of columns.";
                return null;
            }

            string[] formats =
            {
                "yyyy-MM-dd HH:mm:ss",
                "M/d/yyyy H:mm",
                "M/d/yyyy HH:mm",
                "MM/dd/yyyy HH:mm"
            };

            if (!DateTime.TryParseExact(
                    rowData[0].Trim(),
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime occurredAt))
            {
                _lastRowError = "OccurredAt must be a valid date/time (e.g., yyyy-MM-dd HH:mm:ss).";
                return null;
            }

            if (occurredAt > DateTime.Now)
            {
                _lastRowError = "OccurredAt cannot be in the future.";
                return null;
            }

            if (!int.TryParse(rowData[1].Trim(), out int equipmentId) || equipmentId <= 0)
            {
                _lastRowError = "EquipmentId must be a valid positive integer.";
                return null;
            }

            if (!int.TryParse(rowData[2].Trim(), out int shiftId) || shiftId <= 0)
            {
                _lastRowError = "ShiftId must be a valid positive integer.";
                return null;
            }

            int? sopId = null;
            string sopValue = rowData[3].Trim();

            if (!string.IsNullOrWhiteSpace(sopValue))
            {
                if (!int.TryParse(sopValue, out int parsedSopId) || parsedSopId <= 0)
                {
                    _lastRowError = "SopId must be blank or a valid positive integer.";
                    return null;
                }

                sopId = parsedSopId;
            }

            if (!_referenceDataSet.Equipment.Any(e => e.EquipmentId == equipmentId))
            {
                _lastRowError = "EquipmentId does not exist in reference data.";
                return null;
            }

            if (!_referenceDataSet.Shifts.Any(s => s.ShiftId == shiftId))
            {
                _lastRowError = "ShiftId does not exist in reference data.";
                return null;
            }

            if (sopId.HasValue)
            {
                Domain.SOP? matchingSop = _referenceDataSet.Sops.FirstOrDefault(s => s.SopId == sopId.Value);

                if (matchingSop == null)
                {
                    _lastRowError = "SopId does not exist in reference data.";
                    return null;
                }

                if (matchingSop.EquipmentId != equipmentId)
                {
                    _lastRowError = "SopId is not associated with the selected EquipmentId.";
                    return null;
                }
            }

            Incident incident = new()
            {
                OccurredAt = occurredAt,
                EquipmentId = equipmentId,
                ShiftId = shiftId,
                SopId = sopId
            };

            if (!incident.IsCompleteForCreation())
            {
                _lastRowError = "The incident row is missing required values or contains invalid values.";
                return null;
            }

            return incident;
        }

        /// <summary>
        /// Inserts a collection of validated incident records into the data store.
        /// </summary>
        /// <param name="incidentCollection">A list of validated incident records.</param>
        /// <returns>true if all records are successfully inserted; otherwise, false.</returns>
        private bool InsertValidatedRecords(List<Incident> incidentCollection)
        {
            try
            {
                if (incidentCollection == null || incidentCollection.Count == 0)
                {
                    return false;
                }

                return _incidentRepository.InsertIncidents(incidentCollection);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the header row matches the supported incident import header.
        /// </summary>
        /// <param name="headerRow">The header row to validate.</param>
        /// <returns>true if the header is supported; otherwise, false.</returns>
        private static bool IsSupportedHeader(string[] headerRow)
        {
            return headerRow.Length == ColumnCount
                && string.Equals(headerRow[0], "OccurredAt", StringComparison.OrdinalIgnoreCase)
                && string.Equals(headerRow[1], "EquipmentId", StringComparison.OrdinalIgnoreCase)
                && string.Equals(headerRow[2], "ShiftId", StringComparison.OrdinalIgnoreCase)
                && string.Equals(headerRow[3], "SopId", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether all values in a row are blank.
        /// </summary>
        /// <param name="rowData">The row to inspect.</param>
        /// <returns>true if the row is blank; otherwise, false.</returns>
        private static bool IsBlankRow(string[] rowData)
        {
            return rowData.All(value => string.IsNullOrWhiteSpace(value));
        }

        /// <summary>
        /// Parses a single CSV line, including quoted values and escaped quotes.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>A list of values for the CSV row.</returns>
        private static List<string> ParseCsvLine(string line)
        {
            List<string> values = new();
            StringBuilder currentValue = new();
            bool insideQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char currentCharacter = line[i];

                if (currentCharacter == '"')
                {
                    if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }

                    continue;
                }

                if (currentCharacter == ',' && !insideQuotes)
                {
                    values.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                    continue;
                }

                currentValue.Append(currentCharacter);
            }

            values.Add(currentValue.ToString().Trim());
            return values;
        }
    }
}