# CoNSoL-TakeOff

**CoNSoL-TakeOff** is a **visual-first construction take-off and estimation tool** that enables users to draw construction elements and automatically compute quantities, materials, and costs.

> **Drawing is not decoration. Drawing is data input.**

---

## 📋 Overview

### What is CoNSoL-TakeOff?

CoNSoL-TakeOff provides:
- **Visual 2D drawing interface** for construction elements (walls, slabs, rooms, columns)
- **Metadata-driven objects** that carry business meaning (materials, formulas, pricing)
- **Automatic quantity calculation** (D0: count, D1: length, D2: area, D3: volume)
- **Cost aggregation** through Smart Tags and dimension modes
- **Flexible deployment** (standalone desktop or integrated with CoNSoL-Suite)

### Problem Statement

Current construction workflows suffer from:
- Manual spreadsheets prone to errors
- Disconnected CAD tools without business logic
- Time-consuming manual quantity take-off
- Rework across multiple tools during design changes

### Solution Approach

CoNSoL-TakeOff bridges the gap by treating drawing geometry as **first-class business data**, enabling seamless visual estimation with automatic rollups.

---

## 🏗️ Architecture Overview

### Layered Design

```
┌─────────────────────────────────────┐
│       UI Layer (Desktop)            │  WinForms, Canvas, Panels, Tools
├─────────────────────────────────────┤
│     Application Layer               │  Use Cases, Services, Orchestration
├─────────────────────────────────────┤
│       Domain Layer                  │  Entities, Business Logic, Utilities
├─────────────────────────────────────┤
│    Infrastructure Layer             │  Config, Logging, IO, Crypto, JSON
└─────────────────────────────────────┘
```

### Core Components

| Component | Purpose | Documentation |
|-----------|---------|----------------|
| **Domain** | Data entities, geometry utilities, business definitions | [Domain/README.md](Domain/README.md) |
| **Application** | Use case orchestration, calculation engine, services | [Application/README.md](Application/README.md) |
| **Infrastructure** | Configuration, logging, persistence, security | [Infrastructure/README.md](Infrastructure/README.md) |
| **Desktop** | WinForms UI, canvas rendering, tool interaction | [Desktop/README.md](Desktop/README.md) |

---

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 or higher
- Windows 10+ (for WinForms)
- Visual Studio 2022 or Visual Studio Code

### Build & Run

```bash
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run desktop application
dotnet run --project Desktop/Desktop.vbproj
```

### First Drawing

1. Launch the application
2. Select the **Line** tool from the toolbar
3. Click two points on the canvas to create a line
4. Switch to **Rectangle** tool and draw a room
5. Select objects and assign **Smart Tags** (Material, Quantity) in the Properties panel
6. View automatic **quantity calculations** in real-time

---

## 📚 SDLC Documentation

This project follows a structured **Software Development Lifecycle (SDLC)** documented in the **Mega-File.md** library.

### Document Structure (from Mega-File.md)

| Phase | Document | Purpose |
|-------|----------|---------|
| **Inception** | [0101-Requirement_Analysis](Mega-File.md#-0101--requirement-analysis-) | Business vision, scope, success criteria |
| **Inception** | [0102-Planning](Mega-File.md#--0102--planning-) | Roadmap, risks, milestones |
| **Inception** | [0104-SRS](Mega-File.md#-0104--software-requirements-specification-srs) | Functional & non-functional requirements |
| **Design** | [0201-Design_Documentation](Mega-File.md#-0201--design-documentation) | Architecture, system context, components |
| **Design** | [020103-Data_Model](Mega-File.md#-020103--data-model) | Entity relationships, schema, serialization |
| **Design** | [0208-UX_UI_Design](Mega-File.md#-0208--ux--ui-design) | Interaction model, tools, validation rules |
| **Implementation** | [0301-Development_Documentation](Mega-File.md#-0301--development-documentation) | Coding standards, patterns, architecture patterns |
| **Verification** | [0401-Testing_Documentation](Mega-File.md#-0401--testing-documentation) | Test strategy, automation, UAT |
| **Delivery** | [0501-Deployment_Documentation](Mega-File.md#-0501--deployment-documentation) | Release notes, deployment runbooks |

**See [Mega-File.md](Mega-File.md) for complete SDLC documentation library.**

---

## 🎯 Key Concepts

### Dimension Modes

Each drawn object uses exactly **one dimension mode** for quantity calculation:

| Mode | Description | Example |
|------|-------------|---------|
| **D0** | Count | Number of doors, windows |
| **D1** | Length | Wall length (m) |
| **D2** | Area | Floor area (m²) |
| **D3** | Volume | Concrete volume (m³) |

### Nested Objects

Objects can contain other objects:
- **Door** inside **Wall** → reduces wall area
- **Window** inside **Wall** → reduces wall area
- **Opening** inside **Slab** → reduces slab area

The calculation engine automatically handles subtraction.

### Smart Tags & Custom Marks

- **Smart Tags** = data metadata (Material, Quantity, Unit Price) — aggregatable numerically
- **Custom Marks** = visual metadata (Inspection issue, Rework needed) — not aggregated

### Deployment Modes

| Mode | Configuration | Use Case |
|------|---------------|----------|
| **Standalone** | Local SQLite DB, local files | Desktop app, single-user |
| **Integrated** | Shared SQL Server DB, CoNSoL-Suite | Multi-user suite, enterprise |

---

## 📂 Project Structure

```
CoNSoL-TakeOff/
├── Domain/                              # Business logic, entities, utilities
│   ├── Entities/
│   │   ├── CanvasElement.vb            # Shape + metadata container
│   │   ├── CanvasLayout.vb             # Drawing canvas state
│   │   ├── BusinessDefinition.vb       # Material, quantity, pricing info
│   │   ├── BlockModels.vb              # Block/Symbol definitions
│   │   └── ElementRelationship.vb      # Nested object relationships
│   ├── Utilities/
│   │   └── Geometry.vb                 # Geometric calculations
│   └── README.md
│
├── Application/                         # Use case orchestration, services
│   ├── Services/
│   │   ├── TakeOffService.vb           # Quantity aggregation
│   │   └── MaterialService.vb          # Material lookups
│   ├── TakeOffCalculator.vb            # Calculation engine
│   ├── TakeOffContext.vb               # Calculation context
│   ├── TakeOffResult.vb                # Result aggregates
│   └── README.md
│
├── Infrastructure/                      # Cross-cutting concerns
│   ├── Config/
│   │   └── AppConfig.vb                # Configuration management
│   ├── Logging/
│   │   ├── ILogger.vb                  # Logging interface
│   │   └── FileLogger.vb               # File-based logger
│   ├── IO/
│   │   ├── TakeOffFileStore.vb         # File persistence
│   │   └── MaterialJsonStore.vb        # Material JSON storage
│   ├── Crypto/
│   │   ├── CryptoService.vb            # Encryption/decryption
│   │   └── Hashing.vb                  # Hashing utilities
│   ├── Wrappers/
│   │   └── JsonSerializer.vb           # JSON serialization wrapper
│   └── README.md
│
├── Desktop/                             # WinForms UI layer
│   ├── Forms/
│   │   ├── MainForm.vb                 # Main application window
│   │   ├── BlockAssignmentForm.vb      # Block assignment dialog
│   │   └── MaterialCrudForm.vb         # Material management dialog
│   ├── Controls/
│   │   ├── CanvasControl.vb            # 2D drawing canvas
│   │   ├── PropertiesPanel.vb          # Property inspector
│   │   └── LineShape.vb                # Line shape implementation
│   ├── CompositionRoot.vb              # Dependency injection setup
│   ├── Program.vb                      # Entry point
│   ├── ApplicationEvents.vb            # VB app framework events
│   └── README.md
│
├── Mega-File.md                        # SDLC documentation library
└── README.md                           # This file
```

---

## 🔄 Core Workflows

### Drawing & Estimation Flow

```
Setup → Draw → Define → Store → Calculate → Report
```

1. **Setup** - Configure layers, materials, formulas
2. **Draw** - Create shapes using drawing tools
3. **Define** - Assign business meaning (Material, Quantity, Price)
4. **Store** - Save to JSON/database
5. **Calculate** - Compute quantities and costs
6. **Report** - Export aggregation summaries

### Drawing Tool Interaction

```
MouseDown → Capture start point
  ↓
MouseMove → Render rubber-band preview
  ↓
MouseUp → Commit shape object
  ↓
(Escape) → Cancel operation
```

---

## 🧪 Testing

See [0401-Testing_Documentation](Mega-File.md#-0401--testing-documentation) for:
- Test strategy and coverage goals
- Unit test conventions
- Integration test patterns
- UAT procedures

Current test hooks are in place for:
- Geometry calculations
- Quantity aggregations
- Material lookups
- JSON serialization/deserialization

---

## 📡 Observability

### Logging

- **File Logger** writes to `logs/` directory
- **Log Levels**: Debug, Info, Warn, Error
- **Use Case**: Trace user actions, errors, performance metrics

### Configuration

See [Infrastructure/AppConfig.vb](Infrastructure/Config/AppConfig.vb):
- Database connection strings
- Feature flags
- Log file paths
- Deployment mode (Standalone/Integrated)

---

## 🔒 Security Considerations

- **Input Validation** - All geometry and business data validated at the domain layer
- **File Access** - Controlled via `TakeOffFileStore`
- **Encryption** - Optional file encryption via `CryptoService`
- **Hashing** - Passwords/secrets via `Hashing` utilities

See [0202-Security_Documentation](Mega-File.md#-0202--security-documentation) for threat modeling and controls.

---

## 📦 Deployment

### Standalone Mode

```bash
# Build standalone executable
dotnet publish -c Release -o ./publish

# Run application
./publish/Desktop.exe
```

**Features:**
- Local SQLite database
- Single-user
- Offline-first
- Portable executable

### Integrated Mode

Deploy as a module within **CoNSoL-Suite**:
- Shared SQL Server database
- Multi-user environment
- Connected to Project Manager
- Unified licensing

See [0501-Deployment_Documentation](Mega-File.md#-0501--deployment-documentation) for runbooks.

---

## 🤝 Contributing

### Code Standards

- **Language**: Visual Basic .NET (VB 16+)
- **Framework**: .NET 8.0
- **Style**: See [0301-Development_Documentation](Mega-File.md#-0301--development-documentation)
- **Patterns**: Layered architecture, dependency injection, repository pattern

### Branch Workflow

```
master (main) ← develop ← feature/xxx
```

### Commit Message Format

```
[AREA] Short description (50 chars)

Longer explanation (72 chars max per line).
References Mega-File section if applicable.
```

---

## 📞 Support & Contact

| Topic | Reference |
|-------|-----------|
| Requirements Questions | [0101-Requirement_Analysis](Mega-File.md#-0101--requirement-analysis-) |
| Architecture Concerns | [0201-Design_Documentation](Mega-File.md#-0201--design-documentation) |
| Development Standards | [0301-Development_Documentation](Mega-File.md#-0301--development-documentation) |
| UI/UX Issues | [0208-UX_UI_Design](Mega-File.md#-0208--ux--ui-design) |
| Test Strategy | [0401-Testing_Documentation](Mega-File.md#-0401--testing-documentation) |
| Deployment | [0501-Deployment_Documentation](Mega-File.md#-0501--deployment-documentation) |

---

## 📄 License

See LICENSE file for details.

---

## 📝 Version History

| Version | Date | Notes |
|---------|------|-------|
| 1.0.0 | TBD | Initial release |

---

**Last Updated:** 19 April 2026  
**Repository:** E:\Users\GoingIForMal\CoNSoL-TakeOff  
**Master Document:** [Mega-File.md](Mega-File.md)
