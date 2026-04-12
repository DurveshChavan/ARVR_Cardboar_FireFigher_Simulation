# Phase 3: Kitchen Environment - Context

**Gathered:** 2026-04-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Kitchen scene geometry (floor, walls, counter, stove, LPG cylinder), fire particle system, extinguisher rack with labels, and wall sign — all using Unity primitives (Cube, Cylinder, Sphere, Plane) with simple colored Materials. Everything is parented under the "Kitchen" root GameObject (already created in Phase 2, set inactive at start). Includes a stub FireController.cs and "FireZone" tag registration.

</domain>

<decisions>
## Implementation Decisions

### Kitchen Root
- **D-01:** Use existing empty GameObject "Kitchen" (created Phase 2), position (0,0,0), DISABLED at start
- **D-02:** All geometry below is created as children of "Kitchen"

### Floor
- **D-03:** Plane, scale (1,1,1), position (0,0,0), grey Material (#808080)

### Walls (4 Cubes)
- **D-04:** WallNorth: Cube, scale (8,3,0.2), position (0,1.5,4), white Material
- **D-05:** WallSouth: Cube, scale (8,3,0.2), position (0,1.5,-4), white Material
- **D-06:** WallEast: Cube, scale (0.2,3,8), position (4,1.5,0), white Material
- **D-07:** WallWest: Cube, scale (0.2,3,8), position (-4,1.5,0), white Material

### Counter & Stove
- **D-08:** Counter: Cube, scale (3,0.9,1), position (-2,0.45,-3), grey Material (#A0A0A0)
- **D-09:** Stove top: Cube, scale (1.2,0.05,0.9), position (-2,0.92,-3), dark grey Material (#303030)

### LPG Cylinder
- **D-10:** Cylinder, scale (0.3,0.4,0.3), position (-1,0.4,-3), red Material (#CC2200)
- **D-11:** Named "LPGCylinder"

### Fire Particle System (on LPGCylinder)
- **D-12:** Add Particle System component (or child empty with Particle System)
- **D-13:** Settings:
  - Start Lifetime: 0.8
  - Start Speed: 1.5
  - Start Size: 0.3
  - Start Color: gradient orange (#FF6600) to yellow (#FFCC00)
  - Emission Rate: 30/second
  - Shape: Sphere, radius 0.1
  - Renderer Material: Default-Particle
  - Looping: true
  - Play On Awake: true
- **D-14:** Add Sphere Collider to the fire object: radius 0.4, Is Trigger: true
- **D-15:** Tag this object: "FireZone" — register "FireZone" in Unity's Tag Manager before assignment
- **D-16:** Attach FireController.cs stub (see D-30 below)

### Extinguisher Rack (3 cylinders near WallWest)
- **D-17:** ExtRack_DCP: Cylinder, scale (0.12,0.5,0.12), position (-3.5,0.5,2), red Material
- **D-18:** ExtRack_CO2: Cylinder, scale (0.12,0.5,0.12), position (-3.5,0.5,2.4), black Material (#111111)
- **D-19:** ExtRack_Water: Cylinder, scale (0.12,0.5,0.12), position (-3.5,0.5,2.8), blue Material (#2244CC)
- **D-20:** Small label Cubes above each extinguisher: scale (0.3,0.05,0.15)
- **D-21:** Label sign on wall: Cube, scale (0.8,0.3,0.05), with child 3D TMP_Text reading "DCP | CO2 | Water"

### Wall Sign (on WallSouth, facing inward)
- **D-22:** Cube: scale (2,0.8,0.05), position (0,2,-3.9), yellow Material (#FFEE00)
- **D-23:** Child TMP_Text (3D Text):
  - Text: "GAS LEAK — LPG Cylinder\nClass C Fire\nSelect correct extinguisher"
  - Font size: 4
  - Color: black
  - Center aligned
  - Overflow: overflow

### Materials (Create → Material, simple colored)
- **D-24:** All materials are simple solid-color URP/Lit materials created via Create → Material
- **D-25:** Material list:
  - Grey (#808080) — floor
  - White — walls
  - Light grey (#A0A0A0) — counter
  - Dark grey (#303030) — stove top
  - Red (#CC2200) — LPG cylinder, DCP extinguisher
  - Black (#111111) — CO2 extinguisher
  - Blue (#2244CC) — Water extinguisher
  - Yellow (#FFEE00) — wall sign

### FireZone Tag Registration
- **D-26:** Register "FireZone" as a custom tag in Unity's TagManager (ProjectSettings/TagManager.asset)
- **D-27:** Apply "FireZone" tag to the fire particle system object on LPGCylinder

### FireController.cs — Stub (Locked)
- **D-28:** Create stub FireController.cs in Assets/Scripts/ — follows same pattern as GameManager.cs stub from Phase 2
- **D-29:** Stub contains placeholder methods that Phase 5 will implement (fire suppression logic)
- **D-30:** Attach to the fire object on LPGCylinder so Inspector reference is wired

### No Imported Assets
- **D-31:** ONLY Unity primitives (Cube, Cylinder, Sphere, Plane) — no imported 3D models, textures, or asset store packages
- **D-32:** Default-Particle material for particle renderer (built-in Unity material)

### Agent's Discretion
- None — all decisions locked by user

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

No external specs — requirements fully captured in decisions above. All implementation details are user-specified verbatim.

### Requirements
- `.planning/REQUIREMENTS.md` — KIT-01 through KIT-05 define acceptance criteria
- `.planning/ROADMAP.md` — Phase 3 success criteria and plan structure

### Prior Phase Context
- `.planning/phases/01-playerrig-controller-input/01-CONTEXT.md` — PlayerRig setup, camera at 1.6m height
- `.planning/phases/02-scenario-selection-screen/02-CONTEXT.md` — Kitchen root GameObject (D-20, set inactive), ScenarioSelector wiring, GameManager.cs stub pattern

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Scripts/PlayerMovement.cs` — Gamepad.current pattern from Phase 1
- `Assets/Scripts/Billboard.cs` — simple Update() component from Phase 2
- `Assets/Scripts/ScenarioSelector.cs` — references Kitchen root, activates on A button
- `Assets/Scripts/GameManager.cs` — stub with Scenario enum (Kitchen, ServerRoom), follows same stub pattern for FireController.cs

### Established Patterns
- `Gamepad.current` with null check in Update() — mandatory for all input scripts
- Stub scripts with placeholder methods and Debug.Log — used for GameManager.cs in Phase 2
- Unity primitives only — no imported assets
- URP rendering pipeline (`com.unity.render-pipelines.universal` 17.3.0)
- TextMeshPro available for 3D text (TMP_Text)

### Integration Points
- `Assets/Scripts/` — target directory for FireController.cs stub
- "Kitchen" root GameObject (Phase 2) — all geometry parents to this
- ScenarioSelector.cs references `kitchenRoot` — the Kitchen object this phase populates
- FireController.cs will be extended in Phase 5 with full suppression logic
- "FireZone" tag used by SprayController.cs in Phase 5 for trigger detection

</code_context>

<specifics>
## Specific Ideas

- User provided exact positions, scales, and hex colors for every object — implement verbatim
- Particle system settings are precise: lifetime 0.8, speed 1.5, size 0.3, emission 30/s, sphere shape radius 0.1
- Sphere Collider on fire object is specifically radius 0.4, isTrigger=true — used by Phase 5 spray detection
- TMP_Text for both wall sign and rack label — 3D Text variant, not UI Text
- FireController.cs stub follows same pattern as GameManager.cs stub from Phase 2 (placeholder with Debug.Log)
- Default-Particle is a built-in Unity material — no need to create custom particle material
- Wall sign positioned at (0,2,-3.9) — 0.1 offset from WallSouth at z=-4 so sign is on the wall surface

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 03-kitchen-environment*
*Context gathered: 2026-04-13*
