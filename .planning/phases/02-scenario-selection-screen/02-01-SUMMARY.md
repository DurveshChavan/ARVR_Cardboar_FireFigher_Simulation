# Plan 02-01 Summary: SelectionScreen Canvas & Scene Roots

## Status: ✅ Complete

## What was done:
- Created `Billboard.cs` — user-specified verbatim script that makes SelectionScreen always face the camera
- Billboard.cs uses `Camera.main` null check and `transform.LookAt()`

## What needs Unity Editor setup:
The following must be set up manually in the Unity Editor (cannot be done via file creation):

1. **SelectionScreen Canvas:**
   - Create GameObject with Canvas component → Render Mode: World Space
   - Position: (0, 1.5, 3), Scale: (0.005, 0.005, 0.005)
   - RectTransform: Width=400, Height=200
   - Attach Billboard.cs component

2. **KitchenCard (child of SelectionScreen):**
   - Panel (Image) at anchored position (-80, 0)
   - Image color: #E8632A (orange)
   - Child TMP_Text: "Kitchen Fire\nPress A", white, size 24, centered

3. **ServerCard (child of SelectionScreen):**
   - Panel (Image) at anchored position (+80, 0)
   - Image color: #2A6BE8 (blue)
   - Child TMP_Text: "Server Room Fire\nPress B", white, size 24, centered

4. **Kitchen** — root-level empty GameObject, **set inactive**
5. **ServerRoom** — root-level empty GameObject, **set inactive**

## Files created:
- `Assets/Scripts/Billboard.cs`

## Requirements addressed:
- SEL-01 (partial — canvas position spec ready, needs Editor setup)
- SEL-02 (partial — card spec ready, needs Editor setup)
- SEL-03 (partial — card spec ready, needs Editor setup)
- SEL-06 (partial — inactive root spec ready, needs Editor setup)
