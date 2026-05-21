---
doc_id: 20103
phase: design
owner: data-architecture
status: draft
last_updated: 2026-01-10
tags:
  - sdlc/design
  - data-model
color: "#e75321"
---

# 🗄️ 020103 — Data Model ^020103
>CoNSoL-Documents-Library/02-Design/020103-Data Model
---
## 🎯 Purpose ^020103P

Define the logical and physical data structures used by the CoNSoL-TakeOff system, including:

- Drawing data (geometry)
- Business data (materials, formulas)
- Metadata (tags, layers)
- Persistence (JSON + database)

---

### 📥 Inputs ^020103PIN

- [[CoNSoL-Documents-Library-V2/MegaFile/02-Design/0201-Design Documentation|0201-Design_Documentation]]
- [[CoNSoL-Documents-Library-V2/MegaFile/01-Inception/0101-Requirement_Analysis|0101-Requirement_Analysis]]

---

### 📤 Outputs ^020103POUT

- Database schema
- JSON structure
- Data relationships
- Storage strategies

---

# 🧠 1. Data Model Overview^02010301

## 1.1 Design Principles^020103011

- ✅ **Separation of Concerns**
  - Geometry vs Business data
- ✅ **Metadata-Driven**
  - Flexible tagging system
- ✅ **Extensible**
  - Supports plugins (tags, marks, symbols)
- ✅ **Version-Ability**
  - JSON-based persistence

---

## 1.2 Core Data Domains ^020103012

| Domain      | Description           |
| ----------- | --------------------- |
| Geometry    | Shapes & spatial data |
| Business    | Materials, formulas   |
| Metadata    | Tags, layers, marks   |
| Persistence | JSON + database       |

---

# 🧱 2. Core Entity Model ^020102

## 2.1 DrawingObject (Base Entity) ^0201021

```json
{
  "id": "uuid",
  "type": "rectangle",
  "layerId": "layer-001",
  "geometry": {},
  "business": {},
  "tags": [],
  "marks": []
}
```

---

## 2.2 Geometry Model ^0201022

```json
"geometry": {
  "type": "rectangle",
  "topLeft": { "x": 100, "y": 150 },
  "width": 200,
  "height": 50
}
```

### Supported Types

- Line
- Rectangle
- Circle
- Polyline
- Text
- Symbol

---

## 2.3 Business Model ^020103

```json
"business": {
  "blockRef": "BLOCK_WALL",
  "dimensionMode": "D2",
  "formulaCode": "M15_1_2_4",
  "quantity": 10,
  "unit": "m2",
  "unitPrice": 20.5
}
```

---

## 2.4 Logical 3D Properties ^0201024

```json
"logical3D": {
  "height": 3.0,
  "width": 5.0,
  "length": 10.0,
  "volume": 150,
  "area": 50
}
```

---

# 🧮 3. Dimension Model ^020103

## 3.1 Dimension Modes ^0201031

| Mode | Description |
| ---- | ----------- |
| D0   | Count       |
| D1   | Length      |
| D2   | Area        |
| D3   | Volume      |

---

## 3.2 Derived Values ^0201032

| Property | Source         |
| -------- | -------------- |
| Length   | Line geometry  |
| Area     | Width × Height |
| Volume   | Area × Depth   |

---

# 🧩 4. Metadata Model ^020104

## 4.1 Smart Tags ^0201041

### Definition

```json
{
  "tagId": "tag-001",
  "name": "Material",
  "valueType": "text"
}
```

---

### Instance

JSON
```json
{
  "objectId": "obj-001",
  "tagId": "tag-001",
  "value": "Concrete"
}
```

---

## 4.2 Custom Marks ^0201042

```json
{
  "markId": "mark-001",
  "type": "Inspection",
  "position": { "x": 100, "y": 200 },
  "label": "Issue #1"
}
```

---

## 4.3 Layers ^0201043

```json
{
  "layerId": "layer-001",
  "name": "Walls",
  "visible": true,
  "locked": false,
  "color": "#FF0000"
}
```

---

# 🗄️ 5. Database Schema ^020105

## 5.1 CanvasLayouts ^0201051

|Field|Type|
|---|---|
|CanvasId|TEXT (PK)|
|Unit|TEXT|
|ScaleFactor|REAL|
|Background|TEXT|
|CreatedAt|DATETIME|

---

## 5.2 CanvasElements ^0201052

| Field        | Type      |
| ------------ | --------- |
| ElementId    | TEXT (PK) |
| CanvasId     | TEXT (FK) |
| Type         | TEXT      |
| GeometryJSON | TEXT      |
| BusinessJSON | TEXT      |

---

## 5.3 Blocks ^0201053

|Field|Type|
|---|---|
|BlockId|TEXT|
|Name|TEXT|
|DimensionMode|TEXT|
|FormulaCode|TEXT|

---

## 5.4 Materials ^0201054

|Field|Type|
|---|---|
|MaterialId|TEXT|
|Name|TEXT|
|Unit|TEXT|
|Price|REAL|

---

## 5.5 Formulas ^0201055

|Field|Type|
|---|---|
|FormulaCode|TEXT|
|Expression|TEXT|
|DimensionMode|TEXT|

---

## 5.6 Prices ^0201056

|Field|Type|
|---|---|
|MaterialId|TEXT|
|Price|REAL|
|EffectiveDate|DATETIME|

---

# 🔗 6. Entity Relationships ^020106

```text
CanvasLayouts
  └── CanvasElements
        ├── Blocks
        ├── Materials
        └── Formulas
``` 

---

## 6.1 Key Relationships ^0201061

- Canvas → Elements (1:N)
- Element → Block (N:1)
- Block → Formula (1:1)
- Formula → Materials (1:N)

---

# 💾 7. Serialization Strategy ^020107

## 7.1 File Format ^0201071

- Extension: `.takeoff`
- Format: JSON
- Optional: Compression + Encryption

---

## 7.2 JSON Structure ^0201072

```JSON
{  
"canvas": {},  
"elements": [],  
"materials": {},  
"calculations": {}  
}  
```

---

## 7.3 Versioning ^0201073

```json
"version": "1.0"
```

---

# 🔄 8. Data Lifecycle ^020108

## 8.1 Flow ^0201081


```Plain Text
User Input → Shape Creation → Metadata Assignment → Save JSON → Persist DB → Calculation  
```

---

## 8.2 State Types ^0201082

- Draft (unsaved)
- Saved (file/db)
- Calculated (post-processing)

---

# 📡 9. Aggregation Model ^020109

## 9.1 Aggregation Inputs ^0201091

- Tags
- Layers
- Object Types
- Logical 3D values

---

## 9.2 Aggregation Outputs ^0201092

```json
{
  "materialSummary": {
    "cement": { "quantity": 10, "cost": 50 }
  }
}
```

---

# ⚠️ 10. Constraints & Rules ^0201010

## 10.1 Validation Rules ^02010101

- Geometry must be valid (>0 size)
- Dimension mode must match object type
- Formula must match dimension mode

---

## 10.2 Data Integrity ^02010102

- Foreign key relationships enforced
- JSON schema validation
- Duplicate IDs prevented

---

# ✅ 11. Data Governance ^0201011

## 11.1 Ownership ^02010111

|Data|Owner|
|---|---|
|Geometry|Engine|
|Business|Product|
|Pricing|Admin|

---

## 11.2 Retention ^02010112

- Local files retained indefinitely
- DB cleanup optional

---

# ✅ Data Model Checklist ^Checklist

- [ ]  Geometry separated from business
- [ ]  JSON schema defined
- [ ]  DB schema defined
- [ ]  Relationships mapped
- [ ]  Validation rules defined

---

# 📌 Notes ^Notes

This model supports:

- Standalone mode ✅
- Integrated mode ✅
- Future cloud sync ✅

---
> END — Data Model Documentation

