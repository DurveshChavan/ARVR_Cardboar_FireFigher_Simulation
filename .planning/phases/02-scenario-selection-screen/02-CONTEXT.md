# Phase 2: Scenario Selection Screen - Context

**Gathered:** 2026-04-13
**Status:** Ready for planning

<domain>
## Phase Boundary

World-space selection canvas with two scenario cards (Kitchen / Server Room) and A/B controller button input to activate the correct environment. SelectionScreen is visible at game start, deactivated on selection. Kitchen and ServerRoom root GameObjects start inactive and are activated on selection.

</domain>

<decisions>
## Implementation Decisions

### SelectionScreen Canvas Setup
- **D-01:** World Space Canvas named "SelectionScreen"
- **D-02:** Position: (0, 1.5, 3) — 3m in front of PlayerRig start, at 1.5m height
- **D-03:** Canvas RectTransform: Width=400, Height=200
- **D-04:** Scale: (0.005, 0.005, 0.005) — readable in world space
- **D-05:** Attach Billboard.cs script to SelectionScreen so it always faces the player

### Card Layout
- **D-06:** Two Panel (Image) children inside SelectionScreen canvas
- **D-07:** KitchenCard: Panel at local position (-80, 0) in canvas space, orange color (#E8632A)
  - Child TMP_Text: "Kitchen Fire\nPress A", white, font size 24, center aligned
- **D-08:** ServerCard: Panel at local position (+80, 0) in canvas space, blue color (#2A6BE8)
  - Child TMP_Text: "Server Room Fire\nPress B", white, font size 24, center aligned

### Billboard.cs — Exact Implementation (Locked)
- **D-09:** Attached to SelectionScreen GameObject
- **D-10:** In Update(), if Camera.main != null, does `transform.LookAt(Camera.main.transform)`
- **D-11:** Complete script provided verbatim:

```csharp
using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }
}
```

### ScenarioSelector.cs — Exact Implementation (Locked)
- **D-12:** Attached to GameManager object (or new empty GameObject)
- **D-13:** Public references: selectionScreen, kitchenRoot, serverRoomRoot, gameManager (GameManager)
- **D-14:** In Update(), checks selectionScreen is active; if not, returns
- **D-15:** Gamepad.current null check — `if (gp == null) return;`
- **D-16:** buttonSouth (A) → StartScenario(kitchenRoot, GameManager.Scenario.Kitchen)
- **D-17:** buttonEast (B) → StartScenario(serverRoomRoot, GameManager.Scenario.ServerRoom)
- **D-18:** StartScenario() deactivates selectionScreen, activates scenarioRoot, calls gameManager.StartGame(scenario)
- **D-19:** Complete script provided verbatim:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioSelector : MonoBehaviour
{
    public GameObject selectionScreen;
    public GameObject kitchenRoot;
    public GameObject serverRoomRoot;
    public GameManager gameManager;

    void Update()
    {
        if (selectionScreen == null || !selectionScreen.activeSelf) return;

        var gp = Gamepad.current;
        if (gp == null) return;

        if (gp.buttonSouth.wasPressedThisFrame)
            StartScenario(kitchenRoot, GameManager.Scenario.Kitchen);

        if (gp.buttonEast.wasPressedThisFrame)
            StartScenario(serverRoomRoot, GameManager.Scenario.ServerRoom);
    }

    void StartScenario(GameObject scenarioRoot, GameManager.Scenario scenario)
    {
        selectionScreen.SetActive(false);
        scenarioRoot.SetActive(true);
        gameManager.StartGame(scenario);
    }
}
```

### Scene Root Objects
- **D-20:** Create empty GameObject "Kitchen" — SET INACTIVE at start
- **D-21:** Create empty GameObject "ServerRoom" — SET INACTIVE at start
- **D-22:** Wire up all references in Inspector once objects are created

### GameManager Dependency
- **D-23:** ScenarioSelector references `GameManager.Scenario` enum and `GameManager.StartGame(scenario)` — GameManager.cs must define these. Create a minimal stub GameManager.cs in this phase with the Scenario enum and StartGame() method so ScenarioSelector compiles. Full GameManager logic is Phase 5.

### Agent's Discretion
- None — all decisions locked by user

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

No external specs — requirements fully captured in decisions above. All implementation details are user-specified verbatim.

### Requirements
- `.planning/REQUIREMENTS.md` — SEL-01 through SEL-06 define acceptance criteria
- `.planning/ROADMAP.md` — Phase 2 success criteria and plan structure

### Prior Phase Context
- `.planning/phases/01-playerrig-controller-input/01-CONTEXT.md` — PlayerRig setup, camera hierarchy, PlayerMovement.cs (Phase 2 builds on this)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Scripts/PlayerMovement.cs` — existing script from Phase 1, uses Gamepad.current pattern
- PlayerRig with CharacterController and Main Camera child already in scene

### Established Patterns
- `Gamepad.current` with null check at top of Update() — mandatory pattern (see D-15)
- Unity Input System (`com.unity.inputsystem` 1.19.0) in packages
- Google Cardboard XR Plugin in packages
- URP rendering pipeline

### Integration Points
- `Assets/Scripts/` — target directory for Billboard.cs, ScenarioSelector.cs, GameManager.cs stub
- SelectionScreen canvas placed relative to PlayerRig start position (0,0,0) → canvas at (0,1.5,3)
- Kitchen and ServerRoom root objects will be parents for all environment geometry in Phases 3/4
- GameManager stub created here will be extended in Phase 5 with full game logic

</code_context>

<specifics>
## Specific Ideas

- User provided exact Billboard.cs and ScenarioSelector.cs code — implement verbatim, no modifications
- Canvas scale (0.005) is specifically tuned for world-space readability in Cardboard VR
- Card colors are intentional: orange (#E8632A) for kitchen/fire, blue (#2A6BE8) for server/electrical
- SelectionScreen at 1.5m height places it naturally at eye level for the 1.6m camera
- ScenarioSelector references GameManager.Scenario enum — need stub GameManager to compile

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 02-scenario-selection-screen*
*Context gathered: 2026-04-13*
