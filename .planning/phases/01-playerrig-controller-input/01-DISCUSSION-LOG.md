# Phase 1: PlayerRig & Controller Input - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-13
**Phase:** 01-playerrig-controller-input
**Areas discussed:** PlayerRig Setup, Main Camera Setup, PlayerMovement.cs, Scene Objects, Verification

---

## PlayerRig Setup

| Option | Description | Selected |
|--------|-------------|----------|
| User-specified exact setup | Empty GO at (0,0,0), CharacterController h=1.8, r=0.3, center=(0,0.9,0) | ✓ |

**User's choice:** All parameters locked by user — no alternatives presented
**Notes:** User provided exact CharacterController dimensions. No discussion needed.

---

## Main Camera Setup

| Option | Description | Selected |
|--------|-------------|----------|
| Child of PlayerRig at (0,1.6,0) | Delete default camera, create new as child, tag MainCamera, no rotation scripts | ✓ |

**User's choice:** Exact hierarchy locked — Cardboard XR handles head tracking
**Notes:** User explicitly stated "DO NOT add any MouseLook, CameraController, or rotation scripts"

---

## PlayerMovement.cs

| Option | Description | Selected |
|--------|-------------|----------|
| User-provided verbatim code | Full script with Gamepad.current, leftStick, CharacterController, gravity | ✓ |

**User's choice:** Exact code provided — implement without modification
**Notes:** Script covers all RIG requirements: null check, left stick input, gravity, speed 3f

---

## Scene Objects

| Option | Description | Selected |
|--------|-------------|----------|
| Minimal scene | Default Directional Light + Plane(0,0,0) scale(5,1,5) as test floor | ✓ |

**User's choice:** No additional objects in Phase 1
**Notes:** Test floor is temporary — replaced in Phase 3/4 with environment geometry

---

## Verification

| Option | Description | Selected |
|--------|-------------|----------|
| Editor Play mode test | Left stick → PlayerRig moves, no console errors | ✓ |

**User's choice:** Simple Play mode verification
**Notes:** On-device Cardboard verification deferred to Phase 7

---

## Agent's Discretion

None — all decisions locked by user

## Deferred Ideas

None — discussion stayed within phase scope
