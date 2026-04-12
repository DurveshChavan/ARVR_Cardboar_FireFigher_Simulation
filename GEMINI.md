<!-- GSD:project-start source:PROJECT.md -->
## Project

**VR Firefighter Training Game**

A mobile VR fire extinguisher training game built for Google Cardboard + Android with Xbox controller input. Players are presented with a selection of two fire scenarios (kitchen gas fire or server room electrical fire), navigate the environment using a Bluetooth Xbox controller, select the correct extinguisher type, and suppress the fire before a 60-second timer expires. Built in Unity 6.3 LTS using the Google Cardboard XR Plugin and Unity's New Input System — phone gyroscope provides head-tracking automatically via Cardboard XR.

**Core Value:** Players correctly identify and apply the right fire extinguisher class to suppress a simulated fire before time runs out — teaching real-world fire safety decision-making in an immersive VR context.

### Constraints

- **Tech Stack**: Unity 6.3 LTS, Google Cardboard XR Plugin, New Input System — non-negotiable
- **Input API**: Only `UnityEngine.InputSystem` and `Gamepad.current` — Old Input Manager disabled
- **Timeline**: 12-hour deadline — strict scope adherence required
- **Platform**: Android + Google Cardboard — no PC VR, no Quest
- **Dependencies**: Google Cardboard XR Plugin must be in Packages/manifest.json
- **Null Safety**: Every input script must check `if (Gamepad.current == null) return;` in Update()
<!-- GSD:project-end -->

<!-- GSD:stack-start source:STACK.md -->
## Technology Stack

Technology stack not yet documented. Will populate after codebase mapping or first phase.
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->
## Project Skills

No project skills found. Add skills to any of: `.agent/skills/`, `.agents/skills/`, `.cursor/skills/`, or `.github/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
