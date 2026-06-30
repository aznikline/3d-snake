---
status: reverse-documented
source: Assets/Scripts/
date: 2026-06-15
verified-by: User
---

# NEON SERPENT — Game Concept Document

> **Note**: This document was reverse-engineered from the existing implementation.
> It captures current behavior and clarified design intent. Some sections may be
> incomplete where implementation is partial or intent was unclear.

## Overview

NEON SERPENT is a first-person 3D snake game built in a low-poly geometric visual style. Players navigate a volumetric grid arena from an immersive first-person perspective, consuming food to grow while avoiding walls, obstacles, and their own tail. The game reimagines the classic snake formula as a spatial 3D experience with camera-relative controls, adaptive procedural audio, and a fully programmatic art pipeline requiring zero external assets.

## Player Fantasy

**"I am a neon serpent threading through a digital void."**

The player should feel like a luminous entity flowing through a dark computational space. The first-person view creates visceral immersion — turns are felt, not just seen. Speed escalation generates increasing tension as the snake grows longer and the arena feels smaller. The low-poly geometric style — flat-shaded facets, clean silhouettes, selective emission glow on gameplay elements — reinforces the fantasy of being data incarnate.

## Design Pillars

1. **Spatial Immersion** — First-person 3D transforms flat snake into a volumetric navigation challenge
2. **Procedural Identity** — Zero external assets; all visuals, audio, and UI generated at runtime
3. **Escalating Tension** — Tick-rate acceleration creates organic difficulty curve without artificial level gates
4. **Responsive Precision** — Input queuing and camera-relative controls make discrete movement feel fluid

## MDA Analysis

### Mechanics
- Tick-based grid movement (16×16×16 arena)
- Camera-relative directional input
- Food consumption → growth + speed increase
- Three death conditions (wall, self, obstacle)
- Time-gated power-ups (SpeedBoost, SlowDown, WallPass)
- Stem-based adaptive music driven by gameplay intensity
- JSON persistence with rolling backups

### Dynamics
- Speed escalation compresses decision windows over time
- 3D navigation requires spatial memory beyond traditional snake
- Power-ups create temporary rule-breaking moments
- Procedural audio provides continuous feedback without repetition fatigue
- First-person view limits peripheral awareness, increasing collision surprise

### Aesthetics
- **Sensation** — Visceral feeling of speed and spatial flow
- **Challenge** — Escalating difficulty through mechanical acceleration
- **Discovery** — Learning 3D navigation patterns in a novel context
- **Expression** — Low-poly visual identity as personal style statement

## Scope Tiers

### MVP (Current Implementation)
- [x] Core tick-based movement in 3D grid
- [x] First-person camera with relative input
- [x] Food spawning and consumption
- [x] Obstacle generation
- [x] Three power-up types
- [x] Death sequence with VFX
- [x] Score tracking and high score persistence
- [x] Full UI flow (menu, HUD, pause, game over, tutorial)
- [x] Procedural audio (music + SFX)
- [x] Poly-style visual identity
- [x] Save system with settings persistence

### Planned (Not Yet Implemented)
- [ ] Campaign mode with level progression and par times
- [ ] Endless mode with persistent high scores
- [ ] Challenge mode with modifier rules
- [ ] Extended scoring (combos, time bonuses, rank system)
- [ ] Steam integration (achievements, leaderboards, cloud saves)
- [ ] Skin/theme customization

### Future Considerations
- [ ] Multiplayer (competitive or cooperative)
- [ ] Custom level editor
- [ ] Mod support
- [ ] Additional power-up types
- [ ] Narrative elements / environmental storytelling

## Target Audience

- Retro game enthusiasts seeking modernized classics
- Low-poly / aesthetic-driven players
- Speedrun and high-score chasers
- Players who enjoy spatial puzzle challenges

## Platform & Technical

- **Engine**: Unity 6 (6000.4.8f1)
- **Render Pipeline**: URP
- **Input**: Unity Input System
- **Audio**: Procedural synthesis (no external clips)
- **Art**: Procedural primitives + custom shaders (no external models/textures)
- **Target Platforms**: PC (Windows/macOS/Linux), potential console port

## Key Differentiators

1. **First-person 3D snake** — virtually unexplored design space
2. **Zero-asset pipeline** — entire game generated procedurally at runtime
3. **Adaptive procedural audio** — music and SFX respond to gameplay state
4. **Volumetric grid navigation** — true 3D movement across all six cardinal directions
