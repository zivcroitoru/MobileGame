# AGENTS.md — context for AI coding tools

AI tools read this file before working on the repo. Keep it short, true and up to date.
If you use Claude Code, also add a `CLAUDE.md` containing one line: `@AGENTS.md`.

## The game
**Paw Path** is a 3D casual mobile pet-feeding game built for Android in portrait mode with one-thumb tap controls. Players tap to guide/spawn items in levels, earn coins, and return to the Home screen where coins and audio settings persist across sessions.

## Tech
- Engine: Unity 6 LTS, C#
- Target: Android (portrait, ARM64 only, Minimum API Level 29)[cite: 1]
- Package ID: `com.ziv.pawpath`[cite: 1]
- Packages & Services: TextMeshPro (`Unity.TextMeshPro`), `ISaveService` (`PlayerPrefsSaveService`), and `ISettingsService` (`SettingsService`)[cite: 4, 7]

## Architecture
- **Scenes & Build Order:** `Bootstrap` (index 0) → `Home` ↔ `Level` (managed via `ScreenFlow`)[cite: 2, 5].
- **Init Order:** `Bootstrap.Awake()` marks itself `DontDestroyOnLoad`, initializes `Services.Save = new PlayerPrefsSaveService()` first, then `Services.Settings = new SettingsService(Services.Save)`, and loads `Home`[cite: 2].
- **Assemblies:** Two assembly definitions under `Assets/Scripts/`:
  - `Services` (`Assets/Scripts/Services/Services.asmdef`): service interfaces, implementations, and static `Services` locator; zero external assembly references.
  - `Game` (`Assets/Scripts/Game/Game.asmdef`): `Bootstrap`, screen navigation, gameplay, and UI scripts; references `Services` and `Unity.TextMeshPro`.

## Rules for AI
- Follow the existing folder structure and naming. Don't create new top-level folders[cite: 12].
- Never call `PlayerPrefs` directly in `Game` scripts or screens; always use `Services.Save` and `Services.Settings`.
- Keep the dependency one-way: `Game` references `Services`; `Services` must never reference `Game`.
- Don't invent APIs. If unsure about an SDK or Unity API, say so and point to the docs[cite: 12].
- Don't change Player Settings, package name, API levels or build settings unless asked[cite: 12].
- Keep changes small and explain what you changed and why[cite: 12].
- Never commit secrets (keystores, passwords, API keys)[cite: 12].

## Conventions
- **Folder layout:** Service contracts (`I*Service.cs`), implementations, and `Services.cs` live in `Assets/Scripts/Services/`; all scene, UI, and gameplay `MonoBehaviour` scripts (`Bootstrap`, `ScreenFlow`, `SoundToggleButton`, `CoinCounter`, `SafeAreaFitter`, `TapSpawner`, `TapToMove3D`) live in `Assets/Scripts/Game/`.
- **Naming:** PascalCase for classes, interfaces (`I` prefix), public methods, and properties; camelCase with `_` prefix for private fields.