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

### Running

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

