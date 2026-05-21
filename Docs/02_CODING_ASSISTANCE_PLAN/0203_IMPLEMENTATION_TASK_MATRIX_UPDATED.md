---
color: "#04509a"
---
# CoNSoL-TakeOff Implementation Task Matrix - UPDATED

**Purpose:** Map SDLC requirements to concrete implementation tasks across all layers  
**Alignment:** SDLC 0104-SRS §5 (Functional Requirements) → Implementation Checklist  
**Last Updated:** 2025 (Current Implementation Status)  
**Status Tracking:** Coverage map with actual code completion

---
# 📑 Executive Summary - Current Status

**Overall Project Status: 35% Complete**

This is an updated version of the original task matrix reflecting the **actual state of code** as of the current branch (ChkAntigravity).

| Layer | Target | Actual | % Complete | Blocker | Status |
|-------|--------|--------|------------|---------|--------|
| 🏗️ Foundation | 20 tasks | 2 done, 2 in-progress | 20% | ✅ CRITICAL | 🔴 Need validation modules |
| 🎨 Rendering | 14 tasks | 5 done, 2 in-progress | 50% | ❌ None | 🟢 Nearly complete |
| ⌨️ Interaction | 14 tasks | 7 done, 2 in-progress | 64% | ❌ None | 🟢 Mostly complete |
| 💼 Business | 19 tasks | 2 done, 1 in-progress | 16% | ✅ CRITICAL | 🔴 Calculator incomplete |
| 🔗 Integration | 19 tasks | 4 done, 2 in-progress | 32% | ✅ CRITICAL | 🟡 Layer panel missing |
| **TOTAL** | **86 tasks** | **30 done** | **35%** | — | 🟡 On track for MVP |

---
# 🏗️ Foundation Layer - UPDATED

**Scope:** Domain entities, utilities, infrastructure  
**Current Status:** 10% Complete (2/20 done, 2 in-progress)

## Completion Status by Task

### DONE ✅

| ID | Feature | Entity | Completed |
|----|---------|--------|-----------|
| FND-001 | Create CanvasLayout entity | CanvasLayout | ✅ 2025 |
| FND-005 | Create CanvasElement entity | CanvasElement | ✅ 2025 |
| FND-010 | Create BusinessDefinition entity | BusinessDefinition | ✅ 2025 |
| FND-015 | Create AppConfig with validation | AppConfig | ✅ 2025 |
| FND-017 | JSON serialization wrapper | JSON | ✅ 2025 |
| FND-019 | File I/O service (.takeoff files) | FileStore | ✅ 2025 |

### IN-PROGRESS 🟨

| ID | Feature | Entity | Issue |
|----|---------|--------|-------|
| FND-002 | Implement CanvasLayout invariants | CanvasLayout | Validation not enforced in properties |
| FND-016 | Load config from file (environment-aware) | AppConfig | Partial - needs env detection |

### TODO 🔲 (Blocking Other Layers)

| ID | Feature | Entity | Priority | Blocks |
|----|---------|--------|----------|--------|
| **FND-003** | ❌ Add CanvasLayout validation module | CanvasLayout | **P0** | All layers |
| **FND-007** | ❌ Add CanvasElement validation module | CanvasElement | **P0** | Business, Integration |
| **FND-013** | ❌ Create Layer entity | Layer | **P0** | UC-002, UC-007 |
| **FND-014** | ❌ Implement Layer visibility/lock logic | Layer | **P0** | UC-002, UC-007 |
| FND-004 | Add/Delete layer with reassignment | CanvasLayout | P1 | Workflow |
| FND-006 | Implement CanvasElement invariants | CanvasElement | P0 | Validation |
| FND-008 | Add geometry validation by shape type | CanvasElement | P0 | Data integrity |
| FND-009 | Implement parent-child relationship validation | CanvasElement | P1 | Nested objects |
| FND-011 | Implement BusinessDefinition invariants | BusinessDefinition | P0 | Calculation |
| FND-012 | Dimension mode validation (D0-D3) | BusinessDefinition | P0 | Calculation |
| FND-018 | Round-trip serialization validation | JSON | P1 | Testing |
| FND-020 | Material JSON store | MatStore | P1 | Export |

---

# 🎨 Rendering Layer - UPDATED

**Scope:** 2D canvas, shape rendering, visual feedback  
**Current Status:** 60% Complete (5/8 done, 2 in-progress)

## Completion Status by Task

### DONE ✅

| ID      | Feature                                                    | Component         | Completed |
| ------- | ---------------------------------------------------------- | ----------------- | --------- |
| RND-001 | ✅ Create CanvasControl (user control)                      | CanvasControl     | ✅ 2025    |
| RND-002 | ✅ Implement coordinate transformation (physical ↔ logical) | CanvasControl     | ✅ 2025    |
| RND-003 | ✅ Implement OnPaint rendering loop                         | CanvasControl     | ✅ 2025    |
| RND-004 | ✅ Render visible layers only                               | CanvasControl     | ✅ 2025    |
| RND-005 | ✅ Render Line shapes                                       | LineShape         | ✅ 2025    |
| RND-008 | ✅ Render selection highlights                              | SelectionRenderer | ✅ 2025    |
| RND-009 | ✅ Render preview geometry (rubber-band)                    | PreviewRenderer   | ✅ 2025    |

### IN-PROGRESS 🟨

| ID | Feature | Component | Work Remaining |
|----|---------|-----------|-----------------|
| RND-006 | Render Rectangle shapes | RectangleShape | ~2 hours - finish shape impl |
| RND-007 | Render Circle shapes | CircleShape | ~2 hours - finish shape impl |

### TODO 🔲

| ID | Feature | Component | Priority |
|----|---------|-----------|----------|
| RND-010 | Implement zoom (scale factor) | CanvasControl | P1 |
| RND-011 | Implement pan (offset) | CanvasControl | P1 |
| RND-012 | Render grid background | GridRenderer | P2 |
| RND-013 | Hit test for shape selection | HitTestService | P1 |
| RND-014 | Shape factory for type-specific rendering | ShapeFactory | P1 |

**Notes:** Zoom + Pan are already implemented! Grid rendering exists. Hit testing works for selection.

---
# ⌨️ Interaction Layer - UPDATED

**Scope:** User input, tool state machines, multi-selection  
**Current Status:** 70% Complete (7/10 done, 2 in-progress)

## Completion Status by Task

### DONE ✅

| ID | Feature | Component | Completed |
|----|---------|-----------|-----------|
| INT-001 | ✅ Create BaseTool abstract class | ToolSystem | ✅ 2025 |
| INT-002 | ✅ Implement SelectTool | SelectTool | ✅ 2025 |
| INT-003 | ✅ Implement LineTool | LineTool | ✅ 2025 |
| INT-004 | ✅ Implement RectangleTool | RectangleTool | ✅ 2025 |
| INT-005 | ✅ Implement CircleTool | CircleTool | ✅ 2025 |
| INT-006 | ✅ Implement PanTool | PanTool | ✅ 2025 |
| INT-007 | ✅ Implement ZoomTool | ZoomTool | ✅ 2025 |
| INT-008 | ✅ Tool manager (activate/deactivate) | ToolManager | ✅ 2025 |
| INT-009 | ✅ Create SelectionManager | SelectionManager | ✅ 2025 |

### IN-PROGRESS 🟨

| ID | Feature | Component | Work Remaining |
|----|---------|-----------|-----------------|
| INT-010 | Single selection (click) | SelectionManager | ~1 hour - add logging |
| INT-011 | Multi-selection (Ctrl+click) | SelectionManager | ~1 hour - add logging |

### TODO 🔲

| ID | Feature | Component | Priority |
|----|---------|-----------|----------|
| INT-012 | Window selection (drag rectangle) | SelectionManager | P1 |
| INT-013 | Escape key handling (cancel drawing) | InputHandler | P0 |
| INT-014 | Keyboard shortcuts (file, edit, view) | KeyboardHandler | P2 |

---
# 💼 Business Layer - UPDATED

**Scope:** Use case orchestration, calculations, aggregations  
**Current Status:** 30% Complete (2/8 done, 1 in-progress)

## Completion Status by Task

### DONE ✅

| ID | Feature | Component | Completed |
|----|---------|-----------|-----------|
| BUS-001 | ✅ Create TakeOffContext | Context | ✅ 2025 |
| BUS-002 | ✅ Create TakeOffResult | Result | ✅ 2025 |

### IN-PROGRESS 🟨

| ID | Feature | Component | Work Remaining |
|----|---------|-----------|-----------------|
| BUS-003 | Create TakeOffCalculator | Calculator | Skeleton exists, needs implementation |

### TODO 🔲 (CRITICAL - Blocks UC-004)

| ID | Feature | Component | Priority | Blocks |
|----|---------|-----------|----------|--------|
| **BUS-004** | ❌ Implement Calculate() - main method | Calculator | **P0** | **UC-004** |
| **BUS-005** | ❌ Extract dimension (D1: length) | Calculator | **P0** | **UC-004** |
| **BUS-006** | ❌ Extract dimension (D2: area) | Calculator | **P0** | **UC-004** |
| **BUS-008** | ❌ Apply nested object logic (subtract) | Calculator | **P0** | **UC-004** |
| BUS-007 | Extract dimension (D3: volume) | Calculator | P1 | UC-004 |
| BUS-009 | Calculate total cost | Calculator | P0 | UC-004 |
| BUS-010 | Handle formula application | Calculator | P1 | UC-004 |

---
# 🔗 Integration Layer - UPDATED

**Scope:** Dependency injection, form orchestration, events  
**Current Status:** 40% Complete (4/10 done, 2 in-progress)

## Completion Status by Task

### DONE ✅

| ID | Feature | Component | Completed |
|----|---------|-----------|-----------|
| IGN-001 | ✅ Create CompositionRoot | DI | ✅ 2025 |
| IGN-002 | ✅ Register infrastructure services | DI | ✅ 2025 |
| IGN-003 | ✅ Register application services | DI | ✅ 2025 |
| IGN-004 | ✅ Register UI components | DI | ✅ 2025 |
| IGN-006 | ✅ Create MainForm | UI | ✅ 2025 |
| IGN-007 | ✅ Add toolbar with tool buttons | UI | ✅ 2025 |
| IGN-008 | ✅ Add canvas control | UI | ✅ 2025 |

### IN-PROGRESS 🟨

| ID | Feature | Component | Work Remaining |
|----|---------|-----------|-----------------|
| IGN-005 | Implement Resolve(Of T)() method | DI | ~1 hour - error handling |
| IGN-009 | Add property panel | UI | ~3 hours - wire to selection |

### TODO 🔲 (CRITICAL)

| ID | Feature | Component | Priority | Blocks |
|----|---------|-----------|----------|--------|
| **IGN-010** | ❌ Add layer panel | UI | **P0** | **UC-002, UC-007** |
| IGN-011 | Wire event handlers | UI | P0 | UC-001 |
| IGN-012 | Implement File menu | UI | P1 | Save/Load |
| IGN-013 | Implement Edit menu | UI | P1 | Undo/Redo |
| IGN-014 | Implement View menu | UI | P2 | Full zoom control |

---
# 📋 Use Case Coverage - UPDATED

## UC-001: Draw Line on Canvas

**Status:** 🟢 70% Complete  
**Impact:** PRIMARY FEATURE

| Layer | Tasks | Status | Notes |
|-------|-------|--------|-------|
| Foundation | FND-001, 005, 010 | ✅ DONE | Entities defined |
| Rendering | RND-001-009 | ✅ DONE | Canvas + rendering working |
| Interaction | INT-001-011 | 🟨 MOSTLY DONE | Need logging (INT-010, INT-011) |
| Business | — | — | Not needed for draw |
| Integration | IGN-006-008 | ✅ DONE | MainForm + buttons working |

**What's working:**
- ✅ User can draw line, rectangle, circle, ellipse, polyline
- ✅ Click to select, drag to move
- ✅ Pan and zoom viewport
- ✅ Grid rendering

**What needs work:**
- 🟨 Add logging for user actions (INT-010, INT-011)
- 🟨 Add XML documentation (all components)

**How to test:**
```
1. Run application
2. Click "Line" button
3. Click on canvas twice
4. Line should appear
5. Click "Select" button
6. Click line to select
7. Dashed outline should appear
```

---
## UC-002: Assign Object to Layer

**Status:** 🔴 20% Complete  
**Impact:** WORKFLOW

| Layer | Tasks | Status | Blocker |
|-------|-------|--------|---------|
| Foundation | FND-001, 013, 014 | 🔴 INCOMPLETE | **Layer entity missing** |
| Rendering | RND-004 | ✅ DONE | Layer filtering works |
| Interaction | — | — | Depends on Foundation |
| Business | — | — | Depends on Foundation |
| Integration | IGN-010 | 🔴 TODO | **Layer panel needed** |

**Blocked by:**
- ❌ FND-013: Layer entity not created
- ❌ FND-014: Layer visibility/lock not implemented
- ❌ IGN-010: Layer panel UI not built

**To complete:**
1. Create Layer entity (2 hours)
2. Add layer management to CanvasLayout (2 hours)
3. Build LayerPanel UI (6 hours)
4. Wire events (2 hours)

---
## UC-003: Attach Smart Tag to Object

**Status:** 🔴 10% Complete  
**Impact:** NICE-TO-HAVE (P1)

**Not started - defer to Phase 2**

---
## UC-004: Run Take-off Quantity Summary

**Status:** 🔴 20% Complete  
**Impact:** PRIMARY FEATURE

| Layer | Tasks | Status | Blocker |
|-------|-------|--------|---------|
| Foundation | FND-001-020 | 🔴 PARTIAL | Need validation |
| Rendering | — | — | Not needed |
| Interaction | — | — | Not needed |
| Business | BUS-001-010 | 🔴 INCOMPLETE | **Calculator not implemented** |
| Integration | IGN-012 | 🔲 TODO | Export UI needed |

**Blocked by:**
- ❌ BUS-004: Calculate() method not implemented
- ❌ BUS-005: Dimension extraction (D1, D2) not implemented
- ❌ BUS-008: Nested object logic not implemented

**To complete:**
1. Implement TakeOffCalculator.Calculate() (4 hours)
2. Add dimension extraction (D1, D2, D3) (3 hours)
3. Implement nested object logic (2 hours)
4. Add cost calculation (1 hour)
5. Build export UI (2 hours)

---
## UC-005: Insert Symbol

**Status:** 🔴 10% Complete  
**Impact:** NICE-TO-HAVE (P1)

**Not started - defer to Phase 2**

---
## UC-006: Edit Multi-Selection

**Status:** 🟡 40% Complete  
**Impact:** WORKFLOW

| Layer | Tasks | Status | Notes |
|-------|-------|--------|-------|
| Foundation | FND-001-012 | ✅ DONE | Entities defined |
| Rendering | RND-008 | ✅ DONE | Multi-highlight works |
| Interaction | INT-009-011 | 🟨 MOSTLY DONE | Selection works, need logging |
| Business | — | — | Not needed |
| Integration | IGN-009 | 🟨 IN-PROGRESS | Property panel not wired |

**What's working:**
- ✅ Can select multiple objects
- ✅ Visual highlighting shows all selected

**What needs work:**
- 🟨 IGN-009: Property panel needs to connect to selection
- 🟨 Add logging for property changes

**To complete:**
1. Wire PropertiesPanel to SelectionManager (3 hours)
2. Display shared properties as "(mixed)" (2 hours)
3. Add change handlers (1 hour)
4. Add logging (1 hour)

---
## UC-007: Delete Layer with Objects

**Status:** 🔴 20% Complete  
**Impact:** WORKFLOW

| Layer | Tasks | Status | Blocker |
|-------|-------|--------|---------|
| Foundation | FND-001-004 | 🔴 INCOMPLETE | Layer entity missing |
| Rendering | RND-004 | ✅ DONE | Layer filtering works |
| Interaction | INT-009 | ✅ DONE | Selection works |
| Business | — | — | Not needed |
| Integration | IGN-010 | 🔴 TODO | Layer panel needed |

**Blocked by:**
- ❌ Layer entity and management
- ❌ Layer panel UI

---
## UC-008: Switch Between Standalone and Integrated Mode

**Status:** 🟢 60% Complete  
**Impact:** DEPLOYMENT

| Layer | Tasks | Status | Notes |
|-------|-------|--------|-------|
| Foundation | FND-015, 016 | 🟨 IN-PROGRESS | Config loading partial |
| Rendering | — | — | Not needed |
| Interaction | — | — | Not needed |
| Business | — | — | Not needed |
| Integration | IGN-001-005 | ✅ DONE | DI works |

**What's working:**
- ✅ AppConfig loads from file
- ✅ DI container set up for both modes
- ✅ Services registered correctly

**What needs work:**
- 🟨 FND-016: Environment-aware config path

**To complete:**
1. Add environment detection (1 hour)
2. Load correct config file per environment (1 hour)
3. Add integration tests (2 hours)

---
# 🔴 Critical Path to MVP

## Minimum Tasks for MVP (7 weeks)

```
Phase 1: Foundation (Weeks 1-2)
├─ FND-003: CanvasLayout validation module (2h)
├─ FND-007: CanvasElement validation module (2h)
├─ FND-013: Create Layer entity (3h)
├─ FND-014: Layer management logic (3h)
└─ FND-011, FND-012: BusinessDefinition validation (2h)

Phase 2: Business Logic (Weeks 3-4)
├─ BUS-004: Calculator.Calculate() (4h)
├─ BUS-005: Dimension extraction D1, D2 (3h)
├─ BUS-006, BUS-008: Cost & nested objects (3h)
└─ BUS-009: Total cost calculation (1h)

Phase 3: UI Completion (Weeks 5-7)
├─ IGN-010: Layer panel (8h)
├─ IGN-009: Wire PropertiesPanel (3h)
├─ IGN-011, IGN-012: Menus (4h)
├─ Testing & Logging (8h)
└─ Bug fixes (4h)

Total MVP Hours: ~50 hours (~1.25 developer weeks)
```

---
# ✅ Quick Wins (Can Start Immediately)

These require NO blocking work and add immediate value:

1. **Add XML Documentation** (2-3h per layer)
   - Document all public classes/properties
   - Add method summaries
   - No code changes needed

2. **Add Logging** (1-2h per layer)
   - Log tool activation (INT)
   - Log file operations (Infrastructure)
   - Log calculations (Business)
   - No blocking issues

3. **Complete Shape Rendering** (2-3h)
   - Finish RectangleShape implementation
   - Finish CircleShape implementation
   - Already have framework

4. **Create Test Projects** (2-3h)
   - Domain.Tests
   - Infrastructure.Tests
   - Set up xUnit or NUnit

---
# 📊 Recommended Work Order

## PRIORITY 1: Fix Data Integrity (Foundation)

**Why:** All other layers depend on validated data

1. Create validation modules (FND-003, FND-007)
2. Add Layer entity (FND-013, FND-014)
3. Add invariant enforcement in properties
4. **Estimated:** 8-10 hours | **Enables:** All layers

## PRIORITY 2: Implement Business Logic (Business)

**Why:** Enables UC-004 (primary feature)

1. Implement TakeOffCalculator.Calculate()
2. Add dimension extraction
3. Add cost calculations
4. **Estimated:** 10-12 hours | **Enables:** Take-off export

## PRIORITY 3: Complete UI (Integration)

**Why:** Enables user workflows

1. Build LayerPanel (IGN-010)
2. Wire PropertiesPanel (IGN-009)
3. Add menus (IGN-012, IGN-013)
4. **Estimated:** 15-20 hours | **Enables:** All use cases

## PRIORITY 4: Quality Improvements (All)

**Why:** Maintainability and debugging

1. Add XML documentation
2. Add logging strategically
3. Create test projects
4. Write unit tests
5. **Estimated:** 15-20 hours | **Enables:** Future development

---
# 📝 Task Template (Copy for Each Task)

```markdown
## Task: [Task ID] - [Feature Name]

**Status:** 🔲 TODO | 🟨 IN-PROGRESS | ✅ DONE

**SDLC Reference:** 0104-SRS §5.x

**Description:** [What needs to be done]

**Subtasks:**
- [ ] Subtask 1
- [ ] Subtask 2
- [ ] Subtask 3

**Verification:**
- [ ] Code compiles without errors
- [ ] Unit test passes
- [ ] Code review passed

**Blocked By:** None  
**Blocks:** [Other task IDs]  
**Estimated Hours:** X  
**Actual Hours:** —  

**Notes:**
- [Any special considerations]
```

---

**Document Version:** 2.0 (Updated with actual implementation status)  
**Last Updated:** 2025  
**Branch:** ChkAntigravity  
**Next Review:** After Phase 1 completion (Foundation fixes)
