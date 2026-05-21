---
doc_id: 101
phase: inception
owner: product
status: draft
last_updated: 2026-01-10
tags:
  - sdlc/inception
  - product
  - requirements
aliases:
  - Requirement Analysis
color: "#460303"
---
⬅️ [🏠 Back to Platform Index](0000-CoNSoL-Platform-Index.md)
# 🧠 0101 — Requirement Analysis ^0101

---

## 🎯 Purpose ^010100

Define the **business vision, product scope, users, and success criteria** for the CoNSoL platform and its flagship application, **CoNSoL‑TakeOff**.

| **This document answers:**          |
| ----------------------------------- |
| - What problem are we solving?      |
| - What is CoNSoL?                   |
| - What problem are we solving?      |
| - What is CoNSoL?                   |
| - Who is this for?                  |
| - What is in scope vs out of scope? |

---

## 🏗️ 1. What is CoNSoL? ^010101

**CoNSoL (Construction Solution)** is a **modular construction management platform** designed around a hub‑and‑spoke architecture.

### Platform Characteristics ^010101a
	
- **Core Engine:** CoNSoL‑Engine ^010101a1
	- Orchestration
	- Shared services
	- Licensing
	- Inter‑module communication
	
- **Mandatory Module:** CoNSoL‑Project Manager ^010101ab
	- Projects
	- Timelines
	- Dependencies
	- Resource allocation
	
- **Optional Modules:** ^010101ac
	- CoNSoL‑TakeOff
	- CoNSoL‑HR
	- CoNSoL‑Docs
	- Others (future)
	
### Architectural Principle ^010101b

> **Some modules can run standalone, others require the Core Engine.**

---

## 🧱 2. What is CoNSoL‑TakeOff? ^010102

**CoNSoL‑TakeOff** is a **visual-first construction take‑off and estimation tool**.

### Core Idea ^010102a

> ✏️ **Drawing is not decoration. Drawing is data input.**

Users **draw construction elements visually**, and those drawings are treated as:
- Data objects
- With geometry
- With business meaning
- With calculable quantities and costs

---

### Key Capabilities ^010102b
	
- Draw physical elements (walls, slabs, rooms, columns)
- Assign real‑world meaning to shapes
- Link shapes to:
	- Materials
	- Formulas
	- Prices
- Automatically compute:
	- Quantities
	- Costs
	- Material breakdowns
	
---

### Deployment Modes ^010102c

| Mode | Description |
|----|----|
| Standalone | Desktop app, local DB, standalone license |
| Integrated | Embedded in CoNSoL‑Engine, shared DB, suite license |

---

## ❗ 3. Problem Statement ^010103

### Current Pain Points ^010103a
	
- Manual spreadsheets are error‑prone
- CAD tools are disconnected from estimation
- Quantity take‑off is slow and inconsistent
- Changes require rework across tools
	
---

### Why Existing Tools Fail^010103

| Tool Type | Limitation |
|---------|------------|
| Excel | No visual context |
| CAD | No business logic |
| Estimation software | No direct drawing input |

---

## ✅ 4. Proposed Solution ^010104

- CoNSoL‑TakeOff provides:
	
	- ✅ Visual drawing interface
	- ✅ Metadata‑driven objects
	- ✅ Real‑time quantity calculation
	- ✅ Database‑backed materials and formulas
	- ✅ Automatic cost rollups
	
---

## 👥 5. Target Users ^010105

### Primary Users^010105a
	
- Construction estimators
- Site engineers
- Quantity surveyors
	
### Secondary Users^010105b
	
- Architects / designers
- Procurement teams
- Project managers
	
---

## 🔄 6. High-Level User Workflow ^010106

```text
Setup → Draw → Define → Store → Calculate → Report
```

Where:
	
- **Draw** = visual geometry
- **Define** = business meaning
- **Calculate** = quantities & cost

---

## 🧩 7. Core Concepts ^010107

### 7.1 Dimension Modes^0101071

|Mode|Meaning|
|---|---|
|D0|Count|
|D1|Length|
|D2|Area|
|D3|Volume|

Each drawn object uses **exactly one dimension mode**.

---

### 7.2 Nested Objects^0101072
	
- Doors inside walls
- Openings inside slabs
- Windows subtract from wall area
	
---

## 📦 8. Scope ^010108

### ✅ In Scope (v1 / Demo)^010108a
	
- 2D drawing canvas
- Logical 3D attributes (H × W × L)
- Materials & formulas
- Cost estimation
- Standalone deployment
	
---

### ❌ Out of Scope (v1)^010108b
	
- True 3D rendering
- Real‑time collaboration
- Cloud sync
- AI‑assisted drawing (future)
	
---

## 📊 9. Success Criteria ^010109

### Demo Success^010109a
	
- User can draw a wall in < 5 minutes
- System shows automatic quantity
- Cost estimate is generated
- Exportable summary exists
	
---

### Product Success^010109b
	
- Reduction in estimation time
- Fewer quantity errors
- Faster design iteration

---

## ⚠️ 10. Assumptions & Constraints ^010110

### Assumptions ^010110a
	
- Users are familiar with construction concepts
- Desktop-first usage
- Single-user for demo phase

### Constraints ^010110b
	
- Windows platform initially
- .NET ecosystem
- Offline-first in standalone mode

---

## 🔗 Related Documents ^010111

- [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0102-Planning|0102-Planning]]
- [[CoNSoL-Documents-Library-V2/MegaFile/02-Design/020101-System Context|0201-Design_Documentation]]
- [[CoNSoL-Documents-Library-V2/MegaFile/02-Design/0208-UX_UI Design|0208-UX_UI_Design]]
- [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0104-SRS|0104-SRS]] 

⬅️ [🏠 Back to Platform Index](0000-CoNSoL-Platform-Index.md)

---
> END — Requirement Analysis
---
