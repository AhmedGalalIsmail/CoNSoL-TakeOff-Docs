---
doc_id: 201
phase: design
owner: architecture
status: draft
last_updated: 2026-01-10
tags:
  - sdlc/design
  - architecture
color: "#e75321"
---
# 📐 0201 — Design Documentation ^0201

---

## 🎯 Purpose ^0201P

Define the system architecture, design decisions, and technical structure for the CoNSoL platform and CoNSoL-TakeOff application.

This document acts as the **single source of truth** for:
- System structure
- Component responsibilities
- Data flow
- Integration boundaries

---

### 📥 Inputs  ^0201PIN

- [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0101-Requirement_Analysis|0101-Requirement_Analysis]]
- [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0102-Planning|0102-Planning]]
- Business workflows
- Product vision

---

### 📤 Outputs ^0201POUT

- Architecture diagrams
- Component definitions
- Data model
- Integration contracts

---

## 🏛️ 1. System Overview ^020101

### 1.1 Platform Structure ^0201011

```txt
CoNSoL-Suite
├── Core: CoNSoL-Engine
├── Mandatory: Project Manager
├── Modules:
│   ├── TakeOff
│   ├── HR
│   └── Docs
```

---
### 1.2 Architectural Style ^0201012

- Modular (Hub-and-Spoke)
- Layered Architecture
- Plugin/Extensible system
- Metadata-driven design

---

### 1.3 Deployment Modes ^0201012

#### ✅ Standalone Mode

- Local database (SQLite)
- Single-user
- Offline-first

#### ✅ Integrated Mode

- Shared database
- Connected to CoNSoL-Engine
- Multi-user environment

---

🔗 Related:

- [[CoNSoL-Documents-Library-V2/MegaFile/02-Design/0205-Architecture Decision Records-ADR|0205-Architecture_Decision_Records_ADR]]

---

## 🌍 2. System Context ^020102

### 2.1 Actors ^0201021

- End User (Engineer, Estimator)
- Admin (Licensing & Deployment)
- External Systems (Future APIs)

---

### 2.2 External Dependencies ^0201022

- Database systems (SQLite / SQL Server)
- OS rendering APIs
- File system

---

### 2.3 Trust Boundaries ^0201023

- Local execution vs shared environment
- File-based vs DB-based storage

---

## 🧩 3. Core Architecture Components ^020103

### 3.1 High-Level Layers ^0201031

```txt
Presentation Layer
↓
Application Layer
↓
Domain (Business Logic)
↓
Data Access Layer
↓
```

---

### 3.2 Component Breakdown ^0201032

#### 🎨 3.2.1 UI Layer ^02010321

- Canvas (drawing surface)
- Property Panel
- Layer Panel
- Toolbars

---

#### 🧠 3.2.2 Application Layer ^02010322

- Orchestration logic
- Workflow handling
- User interaction management

---

#### ⚙️ 3.2.3 Domain Layer (Core Engine) ^02010323

- Drawing Engine
- Calculation Engine
- Tag Engine
- Mark Engine
- Layer Service

---

#### 💾 3.2.4 Data Layer ^02010324

- JSON serialization
- Database access (Repository pattern)

---

🔗 Related:

- [[0301-Development_Documentation]]

---

## 🧱 4. Drawing Engine Architecture ^020104

### 4.1 Drawing Engine as Reusable Component ^0201041

The Drawing Engine is implemented as a UI‑agnostic core component.
It exposes:
	
- Geometry models
- Interaction lifecycle
- Validation rules
- Command stack

Host applications (Desktop/Web) consume the engine via adapters
responsible only for rendering and event forwarding.

### 4.2 Core Abstractions ^0201042

#### Shape Model ^020104421

- Geometry (visual)
- Business metadata (logical)

---

#### Shape Hierarchy422

```
Shape (Base)
├── Line
├── Rectangle
├── Circle
├── Polyline
├── Text
├── Symbol
```

---
### 4.3 Interaction Model ^02010423

- MouseDown → Capture start point
- MouseMove → Render preview (rubber-band)
- MouseUp → Commit object

---

### 4.4 Coordinate System ^02010424

- Logical coordinates (units)
- Physical coordinates (pixels)
- ScaleFactor controls mapping

---

## 🔄 5. Workflow Design ^020105

### 5.1 Core User Flow ^0201051

```mermaid
flowchart LR  
A[Setup] --> B[Draw]  
B --> C[Define]  
C --> D[Store]  
D --> E[Calculate]  
E --> F[Report]  
```

---

### 5.2 Definition Stage ^0201052

- Assign shape → block/material
- Select dimension mode (D0–D3)
- Attach formulas
- Handle nested objects

---

## 🧮 6. Calculation Architecture ^020106

### 6.1 Dimension Modes ^0201061

|Mode|Description|
|---|---|
|D0|Count|
|D1|Length|
|D2|Area|
|D3|Volume|

---

### 6.2 Calculation Engine Responsibilities ^0201062

- Compute quantities
- Apply formulas
- Aggregate materials
- Calculate cost

---

### 6.3 Nested Objects Handling ^0201063

- Child objects subtract from parent
- Example:
    - Door inside wall → reduces area

---

🔗 Related:

- [[0301-Development_Documentation]]
- [[0401-Testing_Documentation]]

---

## 🗄️ 7. Data Design (High-Level) ^020107

### 7.1 Data Types ^0201071

- Geometry data → shapes
- Business data → materials, blocks
- Metadata → tags, marks

---

### 7.2 Storage Strategy ^0201072

- JSON for canvas state
- Database for persistence
- Hybrid approach

---

🔗 Detailed model:

- [[020103-Data_Model]]

---

## 🔌 8. Integration Design ^020108

### 8.1 Internal Integration ^0201081

- Engine ↔ UI components
- Calculation ↔ Data model

---

### 8.2 External Integration (Future) ^0201082

- REST APIs
- File import/export
- Reporting tools

---

### 8.3 Plugin Architecture ^0201083

- Smart Tags extension
- Custom Marks extension
- Symbol libraries

---

## 🌐 9. Deployment Architecture ^020109

### 9.1 Standalone ^0201091

- Executable application
- Embedded DB
- Local storage

---

### 9.2 Integrated ^0201092

- Hosted within CoNSoL Engine
- Shared DB
- Multi-module communication

---

🔗 Related:

- [[0501-Deployment_Documentation]]

---

## 📡 10. Observability Design ^0201010

### 10.1 Logging ^02010101

- User actions
- Errors
- Performance metrics

---

### 10.2 Monitoring ^02010102

- Rendering performance
- Calculation time

---

### 10.3 Alerts (future) ^02010103

- System failures
- Data corruption

---

## ✅ 11. Quality Attributes ^0201011

### 11.1 Performance ^02010111

- <100ms interaction response
- Smooth rendering with large datasets

---

### 11.2 Scalability ^02010112

- Support thousands of shapes
- Efficient memory usage

---

### 11.3 Reliability ^02010113

- Autosave mechanism
- Crash recovery

---

### 11.4 Maintainability ^02010114

- Layer separation
- Modular components

---

### 11.5 Security ^02010115

- Controlled file access
- Safe calculation logic

---

## 🎨 12. UX & Accessibility Considerations ^0201012

- Grid + snapping
- Keyboard shortcuts
- Dark/light mode
- Accessibility (contrast, scaling)

---

🔗 Related:

- [[0208-ux_ui_design_en#^0208|🎨  0208-UX_UI Design]]

---
## ⚠️ 13. Risks & Considerations ^0201013

- Complex multi-selection logic
- Performance with large drawings
- Data consistency between JSON and DB
- UI vs engine validation separation

---

## ✅ Design Checklist ^Checklist

- [ ]  Architecture defined
- [ ]  Components identified
- [ ]  Data separation clear
- [ ]  Integration points defined
- [ ]  Deployment model validated

---

## 📌 Notes ^Notes

This document is linked to:

- Requirements → [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0101-Requirement_Analysis|0101-Requirement_Analysis]]
- Planning → [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0102-Planning|0102-Planning]]
- Development → [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0103-Requirements Traceability|0301-Development_Documentation]]
- Testing → [[CoNSoL-Documents-Library-V2/MegaFile/04-Verification/0401-Testing Documentation|0401-Testing_Documentation]]

---
> END — Design Documentation
---
