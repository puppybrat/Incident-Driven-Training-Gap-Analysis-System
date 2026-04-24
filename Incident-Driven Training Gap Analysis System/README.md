# Incident-Driven Training Gap Analysis System

## Overview

The Incident-Driven Training Gap Analysis System is a Windows desktop application that stores incident records and generates rule-based reports to help identify repeated incident patterns. These patterns may indicate areas where additional training, SOP review, or process review may be useful.

The system is intended for decision support only. It does not diagnose root causes, assign blame, or prescribe corrective actions.

## Purpose

This application supports incident review by allowing the user to:

- Create individual incident records
- Import incident records from CSV files
- Configure rule thresholds
- Generate filtered and grouped reports
- Identify repeated incident patterns
- Review incidents with missing SOP references
- Export incident datasets and report results

## System Type

- Windows desktop application
- Single-user system
- Local/offline data storage
- SQLite database
- No network transmission of incident data

## Application Navigation

The main application window provides access to the primary workflows:

- New Incident
- Configure Rules
- Generate Report
- Import CSV
- Export Data
- Help

Use the menu options or available buttons to open each workflow.

## First-Time Use

When the application is opened, it prepares the local application data needed to run the system.

On first use, the system initializes the database and required reference data. After initialization, incident data and saved rule settings persist between sessions.

## Creating an Incident

The New Incident screen allows the user to create one incident record at a time.

### Required Fields

The following fields are required:

- Occurred At
- Shift
- Line
- Equipment

### Optional Field

The SOP field is optional.

An incident may be saved without an SOP reference. These records can later be reviewed through the Missing SOP report option.

### Selection Behavior

The form uses dependent selections:

- The equipment list changes based on the selected line.
- The SOP list changes based on the selected equipment.

### Validation

Before an incident is saved, the system validates that required fields are complete and valid.

An incident is not saved if required data is missing or invalid.

## Configuring Rules

The Configure Rules screen controls how generated report rows are evaluated.

### Configurable Rule Values

The rule configuration includes:

- Threshold Value
- Grouping Type
- Time Window
- Flagging Enabled

### Threshold Value

The threshold value determines the incident count needed for a report row to be flagged.

### Grouping Type

Grouping Type determines the main category used when evaluating and organizing report results.

### Time Window

The time window controls the period considered by the rule configuration.

### Flagging Enabled

When flagging is enabled, report rows that meet or exceed the configured threshold are marked as flagged.

## Generating Reports

The Generate Report screen builds reports from stored incident data.

### Report Presets

Report presets provide predefined report configurations.

Selecting a preset populates related report options automatically. Changing a populated preset option switches the preset to Custom.

### Filters

Filters narrow which incident records are included in the report.

### Grouping Type

Grouping Type controls how matching incidents are aggregated.

### Included Fields

Included fields control which columns appear in the report output.

### Missing SOP

The Missing SOP option includes incidents where the SOP field is blank.

### Date Range

A date range may be used to limit the report to incidents within a selected period.

### Output Type

Reports may be displayed as table or graph output.

## Importing CSV Data

The CSV file must use the following header:

OccurredAt,EquipmentId,ShiftId,SopId

The SopId value may be blank.

Imported rows are validated before insertion.

## Exporting Data

The system can export:

- Incident dataset
- Report results

## Data Storage

The system stores data locally using SQLite.

## Constraints and Notes

- Single-user system
- No network communication
- Data accuracy depends on inputs

## Recommended Workflow

1. Create or import incidents
2. Configure rules
3. Generate report
4. Review results
5. Export if needed

## CSV Example

Example CSV format for importing incident data:

OccurredAt,EquipmentId,ShiftId,SopId

2026-04-01 08:30:00,1,1,2
2026-04-01 10:15:00,2,1,
2026-04-02 14:45:00,3,2,4

A blank SopId value represents an incident without an SOP reference.

## Troubleshooting

### CSV Import Issues
- Ensure the file uses the correct header:
  OccurredAt,EquipmentId,ShiftId,SopId
- Ensure the file is not open in another program
- Ensure values match valid system data

### Report Shows No Results
- Verify incidents exist
- Check filters are not too restrictive
- Confirm date range includes data

### Missing SOP Not Appearing
- Only incidents with blank SOP values are included

### Export Issues
- Ensure a report has been generated before exporting report results
- Ensure the destination file is not open