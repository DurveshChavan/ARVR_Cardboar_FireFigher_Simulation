# Phase 06: HUD Canvas - Context

**Gathered:** 2026-04-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Build the front-end HUD interface for the VR Player leveraging World Space Canvas mounted to the VR Main Camera constraint. Implement dynamic text overlays for the timer, contextual extinguisher warnings/information, and mission outcome state.
</domain>

<decisions>
## Implementation Decisions

### Canvas Structure
- **Root:** "HUD_Canvas", `World Space` render mode.
- **Parenting:** Child of the `Main Camera` (NOT PlayerRig) so it moves with the head.
- **Transform:** 
  - Width: 1920, Height: 1080 
  - Scale: (0.0005, 0.0005, 0.0005)
  - Local Position: (0, 0, 1.5)
  - Local Rotation: (0, 0, 0)

### UI Elements (TextMeshPro 3D)
1. **TimerText**:
   - Location: Anchor top center (0.5, 1). Local Pos: (0, -60, 0)
   - Style: Size 80, White, Align Center
   - Initial Text: "Time: 60s" (Active at start)
2. **ExtinguisherText**:
   - Location: Anchor bottom left (0, 0). Local Pos: (200, 100, 0)
   - Style: Size 50, Yellow (#FFEE00), Align Left, Width 900
   - Initial Text: empty (Inactive at start)
3. **ResultText**:
   - Location: Anchor center (0.5, 0.5). Local Pos: (0, 0, 0)
   - Style: Size 100, White, Align Center, Width 1600, Word Wrap enabled
   - Initial Text: empty (Inactive at start)

### Verification
- Ensure Phase 2 `SelectionScreen` canvas is decoupled from the camera and floats independently at `(0, 1.5, 3)` in World Space.
</decisions>

<canonical_refs>
## Canonical References
Requirements provided directly inline during Phase 06 discussion.
</canonical_refs>

<code_context>
## Existing Code Insights
### Reusable Assets
- `GameManager` script exposes `TMP_Text` serializable fields for `timerText`, `extText`, and `resultText`.
</code_context>

<specifics>
## Specific Ideas
- Implementing via Unity Editor Script minimizes manual Canvas/RectTransform GUI interaction and guarantees identical scale/anchors as documented.
</specifics>

<deferred>
## Deferred Ideas
None.
</deferred>
