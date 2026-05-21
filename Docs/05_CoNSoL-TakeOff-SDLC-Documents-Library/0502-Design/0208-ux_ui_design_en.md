---
doc_id: 208
phase: design
owner: ux-ui
status: draft
last_updated: 2026-01-10
tags:
  - sdlc/design
  - ux
  - ui
color: "#e75321"
aliases:
  - 🎨 0208 — UX & UI Design
---

## 🎯 Purpose^0208A

Define the **user experience, user interface behavior, and interaction rules** for the CoNSoL‑TakeOff Drawing Engine.

This document specifies:
- Drawing tools and their variants
- Panels (Property, Layer, Symbol)
- Validation behavior
- Logical 3D interaction
- Aggregation-oriented UX concepts

---

### 📥 Inputs^0208A1

- [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0101-Requirement_Analysis|0101-Requirement_Analysis]]
- Platform strategy & roadmap
- Drawing Tools, Panels & UI Validation Spec (source)

---

### 📤 Outputs^0208A2

- UI behavior definitions
- Interaction rules
- Validation logic (UI-level)
- UX constraints for implementation

---

# 🧠 1. UX Design Principles ^020801

## 1.1 Core Principles ^02080101
	
- **Visual-first** — drawing is primary input
- **Data-driven** — UI reflects underlying metadata
- **Non-destructive** — warn before blocking
- **Context-sensitive** — panels adapt to selection state
- **Reusable** — designed as embeddable components

---

## 1.2 Canvas Philosophy ^02080102
	
- Canvas is **2D visually**
- Objects may carry **logical 3D attributes**
- Rendering ≠ business meaning

---

# ✏️ 2. Drawing Tools ^020802

## 2.1 Basic Shapes ^0208020201

### 2.1.1 Line ^0208020101
	
- Single segment: click start → click end
- Multi-segment (polyline mode):
	- Continuous clicks
	- Double-click / Enter to commit
	
#### Properties ^02080201P
- Start (X,Y)
- End (X,Y)
- Length (auto)
- Angle (auto)
- Line Style
- Line Weight
- Color

---

### 2.1.2 Rectangle ^0208020102
	
- By corner drag
- By center drag
	
#### Properties ^0208020102P
- Origin (X,Y)
- Width
- Height
- Rotation
- Corner Radius (optional)
- Line Style
- Fill

---

### 2.1.3 Circle ^0208020103
	
- Center + radius
- 2-point diameter
- 3-point circle
	
#### Properties ^0208020103P
- Center (X,Y)
- Radius
- Diameter
- Line Style
- Fill

---

### 2.1.4 Ellipse ^0208020104
	
- Center + axes
- Bounding box
	
#### Properties ^0208020104P
- Semi-major axis
- Semi-minor axis
- Rotation
- Line Style
- Fill

---

## 2.2 Curves ^02080202

### 2.2.1 Arc ^0208020201

- 3-point arc
- Start + Center + End
- Start + Center + Angle

#### Properties ^0208020201P
- Center
- Radius
- Start Angle
- End Angle
- Arc Length
- Direction (CW/CCW)

---

### 2.2.2 Spline ^0208020202

- Control points with tension handles
- Open or closed

#### Properties ^0208020202P
- Control points list
- Tension
- Closed toggle

---

### 2.2.3 Bezier Curve ^0208020203

- Cubic Bezier
- Chainable

#### Properties ^0208020203P
- Anchor points
- Control handles

---

## 2.3 Text & Annotation ^02080203

### 2.3.1 Text ^0208020301

- Single-line

#### Properties ^0208020301P
- Content
- Font
- Size
- Style flags
- Alignment
- Rotation

---

### 2.3.2 Multiline Text (MText) ^0208020302

- Bounded box
- Auto word-wrap

---

### 2.3.3 Leader / Callout ^0208020303

- Arrow + annotation text

---

## 2.4 Dimensions ^02080204

- Linear
- Aligned
- Angular
- Radius
- Diameter

**Rules**
- Values auto-calculated
- Override allowed (with warning)

---

## 2.5 Symbols & Blocks ^02080205

### Block Instance ^0208020501

- Inserted as a single unit
- Scalable and rotatable

### Block Definition ^0208020502

- Base point
- Child geometry
- Attribute definitions

---

## 2.6 Smart Tags ^02080206

### Purpose ^0208020601

**Smart Tags are data aggregators**, not visual markers.

- Name
- Value Type (text / number / boolean / list)
- Optional unit

**Display Modes** ^0208020602
- Hidden
- Label
- Badge

---

## 2.7 Custom Marks ^02080207

### Purpose

**Custom Marks are visual aggregators**, not data fields.

- Shape (circle, diamond, SVG)
- Color
- Label template

---

### ✅ Key Distinction

| Feature | Smart Tag | Custom Mark |
|------|---------|-------------|
| Carries value | ✅ | ❌ |
| Aggregated numerically | ✅ | ❌ |
| Visual emphasis | ⚠️ | ✅ |

---

# 🧱 3. Layer Panel ^020803

## 3.1 Layer Properties ^02080301

- Name
- Visibility
- Lock
- Print
- Color
- Line Style
- Line Weight
- Object Count (read-only)

---

## 3.2 Layer Rules ^02080302
	
- One active layer at all times
- Locked layers:
	- Visible
	- Not editable
- Objects inherit visual defaults from layer unless overridden
	
---

## 3.3 Layer Lifecycle ^02080303
	
- Create
- Duplicate
- Merge
- Delete (with reassignment or delete confirmation)
	
---

# 🧰 4. Property Panel ^020804

## 4.1 Context Sensitivity ^02080401

| Selection | Behavior |
|--------|---------|
| None | Canvas properties |
| Single object | Full properties |
| Multi, same type | Shared + `(mixed)` |
| Multi, mixed types | Universal only |
| Active tool | Tool defaults |

---

## 4.2 Universal Properties ^02080402
	
- Object ID (read-only)
- Object Type (read-only)
- Layer
- Color
- Line Style
- Line Weight
- Visibility
- Lock
- Notes
- Tags
- Marks
	
---

## 4.3 Logical 3D Properties ^02080403
	
> Used **only for quantity & pricing**, not rendering.
	
- Height (H)
- Width (W)
- Length / Depth (L)
- Unit System
- Area (auto)
- Volume (auto)
- Quantity multiplier
- Unit price
- Total cost (auto)

---

# ✅ 5. Validation Rules (UI-Level) ^020805

## 5.1 Drawing Validations ^02080501
	
- Zero-size shapes blocked
- Minimum points enforced
- Degenerate arcs blocked
- Self-intersecting polygons warned (not blocked)

---

## 5.2 Property Panel Validations ^02080502
	
- Numeric-only fields
- Rotation normalized (0–360)
- Quantity ≥ 1
- Price ≥ 0

---

## 5.3 Layer Validations ^02080503
	
- Duplicate names blocked
- Deleting active layer blocked
- All layers hidden → warning

---

## 5.4 Multi-Selection Rules ^02080504
	
- `(mixed)` placeholder shown
- Editing overrides all selected
- Type-specific fields hidden for mixed selection

---

# ⚠️ 6. Highlighted Design Concerns ^020806

## 6.1 Logical 3D Applicability ^02080601
	
- Applies only to:
	- Shapes representing physical elements
- Does NOT apply to:
	- Text
	- Dimensions
	
---

## 6.2 Smart Tags vs Custom Marks ^02080602
	
- Separate engines
- Shared aggregation infrastructure possible (future)
	
---

## 6.3 Validation Separation ^02080603
	
- Data validation → Engine
- Visual feedback → UI
	
---

# 🧩 7. Component Reusability ^020807
	
- Panels are metadata-driven
- Tools are pluggable
- Validation rules are declarative
	
---

## 🔗 Related Documents
	
- [[CoNSoL-Documents-Library-V2/MegaFile/02-Design/0201-Design Documentation|0201-Design_Documentation]]
- [[CoNSoL-Documents-Library-V2/MegaFile/02-Design/020103-Data Model|020103-Data_Model]]
- [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0104-SRS|0104-SRS]] 
- [[CoNSoL-Documents-Library-V2/MegaFile/03-Implementation/0301-Development Documentation|0301-Development_Documentation]]
	
---
> END — UX & UI Design

