---
status: approved
date: 2026-06-15
source: reverse-documented from Assets/Scripts/Core/PolyMaterials.cs
verified-by: User
---

# NEON SERPENT — Art Bible

## 1. Visual Identity Summary

**Style**: Low-poly geometric — flat/toon-shaded faceted forms, clean silhouettes, minimal geometry
**Mood**: Immersive digital void — dark, atmospheric, gameplay elements punctuated by selective emission glow
**Reference Points**: Minimalist low-poly art meets clean voxel-like geometry; disciplined palette with emission reserved as an accent for gameplay-critical objects only
**Core Principle**: Every visual element is procedurally generated at runtime. No external textures, models, or images. The art IS the code.

## 2. Color Palette

All colors defined in `PolyMaterials.PolyPalette`. RGB values are linear Unity color space.

### Environment (Muted, Receding)

| Name | RGB | Usage |
|------|-----|-------|
| Ground | (0.22, 0.24, 0.28) | Arena floor base |
| Wall | (0.30, 0.33, 0.38) | Interior surfaces |
| BoundaryWall | (0.28, 0.30, 0.35) | Arena edge faces |
| Fog | (0.15, 0.16, 0.20) | Atmospheric depth cue |

Environment colors are deliberately desaturated and close in value to create a unified dark backdrop that makes gameplay elements pop.

### Buildings (5-Tone Variety, Reserved for Future City Generation)

| Index | RGB | Notes |
|-------|-----|-------|
| 0 | (0.35, 0.38, 0.44) | Cool gray |
| 1 | (0.40, 0.38, 0.42) | Warm mauve |
| 2 | (0.38, 0.42, 0.40) | Sage |
| 3 | (0.42, 0.40, 0.38) | Sand |
| 4 | (0.36, 0.36, 0.42) | Lavender gray |

| Name | RGB | Usage |
|------|-----|-------|
| WindowLit | (0.95, 0.85, 0.55) | Warm interior glow |
| WindowDark | (0.18, 0.18, 0.22) | Unlit window recess |
| Antenna | (0.40, 0.40, 0.45) | Rooftop detail |
| AntennaLight | (0.90, 0.30, 0.30) | Blinking warning light |
| Billboard | (0.35, 0.75, 0.80) | Neon sign surface |

### Gameplay Elements (High Saturation, High Contrast)

| Name | RGB | Usage | Emission |
|------|-----|-------|----------|
| SnakeHead | (0.30, 0.85, 0.90) | Player avatar head | Yes (cyan glow) |
| SnakeTail | (0.70, 0.40, 0.85) | Body gradient endpoint | Subtle |
| Food | (0.95, 0.80, 0.25) | Primary collectible | Yes (gold pulse) |
| Obstacle | (0.90, 0.30, 0.25) | Static hazard | Yes (red warning) |
| PowerUpSpeed | (0.95, 0.85, 0.30) | Speed boost | Yes (gold) |
| PowerUpSlow | (0.30, 0.70, 0.95) | Slow effect | Yes (cyan) |
| PowerUpWall | (0.75, 0.35, 0.90) | Wall pass | Yes (purple) |

**Design Rule**: Gameplay elements MUST have high saturation and emission to remain readable against the muted environment. The snake's head-to-tail gradient (cyan → purple) provides directional readability in first-person view.

### Particles & Atmosphere

| Name | RGB | Usage |
|------|-----|-------|
| Spark | (0.95, 0.85, 0.55) | Death explosion fragments |
| DataStream | (0.30, 0.85, 0.90) | Trail effects |
| Dust | (0.55, 0.55, 0.60) | Ambient particles |
| Rain | (0.60, 0.70, 0.85, α=0.30) | Atmospheric precipitation |

### HUD Feedback Colors

| Name | RGB | Usage |
|------|-----|-------|
| SpeedLow | (0.30, 0.85, 0.90) | Safe speed indicator (cyan) |
| SpeedHigh | (0.90, 0.30, 0.25) | Danger speed indicator (red) |
| FoodGlow | (0.90, 0.92, 0.95) | Collection flash |
| PowerUpGlow | (0.30, 0.95, 1.00) | Power-up activation |
| CollectFlash | (0.30, 0.95, 1.00, α=0.30) | Pickup feedback overlay |
| DeathFlash | (0.90, 0.15, 0.10, α=0.40) | Death screen flash |

**Design Rule**: HUD speed indicator interpolates cyan→red as tick rate increases, providing implicit danger feedback without explicit UI text.

## 3. Material System

Six shader types with fallback chains ensure rendering under all conditions:

| Shader | Purpose | Fallback Chain |
|--------|---------|---------------|
| `NeonSerpent/PolyLit` | Toon shading + flat shading + rim light + emission | URP Simple Lit → Built-in |
| `NeonSerpent/PolyParticle` | Particle rendering with emission | URP Particles/Unlit → Built-in |
| `NeonSerpent/PolyUI` | UI panels with optional border glow | UI/Default → Built-in |
| `NeonSerpent/PolyGrid` | Ground plane with procedural grid lines | PolyLit → Built-in |
| `URP/Unlit` | Simple solid color | Unlit/Color → Error shader |
| `URP/Particles/Unlit` | Basic particle | Unlit → Error shader |

### Material Factories

- **CreatePolyLit**: Full toon material with configurable shade color, emission, rim light, flat/smooth toggle
- **CreatePolyEmissive**: Dimmed base (30%) + boosted emission for glowing objects (food, power-ups, signs)
- **CreatePolyParticle**: Emissive particle material
- **CreatePolyUI**: Panel material with optional neon border glow
- **CreatePolyGrid**: Ground material with procedural grid line overlay

### Shading Characteristics

- **Flat shading enabled by default** on all geometry for consistent low-poly faceted look
- **Toon ramp**: Two-tone (base + shade at 55% brightness) for readable form without smooth gradients
- **Rim lighting**: Optional edge highlight for silhouette readability against dark backgrounds
- **Emission**: Used selectively for gameplay-critical objects; never on environment geometry

## 4. Lighting & Atmosphere

- **Directional light**: Single key light for consistent toon shading across all surfaces
- **Fog**: Linear fog matching `PolyPalette.Fog` color for depth cueing and arena boundary softening
- **Emissive objects**: Food, power-ups, snake head, and arena edges emit light into the scene via URP emission
- **No real-time shadows on gameplay elements**: Grid-logical collision means shadow accuracy is unnecessary; saves performance
- **Ambient**: Dark ambient matching environment palette to maintain contrast ratio

## 5. Typography

- **Font**: TextMesh Pro (loaded from Resources at runtime)
- **Hierarchy**: Bold uppercase for headings, regular weight for body
- **Colors**: `UITextPrimary` (near-white) for primary info, `UITextSecondary` (muted gray) for labels
- **Readability**: All text rendered on semi-transparent dark panels (`UIBackground` at 92% opacity) to guarantee contrast against any background

## 6. UI Visual Language

### Panel Style
- Dark translucent backgrounds (`UIBackground`: 0.10, 0.11, 0.15, α=0.92)
- Inner panels slightly lighter (`UIPanel`: 0.14, 0.16, 0.22, α=0.90)
- Optional neon border glow using `CreatePolyUI` with accent color

### Button States
| State | Background | Text |
|-------|-----------|------|
| Normal | `UIButtonBG` (0.18, 0.20, 0.26) | `UIButtonText` (cyan) |
| Hover | `UIButtonHover` (0.24, 0.28, 0.36) | Cyan brightened |
| Pressed | `UIButtonPressed` (0.30, 0.35, 0.42) | White |

### Semantic Colors
| Intent | Color | Usage |
|--------|-------|-------|
| Accent | `UIAccent` (cyan) | Primary actions, highlights |
| Alt Accent | `UIAccentAlt` (pink) | Secondary emphasis |
| Danger | `UIDanger` (red) | Destructive actions, warnings |
| Success | `UISuccess` (green) | Confirmations, completions |
| Warning | `UIWarning` (gold) | Caution states |

### Canvas Sorting Order
HUD (0) < MainMenu (10) < Tutorial (50) < Pause (100) < GameOver (200) < Transition (9999)

## 7. Motion & Animation Principles

- **Discrete logic, smooth visuals**: All gameplay is tick-based grid movement; rendering uses Lerp interpolation (speed 15) for fluid appearance
- **Food pulse**: Sinusoidal scale oscillation + 90°/s rotation + emission intensity 2±1
- **Death sequence**: Screen shake (0.5 intensity, 0.3s decay) → particle explosion (12 fragments) → expanding flash cube → respawn delay
- **UI transitions**: Panel fades (0.3s), slides (0.3s ease-out), scene transitions (0.5s ease-in-out)
- **All UI animation uses unscaled time** to function during pause states

## 8. Visual Hierarchy Rules

1. **Gameplay elements ALWAYS read first**: High saturation + emission ensures snake, food, obstacles are instantly visible
2. **Environment recedes**: Muted, desaturated, no emission on static geometry
3. **UI overlays are legible**: Dark translucent backing guarantees text contrast
4. **Feedback is immediate**: Collection flashes, death shakes, and speed color shifts provide instant state communication
5. **Snake gradient communicates direction**: Cyan head → purple tail makes forward orientation clear in first-person view

## 9. Asset Standards (Procedural)

Since all assets are generated at runtime, "asset standards" translate to code constraints:

| Constraint | Value | Rationale |
|-----------|-------|-----------|
| Primitive type | Cube, Sphere, Quad only | Minimal geometry for consistent poly style |
| Material creation | Via `PolyMaterials` factories only | Guarantees palette/shader consistency |
| Color source | `PolyPalette` constants only | Prevents ad-hoc color drift |
| Shader fallback | Always specified | Game renders even if custom shaders missing |
| Collider policy | Strip from all visual-only geometry | Physics reserved for gameplay-logical checks |
| Emission budget | Gameplay elements only | Performance + visual hierarchy |
