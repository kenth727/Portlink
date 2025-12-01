# Portlink - Port Management Information System (PMIS)

A sophisticated **maritime port operations management system** built with ASP.NET Core 8 and Angular 18. Portlink manages vessel tracking, berth allocation, port call scheduling, and features AI-powered decision support through LM Studio integration.

## Table of Contents
- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Project Architecture](#project-architecture)
- [Getting Started](#getting-started)
- [Domain Model](#domain-model)
- [API Endpoints](#api-endpoints)
- [Real-Time Features](#real-time-features)
- [AI Integration](#ai-integration)
- [Authentication & Authorization](#authentication--authorization)
- [Testing](#testing)
- [Configuration](#configuration)

---

## Overview

**Portlink** is an enterprise-grade Port Management Information System that enables port operators to:
- Track vessel movements and statuses
- Manage berth capacity and availability
- Schedule and approve port calls with conflict detection
- Monitor operations in real-time through live dashboards
- Leverage AI for berth recommendations and natural language queries
- Simulate load conditions for stress testing

The system implements a clean three-tier architecture with separated concerns across domain, API, and presentation layers.

---

## Key Features

### Core Operations
- **Vessel Management** - Track vessel details, specifications, and current status
- **Berth Allocation** - Manage dock capacity, availability, and maintenance schedules
- **Port Call Scheduling** - Request, approve, and manage vessel visits with automatic conflict detection
- **Business Validation** - Prevent double-bookings, capacity violations, and scheduling conflicts

### Advanced Features
- **Real-Time Updates** - SignalR broadcasts changes instantly to all connected clients
- **AI Decision Support** - Natural language Q&A and intelligent berth recommendations via LM Studio
- **Load Simulator** - Automated realistic scenario generation for stress testing
- **Health Monitoring** - Database and AI service health checks
- **Structured Logging** - Comprehensive audit trail with Serilog
- **Global Exception Handling** - RFC 7807 ProblemDetails responses

### Security & Access Control
- **JWT Authentication** - Secure token-based authentication
- **Role-Based Authorization** - PortOperator and Viewer roles
- **ASP.NET Core Identity** - Full user management with password hashing

---

## Technology Stack

### Backend
- **ASP.NET Core 8** - Web API framework
- **Entity Framework Core 8** - ORM with PostgreSQL provider
- **PostgreSQL 16** - Production database
- **SignalR** - Real-time bidirectional communication
- **Serilog** - Structured logging to file and console
- **ASP.NET Core Identity** - User authentication and authorization
- **JWT Bearer Tokens** - Stateless authentication
- **Swagger/OpenAPI** - Interactive API documentation

### Frontend
- **Angular 18** - Single-page application framework
- **TypeScript** - Type-safe JavaScript
- **SCSS** - Styled component stylesheets
- **Microsoft SignalR Client** - Real-time dashboard updates
- **RxJS** - Reactive programming with observables
- **HttpClient** - API communication with JWT interceptor

### AI & Integration
- **LM Studio** - Local AI server (port 1234)
- **Qwen 2.5 7B Instruct** - Large language model for AI features
- **Natural Language Processing** - Chat interface and recommendations

### Infrastructure
- **Docker Compose** - PostgreSQL container orchestration
- **xUnit** - Unit testing framework
- **In-Memory Database** - EF Core provider for testing

---

## Project Architecture

### Three-Tier Architecture

```
┌─────────────────────────────────────────────────────┐
│        Portlink Dashboard (Angular 18)              │
│  ┌─────────────────────────────────────────────┐   │
│  │ Components: App, Chat, BerthOverview,       │   │
│  │             PortCallApproval                │   │
│  │ Services: Dashboard, Auth, SignalR Hub      │   │
│  └─────────────────────────────────────────────┘   │
└────────────────┬────────────────────────────────────┘
                 │ HTTP (REST) + SignalR WebSocket
                 │ Port 4200 → 5159
┌────────────────▼────────────────────────────────────┐
│        Portlink.Api (ASP.NET Core 8)                │
│  ┌─────────────────────────────────────────────┐   │
│  │ Controllers: Vessels, Berths, PortCalls,    │   │
│  │              Auth, AI, LoadSimulator        │   │
│  │ SignalR Hub: PortOperationsHub              │   │
│  │ Middleware: Authentication, Exception       │   │
│  │             Handling, CORS                  │   │
│  └─────────────────────────────────────────────┘   │
└────────────────┬────────────────────────────────────┘
                 │ Dependency Injection
┌────────────────▼────────────────────────────────────┐
│        Portlink.Core (Domain Layer)                 │
│  ┌─────────────────────────────────────────────┐   │
│  │ Entities: Vessel, Berth, PortCall,          │   │
│  │           ApplicationUser                   │   │
│  │ Repositories: IVesselRepository,            │   │
│  │               IBerthRepository,             │   │
│  │               IPortCallRepository           │   │
│  │ Services: LMStudioAIService,                │   │
│  │           LoadSimulatorService              │   │
│  │ DbContext: PortlinkDbContext                │   │
│  │ Configurations: Entity Type Configs         │   │
│  │ Exceptions: Business rule violations        │   │
│  └─────────────────────────────────────────────┘   │
└────────────────┬────────────────────────────────────┘
                 │ Entity Framework Core
┌────────────────▼────────────────────────────────────┐
│        PostgreSQL Database (Docker)                 │
│  - Vessels, Berths, PortCalls                       │
│  - AspNetUsers, AspNetRoles (Identity)              │
└─────────────────────────────────────────────────────┘

         External AI Service
┌─────────────────────────────────────────────────────┐
│        LM Studio Server (Optional)                  │
│  http://127.0.0.1:1234                              │
│  - Qwen 2.5 7B Instruct Model                       │
│  - Chat completions API                             │
└─────────────────────────────────────────────────────┘
```

### Layer Responsibilities

#### **Portlink.Core** (Domain Layer)
- **Entities** - Domain models with navigation properties
- **Repositories** - Data access abstraction with business logic
- **Services** - Business services (AI, load simulation)
- **DbContext** - EF Core database context
- **Configurations** - Entity type configurations (indexes, constraints)
- **Exceptions** - Custom business exceptions
- **Initializers** - Data seeding for maritime and auth data

**Key files:**
- [Portlink.Core/Entities/Vessel.cs](Portlink.Core/Entities/Vessel.cs)
- [Portlink.Core/Entities/Berth.cs](Portlink.Core/Entities/Berth.cs)
- [Portlink.Core/Entities/PortCall.cs](Portlink.Core/Entities/PortCall.cs)
- [Portlink.Core/Repositories/IVesselRepository.cs](Portlink.Core/Repositories/IVesselRepository.cs)
- [Portlink.Core/Services/LMStudioAIService.cs](Portlink.Core/Services/LMStudioAIService.cs)
- [Portlink.Core/Data/PortlinkDbContext.cs](Portlink.Core/Data/PortlinkDbContext.cs)

#### **Portlink.Api** (Web API Layer)
- **Controllers** - RESTful API endpoints
- **SignalR Hubs** - Real-time communication
- **Middleware** - Authentication, exception handling, CORS
- **DTOs** - Data transfer objects for API responses
- **Background Services** - Load simulator hosted service

**Key files:**
- [Portlink.Api/Controllers/VesselsController.cs](Portlink.Api/Controllers/VesselsController.cs)
- [Portlink.Api/Controllers/BerthsController.cs](Portlink.Api/Controllers/BerthsController.cs)
- [Portlink.Api/Controllers/PortCallsController.cs](Portlink.Api/Controllers/PortCallsController.cs)
- [Portlink.Api/Controllers/AIController.cs](Portlink.Api/Controllers/AIController.cs)
- [Portlink.Api/Controllers/AuthController.cs](Portlink.Api/Controllers/AuthController.cs)
- [Portlink.Api/Hubs/PortOperationsHub.cs](Portlink.Api/Hubs/PortOperationsHub.cs)
- [Portlink.Api/Middleware/GlobalExceptionHandlerMiddleware.cs](Portlink.Api/Middleware/GlobalExceptionHandlerMiddleware.cs)
- [Portlink.Api/Services/LoadSimulatorService.cs](Portlink.Api/Services/LoadSimulatorService.cs)

#### **portlink-dashboard** (Angular Frontend)
- **Components** - UI components with templates and styles
- **Services** - HTTP clients, SignalR hub, authentication
- **Interceptors** - JWT token injection
- **Models** - TypeScript interfaces matching API DTOs

**Key files:**
- [portlink-dashboard/src/app/app.component.ts](portlink-dashboard/src/app/app.component.ts)
- [portlink-dashboard/src/app/chat/chat.component.ts](portlink-dashboard/src/app/chat/chat.component.ts)
- [portlink-dashboard/src/app/berth-overview/berth-overview.component.ts](portlink-dashboard/src/app/berth-overview/berth-overview.component.ts)
- [portlink-dashboard/src/app/port-call-approval/port-call-approval.component.ts](portlink-dashboard/src/app/port-call-approval/port-call-approval.component.ts)
- [portlink-dashboard/src/app/services/dashboard.service.ts](portlink-dashboard/src/app/services/dashboard.service.ts)
- [portlink-dashboard/src/app/services/port-operations-hub.service.ts](portlink-dashboard/src/app/services/port-operations-hub.service.ts)

#### **Portlink.Tests** (Unit Tests)
- **ApiEndpointTests** - Controller and service tests
- **Test Infrastructure** - In-memory database, mocks

**Key files:**
- [Portlink.Tests/ApiEndpointTests.cs](Portlink.Tests/ApiEndpointTests.cs)

---

## Getting Started

### Prerequisites

1. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js 18+** - [Download here](https://nodejs.org/)
3. **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop)
4. **LM Studio** (Optional) - [Download here](https://lmstudio.ai/) for AI features
5. **Git** - For version control

### Quick Start

#### 1. Clone the Repository
```bash
git clone https://github.com/kenth727/Portlink
cd EmployeeWebApplication
```

#### 2. Start PostgreSQL Database
```bash
docker-compose -f docker-compose.dev.yml up -d
```

This starts PostgreSQL 16 on port 5432 with:
- Database: `portlink`
- User: `portadmin`
- Password: `Port@Dev2024`

#### 3. Apply Database Migrations
```bash
dotnet ef database update --project Portlink.Core --startup-project Portlink.Api
```

This creates the database schema and seeds initial data:
- 5 berths (container, bulk, tanker, RoRo terminals)
- 5 vessels (container ships, tanker, bulk carrier, cruise ship)
- 4 sample port calls
- 2 users (admin@portlink.com / Admin123!, viewer@portlink.com / Viewer123!)

#### 4. Start the Backend API
```bash
cd Portlink.Api
dotnet run
```

The API will start on:
- **HTTP**: http://localhost:5159
- **Swagger UI**: http://localhost:5159/swagger
- **SignalR Hub**: http://localhost:5159/hubs/port-operations

#### 5. Start the Frontend Dashboard
```bash
cd portlink-dashboard
npm install
npm start
```

The dashboard will open at:
- **Angular App**: http://localhost:4200

#### 6. (Optional) Start LM Studio for AI Features
1. Download and install LM Studio
2. Download the **Qwen 2.5 7B Instruct** model
3. Start the local server on port 1234
4. AI chat and berth recommendations will now work in the dashboard

### Login Credentials

| Email | Password | Role | Access |
|-------|----------|------|--------|
| admin@portlink.com | Admin123! | PortOperator | Full access (CRUD, AI, Load Simulator) |
| viewer@portlink.com | Viewer123! | Viewer | Read-only access |

---

## Domain Model

### Core Entities

#### **Vessel** [Portlink.Core/Entities/Vessel.cs](Portlink.Core/Entities/Vessel.cs)
Represents maritime vessels visiting the port.

**Properties:**
- `Id` (int) - Primary key
- `Name` (string) - Vessel name
- `ImoNumber` (string) - Unique International Maritime Organization number
- `VesselType` (enum) - Container, Tanker, BulkCarrier, RoRo, Cruise, GeneralCargo, Reefer
- `FlagCountry` (string) - Country of registration
- `LengthOverall` (decimal) - Vessel length in meters
- `Beam` (decimal) - Vessel width in meters
- `Draft` (decimal) - Vessel depth in water in meters
- `CargoType` (string) - Type of cargo carried
- `Capacity` (decimal) - Cargo capacity (TEU, tons, passengers, etc.)
- `Status` (enum) - Approaching, Anchored, Docked, LoadingUnloading, Departed, UnderMaintenance
- `OwnerCompany` (string) - Vessel owner
- `AgentEmail` (string) - Shipping agent contact

**Relationships:**
- `PortCalls` - One-to-many with PortCall (history of visits)

**Business Rules:**
- IMO number must be unique
- Dimensions must be positive values
- Status transitions managed through port call updates

#### **Berth** [Portlink.Core/Entities/Berth.cs](Portlink.Core/Entities/Berth.cs)
Represents docking berths in the port.

**Properties:**
- `Id` (int) - Primary key
- `BerthCode` (string) - Unique berth identifier (e.g., "TERM-A-01")
- `TerminalName` (string) - Terminal name
- `MaxVesselLength` (decimal) - Maximum vessel length in meters
- `MaxDraft` (decimal) - Maximum draft in meters
- `Facilities` (string) - Available facilities (cranes, fuel, water, power)
- `Status` (enum) - Available, Occupied, Reserved, UnderMaintenance, Closed

**Relationships:**
- `PortCalls` - One-to-many with PortCall (booking history)

**Business Rules:**
- Berth code must be unique
- Cannot assign vessels exceeding capacity constraints
- Berths under maintenance cannot accept new port calls

#### **PortCall** [Portlink.Core/Entities/PortCall.cs](Portlink.Core/Entities/PortCall.cs)
Represents a scheduled or active vessel visit to a berth.

**Properties:**
- `Id` (int) - Primary key
- `VesselId` (int) - Foreign key to Vessel
- `BerthId` (int) - Foreign key to Berth
- `EstimatedTimeOfArrival` (DateTime) - ETA
- `EstimatedTimeOfDeparture` (DateTime) - ETD
- `ActualTimeOfArrival` (DateTime?) - Actual arrival (nullable)
- `ActualTimeOfDeparture` (DateTime?) - Actual departure (nullable)
- `Status` (enum) - Scheduled, Approaching, Arrived, Berthed, InProgress, Completed, Cancelled, Delayed
- `CargoDescription` (string) - Cargo details
- `CargoQuantity` (decimal?) - Cargo amount
- `CargoUnit` (string) - Unit (TEU, tons, units, passengers)
- `DelayReason` (string) - Reason if delayed
- `PriorityLevel` (int) - Priority 1-5
- `Notes` (string) - Additional notes

**Relationships:**
- `Vessel` - Many-to-one with Vessel
- `Berth` - Many-to-one with Berth

**Business Rules:**
- Vessel must exist and fit berth dimensions (length, draft)
- Berth must not be under maintenance
- ETA must be before ETD
- **Conflict Detection**:
  - For `Scheduled` status: Only blocks if conflicting with Approaching/Arrived/Berthed/InProgress calls
  - For other statuses: Blocks all non-completed/cancelled overlapping calls
- Status `Scheduled` = Request pending approval
- Status `Approaching` = Approved and en route

#### **ApplicationUser** [Portlink.Core/Entities/ApplicationUser.cs](Portlink.Core/Entities/ApplicationUser.cs)
Extends ASP.NET Core IdentityUser for authentication.

**Additional Properties:**
- `FullName` (string) - User's full name
- `CreatedAt` (DateTime) - Account creation date

**Roles:**
- `PortOperator` - Full access to all features
- `Viewer` - Read-only access

### Status Enumerations

**VesselStatus:**
- `Approaching` - Vessel heading to port
- `Anchored` - Vessel waiting at anchorage
- `Docked` - Vessel at berth
- `LoadingUnloading` - Cargo operations in progress
- `Departed` - Vessel has left port
- `UnderMaintenance` - Vessel in maintenance

**BerthStatus:**
- `Available` - Ready for use
- `Occupied` - Currently in use
- `Reserved` - Booked for future use
- `UnderMaintenance` - Not operational
- `Closed` - Temporarily closed

**PortCallStatus:**
- `Scheduled` - Pending approval (request)
- `Approaching` - Approved and en route
- `Arrived` - At anchorage/pilot station
- `Berthed` - Docked at berth
- `InProgress` - Operations ongoing
- `Completed` - Operations finished
- `Cancelled` - Port call cancelled
- `Delayed` - Behind schedule

---

## API Endpoints

Base URL: `http://localhost:5159`

### Authentication Endpoints

#### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "fullName": "John Doe"
}
```

**Response:** 200 OK
```json
{
  "message": "User registered successfully"
}
```

#### POST /api/auth/login
Login and receive JWT token.

**Request Body:**
```json
{
  "email": "admin@portlink.com",
  "password": "Admin123!"
}
```

**Response:** 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-12-01T08:00:00Z",
  "role": "PortOperator",
  "fullName": "Admin User"
}
```

#### GET /api/auth/me
Get current user information (requires authentication).

**Response:** 200 OK
```json
{
  "id": "user-guid",
  "email": "admin@portlink.com",
  "fullName": "Admin User",
  "role": "PortOperator"
}
```

### Vessel Endpoints

All vessel endpoints require authentication (`[Authorize]`).

#### GET /api/vessels
Get all vessels with optional filtering and pagination.

**Query Parameters:**
- `status` (optional) - Filter by VesselStatus
- `vesselType` (optional) - Filter by VesselType
- `pageNumber` (default: 1) - Page number
- `pageSize` (default: 10) - Items per page

**Response:** 200 OK
```json
{
  "vessels": [
    {
      "id": 1,
      "name": "MSC Oscar",
      "imoNumber": "9703291",
      "vesselType": "Container",
      "lengthOverall": 395.4,
      "beam": 59.0,
      "draft": 16.0,
      "status": "Docked"
    }
  ],
  "totalCount": 15
}
```

#### GET /api/vessels/{id}
Get vessel by ID with port call history.

**Response:** 200 OK or 404 Not Found

#### POST /api/vessels
Create a new vessel.

**Request Body:**
```json
{
  "name": "New Vessel",
  "imoNumber": "1234567",
  "vesselType": "Container",
  "flagCountry": "Panama",
  "lengthOverall": 350.0,
  "beam": 48.0,
  "draft": 14.5,
  "cargoType": "Containers",
  "capacity": 15000,
  "status": "Approaching",
  "ownerCompany": "Shipping Co",
  "agentEmail": "agent@example.com"
}
```

**Response:** 201 Created with Location header

#### PUT /api/vessels/{id}
Update an existing vessel.

**Response:** 204 No Content or 404 Not Found

#### DELETE /api/vessels/{id}
Delete a vessel (cascades to related port calls).

**Response:** 204 No Content or 404 Not Found

### Berth Endpoints

All berth endpoints require authentication.

#### GET /api/berths
Get all berths with optional filtering.

**Query Parameters:**
- `status` (optional) - Filter by BerthStatus
- `pageNumber` (default: 1)
- `pageSize` (default: 10)

#### GET /api/berths/available
Get only available berths.

#### GET /api/berths/{id}
Get berth by ID.

#### POST /api/berths
Create a new berth.

**Request Body:**
```json
{
  "berthCode": "TERM-E-01",
  "terminalName": "Terminal E",
  "maxVesselLength": 400.0,
  "maxDraft": 18.0,
  "facilities": "2x STS Cranes, Fuel, Fresh Water, Shore Power",
  "status": "Available"
}
```

#### PUT /api/berths/{id}
Update an existing berth.

#### DELETE /api/berths/{id}
Delete a berth.

### Port Call Endpoints

All port call endpoints require authentication.

#### GET /api/portcalls
Get all port calls with filtering.

**Query Parameters:**
- `status` (optional) - Filter by PortCallStatus
- `pageNumber` (default: 1)
- `pageSize` (default: 10)

**Response includes vessel and berth details in navigation properties.**

#### GET /api/portcalls/upcoming
Get upcoming port calls (future arrivals).

**Query Parameters:**
- `hours` (default: 24) - Look ahead window
- `limit` (default: 10) - Maximum results

#### GET /api/portcalls/active
Get active port calls (Berthed or InProgress status).

#### GET /api/portcalls/{id}
Get port call by ID.

#### POST /api/portcalls
Create a new port call (request approval).

**Request Body:**
```json
{
  "vesselId": 1,
  "berthId": 2,
  "estimatedTimeOfArrival": "2025-12-01T08:00:00Z",
  "estimatedTimeOfDeparture": "2025-12-01T20:00:00Z",
  "status": "Scheduled",
  "cargoDescription": "Mixed Containers",
  "cargoQuantity": 5000,
  "cargoUnit": "TEU",
  "priorityLevel": 3,
  "notes": "Refrigerated containers require shore power"
}
```

**Validation:**
- Vessel exists and fits berth (length, draft)
- Berth not under maintenance
- No overlapping port calls (context-sensitive)
- ETA < ETD

**Response:** 201 Created or 400/404/409 with business exception details

#### PUT /api/portcalls/{id}
Update a port call (e.g., approve by changing status to "Approaching").

#### DELETE /api/portcalls/{id}
Delete/cancel a port call.

### AI Endpoints

AI endpoints require `PortOperator` role.

#### POST /api/ai/chat
Ask natural language questions about port operations.

**Request Body:**
```json
{
  "question": "What vessels are currently at berth?"
}
```

**Response:** 200 OK
```json
{
  "answer": "Currently, there are 3 vessels at berth: MSC Oscar at TERM-A-01 (Container terminal), TI Europe at TERM-C-01 (Tanker terminal), and Harmony of the Seas at TERM-D-01 (RoRo terminal)."
}
```

**Context provided to AI:**
- All current vessels with status
- All berths with status and current occupants
- Active port calls (Berthed/InProgress)
- Pending requests (Scheduled)
- Approved upcoming arrivals (Approaching)

#### GET /api/ai/recommend-berth/{vesselId}
Get AI recommendation for best berth for a vessel.

**Response:** 200 OK
```json
{
  "recommendation": "For the MSC Oscar (Container vessel, 395m length, 16m draft), I recommend TERM-A-01. This container terminal can accommodate vessels up to 400m with 16m draft and has specialized container handling facilities including STS cranes."
}
```

**Analysis considers:**
- Vessel type matching terminal specialization
- Dimension constraints (length, draft)
- Available facilities
- Current berth availability

#### GET /api/ai/health
Check if LM Studio AI service is available.

**Response:** 200 OK or 503 Service Unavailable
```json
{
  "isAvailable": true
}
```

### Load Simulator Endpoints

Load simulator endpoints require authentication.

#### POST /api/load-simulator/start
Start generating realistic port call scenarios.

**Query Parameters:**
- `operationsPerSecond` (default: 1) - Generation rate

**Response:** 200 OK
```json
{
  "message": "Load simulator started"
}
```

**Behavior:**
- Generates new vessel every 5 seconds
- Sizes vessels to fit available berths (50-90% of max)
- Assigns appropriate cargo types
- Schedules port calls avoiding conflicts
- Broadcasts all changes via SignalR

#### POST /api/load-simulator/stop
Stop the load simulator.

**Response:** 200 OK
```json
{
  "message": "Load simulator stopped"
}
```

### Health Check Endpoint

#### GET /health
Check system health (database, AI service).

**Response:** 200 Healthy or 503 Unhealthy

---

## Real-Time Features

### SignalR Hub

**Endpoint:** `ws://localhost:5159/hubs/port-operations`

**Authentication:** JWT token via query string:
```
ws://localhost:5159/hubs/port-operations?access_token={token}
```

### Events Broadcast

The PortOperationsHub broadcasts these events to all connected clients:

| Event | Payload | Trigger |
|-------|---------|---------|
| `VesselChanged` | Vessel object | Vessel created or updated |
| `VesselDeleted` | Vessel ID | Vessel deleted |
| `BerthChanged` | Berth object | Berth created or updated |
| `BerthDeleted` | Berth ID | Berth deleted |
| `PortCallChanged` | PortCall object | Port call created or updated |
| `PortCallDeleted` | PortCall ID | Port call deleted |
| `LoadSimulatorMetrics` | Metrics object | Load simulator status update |

### Angular SignalR Integration

The dashboard uses [PortOperationsHubService](portlink-dashboard/src/app/services/port-operations-hub.service.ts) to:
- Automatically connect on app initialization
- Handle reconnection on connection loss
- Expose observable streams for each event type
- Run callbacks in NgZone for change detection
- Provide connection status monitoring

**Usage in components:**
```typescript
constructor(private hubService: PortOperationsHubService) {}

ngOnInit() {
  this.hubService.vesselChanged$.subscribe(vessel => {
    console.log('Vessel updated:', vessel);
    this.refreshVessels();
  });
}
```

---

## AI Integration

### LM Studio Configuration

**Service:** [LMStudioAIService](Portlink.Core/Services/LMStudioAIService.cs)

**Endpoint:** `http://127.0.0.1:1234/v1/chat/completions`

**Model:** Qwen 2.5 7B Instruct

**Parameters:**
- Temperature: 0.8
- Max Tokens: 4096
- Timeout: 30 seconds

### AI Capabilities

#### 1. Natural Language Q&A
Ask questions about current port status:
- "What vessels are currently docked?"
- "Which berths are available?"
- "How many pending port call requests do we have?"
- "What's the status of MSC Oscar?"

The AI receives full context including:
- All vessels with current status
- All berths with occupancy
- Active operations (Berthed/InProgress)
- Pending requests (Scheduled)
- Approved arrivals (Approaching)

#### 2. Berth Recommendations
Given a vessel ID, recommends the best berth considering:
- Vessel type and terminal specialization
- Dimension constraints (length, draft)
- Available facilities (cranes, fuel, shore power)
- Current availability

#### 3. Semantic Status Understanding
The AI is trained to understand port call status semantics:
- **Scheduled** = Pending request (not yet approved)
- **Approaching** = Approved and en route
- **Berthed/InProgress** = Active operations

### Installation Guide

1. Download LM Studio from https://lmstudio.ai/
2. Install and launch LM Studio
3. Download **Qwen 2.5 7B Instruct** model from the model library
4. Go to Local Server tab
5. Select Qwen 2.5 7B Instruct model
6. Start server on port 1234
7. Restart Portlink API to detect AI service

### Fallback Behavior

When LM Studio is not available:
- AI endpoints return 503 Service Unavailable
- Chat component shows "AI service unavailable" message
- Other features continue to work normally
- Health check reports AI service status

---

## Authentication & Authorization

### JWT Configuration

**Settings** (appsettings.json):
```json
{
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long",
    "Issuer": "PortLinkAPI",
    "Audience": "PortLinkClients",
    "ExpirationMinutes": 480
  }
}
```

**Token Contents:**
- **NameIdentifier** - User ID
- **Email** - User email
- **Name** - Full name
- **Role** - User role(s)

**Expiration:** 8 hours (480 minutes)

### Authorization Policies

**Policies defined in Program.cs:**
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PortOperator", policy =>
        policy.RequireRole("PortOperator"));
    options.AddPolicy("Viewer", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("PortOperator") ||
            context.User.IsInRole("Viewer")));
});
```

### Role-Based Access

| Role | Access Level | Capabilities |
|------|--------------|--------------|
| **PortOperator** | Full Access | - All CRUD operations<br>- AI chat and recommendations<br>- Load simulator control<br>- Port call approval |
| **Viewer** | Read Only | - View vessels, berths, port calls<br>- View dashboard metrics<br>- No create/update/delete<br>- No AI or simulator access |

### Controller Authorization

**Examples from controllers:**
```csharp
[Authorize] // Any authenticated user
public class VesselsController : ControllerBase { }

[Authorize(Policy = "PortOperator")] // PortOperator role required
public class AIController : ControllerBase { }
```

### Frontend Authentication

**AuthService** ([auth.service.ts](portlink-dashboard/src/app/services/auth.service.ts)):
- Stores JWT token in localStorage
- Provides login/logout methods
- HTTP interceptor adds Authorization header
- Exposes `currentUser$` observable

**HTTP Interceptor:**
```typescript
intercept(req: HttpRequest<any>, next: HttpHandler) {
  const token = this.authService.getToken();
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next.handle(req);
}
```

### Default Users

Seeded on first database initialization:

| Email | Password | Role | Purpose |
|-------|----------|------|---------|
| admin@portlink.com | Admin123! | PortOperator | Full system administration |
| viewer@portlink.com | Viewer123! | Viewer | Read-only monitoring |

**Change passwords in production!**

---

## Testing

### Test Project Structure

**Portlink.Tests** (.NET 9 test project targeting .NET 8 libraries)

**Framework:** xUnit

**Test Infrastructure:**
- **In-Memory Database** - EF Core InMemory provider
- **Mock Services** - TestAiService, NoopPortOperationsHubContext
- **Test Data Seeding** - Representative vessels, berths, port calls

### Test Cases

**File:** [Portlink.Tests/ApiEndpointTests.cs](Portlink.Tests/ApiEndpointTests.cs)

#### Vessels_GetAll_ReturnsPagedResult
Tests pagination functionality:
- Seeds 3 vessels
- Requests page 1 with page size 2
- Verifies exactly 2 vessels returned
- Verifies totalCount is 3

#### AI_Chat_UsesServiceAndReturnsAnswer
Tests AI integration:
- Uses mocked IAIService
- Posts question to /api/ai/chat
- Verifies service method called with question
- Verifies mocked answer returned

#### LoadSimulator_Start_ReturnsOkResult
Tests load simulator controller:
- Posts to /api/load-simulator/start
- Verifies 200 OK response
- Verifies success message

#### Maritime_Manual_Post_DoesNotThrowAndCreatesPortCall
Tests MVC controller:
- Posts form data to /Maritime/Manual
- Verifies no exceptions thrown
- Verifies redirect response
- Verifies port call created in database

### Running Tests

```bash
cd Portlink.Tests
dotnet test
```

**Output:**
```
Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4
```

### Test Database

Tests use EF Core InMemory database provider:
```csharp
services.AddDbContext<PortlinkDbContext>(options =>
    options.UseInMemoryDatabase("PortlinkTestDb"));
```

**Benefits:**
- Fast execution (no I/O)
- No external dependencies
- Clean slate for each test
- No cleanup required

---

## Configuration

### Connection Strings

**Priority order:**
1. Environment variable: `PORTLINK_CONNECTION` (preferred)
2. Environment variable: `APPRENTICEAPP_CONNECTION` (legacy, still supported)
3. appsettings.json: `ConnectionStrings:DefaultConnection`
4. Hardcoded fallback (not recommended for production)

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=portlink;Username=portadmin;Password=Port@Dev2024"
  }
}
```

**Environment variable (Windows):**
```cmd
set PORTLINK_CONNECTION=Host=localhost;Port=5432;Database=portlink;Username=portadmin;Password=Port@Dev2024
```

**Environment variable (Linux/Mac):**
```bash
export PORTLINK_CONNECTION="Host=localhost;Port=5432;Database=portlink;Username=portadmin;Password=Port@Dev2024"
```

### Application Settings

**appsettings.json structure:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long",
    "Issuer": "PortLinkAPI",
    "Audience": "PortLinkClients",
    "ExpirationMinutes": 480
  },
  "AllowedHosts": "*"
}
```

### CORS Configuration

**Configured in Program.cs:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevServer",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});
```

**Applied to app:**
```csharp
app.UseCors("AllowAngularDevServer");
```

### Logging Configuration

**Serilog** configured for:
- Console output (structured JSON in production)
- File output: `Portlink.Api/logs/portlink-{Date}.log`
- Rolling daily log files
- Minimum level: Information
- Includes request tracing

**Sample log entry:**
```json
{
  "@t": "2025-11-30T12:34:56.7890123Z",
  "@mt": "HTTP {RequestMethod} {RequestPath} responded {StatusCode}",
  "RequestMethod": "GET",
  "RequestPath": "/api/vessels",
  "StatusCode": 200,
  "Elapsed": 45.3
}
```

### Docker Compose

**File:** [docker-compose.dev.yml](docker-compose.dev.yml)

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: portlink-postgres-dev
    environment:
      POSTGRES_DB: portlink
      POSTGRES_USER: portadmin
      POSTGRES_PASSWORD: Port@Dev2024
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U portadmin -d portlink"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres-data:
```

**Commands:**
```bash
# Start database
docker-compose -f docker-compose.dev.yml up -d

# Stop database
docker-compose -f docker-compose.dev.yml down

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Remove volumes (wipes data)
docker-compose -f docker-compose.dev.yml down -v
```

---

## Project Evolution

### Background

This project was originally an apprentice management system and has been transformed into a maritime port operations system to demonstrate:
- Domain-driven design
- Maritime industry knowledge
- Full-stack development skills
- Real-time application architecture

### Migration Evidence

Some artifacts remain from the original codebase:
- Namespace: `PortlinkApp` for backend and frontend code
- DbContext: `PortlinkDbContext`
- Database: Schema completely replaced (Vessel, Berth, PortCall)
- API: Fully reimplemented for maritime domain
- Frontend: Completely rebuilt for port operations

### Purpose

Developed as a portfolio project demonstrating expertise in:
- **Maritime Domain**: Vessel management, berth allocation, port call scheduling
- **Modern Technologies**: .NET 8, Angular 18, SignalR, PostgreSQL
- **AI Integration**: LM Studio with local LLM
- **Architecture**: Clean three-tier architecture with separation of concerns
- **Real-Time Systems**: SignalR for live operational updates
- **Security**: JWT authentication, role-based authorization
- **Testing**: xUnit with in-memory database

---

## API Documentation

### Swagger UI

Interactive API documentation available at:
- **Development**: http://localhost:5159/swagger
- **Swagger JSON**: http://localhost:5159/swagger/v1/swagger.json

### Authentication in Swagger

1. Click "Authorize" button in Swagger UI
2. Login via `/api/auth/login` endpoint to get token
3. Enter token in format: `Bearer {token}`
4. Click "Authorize"
5. All subsequent requests will include authentication

### Sample Requests

**File:** [Portlink.Api/Portlink.Api.http](Portlink.Api/Portlink.Api.http)

Contains sample HTTP requests for all endpoints:
- Authentication (register, login)
- Vessel CRUD operations
- Berth management
- Port call scheduling
- AI chat and recommendations
- Load simulator control

**Usage with VS Code REST Client extension:**
1. Install REST Client extension
2. Open Portlink.Api.http
3. Click "Send Request" above each request
4. View response in side panel

---

## Development Workflow

### Database Migrations

**Create new migration:**
```bash
dotnet ef migrations add MigrationName --project Portlink.Core --startup-project Portlink.Api
```

**Apply migrations:**
```bash
dotnet ef database update --project Portlink.Core --startup-project Portlink.Api
```

**Rollback migration:**
```bash
dotnet ef database update PreviousMigrationName --project Portlink.Core --startup-project Portlink.Api
```

**Remove last migration:**
```bash
dotnet ef migrations remove --project Portlink.Core --startup-project Portlink.Api
```

### Angular Development

**Generate component:**
```bash
cd portlink-dashboard
ng generate component component-name --standalone
```

**Generate service:**
```bash
ng generate service services/service-name
```

**Build for production:**
```bash
ng build --configuration production
```

### Running in Development

**Backend with hot reload:**
```bash
cd Portlink.Api
dotnet watch run
```

**Frontend with live reload:**
```bash
cd portlink-dashboard
npm start
```

**Run tests with watch:**
```bash
cd Portlink.Tests
dotnet watch test
```

---

## Troubleshooting

### Database Connection Issues

**Error:** "Could not connect to PostgreSQL"

**Solutions:**
1. Verify Docker container is running: `docker ps`
2. Check connection string in appsettings.json
3. Verify port 5432 is not blocked
4. Check Docker logs: `docker-compose -f docker-compose.dev.yml logs postgres`

### AI Service Unavailable

**Error:** "AI service is not available"

**Solutions:**
1. Verify LM Studio is running on port 1234
2. Check model is loaded (Qwen 2.5 7B Instruct)
3. Test endpoint: `curl http://127.0.0.1:1234/v1/models`
4. Check firewall settings for localhost

### SignalR Connection Failed

**Error:** "Failed to connect to SignalR hub"

**Solutions:**
1. Verify API is running on port 5159
2. Check CORS policy includes your origin
3. Verify JWT token is valid and not expired
4. Check browser console for detailed error
5. Verify WebSocket support in browser

### Port Already in Use

**Error:** "Address already in use"

**Solutions:**
1. Change port in Program.cs (backend) or angular.json (frontend)
2. Kill process using port:
   - Windows: `netstat -ano | findstr :5159` then `taskkill /PID <PID> /F`
   - Linux/Mac: `lsof -ti:5159 | xargs kill`

### Entity Framework Errors

**Error:** "No migrations found"

**Solution:**
```bash
# Ensure migrations exist
dotnet ef migrations list --project Portlink.Core --startup-project Portlink.Api

# If none exist, create initial migration
dotnet ef migrations add InitialCreate --project Portlink.Core --startup-project Portlink.Api
dotnet ef database update --project Portlink.Core --startup-project Portlink.Api
```

---

## License

This project is for portfolio and demonstration purposes.

---

## Contact

For questions or collaboration opportunities, please reach out via GitHub.

---

## Acknowledgments

- **Wärtsilä** - Inspiration for port management domain
- **LM Studio** - Local AI inference platform
- **ASP.NET Core Team** - Excellent web framework
- **Angular Team** - Modern frontend framework
- **PostgreSQL** - Robust open-source database
