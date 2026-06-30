# NEON SERPENT

> A first-person 3D snake game set in a cyberpunk neon void. Built with Unity 6 and the Universal Render Pipeline. **Zero external assets** — all art, audio, and UI are generated procedurally at runtime.

**Status:** Polish stage · **Version:** 0.1.0 (Build 1) · **Engine:** Unity 6000.4.8f1 (Unity 6, URP)

---

## What Is This

NEON SERPENT reimagines classic snake as a volumetric, first-person navigation challenge. You are a luminous serpent threading through a dark computational space — a 16×16×16 grid arena. Turns are *felt*, not just seen. Each food consumed accelerates the tick rate, compressing your decision window until the arena feels suffocatingly small.

The entire visual and audio identity is **synthesized in code** — no textures, no models, no audio clips, no prefabs. Materials, meshes, shaders, music, and SFX are all built from primitives and procedural generation at startup.

### Design Pillars

1. **Spatial Immersion** — First-person 3D transforms flat snake into a volumetric navigation challenge
2. **Procedural Identity** — Zero external assets; all visuals, audio, and UI generated at runtime
3. **Escalating Tension** — Tick-rate acceleration creates an organic difficulty curve without artificial level gates
4. **Responsive Precision** — Input queuing and camera-relative controls make discrete movement feel fluid

---

## Features

### Core Gameplay
- **Tick-based grid movement** in a 3D volumetric arena (16³ cells, 64-unit world space)
- **First-person camera** with camera-relative input — "left" means screen-left, not world-left
- **Six-axis navigation** — move along any cardinal axis (+X/-X/+Y/-Y/+Z/-Z); the camera up-vector rotates when moving vertically
- **Input gating** — one direction change per tick, 180° reversals rejected, cooldown-locked turns
- **Speed escalation** — tick rate starts at 5 Hz and accelerates +0.15 Hz per food, capped at 12 Hz
- **Three death conditions** — wall, self-collision, obstacle
- **Time-gated power-ups** — SpeedBoost, SlowDown, WallPass (spawn every 30s, last 5s)

### Game Modes
- **Campaign** — sequential level progression with per-level config (grid size, obstacles, target score) and best-rank tracking
- **Endless** — infinite play with persistent high scores
- **Challenge** — modifier-rule levels with unlock gating

### Scoring & Progression
- Food = 100 points, with a 3-second combo window (×0.5 step, up to ×3.0)
- Rank system: C / B / A / S, with time-multiplier thresholds
- JSON save system with rolling backups and settings persistence

### Procedural Pipeline
- **Poly-style shaders** — `PolyLit` (toon/flat), `PolyGrid`, `PolyParticle`, `PolyUI`
- **Adaptive music** — stem-based, intensity-driven (ambient 90 BPM ↔ drum-and-bass 174 BPM), with crossfades
- **Synthesized SFX** — runtime audio synthesis, no audio clips
- **Procedural materials** — neon emission palette centralized in `PolyMaterials`

---

## Technology Stack

| Layer | Choice |
|-------|--------|
| Engine | Unity 6000.4.8f1 (Unity 6) |
| Render Pipeline | Universal Render Pipeline (URP 17.0.4) |
| Language | C# (.NET Standard 2.1) |
| UI | Unity UI (uGUI) + TextMesh Pro |
| Input | Unity Input System 1.11.2 |
| Architecture | Event-driven, tick-based, code-only composition root |
| Asset Pipeline | Procedural — zero external art/audio |

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameBootstrap, GameStateManager, GameConstants, PolyMaterials
│   ├── Player/         # GridSnakeController, GridSnakeBody, GridSnakeRenderer, FirstPersonCameraController
│   ├── Gameplay/       # GridArena, GridFoodSpawner, GridObstacle*, GridPowerUp*, ScoreManager, GridDeathDetector, Campaign/Challenge managers
│   ├── UI/             # HUD, MainMenu, PauseMenu, Settings, GameOver, Tutorial, SceneTransition, UIFactory
│   ├── Progression/    # SaveSystem
│   ├── Audio/          # MusicManager
│   ├── Procedural/Audio/  # ProceduralSFXSystem
│   ├── Platform/       # BuildConfig
│   └── Editor/         # BuildScript, CameraTurnValidator, PolyMaterialCreator
├── Shaders/            # PolyLit, PolyGrid, PolyParticle, PolyUI
├── Settings/           # URP asset, Input Actions
├── Scenes/             # Boot.unity, Gameplay.unity
├── Materials/          # Procedural poly-style materials
├── Resources/          # Runtime-loadable assets (TMP fonts)
└── Tests/
    ├── EditMode/       # 14 test files — grid logic, scoring, save, balance simulations
    └── PlayMode/       # controller + lifecycle integration tests
```

Assembly definitions enforce boundaries: `Scripts` (runtime), `Editor` (editor-only), `EditModeTests`, `PlayModeTests`.

---

## Getting Started

### Prerequisites
- **Unity 6000.4.8f1** (Unity 6) via [Unity Hub](https://unity.com/download)
- **Git**
- A C# IDE: Visual Studio, JetBrains Rider, or VS Code

### Clone & Open
```bash
git clone https://github.com/aznikline/3d-snake.git
cd 3d-snake
```
1. Open Unity Hub → **Open** → select the `3d-snake` folder
2. Wait for Unity to import assets and compile scripts
3. Open `Assets/Scenes/Boot.unity` and press **Play**

### Required Layers
The project expects these layers (set in *Edit → Project Settings → Tags and Layers*):

| Layer | Name |
|-------|------|
| 6 | Environment |
| 7 | Snake |
| 8 | Food |
| 9 | Obstacle |
| 10 | PowerUp |

### Build
From the Unity editor menu: **POLY SERPENT → Build and Run** (defaults to macOS standalone).

For command-line / CI builds, `BuildScript.BuildFromCommandLine` accepts `-target`, `-development`, and `-outputPath` arguments.

---

## Controls

| Input | Action |
|-------|--------|
| WASD / Arrows | Move (camera-relative) |
| Space | Up |
| Ctrl | Down |
| Mouse | Look (settles before next turn is accepted) |
| Esc / P | Pause |
| Enter | Confirm / Retry |

> The snake accepts **one direction change per tick**. 180° reversals are rejected. Turns are gated until the camera settles — this keeps discrete grid movement precise despite smooth visuals.

---

## Architecture

The codebase follows four architectural principles (see `docs/architecture/architecture.md` and the ADRs):

1. **Zero External Assets** — `GameBootstrap` is a code-only composition root (`[RuntimeInitializeOnLoadMethod]`) that constructs every system programmatically. No prefabs, no scene dependencies.
2. **Event-Driven Communication** — systems never hold direct references for state queries. `GameStateManager.OnStateChanged`, `ScoreManager.OnScoreChanged`, and per-system `Action` events mediate all flow.
3. **Procedural Composition Root** — clean create/destroy on menu return; iteration without asset-pipeline overhead.
4. **Tick-Based Determinism** — discrete grid logic is separated from smooth visual interpolation. No Unity Physics for gameplay.

### Architecture Decision Records
- [ADR-001](docs/architecture/adr-001-procedural-pipeline.md) — Procedural pipeline (zero external assets)
- [ADR-002](docs/architecture/adr-002-event-driven-architecture.md) — Event-driven architecture
- [ADR-003](docs/architecture/adr-003-grid-movement-model.md) — Grid movement model
- [ADR-004](docs/architecture/adr-004-camera-relative-input.md) — Camera-relative input
- [ADR-005](docs/architecture/adr-005-procedural-audio.md) — Procedural audio

---

## Testing

The test suite uses Unity Test Framework (NUnit) with separate Edit Mode and Play Mode assemblies.

```
Assets/Tests/
├── EditMode/   # GridSnakeBody, GridObstacle, GridPowerUp, ScoreManager, GridArena, SaveSystem, balance simulations, ...
└── PlayMode/   # GridSnakeController, game lifecycle integration
```

Run tests via **Window → General → Test Runner**, or headless CI via `game-ci/unity-test-runner@v4`.

---

## Development Workflow

This project uses the [Claude Code Game Studios](docs/docs-ccgs/) (CCGS) coordination framework — a skill-driven, document-first workflow for indie game development with Claude Code.

- **Design docs** live in `design/gdd/` (per-system Game Design Documents) and `design/art/art-bible.md`
- **Architecture** is documented in `docs/architecture/`
- **Production tracking** (sprints, epics, QA, playtests) lives in `production/`
- See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines and [CLAUDE.md](CLAUDE.md) for the project charter

The workflow follows: **Question → Options → Decision → Draft → Approval** — no autonomous commits.

---

## License

All rights reserved. Source is currently private — no open-source license granted. Contact the maintainer before reuse.
