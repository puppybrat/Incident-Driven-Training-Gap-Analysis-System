## Build and Run Instructions

### Prerequisites
- Windows operating system
- Visual Studio with .NET desktop development workload
- .NET 10 SDK

### Build
1. Open `Incident-Driven Training Gap Analysis System.sln` in Visual Studio.
2. Set the main WinForms project as the startup project.
3. Select `Build > Build Solution`.

### Run
1. Press `F5` in Visual Studio, or run the executable produced by publishing the project.
2. To publish an executable, right-click the main WinForms project, select `Publish`, choose `Folder`, set configuration to `Release`, target runtime to `win-x64`, and deployment mode to `Self-contained`.

### Test
1. Open Test Explorer in Visual Studio.
2. Select `Run All Tests`.
3. Expected result: 107 tests passing, 0 failing.