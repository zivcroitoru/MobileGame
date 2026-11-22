# Paw Path AI handoff

## Project overview

Paw Path is a portrait 2.5D physics maze. A tap on either half of the screen launches a floppy cat up-left or up-right. Time the next boost to cross gaps and navigate platforms, then touch the green goal to earn coins and return Home. The current course is a Milestone 4 prototype.

- Repository `zivcroitoru/MobileGame`, branch `milestone-4`
- Unity `6000.6.7f1` from `ProjectSettings/ProjectVersion.txt`, C#, URP, Unity Input System
- Android portrait, package `com.ziv.pawpath`, ARM64 and minimum API 29
- Scene flow is Bootstrap → Home → Level → Home. Bootstrap initializes the save, settings and config services. Game depends on Services. Game scripts use those services for persistence.

## Implementation status

| Status | Feature |
| --- | --- |
| Implemented | Directional mouse and primary-touch boosts with cooldown, midair steering and interactive UI exclusion. Orthographic camera follows both axes. |
| Implemented | One playable primitive maze with a recovery floor, steps, an overhang, walls, a ceiling and a visible goal. The goal checks the cat's pelvis collider and invokes the existing Win flow. |
| Implemented | Home navigation, persistent coins and audio setting, Win/Lose rewards, and reward upgrades. The existing Win, Lose and Back buttons remain available for development. |
| Prototype | Full-body ragdoll and level pacing. The current ceiling bounds the height of boosts and rewards timing. The overhang sits 0.20 m lower than in the earlier prototype to give the longer articulated rig more clearance above it. Final feel and performance on a physical phone still need validation. |
| Remaining | Home artwork, Android icon, asset credits and licensing, a team-authored development decision, and final device validation. |

The README's swipe-to-launch description and the older decision log entry describe an earlier design. The scene and current references in this file describe the implemented tap controls. No known reproducible rig disconnection was identified in the tested route. Other input sequences, device performance, and physical touchscreen behavior need additional validation.

## Important files

| File | Purpose |
| --- | --- |
| `Assets/Scripts/Game/TapToPush3D.cs` | Reads mouse and primary touch, raycasts interactive UI, queues one directional boost for FixedUpdate and exposes launch tuning. |
| `Assets/Scripts/Game/CatRagdoll.cs` | Configures per-body solver settings and ignores collisions between cat colliders. Applies mass-proportional impulses and starts a bounded roll about the whole rig center of mass. |
| `Assets/Scripts/Game/SideScrollCamera.cs` | Smooth X/Y camera follow with launch-direction look-ahead. Camera depth and side orientation are fixed. |
| `Assets/Scenes/Level.unity` | Scene-only cat physics overrides, maze, camera, UI, and goal. Expand `Cat_Ragdool_Test` to inspect its physics bones. |
| `Assets/Scenes/Home.unity` | Home UI and entry to Level, including the coin counter, sound setting and upgrade controls. |
| `Assets/Scripts/Game/Bootstrap.cs` | Persistent startup object and service initialization. Open the Bootstrap scene first when testing rewards and navigation. |
| `Assets/Scripts/Services/EconomyConfig.cs` | Defines the economy asset fields. Inspect its assigned asset and the consumers `LevelResultController` and `UpgradeShop` before changing rewards or costs. |
| `Assets/Scripts/Game/LevelGoal.cs` | Sends a pelvis-trigger overlap to `LevelResultController.Win()`. The controller owns reward persistence and scene transition. |
| `DECISIONS.md` | Historical project decisions. The team should record the rationale for the next development decision. |
| `AGENTS.md` | Repository rules, naming, architecture and scope constraints. Read before editing. |

## Physics and Inspector setup

The previous setup already simulated the head, upper and lower segments of all four legs, pelvis and chest. It skipped the mid-spine, had no independent tail motion, and used the same high angular damping on all bodies. The current rig has **13 Rigidbody bodies and 12 CharacterJoints**, plus the pelvis world ConfigurableJoint.

| Bone | Mass | Connected body |
| --- | ---: | --- |
| Root (pelvis) | 1.09 kg | World ConfigurableJoint limits tilt. Rigidbody locks world Z translation. X/Y translation and roll around world Z remain free. |
| Spine | 0.36 kg | Root |
| Spine1 (chest) | 0.80 kg | Spine |
| Head | 0.65 kg | Spine1 |
| LeftArm and RightArm | 0.30 kg each | Spine1 |
| LeftForeArm and RightForeArm | 0.22 kg each | Corresponding Arm |
| LeftUpLeg and RightUpLeg | 0.40 kg each | Root |
| LeftLeg and RightLeg | 0.28 kg each | Corresponding UpLeg |
| Tail | 0.20 kg | Root |

Spine2, Neck, shoulders, paws, toes, ears, head tip, and Tail1–Tail3 follow their physics-driven ancestors through the original skeleton. The Level instance disables the inherited Animator. The source prefab, skin weights and rest pose should stay intact.

Each character joint uses its bone pivot and an explicit matching anchor in the connected body's rest space. The spine and tail twist about the cat's fore-aft axis and swing about the lateral axis. The mid-spine has ±18° twist and 24°/22° swing. The chest has ±24° twist and 26°/18° swing. The head has ±32° twist and 36°/24° swing. The tail has ±18° twist and 42°/36° swing. Distal leg joints are hinge-like, with the swing axes locked. Their proximal joints retain swing for independent contact response. Joint projection limits stretch. Runtime solver settings on the cat are 20 position and 8 velocity iterations, maximum depenetration speed 2.5 m/s and maximum angular speed 12 rad/s. The environment still collides with the limb, torso, head and tail colliders.

- **Cat_Ragdool_Test → Cat Ragdoll** references the Root Rigidbody as Pelvis.
- **Cat_Ragdool_Test → Tap To Push 3D** defaults to 15 N·s sideways and 40 N·s upward, with 5.5 m/s horizontal and 6.8 m/s upward launch caps, 0.26 s cooldown, and 1.6 rad/s added roll per boost. Caps limit what a tap contributes. Gravity and collisions still act normally. Each body gets the same translational velocity change, plus the tangential and angular changes for a roll about the rig center of mass. This starts the tumble with aligned joint anchors and lets physics handle the motion after the tap.
- **Main Camera → Side Scroll Camera** references the same pelvis and Tap To Push 3D. Orthographic size is 2.65, damping 0.12 s, and the horizontal target is at viewport 0.30 when travelling right and 0.70 when travelling left. The camera frames slightly above the pelvis.
- **Goal - Green Marker → Level Goal** references the same pelvis and the Level Result Controller. Keep the marker collider set to Trigger.

No manual scene wiring is required. If a prefab override appears missing, reopen the saved Level scene and inspect the instance components and references before re-running the Ragdoll Wizard. Preserve scene-only overrides.

## Milestone 4 remaining work and next agent order

1. Read `AGENTS.md` and this file. Confirm `git status --short --branch` shows `milestone-4`. Preserve existing user changes. Inspect `Level.unity`, `TapToPush3D.cs`, and `CatRagdoll.cs` before further physics tuning.
2. Test on Android early. Open Bootstrap, choose a portrait Game view, enter Home and run the maze. Tap both halves, reverse while airborne, let the head, paws and tail hit the course, and pause near the green marker to drop into it. Confirm camera framing, goal rewards, manual Win/Lose and Back, plus restart persistence for coins, sound and upgrades. Then use **File → Build Profiles → Android → Build and Run** and repeat on a physical phone. Check notch/safe-area layout and profile multiple impacts with the Unity Profiler.
3. Polish Home with real background, button and coin sprites. Inspect `Home.unity`, the UI prefab and `SafeAreaFitter.cs`; retain button events and service-backed state. Check the portrait layout with and without a cutout.
4. Prepare and assign the Android app icon after choosing artwork. Keep the existing app ID, ABI and minimum SDK. Build and verify the launcher icon on a phone.
5. Add `CREDITS.md` with accurate authors, source links and verified license terms. Check the original PolyOne package listing or purchase records and the provenance of any new art, fonts or audio. The imported folder names alone do not establish license terms.
6. Have the team add the new development decision at the top of `DECISIONS.md`, including the options they actually considered and their reasoning. Review the final Android build and UI after integrating the art.

### Last verified

Unity 6000.6.7f1 Play Mode ran the saved Level at 540 × 1200 in an isolated project copy. Mouse and synthetic touch inputs produced directional launches and midair correction. One right boost rose 2.24 m above the settled pelvis. The head, mid-spine, four legs and tail all rotated relative to their connected bodies during the tested route. The largest sampled joint-anchor gap was 4.2 cm, and the pelvis stayed in the gameplay plane. The camera followed the jump. Reaching the marker awarded coins and returned Home, and Back worked without queuing a boost. This run did not exercise a physical Android device or rerun the manual Win and Lose buttons.

### Risks and unverified assumptions

- Unity Editor and synthetic touch tests exercise the input path. They do not establish Android responsiveness, battery use, or solver cost on hardware.
- Keep axes and anchors in bone-local space when tuning. Avoid enabling Animator or adding a Rigidbody to every bone. Tail and mid-spine contacts are still sensitive to topology and collider size.
- The course ceiling limits real jump height. Tune boost strength and clearance together. Validate a full route before accepting any change to damping, caps or obstacle shape.
- Keep goal rewards behind `LevelResultController` and saves behind `Services.Save`. Do not reset real PlayerPrefs during verification. Use a separate project copy or test save if repeating reward tests.
- The intended handset mix, final Home art and the provenance of third-party assets still need team confirmation.
