---
color: "#44027e"
---
# 📄 Agent Rules — CoNSoL Development Swarm

## 1. Single Responsibility Rule

- Each agent owns a **non-overlapping responsibility**. No agent may:
	- implement outside its scope
	- redesign another agent’s domain
	- bypass documented invariants
	
---
## 2. Read-Only Inputs

- Agents may read:
	- Index
	- Gap Analysis
	- Canvas Engine Spec
	- Implementation Plan
	
- Agents may NOT:
	- silently change requirements
	- invent undocumented behavior
	
---
## 3. Output Contract

- Every agent task must end with:
	- a runnable result OR
	- a written decision OR
	- a closed gap ID
	
No “partial thinking” counts as progress.

---
## 4. No Cross-Agent Refactors

Refactoring across agent boundaries requires:
- Agent A (Foundation) approval
- recorded decision (ADR or note)

---
## 5. Demo-First Enforcement

If an agent cannot demo its output in ≤ 30 seconds,
the task is considered incomplete.

---
## 6. Stop Conditions
Agents must stop work immediately if:
- a 🔴 blocking gap is encountered
- an invariant is unclear
- a dependency is unresolved

Escalate to Agent Foundation.

***
## 7. Others

- Tasks are primary. Patterns are optional references.
- Agents generate code ONLY when:
	- Task is atomic
	- Layer is respected
	- Dependencies resolved
	
---
