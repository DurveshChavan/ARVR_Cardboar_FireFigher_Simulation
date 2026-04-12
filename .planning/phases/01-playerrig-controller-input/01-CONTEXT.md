# Phase 1: PlayerRig & Controller Input - Context

**Gathered:** 2026-04-13
**Status:** Ready for planning

<domain>
## Phase Boundary

PlayerRig with CharacterController movement and verified Xbox controller input via New Input System. This phase delivers the foundational player object, camera hierarchy, movement script, and a temporary test floor. Cardboard XR handles head tracking automatically — no camera rotation scripts.

</domain>

<decisions>
## Implementation Decisions

### PlayerRig Setup
- **D-01:** Empty GameObject named "PlayerRig" at world position (0, 0, 0)
- **D-02:** Add CharacterController component: height=1.8, radius=0.3, center=(0, 0.9, 0)
- **D-03:** Attach PlayerMovement.cs script (see exact code below)

### Main Camera Setup
- **D-04:** Delete the default Main Camera from scene root
- **D-05:** Create new Camera as CHILD of PlayerRig at local position (0, 1.6, 0)
- **D-06:** Tag the camera as "MainCamera"
- **D-07:** DO NOT add any MouseLook, CameraController, or rotation scripts — Cardboard XR plugin handles head tracking automatically on device

### PlayerMovement.cs — Exact Implementation (Locked)
- **D-08:** Script attached to PlayerRig, uses `[RequireComponent(typeof(CharacterController))]`
- **D-09:** Movement speed = 3f
- **D-10:** Uses `Gamepad.current` with null check at top of Update()
- **D-11:** Left stick X → `transform.right`, left stick Y → `transform.forward`
- **D-12:** Gravity applied via `Physics.gravity.y * Time.deltaTime` accumulated in `verticalVelocity`
- **D-13:** `cc.Move(move * Time.deltaTime)` for CharacterController movement
- **D-14:** `verticalVelocity` resets to 0 when `cc.isGrounded`
- **D-15:** Complete script provided verbatim by user:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;
    private CharacterController cc;
    private float verticalVelocity = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;

        float h = gp.leftStick.x.ReadValue();
        float v = gp.leftStick.y.ReadValue();

        Vector3 move = transform.right * h + transform.forward * v;
        move *= speed;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);

        if (cc.isGrounded) verticalVelocity = 0f;
    }
}
```

### Scene Objects
- **D-16:** Keep the default Directional Light — no changes
- **D-17:** Add a Plane at (0, 0, 0) with scale (5, 1, 5) as temporary test floor — gets replaced in Phase 3/4
- **D-18:** No other objects needed in Phase 1

### Verification Criteria
- **D-19:** In Play mode: move left stick on Xbox controller → PlayerRig moves in scene view
- **D-20:** Console shows no errors

### Agent's Discretion
- None — all decisions locked by user

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

No external specs — requirements fully captured in decisions above. All implementation details are user-specified verbatim.

### Requirements
- `.planning/REQUIREMENTS.md` — RIG-01 through RIG-07 define acceptance criteria
- `.planning/ROADMAP.md` — Phase 1 success criteria and plan structure

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- No existing scripts — this is the first phase, creating the foundational PlayerMovement.cs

### Established Patterns
- Unity Input System (`com.unity.inputsystem` 1.19.0) is already in `Packages/manifest.json`
- Google Cardboard XR Plugin (`com.google.xr.cardboard`) is already in `Packages/manifest.json`
- URP (`com.unity.render-pipelines.universal` 17.3.0) is the rendering pipeline

### Integration Points
- `VR_Firefighter/Assets/Scripts/` — target directory for PlayerMovement.cs (needs to be created)
- `VR_Firefighter/Assets/Scenes/` — existing scene directory for scene setup
- PlayerRig will be the anchor for all future phase work (camera child for HUD, movement for navigation)

</code_context>

<specifics>
## Specific Ideas

- User provided the exact PlayerMovement.cs code — implement verbatim, no modifications
- CharacterController parameters are exact: height=1.8, radius=0.3, center=(0,0.9,0)
- Camera at 1.6m simulates standing eye height
- Test floor plane is temporary — replaced by environment geometry in Phases 3/4
- No camera rotation scripts whatsoever — Cardboard XR's phone gyroscope handles all head tracking

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-playerrig-controller-input*
*Context gathered: 2026-04-13*
