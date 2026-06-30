---
status: accepted
date: 2026-06-15
source: reverse-documented from Assets/Scripts/Core/GameBootstrap.cs, PolyMaterials.cs
---

# ADR-001: Zero-Asset Procedural Pipeline

## Context

NEON SERPENT requires a complete visual and audio identity without relying on external asset files (models, textures, audio clips, prefabs). The project targets rapid iteration and minimal build size while maintaining a distinctive low-poly geometric aesthetic with selective emission accents.

## Decision

Adopt a **fully procedural pipeline** where all game assets are generated at runtime:

- **Visuals**: `CreatePrimitive()` + custom shader materials via `PolyMaterials`
- **Audio**: Waveform synthesis in `ProceduralSFXSystem` and `MusicManager`
- **UI**: Programmatic canvas/button/text construction via `UIFactory`
- **Scene**: Runtime assembly in `GameBootstrap.StartGameplay()`

No prefabs, no imported models, no audio clips, no pre-built scenes.

## Rationale

1. **Iteration speed**: Visual/audio changes are code edits, not asset reimports
2. **Build size**: Near-zero asset footprint; entire game is scripts + shaders
3. **Consistency**: Centralized palettes (`PolyPalette`) guarantee visual coherence
4. **Portability**: No platform-specific asset format concerns
5. **Creative constraint**: Forces distinctive style through parameterized generation rather than generic asset store content

## Consequences

### Positive
- Instant hot-reload of visual/audio parameters during development
- Guaranteed unique aesthetic (cannot accidentally look like asset store template)
- Minimal disk/memory footprint for assets
- Simplified version control (no binary asset conflicts)

### Negative
- Higher initial implementation cost (must build everything from primitives)
- Limited visual fidelity compared to hand-crafted assets
- Artist contribution requires shader/material programming skills
- Debugging visual issues requires understanding procedural generation code
- Material/shader fallback chains needed to prevent rendering failures

## Implementation Notes

- `PolyMaterials` caches six shader types with fallback chains (custom → URP → built-in)
- `PolyPalette` defines ~40 colors organized by context (environment, gameplay, UI, effects)
- `UIFactory` uses reference resolution 1920×1080 with `ScaleWithScreenSize` scaler
- Audio synthesis runs at 22050Hz (music) and 44100Hz (SFX)
- All primitives have colliders stripped where physics interaction is not intended

## Related

- [ADR-005](adr-005-procedural-audio.md) — Audio-specific procedural decisions
- [Architecture Overview](architecture.md) — System layer diagram
