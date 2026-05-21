---
color: "#44027e"
---
# Agent Command Protocol

## Command Format

[AgentName] :: [ACTION/GOAL] :: [TASK_ID] + CONSTRAINTS / Description
### Example
Agent Canvas :: Implement fixed-point zoom :: Follow 0209 §4.3 exactly

---

## Allowed Commands
- IMPLEMENT (write code)
- DESIGN (define rules before coding)
- REVIEW (discuss)
- VALIDATE (check invariants / correctness)
- VERIFY (test behavior)
- DEMO
- STOP


---
## Examples

Agent Geometry :: IMPLEMENT :: INT-003 LineTool (mouse state machine)
Agent Canvas :: IMPLEMENT :: RND-003 OnPaint rendering loop
Agent Foundation :: IMPLEMENT :: FND-001 CanvasLayout entity

_also it can be in the following form_

Agent Foundation :: VALIDATE :: Coordinate invariants after unit change  
Agent Canvas :: IMPLEMENT :: Redraw/Refresh pipeline per 0209 §2  
Agent Geometry :: DESIGN :: HitTest algorithms for line, rect, ellipse  
Agent Business :: IMPLEMENT :: Material aggregation logic  
Agent Integrator :: DEMO :: Full draw → save → reopen → undo

---

## Completion Criteria
A command is complete only if:
- code compiles OR
- demo succeeds OR
- decision is recorded

---

## Rule
Agents generate code ONLY when:
- Task is atomic
- Layer is respected
- Dependencies resolved
