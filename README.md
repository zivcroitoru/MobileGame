# Paw Path

**Team:** Ziv Croitoru  
**Course:** Mobile Game Development, Shenkar

## The game in one sentence
Paw Path is a casual physics-based puzzle game where players swipe to launch hungry pets toward food bowls, earn treats, and return daily to unlock new pets and accessories.

## Architecture (Milestone 3)
![Architecture Diagram](docs/architecture.png)

- **Scenes & Build Order:** `Bootstrap` (index 0) → `Home` ↔ `Level` (`ScreenFlow`).
- **Services & Init Order:** `Bootstrap` initializes `1. Services.Save` (`ISaveService` → `PlayerPrefsSaveService`), then `2. Services.Settings` (`ISettingsService` → `SettingsService`), before loading `Home`.
- **Assemblies:** `Game` references `Services` and `Unity.TextMeshPro`; `Services` has zero references and never touches `Game`.

## Milestone 2: Greybox Screens & Android Build
* **Phone Screen Recording (Home to Level to Home):** [Watch Video](HW2/Video.mp4)
* **Device Simulator — Notched Phone (iPhone 13 Pro):** ![Notched Phone](HW2/iphone13pro.png)
* **Device Simulator — 16:9 Phone (Samsung Galaxy S7):** ![16:9 Phone](HW2/s7.png)

## How to open and build
1. Open the project in Unity 6 LTS (`6000.6.7f1`).
2. File -> Build Profiles -> Android -> Switch Platform.
3. Build and run on an Android phone (Min API 29, IL2CPP + ARM64, Portrait).

## Repo map

| File | What it is |
| --- | --- |
| `Assets/` | Unity scenes (`Bootstrap`, `Home`, `Level`), `Game` & `Services` assemblies, and prefabs |
| `docs/` | Architecture diagram (`architecture.png`) |
| `HW2/` | Milestone 2 phone recording and Device Simulator screenshots |
| `pitch.md` | One-page pitch of the game |
| `DECISIONS.md` | Development decisions and their reasoning |
| `AGENTS.md` | Context for AI coding tools: architecture, rules, conventions |