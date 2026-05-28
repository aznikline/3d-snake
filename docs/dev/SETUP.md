# NEON SERPENT — Development Setup

## Prerequisites

- **Unity 2022.3 LTS** (or newer LTS)
- **Git** with LFS support
- **.NET SDK** (for VS Code / Rider IntelliSense)

## Installation

### 1. Clone Repository

```bash
git clone <repo-url>
cd 3d-snake
```

### 2. Install Unity

Download Unity Hub from [unity.com](https://unity.com/download) and install Unity 2022.3 LTS with the following modules:
- **Mac Build Support** (if on macOS)
- **Windows Build Support**
- **Linux Build Support**
- **Visual Studio Editor** (or JetBrains Rider integration)

### 3. Open Project

1. Open Unity Hub
2. Click "Open" → Select the `3d-snake` folder
3. Unity will import assets and compile scripts

### 4. Configure URP

On first open, Unity may prompt to upgrade to URP. Accept the prompt or manually:
1. Window → Rendering → Render Pipeline Converter
2. Select "Built-in to URP" and run

### 5. Install Packages (if needed)

The following packages should be pre-configured in `Packages/manifest.json`:
- `com.unity.render-pipelines.universal` (URP)
- `com.unity.inputsystem` (New Input System)
- `com.unity.test-framework` (Test Runner)
- `com.unity.textmeshpro` (Text rendering)

If missing, install via Window → Package Manager.

### 6. Set Up Input System

1. Edit → Project Settings → Player
2. Under "Active Input Handling", select "Input System Package (New)"
3. Unity will restart

### 7. Configure Layers

Edit → Project Settings → Tags and Layers:
- Layer 6: `Environment`
- Layer 7: `Snake`
- Layer 8: `Food`
- Layer 9: `GrapplePoint`

### 8. Run Tests

Window → General → Test Runner:
- Select "PlayMode" tab → Run All
- Select "EditMode" tab → Run All

## Project Settings

### Quality Settings
- **V Sync Count**: Every V Blank
- **Anti Aliasing**: 4x Multi Sampling
- **Shadows**: Hard Shadows Only
- **Texture Quality**: Full Res

### Physics Settings
- **Gravity**: (0, -20, 0) — Snake uses custom gravity in Verlet system
- **Default Contact Offset**: 0.001
- **Sleep Threshold**: 0.005

### Audio Settings
- **Default Speaker Mode**: Stereo
- **Sample Rate**: 48000
- **DSP Buffer Size**: Best Latency

## Build Targets

### Development Build
1. File → Build Settings
2. Select target platform (PC, Mac, Linux)
3. Check "Development Build" and "Script Debugging"
4. Build

### Release Build
1. Uncheck "Development Build"
2. Set Build → Compression Method to LZ4HC
3. Build

## Steam Integration Setup

1. Download Steamworks SDK from [partner.steamgames.com](https://partner.steamgames.com/)
2. Place `steam_api.dll` / `libsteam_api.dylib` / `libsteam_api.so` in `Assets/Plugins/`
3. Configure Steam App ID in `steam_appid.txt` (root folder, not committed)
4. Install Steamworks.NET via Package Manager or import from GitHub

## Troubleshooting

### Shader Graph Compilation Errors
- Ensure URP is active: Window → Rendering → HDRP/URP Wizard
- Reimport Shaders: Right-click `Assets/Shaders/` → Reimport

### Input System Not Working
- Check "Active Input Handling" is set to "Input System Package"
- Ensure `PlayerInput` component is on the Snake prefab

### Tests Not Running
- Verify Test Framework package is installed
- Check that test files are in `Assets/Tests/` with correct assembly definitions
