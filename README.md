# PathApp

A real-time 3D path drift visualisation tool built with .NET 10, Blazor Server, and gRPC.

## Architecture

The solution consists of three projects:

| Project | Role |
|---------|------|
| **PathDrift.Primary** | Console app that reads coordinate data from a CSV file and streams it via gRPC |
| **PathViewingApp** | Blazor Server app that receives coordinates and renders them as an interactive SVG path |
| **PathDrift.Shared** | Shared library containing models, interfaces, and the gRPC proto definition |

### Data Flow

```
CSV File → PathDrift.Primary → (gRPC stream) → PathViewingApp → Browser (SVG)
```

## Getting Started

### Prerequisites

- .NET 10 SDK

### Running from Visual Studio

1. Right-click the solution in Solution Explorer → **Configure Startup Projects**
2. Select **Multiple startup projects**
3. Set both **PathViewingApp** and **PathDrift.Primary** to **Start**
4. Ensure **PathViewingApp** is listed **above** PathDrift.Primary (it must start first as it hosts the gRPC server)
5. Press **F5** or click **Start**

### Running from the command line

1. **Start the Blazor app** (hosts the gRPC server):
   ```bash
   cd PathViewingApp
   dotnet run
   ```

2. **Start the Primary service** (streams data to the Blazor app):
   ```bash
   cd PathDrift.Primary
   dotnet run
   ```

3. Open your browser to `https://localhost:7255` to see the path visualisation.

   The page shows:
   - An **SVG chart** of the path, switchable between X-Y, X-Z, and Y-Z planes
   - A **coordinate data table** below the chart listing every received point with all columns in real-time

## Features

- Real-time coordinate streaming via gRPC (client-streaming)
- Interactive SVG visualisation with plane selection (X-Y, X-Z, Y-Z)
- Thread-safe in-memory data store with event-driven UI updates
- Start/end point markers on the rendered path
- Coordinate data table displaying all streamed points (Path ID, Index, X, Y, Z, Rx, Ry, Rz) in real-time below the visualisation

## Third-Party Libraries

| Package | Purpose |
|---------|---------|
| Google.Protobuf | Protocol Buffer serialization |
| Grpc.Net.Client | gRPC client |
| Grpc.AspNetCore | gRPC server + code generation |
| Microsoft.Extensions.Hosting | DI and app lifecycle for the console app |
| Bootstrap | CSS framework for UI styling |

## Data Format

The input CSV file uses the following format:

```
ID,Index,X,Y,Z,Rx,Ry,Rz
Path_2,0,-308.3428217,-104.3258526,614.7202431,1.470218752,2.290488904,-91.87757768
```

### Changing the Data File

To use your own coordinate data, replace the contents of `PathDrift.Primary/data/run1.csv` with your data. The file must:

1. Have a header row as the first line (it will be skipped automatically)
2. Use comma-separated values with 8 columns: `ID,Index,X,Y,Z,Rx,Ry,Rz`
3. Keep the filename as `run1.csv`

Example:

```csv
ID,Index,X,Y,Z,Rx,Ry,Rz
MyPath,0,100.0,200.0,300.0,0.5,0.6,0.7
MyPath,1,101.0,201.5,300.2,0.5,0.6,0.7
MyPath,2,102.3,203.0,300.5,0.5,0.6,0.7
```

| Column | Type | Description |
|--------|------|-------------|
| ID | string | Path identifier (groups points into a single path) |
| Index | int | Sequential point index |
| X, Y, Z | double | 3D position coordinates |
| Rx, Ry, Rz | double | Rotation values |

No restart of PathViewingApp is needed — just re-run PathDrift.Primary after updating the file.

## Testing

All tests use **xUnit** with **NSubstitute** for mocking. The PathViewingApp tests also use **bUnit** for Blazor component testing.

### Test Projects

| Test Project | Covers |
|-------------|--------|
| **PathDrift.Primary.Tests** | Services in the PathDrift.Primary console app |
| **PathViewingApp.Tests** | Services and components in the PathViewingApp Blazor app |

### PathDrift.Primary.Tests

| Test File | Service Under Test | What's Tested |
|-----------|--------------------|---------------|
| `CoordinateParserServiceTests.cs` | `CoordinateParserService` | CSV line parsing, header skipping, malformed line handling, null/empty input |
| `FileReaderServiceTests.cs` | `FileReaderService` | Async line-by-line reading, file not found exception, cancellation behaviour |
| `GrpcCoordinateSenderTests.cs` | `GrpcCoordinateSender` | File streaming and gRPC send orchestration |

### PathViewingApp.Tests

| Test File | Service/Component Under Test | What's Tested |
|-----------|------------------------------|---------------|
| `CoordinateStoreTests.cs` | `CoordinateStore` | Adding coordinates, event notification, subscriber error handling, snapshot isolation |
| `CoordinateReceiverServiceTests.cs` | `CoordinateReceiverService` | gRPC stream receiving, empty streams, cancellation handling |
| `PathViewerLogicTests.cs` | `PathViewerLogic` | Axis selection per plane, bounds computation, SVG scaling, polyline generation, axis labels |
| `CoordinateDataTableTests.cs` | `CoordinateDataTable` (Blazor component) | Empty state rendering, table rows, point count badge, number formatting, live updates, column headers |

### Running Tests

```bash
dotnet test
```

Or in Visual Studio: **Test > Run All Tests** (Ctrl+R, A)

## Future Improvements

Future improvements that would enhance the application:

- **3D path visualisation page** — add a dedicated page rendering the path in 3D, allowing rotation and zoom for a more complete spatial view of the drift data
- **Flexible CSV column ordering** — support CSV files where columns appear in a different order by reading the header row and mapping columns by name rather than position
- **Increase unit test coverage** — add more unit tests to bring coverage from 66% up to 100%
- **Use .NET TPL for parallel processing** — leverage the Task Parallel Library to utilise more available CPU when handling large datasets and supporting real-time 2D/3D drawing
- **Extend PathDrift.Primary into an ASP.NET Core app** — replace the console app with a web application that allows users to upload CSV files via a browser, rather than reading from a pre-configured file path

