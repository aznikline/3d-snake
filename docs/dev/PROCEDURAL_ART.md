# NEON SERPENT — Procedural Art & Audio System

> **⚠️ STATUS: OUT OF DATE — pending rewrite or removal**
>
> This document describes procedural systems that were **removed during the
> poly-style conversion** (commit `5456ecb`). The classes it documents no
> longer exist in the codebase:
>
> - `ProceduralBuildingGenerator`, `ProceduralCityGenerator`,
>   `ProceduralNeonSignGenerator` — deleted (no city/sign generation in the
>   current poly arena)
> - `ProceduralParticleEffects`, `ProceduralUIRenderer` — deleted
> - `ProceduralMusicSystem` — replaced by `MusicManager`
> - Gameplay references to dash / wall run / grapple — those abilities were
>   removed; the snake is grid-tick based
>
> It is preserved only as historical reference. Do **not** rely on the APIs,
> usage examples, or feature lists below — they describe deleted code.

## Overview

All visual and audio assets are generated procedurally at runtime. Zero external asset dependencies.

## Current Procedural Systems (authoritative)

For accurate, current documentation, see:

| System | Source of truth |
|--------|-----------------|
| Visual style & materials | `design/art/art-bible.md` + `Assets/Scripts/Core/PolyMaterials.cs` |
| Poly shaders | `Assets/Shaders/` (`PolyLit`, `PolyGrid`, `PolyParticle`, `PolyUI`) |
| Snake rendering | `Assets/Scripts/Player/GridSnakeRenderer.cs` |
| Procedural SFX | `Assets/Scripts/Procedural/Audio/ProceduralSFXSystem.cs` |
| Adaptive music | `Assets/Scripts/Audio/MusicManager.cs` |
| Runtime composition | `Assets/Scripts/Core/GameBootstrap.cs` |
| Architecture | `docs/architecture/architecture.md` + `docs/architecture/adr-001-procedural-pipeline.md` |

The visual identity is **low-poly geometric** — flat/toon-shaded faceted forms
with emission reserved as an accent for gameplay-critical objects only. See
the art bible for the palette (`PolyPalette`) and shader fallback chains.

---

## Historical Reference (deleted code — do not use)

The sections below describe systems that no longer exist. They are retained
only to document what the pre-poly pipeline looked like.

### 1. ProceduralBuildingGenerator
Generates cyberpunk buildings using primitive shapes (cubes, cylinders) with emissive materials.

**Features:**
- Variable dimensions (width/depth/height)
- Neon edge trim with random colors
- Window grids with lit/dark variation
- Antennas with blinking lights
- Billboards with random colors
- Height variation based on distance from city center

**Usage:**
```csharp
var generator = GetComponent<ProceduralBuildingGenerator>();
var building = generator.GenerateBuilding(position, seed);
```

### 2. ProceduralCityGenerator
Orchestrates full city generation with atmospheric effects.

**Features:**
- Grid-based city blocks
- Building density control
- Fog and rain effects
- Ground plane generation
- Street neon signs

**Usage:**
```csharp
var city = GetComponent<ProceduralCityGenerator>();
city.GenerateCity(seed);
```

### 3. ProceduralNeonSignGenerator
Creates various types of neon signs for street decoration.

**Sign Types:**
- Text-like (pixel grid patterns)
- Geometric (circle, triangle, cross, diamond, hexagon)
- Abstract (random lines)

**Features:**
- Random flicker effect
- Multiple neon colors
- Backing plates

### 4. ProceduralParticleEffects
Atmospheric particle systems.

**Effects:**
- Energy sparks (floating, gravity-affected)
- Data streams (vertical rising lines)
- Neon dust (ambient floating particles)

### 5. ProceduralUIRenderer
Cyberpunk-styled UI elements.

**Features:**
- Glowing borders
- Scanline overlays
- Glitch effects
- Hologram flicker

## Procedural Audio Systems

### 1. ProceduralMusicSystem
Real-time synthesized electronic music using OnAudioFilterRead.

**Architecture:**
- **SynthVoice**: Individual oscillator with multiple waveforms (sine, square, saw, triangle, noise)
- **Sequencer**: Pattern-based note generation with A minor pentatonic scale
- **Dynamic BPM**: 90-174 BPM based on gameplay intensity

**Waveforms:**
- Sine: Smooth bass/melody
- Square: Harsh leads
- Saw: Rich bass
- Triangle: Soft pads
- Noise: Percussion

**Usage:**
```csharp
var music = GetComponent<ProceduralMusicSystem>();
music.Play();
music.SetIntensity(0.7f); // 0-1, affects BPM and complexity
```

### 2. ProceduralSFXSystem
Runtime-generated sound effects.

**SFX Types:**
- Food collect (pitched sine sweep)
- Dash (frequency sweep with saw wave)
- Death (descending tone)
- Wall run (noise + resonance)
- Grapple (descending slide)
- UI click (short blip)
- Unlock (arpeggio)

**Usage:**
```csharp
ProceduralSFXSystem.Instance.PlayFoodCollect(position, comboLevel);
ProceduralSFXSystem.Instance.PlayDash(position);
```

## Performance Considerations

### Art
- Buildings use shared materials where possible
- Particle systems use GPU simulation
- City objects are pooled and reused
- LOD not needed (first-person, buildings are simple)

### Audio
- Music uses OnAudioFilterRead (efficient, no clip memory)
- SFX uses AudioClip.Create with short durations
- Audio source pooling prevents allocation spikes
- Spatial blend set per SFX type

## Customization

### Building Colors
Edit `neonColors` array in `ProceduralBuildingGenerator`

### Music Scale
Edit `_scale` and `_bassScale` arrays in `ProceduralMusicSystem`

### SFX Parameters
Adjust frequency, envelope, and duration in each Generate*Clip method

## Future Enhancements

1. **Building Variety**: Add more architectural styles (curved, stacked, floating)
2. **Music Complexity**: Add arpeggiators, chord progressions, drum fills
3. **SFX Layering**: Combine multiple waveforms for richer sounds
4. **Visual Effects**: Add holographic distortion, chromatic aberration
