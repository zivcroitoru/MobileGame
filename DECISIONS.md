# Decision log

Add an entry every time the team makes a real choice: a tool, a design, a trade-off.
About 1–2 entries a week. Newest at the top. Write the reasoning yourselves; AI may fix the wording.

## 2026-11-12 — Minimum Android API Level 29 (Android 10)
Options: API 25 (Android 7.1) / API 29 (Android 10) / API 33 (Android 13)
Chose: API 29 (Android 10)
Why: Around 31.8% of active devices run Android 12 or older, so picking API 33 would lock out a huge chunk of players. API 29 keeps broad backward compatibility on older phones while supporting modern Unity 6 features and our Galaxy S23 Ultra test device.
AI: listed the trade-offs and current Android version distribution stats — kept the course-recommended API 29 baseline.
Who: Ziv Croitoru

## 2026-11-04 — Physics-based swipe trajectory for pet feeding core loop
Options: Direct tap-to-jump / Swipe-to-launch trajectory arc / Endless runner
Chose: Swipe-to-launch trajectory arc
Why: Matches the requested casual puzzle feel, provides satisfying 1-thumb physics feedback within 1-3 minute session targets.
AI: suggested adding complex obstacle paths — simplified to direct arc with basic static/moving obstacles for a manageable 3-level scope.
Who: Ziv Croitoru