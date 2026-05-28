# NEON SERPENT — Architecture Overview

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Game constants, state management, enums
│   ├── Player/         # Snake controller, Verlet body, camera, abilities
│   ├── Gameplay/       # Food, combo, death, scoring
│   ├── Level/          # Level data, procedural generation, editor
│   ├── Audio/          # Music manager, SFX manager
│   ├── Steam/          # Steamworks integration
│   ├── UI/             # HUD, menus, notifications
│   ├── Progression/    # Unlock system, save system
│   ├── Platform/       # Build config, platform-specific code
│   └── Editor/         # Unity Editor extensions (level editor)
├── Shaders/            # Shader Graph files (URP)
├── Materials/          # Material assets
│   ├── Environment/
│   ├── Snake/
│   └── UI/
├── Prefabs/            # Reusable GameObject prefabs
│   ├── Player/
│   ├── Gameplay/
│   ├── Environment/
│   └── UI/
├── Scenes/             # Unity scenes
├── Audio/              # Audio assets
│   ├── Music/
│   └── SFX/
├── LevelData/          # Level definitions (ScriptableObject + JSON)
│   └── Campaign/
├── Resources/          # Runtime-loaded assets
├── Plugins/            # Third-party plugins
└── Tests/              # Test assemblies
    ├── EditMode/       # Edit-mode tests (pure logic)
    └── PlayMode/       # Play-mode tests (runtime physics)
```

## Namespace Convention

All code uses the `NeonSerpent.{Module}` namespace:
- `NeonSerpent.Core` — Constants, enums, state management
- `NeonSerpent.Player` — Player controller and abilities
- `NeonSerpent.Gameplay` — Game rules and mechanics
- `NeonSerpent.Level` — Level system
- `NeonSerpent.Audio` — Audio management
- `NeonSerpent.Steam` — Platform integration
- `NeonSerpent.UI` — User interface
- `NeonSerpent.Tests` — Test assemblies

## Key Design Patterns

### 1. Singleton (GameStateManager)
Global game state machine. All systems subscribe to `OnStateChanged` events.

### 2. Component-Based (Player)
Snake is composed of multiple MonoBehaviour components:
- `SnakeHeadController` — Input and movement
- `VerletSnakeBody` — Physics simulation
- `SnakeBodyRenderer` — Mesh generation
- `WallRunAbility`, `SlideAbility`, `GrappleAbility` — Modular abilities

### 3. Event-Driven (ComboSystem)
`ComboSystem` fires events (`OnComboChanged`, `OnDashStarted`) that UI and audio systems subscribe to.

### 4. ScriptableObject (LevelData)
Level metadata stored as ScriptableObject assets. Geometry data in JSON for editor compatibility.

## Performance Targets

- **60fps minimum** on GTX 1060 / equivalent
- **200+ Verlet nodes** at 60fps
- **Draw calls < 500** per frame
- **Memory < 2GB** RAM usage

## Coding Standards

- Use `PascalCase` for classes, methods, properties
- Use `camelCase` for local variables, parameters
- Use `UPPER_SNAKE_CASE` for constants
- All magic numbers go in `GameConstants`
- Every public method has XML documentation
- Prefer `SerializeField` over public fields
- Use Unity Input System for all input
