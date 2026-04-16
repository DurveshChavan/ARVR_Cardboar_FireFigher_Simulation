# VR Firefighter Training Game
**Project Presentation Reference Document**

This document serves as the foundational reference material for creating a PowerPoint presentation for the VR Firefighter Training Game project. 

---

## 1. Project Overview
**Title:** VR Firefighter Training Game  
**Platform:** Mobile VR (Google Cardboard for Android)  
**Input:** Bluetooth Xbox Controller (Gamepad) + Device Gyroscope  
**Engine:** Unity 6.3 LTS  
**Premise:** An immersive, timed VR simulation that challenges players to correctly identify and suppress different classes of fires (Kitchen Gas Fire vs. Server Room Electrical Fire) using the appropriate type of fire extinguisher before a 60-second timer expires.

---

## 2. Core Value & Objective
- **Educational Goal:** Teach real-world fire safety decision-making in a safe, immersive VR context.
- **Cognitive Training:** Players learn to quickly assess a fire type (Class C Electrical vs. Class B/Gas) and select the correct suppression tool (CO2 vs. Dry Chemical) under simulated time pressure.
- **Engagement:** Gamification of safety protocols through a 60-second countdown timer and realistic visual/audio feedback.

---

## 3. Technology Stack & Architecture
- **Game Engine:** Unity 6.3 LTS
- **VR Framework:** Google Cardboard XR Plugin
- **Input System:** Unity's New Input System (`UnityEngine.InputSystem`)
- **Locomotion:** Custom Rigidbody-based Character Controller
- **Target OS:** Android (Deployed standalone via APK, tested on Samsung A54)

---

## 4. Key Gameplay Mechanics & Features
### A. VR Interactions and Locomotion
- **Head Tracking:** 3DOF (Degrees of Freedom) tracking powered automatically by the phone's gyroscope via the Cardboard XR Plugin.
- **Gamepad Movement:** Players use a Bluetooth-connected Xbox controller's Left Stick to navigate the environments, allowing them to close the distance to the fire safely.

### B. Scenario Selection Screen
- A "hub" world-space UI canvas where the player begins.
- Players press Gamepad face buttons (A or B) to choose between the two training scenarios.
- Smooth transition directly into the loaded environment.

### C. Training Scenarios
1. **The Kitchen Scenario:** A simulated domestic setting with a gas fire originating from an LPG cylinder. Requires a specific approach and extinguisher type.
2. **The Server Room Scenario:** A simulated commercial IT environment with an electrical fire. Requires a CO2 extinguisher to avoid "damaging" server equipment and personal shock hazards.

### D. Extinguisher Weapon System
- **Unified Mechanic:** The player equips an extinguisher, aims it dynamically, and pulls the Right Trigger (`RT`) to fire the particle stream.
- **Collision Detection:** A raycast/particle collision system that validates whether the extinguishing agent is hitting the primary "FireZone" trigger.
- **Suppression Logic:** Fire particle systems visually shrink and intensity decreases as the player successfully applies the extinguisher over time until the fire is completely neutralized.

---

## 5. Development Constraints & Achievements
- **Strict Timeline:** The application was built rapidly within a strict 12-hour deadline, requiring heavy prioritization of core mechanics over extraneous polish.
- **Null Safety & Stability:** Overcame deep Android/JNI and `SIGTRAP` crashes during initialization by strictly enforcing `Gamepad.current` null-safety checks and careful management of Cardboard XR lifecycle and scene loading.
- **Input Evolution:** Successfully deprecated the old Input Manager in favor of exclusively utilizing Unity's modern New Input System for all interactions.

---

## 6. Suggestions for PowerPoint Slide Breakdown
Here is a recommended structure for the presentation:
1. **Title Slide:** Project Name, Team/Developer Name.
2. **The Problem/Premise:** Why VR fire training? Traditional training is expensive/dangerous.
3. **The Solution:** A mobile, accessible Google Cardboard game utilizing gamepads.
4. **Core Tech Stack:** Unity 6.3, Cardboard XR, Android.
5. **Gameplay Loop & Mechanics:** Walkthrough of scenario selection -> movement -> extinguisher usage.
6. **The Scenarios:** Highlight the differences between the Kitchen (Gas) and Server Room (Electrical) fires.
7. **Development Journey:** Mention the 12-hour deadline constraint and how stability (Android crash resolutions) was achieved.
8. **Demo/Video:** (Placeholder for a recorded gameplay session).
9. **Q&A / Conclusion.**
