# Phase 04: Server Room Environment - Context

**Gathered:** 2026-04-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Build the Server Room environment geometry using Unity primitives, setup the electrical fire source on the middle server rack, configure the extinguisher rack (same as kitchen), and place the wall sign warning about electrical fires. Scene root toggled DISABLED matching previous phase setups.

</domain>

<decisions>
## Implementation Decisions

### Server Room Layout & Geometry
- **Root Object:** "ServerRoom", position (0,0,0), DISABLED at start.
- **Floor:** Plane, scale (1,1,1), position (0,0,0), dark grey Material (#2A2A2A).
- **Walls:** 4 Cubes, same dimensions as kitchen walls, dark Material (#1A1A1A).

### Server Racks
- Three server racks (Rack_Left, Rack_Middle, Rack_Right), dark grey (#303030).
- Scale: (0.6,2,0.8). Positions: (-1.5,1,1), (0,1,1), (1.5,1,1) respectively.
- Decorations: Tiny Cubes scale (0.05,0.05,0.05) on front face for LEDs, alternating green (#00FF00) and red (#FF0000).

### Environment Props
- **Cable Tray:** Cube scale (4,0.1,0.3), position (0,2.1,1), dark grey (#404040).
- **UPS Unit:** Cube scale (0.5,1,0.4), position (3,0.5,1), black (#0A0A0A).
- **Office Chair:** Seat Cube scale (0.5,0.1,0.5) position (2.5,0.5,-1); Back Cube scale (0.5,0.6,0.05) position (2.5,0.8,-1.2). Both dark grey.

### Fire Source (Electrical)
- Fire on **Rack_Middle**, child empty at top of rack. Local position to Rack_Middle: (0,2.1,0).
- Particle System:
  - Start Lifetime: 0.5, Start Speed: 2, Start Size: 0.15
  - Start Color: gradient white (#FFFFFF) to blue (#0044FF)
  - Emission Rate: 50/second
  - Shape: Box X=0.4, Y=0.1, Z=0.2
  - Renderer Material: Default-Particle
  - Looping: true, Play On Awake: true
- Attach Box Collider: size (0.6,0.5,0.8), Is Trigger: true. Tag: "FireZone".
- Attach `FireController.cs`.

### Interactive Setup
- **Extinguisher Rack:** Position (-3.5,0.5,2) near WallWest. Same 3 cylinders as Kitchen (DCP red, CO2 black, Water blue).
- **Wall Sign:** On WallSouth, Cube scale (2.2,0.9,0.05), position (0,2,-3.9), red (#CC0000).
- **Sign Text:** Child TMP_Text. Font size 4, color white, center aligned.
  Text string: "ELECTRICAL FIRE\nServer Rack Short Circuit\nDO NOT USE WATER\nCO2 ONLY"

### the agent's Discretion
- Exact layout/spacing of the tiny LED cubes on the rack fronts.
- Tying everything neatly into the ServerRoom parent hierarchy.

</decisions>

<canonical_refs>
## Canonical References

No external specs — requirements are fully captured in decisions above based on User's explicit prompt specification for Phase 4.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `FireController.cs` (stubbed from Kitchen environment phase).
- Wall dimensions/prefabs and Extinguisher cylinder setup patterns from Phase 3 (Kitchen).

</code_context>

<specifics>
## Specific Ideas

- The scene must duplicate the Kitchen's logical layout but represent a Server Room with electrical fire context (blue/white flames vs standard orange).
- Relies *only* on Unity primitives.

</specifics>

<deferred>
## Deferred Ideas

- None — discussion stayed entirely within the required geometry bounds.

</deferred>

---

*Phase: 04-server-room-environment*
*Context gathered: 2026-04-13*
