# AI-Assisted Development Process

> Optimized for solo web development. Artifacts and documents are structured for team onboarding.

---

## Phase 1 — Ideation & Discovery

1. **Capture the idea**
   - Write a rough idea brief (Word doc via `docx` skill, or plain markdown)
   - If there's a market/competitor angle: → skill: `Product Management/competitive-brief`

2. **Brainstorm & validate**
   - Think it through with Claude as a sparring partner → skill: `Product Management/product-brainstorming`
   - Output: refined concept, key assumptions, open questions

3. **Create the PRD**
   - Lightweight, non-exhaustive. Focus on problem, goals, non-goals, key flows
   - → skill: `Product Management/write-spec` (use "PRD mode" — avoid over-speccing)
   - Save this as `docs/PRD.md` in your repo

---

## Phase 2 — Architecture & Design

4. **System design**
   - Feed the PRD in → skill: `Engineering/system-design`
   - Iterate and review the proposal → skill: `Engineering/architecture`
   - Output: `docs/ARCHITECTURE.md` (tech stack, data model, service boundaries, feature slices)

5. **Architecture diagrams** ← Figma enters here
   - Generate system/flow diagrams in FigJam → skill: `Figma/figma-generate-diagram`
   - Use FigJam for planning boards, decision trees, or flow mapping → skill: `Figma/figma-use-figjam`
   - These live in your repo or Figma project as reference for future developers

6. **Define testing strategy** ← moved earlier
   - Define approach before implementation: unit/integration/e2e split, coverage targets, tooling
   - → skill: `Engineering/testing-strategy`
   - Output: `docs/TESTING.md`

7. **UI & Frontend design** ← Figma is central here
   a. Design direction and component spec → skill: `Frontend Design/frontend-design`
   b. Write UX copy for key flows → skill: `Design/ux-copy`
   c. Build the design in Figma → skill: `Figma/figma-generate-design`
   d. Build a reusable component library / design system → skill: `Figma/figma-generate-library`
   e. Critique and refine the design → skill: `Design/design-critique`
   f. Accessibility review → skill: `Design/accessibility-review`
   g. Hand off to implementation → skill: `Design/design-handoff`
   h. Connect Figma components to code → skill: `Figma/figma-code-connect`
   ↳ This is the onboarding bridge: future devs can jump from code to Figma design and back

---

## Phase 3 — Specification & Planning

8. **Write the feature spec**
   - Combine PRD + architecture + design handoff into a structured spec
   - → skill: `Product Management/write-spec`
   - Output: `docs/SPEC_v1.md` (versioned, updated incrementally as the project evolves)

9. **Write CLAUDE.md**
   - Generate from the spec + architecture doc
   - This is the AI context file: project overview, conventions, stack, file structure, do's/don'ts
   - Every future Claude Code session loads this — it's your "AI onboarding doc"
   - Keep it updated as the project evolves

10. **Create the roadmap**
    - Break the spec into milestones and rough timelines
    - → skill: `Product Management/roadmap-update`
    - Output: `docs/ROADMAP.md`

11. **Sprint planning**
    - Before each development sprint, slice the roadmap into concrete tasks
    - → skill: `Product Management/sprint-planning`
    - Output: sprint goal, task list, capacity notes (even for solo work, this keeps you honest)

---

## Phase 4 — Implementation

12. **Build incrementally with Claude Code**
    - Work slice by slice, guided by CLAUDE.md
    - Keep slices small enough to review in one session
    - Run standup notes to track progress → skill: `Engineering/standup`

13. **Code review before every push**
    - → skill: `Engineering/code-review`
    - Focus: security, correctness, N+1s, missing edge cases

14. **Manage tech debt continuously**
    - Flag and categorize debt as it accumulates, don't let it pile up
    - → skill: `Engineering/tech-debt` (run periodically, e.g. end of each sprint)

15. **Debug as needed**
    - → skill: `Engineering/debug`

---

## Phase 5 — Documentation

16. **Technical documentation**
    - Generate from `CLAUDE.md` + `SPEC.md` + `ARCHITECTURE.md`
    - → skill: `Engineering/documentation`
    - Covers: API docs, runbooks, architecture decisions, setup guide
    - Output: `docs/TECHNICAL.md`

17. **User handbook**
    - Separate from technical docs — written for end users, not developers
    - Can be generated from the PRD's user flows and UX copy

---

## Phase 6 — Quality & Delivery

18. **Write and run tests**
    - Implement the strategy defined in Phase 2
    - → skill: `Engineering/testing-strategy` (revisit if reality diverged from plan)

19. **Deploy**
    - → skill: `Engineering/deploy-checklist`
    - Covers: CI status, migrations, feature flags, rollback plan

20. **Post-launch: metrics review**
    - → skill: `Product Management/metrics-review`
    - Track against the goals defined in the PRD

21. **Incident response** (as needed)
    - → skill: `Engineering/incident-response`

---

## Key Artifacts (what lives in your repo)

| File                   | Generated from           | Purpose                                  |
| ---------------------- | ------------------------ | ---------------------------------------- |
| `docs/PRD.md`          | Brainstorming            | Problem definition, goals                |
| `docs/ARCHITECTURE.md` | System design            | Tech decisions, data model               |
| `docs/TESTING.md`      | Testing strategy         | Coverage plan, tooling                   |
| `docs/SPEC_v1.md`      | Write-spec               | Feature slices, acceptance criteria      |
| `CLAUDE.md`            | Spec + Architecture      | AI context for every Claude Code session |
| `docs/ROADMAP.md`      | Roadmap skill            | Milestones and timelines                 |
| `docs/TECHNICAL.md`    | Documentation skill      | Developer onboarding                     |
| Figma project          | figma-generate-\* skills | Design system, screens, diagrams         |

---

## Figma Integration Summary

| When                   | What                           | Skill                    |
| ---------------------- | ------------------------------ | ------------------------ |
| Phase 2 — Architecture | System/flow diagrams           | `figma-generate-diagram` |
| Phase 2 — Architecture | Planning boards, brainstorming | `figma-use-figjam`       |
| Phase 2 — UI design    | Generate screens from spec     | `figma-generate-design`  |
| Phase 2 — UI design    | Build component/design system  | `figma-generate-library` |
| Phase 2 — Handoff      | Connect components to code     | `figma-code-connect`     |
| Ongoing                | Edit/update Figma files        | `figma-use`              |
