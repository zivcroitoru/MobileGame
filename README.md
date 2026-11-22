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

## Milestone 4: Economy

- [Paw Path Economy Sheet](https://docs.google.com/spreadsheets/d/1IdBvFvmOmEKmR6OERU41mmjjL7zqFvws/edit?usp=sharing&ouid=114859143678000248323&rtpof=true&sd=true)
- **Check 1:** First upgrade within the first session — Yes.
- **Check 2:** Upgrades 2–4 within 1–3 sessions each — Yes.
- **Balancing assumption:** 3 wins and 1 loss per session; 130 coins earned per session (no daily gift or consumable sinks implemented yet).

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