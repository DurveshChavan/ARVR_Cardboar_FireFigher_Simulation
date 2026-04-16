# ANTIGRAVITY FIX PROMPTS — VR Firefighter
# These fix the existing project. No rebuilding from scratch.
# Use one new conversation per prompt. /clear between each.

================================================================================
FIX PROMPT 1 — Scene Wiring (Most Critical — Do This First)
================================================================================

Paste in a new Antigravity conversation:

---

/gsd-quick

The VR Firefighter project at C:\AR_SCE\VR_Firefighter has all scripts created
but the Unity scene is NOT wired. Fix this by creating ONE Editor script that
wires the entire scene automatically when run from the Unity menu.

DO NOT rewrite any existing scripts. Only create this one Editor script.

Create: Assets/Editor/MasterSceneWirer.cs

This script adds a menu item "VR Firefighter > Wire Entire Scene" that does ALL
of the following when clicked:

STEP 1 — Ensure GameManager object exists and has correct scripts:
- Find or create empty GameObject named "GameManager" at (0,0,0)
- Ensure it has GameManager.cs component (AddComponent if missing)
- Ensure it has ScenarioSelector.cs component (AddComponent if missing)
- Ensure it has ExtinguisherController.cs component (AddComponent if missing)
- Ensure it has SprayController.cs component (AddComponent if missing)

STEP 2 — Wire ScenarioSelector references via reflection/SerializedObject:
- Find GameObject named "SelectionScreen" → assign to ScenarioSelector.selectionScreen
- Find GameObject named "Kitchen" → assign to ScenarioSelector.kitchenRoot
- Find GameObject named "ServerRoom" → assign to ScenarioSelector.serverRoomRoot
- Find GameManager component on same object → assign to ScenarioSelector.gameManager

STEP 3 — Wire GameManager UI references:
- Find GameObject named "TimerText" → get TMP_Text component → assign to GameManager.timerText
- Find GameObject named "ExtinguisherText" → get TMP_Text → assign to GameManager.extText
- Find GameObject named "ResultText" → get TMP_Text → assign to GameManager.resultText

STEP 4 — Wire PlayerRig scripts:
- Find GameObject named "PlayerRig"
- Ensure it has ExtinguisherController.cs (AddComponent if missing)
- Ensure it has SprayController.cs (AddComponent if missing)

STEP 5 — Wire FireController on Kitchen fire:
- Find GameObject named "LPGCylinder" in scene (search all children including inactive)
- If it has a child with ParticleSystem, ensure that child has FireController.cs
- If LPGCylinder itself has ParticleSystem, ensure FireController.cs is on it

STEP 6 — Wire FireController on ServerRoom fire:
- Find GameObject named "FireSource" in scene (search all children including inactive)
- Ensure it has FireController.cs (AddComponent if missing)

STEP 7 — Ensure correct active/inactive states:
- Find "Kitchen" → SetActive(false)
- Find "ServerRoom" → SetActive(false)
- Find "SelectionScreen" → SetActive(true)

STEP 8 — Wire HUD if it exists:
- Find "HUD_Canvas" → ensure it is child of Main Camera
- If not child of Main Camera, reparent it

STEP 9 — Run the existing editor scripts if they exist:
- Check if "VR Firefighter/Build Kitchen Environment" menu exists → if yes call it
- Check if "VR Firefighter/Build Server Room Environment" menu exists → if yes call it
- Check if "VR Firefighter/Build HUD Canvas" menu exists → if yes call it
- Check if "VR Firefighter/Wire Component Logic" menu exists → if yes call it

STEP 10 — Log a clear summary:
- Print what was found, what was created, what was wired, what was missing
- End with: "Scene wiring complete. Check Console for any MISSING items."

Use UnityEditor, UnityEngine, SerializedObject/SerializedProperty for wiring
serialized fields. Use GameObject.Find for finding objects. Use
Resources.FindObjectsOfTypeAll<GameObject>() to find inactive objects too.

After creating this script, tell me exactly:
1. What menu path to click in Unity
2. What to check in the Console after clicking it
3. What to manually fix if anything shows as MISSING

---

================================================================================
FIX PROMPT 2 — Fix GameManager.cs (if scripts have compile errors)
================================================================================

Only use this if Unity Console shows red errors after running Prompt 1.
Paste in a new conversation:

---

/gsd-quick

Fix compile errors in the VR Firefighter project at C:\AR_SCE\VR_Firefighter.
DO NOT rewrite from scratch. Only fix the specific errors listed below.

[PASTE YOUR UNITY CONSOLE ERROR TEXT HERE]

Rules:
- Keep all existing logic exactly as is
- Only fix what the error says
- Use UnityEngine.InputSystem only — never Input.GetAxis or Input.GetButton
- All Gamepad input must use Gamepad.current with null check

---

================================================================================
FIX PROMPT 3 — Fix SprayController (fire not going out)
================================================================================

Paste in a new conversation AFTER Prompt 1 is done and scene is wired:

---

/gsd-quick

In the VR Firefighter project at C:\AR_SCE\VR_Firefighter, the fire does not
go out when the player holds LT + RT with the correct extinguisher.

Fix ONLY Assets/Scripts/SprayController.cs. Do not touch any other file.

The issue is likely one of:
1. The triggers threshold is too high (change to 0.1f instead of 0.3f)
2. GameManager.Instance is null because GameManager is not finding itself
3. currentFireController is null because StartGame() was called before
   FireController exists in scene

Apply ALL these fixes to SprayController.cs:

Fix 1 — Lower trigger threshold:
  Change: float lt = gp.leftTrigger.ReadValue(); bool aiming = lt > 0.3f;
  To:     float lt = gp.leftTrigger.ReadValue(); bool aiming = lt > 0.1f;
  Same for rt/spraying threshold — change to 0.1f

Fix 2 — Add null safety for GameManager:
  At top of Update() add:
  if (GameManager.Instance == null) return;
  if (!GameManager.Instance.gameActive) return;

Fix 3 — Add null safety for FireController with fallback search:
  After checking fire == null, add:
  if (fire == null) {
      fire = FindObjectOfType<FireController>();
      if (fire != null) GameManager.Instance.currentFireController = fire;
  }
  if (fire == null) { Debug.LogWarning("SprayController: No FireController found"); return; }

Fix 4 — Add Debug.Log so you can see it working:
  After the rate is calculated, add:
  Debug.Log("Spraying: ext=" + ext + " scenario=" + scenario + " rate=" + rate);
  After fire.ReduceFire(rate), add:
  Debug.Log("Fire scale: " + fire.fireScale);

---

================================================================================
FIX PROMPT 4 — Fix Scenario Selection (A/B not working)
================================================================================

Only use this if pressing A or B does nothing. Paste in a new conversation:

---

/gsd-quick

In the VR Firefighter project at C:\AR_SCE\VR_Firefighter, pressing A and B
on the Xbox controller does not load the Kitchen or Server Room scenarios.

Fix ONLY Assets/Scripts/ScenarioSelector.cs. Do not touch any other file.

Apply these fixes:

Fix 1 — Make references public so they can be assigned in Inspector OR found automatically:
  Change all private field declarations to:
  [SerializeField] private GameObject selectionScreen;
  [SerializeField] private GameObject kitchenRoot;
  [SerializeField] private GameObject serverRoomRoot;
  [SerializeField] private GameManager gameManagerRef;

Fix 2 — Add self-wiring in Start() as fallback:
  void Start()
  {
      // Fallback: find by name if not assigned in Inspector
      if (selectionScreen == null) selectionScreen = GameObject.Find("SelectionScreen");
      if (kitchenRoot == null)     kitchenRoot = GameObject.Find("Kitchen");
      if (serverRoomRoot == null)  serverRoomRoot = FindInactive("ServerRoom");
      if (gameManagerRef == null)  gameManagerRef = FindObjectOfType<GameManager>();

      // Ensure correct start state
      if (kitchenRoot != null)     kitchenRoot.SetActive(false);
      if (serverRoomRoot != null)  serverRoomRoot.SetActive(false);
      if (selectionScreen != null) selectionScreen.SetActive(true);

      // Debug
      Debug.Log("ScenarioSelector ready. Kitchen=" + (kitchenRoot != null) +
                " ServerRoom=" + (serverRoomRoot != null) +
                " SelectionScreen=" + (selectionScreen != null));
  }

  // Add this helper method to find inactive GameObjects:
  GameObject FindInactive(string name)
  {
      var all = Resources.FindObjectsOfTypeAll<GameObject>();
      foreach (var go in all)
          if (go.name == name && go.scene.isLoaded) return go;
      return null;
  }

Fix 3 — Add Debug.Log in Update to confirm input is being read:
  In Update(), after null check, add:
  if (gp.buttonSouth.wasPressedThisFrame) Debug.Log("A pressed - loading Kitchen");
  if (gp.buttonEast.wasPressedThisFrame)  Debug.Log("B pressed - loading ServerRoom");

Fix 4 — Make sure gameManager reference uses the fallback:
  In LoadScenario(), change GameManager.Instance to gameManagerRef:
  gameManagerRef.StartGame(scenario);

---

================================================================================
FIX PROMPT 5 — Fix Extinguisher Cycling (Y not working)
================================================================================

Only use this if pressing Y does not cycle extinguishers. Paste in new conversation:

---

/gsd-quick

In the VR Firefighter project at C:\AR_SCE\VR_Firefighter, pressing Y on the
Xbox controller does not cycle the extinguisher or show any popup text.

Fix ONLY Assets/Scripts/ExtinguisherController.cs. Do not touch any other file.

Apply these fixes:

Fix 1 — Add Debug.Log to confirm Y is being detected:
  In Update(), after the null check for Gamepad.current, add:
  if (gp.buttonNorth.wasPressedThisFrame)
      Debug.Log("Y pressed - cycling extinguisher");

Fix 2 — Add null safety for GameManager:
  At top of the Y-press block:
  if (GameManager.Instance == null) { Debug.LogWarning("GameManager.Instance is null"); return; }

Fix 3 — Ensure this script is on an active GameObject:
  This script MUST be on either PlayerRig or GameManager object which are always active.
  Add a check in Start():
  void Start()
  {
      Debug.Log("ExtinguisherController started on: " + gameObject.name);
  }

---

================================================================================
ORDER TO USE THESE PROMPTS
================================================================================

1. Start with FIX PROMPT 1 always — run the menu item in Unity
2. Read the Console output carefully
3. If you see red compile errors → use FIX PROMPT 2 with the error text
4. Test in Play mode with Xbox controller connected:
   - Press A or B → if nothing happens → use FIX PROMPT 4
   - Press Y → if nothing happens → use FIX PROMPT 5
   - Hold LT + RT → if fire doesn't shrink → use FIX PROMPT 3
5. Check Console (Window → General → Console) while testing — the Debug.Logs
   will tell you exactly what is and isn't working

================================================================================
MANUAL CHECKS TO DO IN UNITY BEFORE RUNNING PROMPTS
================================================================================

Before running any Antigravity prompt, do these checks yourself in Unity:

1. Click the GameManager object in Hierarchy → Inspector
   - Does it show ScenarioSelector component? ← must be YES
   - Does ScenarioSelector show Kitchen/ServerRoom/SelectionScreen slots filled?
   - If slots are EMPTY → run FIX PROMPT 1

2. Click PlayerRig in Hierarchy → Inspector
   - Does it show ExtinguisherController component?
   - Does it show SprayController component?
   - If missing either → run FIX PROMPT 1

3. Click LPGCylinder (inside Kitchen) → Inspector
   - Does it have a FireController component?
   - If missing → run FIX PROMPT 1

4. Click FireSource (inside ServerRoom/Rack_Middle) → Inspector
   - Does it have a FireController component?
   - If missing → run FIX PROMPT 1

5. Click HUD_Canvas → Inspector → look at the top
   - Is it a child of Main Camera in the Hierarchy?
   - If not → drag it onto Main Camera in Hierarchy panel