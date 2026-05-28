---
title: "NEON SERPENT — 核心玩法与技术架构实现计划"
type: feat
status: active
date: 2026-05-28
origin: docs/brainstorms/2026-05-28-3d-snake-ghostrunner-fusion-requirements.md
---

# NEON SERPENT — 核心玩法与技术架构实现计划

## Overview

基于需求文档，本计划定义 NEON SERPENT 从 MVP 原型到 Steam 完整版的结构化实现路径。采用 Unity 引擎（C#），分两个阶段交付：
- **Phase 1: MVP 原型**（2–3 个月）— 验证核心玩法循环（第一人称 + 六向移动 + 蛇身物理 + 连击冲刺）
- **Phase 2: 完整版**（6–9 个月）— 战役模式、无尽模式、关卡编辑器、Steam 集成、发布

## Problem Frame

经典贪吃蛇玩法需要扩展到 3D 第一人称空间，并注入 Ghostrunner 式的速度感和操作深度。核心挑战在于：
1. **柔性蛇身物理** — 自定义 Verlet 积分系统在长蛇身（100+ 节）下保持 60fps
2. **第一人称碰撞感知** — 玩家无法直接看到自身蛇身，需通过视觉/音频反馈建立空间意识
3. **难度曲线** — 休闲玩家（5 分钟上手）与硬核玩家（100+ 小时精通）的平衡

(see origin: docs/brainstorms/2026-05-28-3d-snake-ghostrunner-fusion-requirements.md)

## Requirements Trace

| 需求 | 覆盖单元 |
|------|----------|
| R1 第一人称视角 | Unit 2, Unit 3 |
| R2 六向自由移动 | Unit 2, Unit 5 |
| R3 经典贪吃蛇规则 | Unit 3, Unit 4 |
| R4 连击冲刺系统 | Unit 4 |
| R5 环境互动 | Unit 5 |
| R6 蛇身物理（Verlet） | Unit 1, Unit 2 |
| R7 战役模式 | Phase 2 — Unit 6 |
| R8 无尽模式 | Phase 2 — Unit 7 |
| R9 挑战模式 | Phase 2 — Unit 7 |
| R10 关卡编辑器 | Phase 2 — Unit 8 |
| R11 赛博朋克美术 | Unit 1, Unit 9 |
| R12 动态音乐 | Unit 3, Unit 4 |
| R13 死亡回放 | Unit 3 |
| R14 关卡评分 | Phase 2 — Unit 6 |
| R15 解锁系统 | Phase 2 — Unit 6 |
| R16 成就系统 | Phase 2 — Unit 9 |
| R17 Steam 首发 | Phase 2 — Unit 9 |
| R18 买断制 + 免费更新 | 商业决策，非技术规划 |
| R19 Steam 功能集成 | Phase 2 — Unit 9 |

## Scope Boundaries

- **非目标：多人对战** — 首发仅单人，联机排期在发售后 6 个月以后
- **非目标：VR 支持** — 传统屏幕第一人称，VR 为远期可能
- **非目标：主机/移动端首发** — 首发仅 PC（Steam）
- **非目标：复杂剧情叙事** — 环境叙事 only

## Context & Research

### 技术栈
- **引擎**: Unity 2022.3 LTS（稳定版，长期支持）
- **语言**: C# (.NET Standard 2.1)
- **渲染管线**: URP（Universal Render Pipeline）— 性能与跨平台平衡，适合赛博朋克霓虹效果
- **物理**: 自定义 Verlet 积分蛇身系统（非 Unity Physics）
- **输入**: Unity Input System（支持键鼠 + 手柄热切换）
- **音频**: FMOD 或 Unity 内置 AudioMixer + 动态 BPM 切换
- **Steam 集成**: Steamworks.NET + Facepunch.Steamworks
- **版本控制**: Git + Git LFS（美术资源）

### 参考游戏与竞品分析
- **Snake Pass** — 3D 蛇身平台跳跃，验证了蛇身物理在 3D 中的可行性，但节奏过慢
- **Ghostrunner / Ghostrunner 2** — 第一人称高速跑酷标杆，参考其移动手感、死亡回放、关卡设计节奏
- **Thumper** — 高速节奏 + 霓虹视觉的独立游戏成功范例
- **Super Hexagon** — 极简 + 高难度 + 即时重开的成瘾循环

## Key Technical Decisions

- **Unity URP 而非 Built-in/Built-in HDRP**: URP 在性能和中低端硬件上表现更好，且通过 Shader Graph 可高效实现霓虹发光效果。写实赛博朋克可通过高质量材质 + 后处理（Bloom、Vignette、Chromatic Aberration）达成，无需 HDRP 的重量级开销。
- **自定义 Verlet 积分蛇身**: Unity 的 Rigidbody Chain 在长链下性能下降且难以精细控制。自定义 Verlet 积分在 `Update` 或独立 `Job` 中运行，支持 200+ 节点 @ 60fps。
- **蛇身碰撞检测 — 胶囊体连续扫描**: 每节蛇身使用 CapsuleCollider，头部使用 SphereCast 进行连续碰撞检测（避免高速穿墙）。自撞检测使用空间哈希或八叉树加速。
- **关卡数据 — ScriptableObject + JSON**: 战役关卡用 ScriptableObject 存储元数据，几何数据用 JSON/自定义格式，便于关卡编辑器读写。
- **程序化无尽关卡 — Wave Function Collapse**: 使用 WFC 或基于规则的瓦片拼接生成无限地图，难度通过蛇身长度动态调整生成参数（通道宽度、障碍物密度、垂直变化幅度）。

## Open Questions

### Resolved During Planning

- **引擎**: Unity 2022.3 LTS + URP
- **蛇身物理**: 自定义 Verlet 积分
- **美术管线**: URP Shader Graph + 后处理栈，写实赛博朋克通过材质质量而非 HDRP 达成
- **开发节奏**: MVP 原型 2–3 个月 → 完整版 8–12 个月

### Deferred to Implementation

- **蛇身节点数量上限**: 取决于 Verlet 积分性能测试，目标 200+ 节点 @ 60fps
- **连击冲刺数值**: 需通过 playtest 迭代（连击槽容量、冲刺持续时间、速度倍率）
- **战役主题分区**: 关卡设计文档将在 Phase 2 启动时细化
- **首发定价**: $7.99（推测中间值），最终由市场测试决定
- **光追/DLSS**: 非首发目标，若销量达标可作为画质升级包评估

## High-Level Technical Design

> *This illustrates the intended approach and is directional guidance for review, not implementation specification. The implementing agent should treat it as context, not code to reproduce.*

### 系统架构

```
┌─────────────────────────────────────────────────────────────────┐
│                        NEON SERPENT                             │
├─────────────────────────────────────────────────────────────────┤
│  Presentation Layer                                             │
│  ├─ FirstPersonCamera (FOV 110°, head bob, impact shake)       │
│  ├─ SnakeBodyRenderer (mesh generation from Verlet nodes)      │
│  ├─ PostProcessingStack (Bloom, CA, Vignette, Motion Blur)     │
│  ├─ HUD (speed, combo, health, minimap)                        │
│  └─ DeathReplay (time dilation, camera orbit)                  │
├─────────────────────────────────────────────────────────────────┤
│  Gameplay Layer                                                 │
│  ├─ SnakeController (input → head movement)                    │
│  ├─ VerletSnakeBody (node chain, constraint solving)           │
│  ├─ CollisionSystem (head SphereCast, body capsule query)      │
│  ├─ ComboSystem (combo meter, neon dash state)                 │
│  ├─ FoodSpawner (energy core placement, difficulty scaling)    │
│  ├─ EnvironmentInteraction (wall-run, slide, grapple)          │
│  └─ GameModeManager (campaign / endless / challenge)           │
├─────────────────────────────────────────────────────────────────┤
│  Level / Content Layer                                          │
│  ├─ LevelData (ScriptableObject — metadata, scoring rules)     │
│  ├─ LevelGeometry (JSON — tile grid, obstacles, spawn points)  │
│  ├─ ProceduralGenerator (WFC / rule-based infinite map)        │
│  └─ LevelEditor (runtime editor, export to JSON/Workshop)      │
├─────────────────────────────────────────────────────────────────┤
│  Audio Layer                                                    │
│  ├─ MusicManager (stems, dynamic BPM crossfade)                │
│  ├─ SFXManager (spatial audio, doppler, impact sounds)         │
│  └─ AudioReactive (visuals synced to music beats)               │
├─────────────────────────────────────────────────────────────────┤
│  Platform / Services Layer                                      │
│  ├─ SteamManager (Steamworks.NET — auth, leaderboards, cloud)  │
│  ├─ AchievementManager (Steam achievements trigger)            │
│  ├─ WorkshopUploader (level export + Steam Workshop share)     │
│  └─ SaveSystem (JSON serialization, cloud sync)                │
└─────────────────────────────────────────────────────────────────┘
```

### Verlet 积分蛇身伪代码

```
// Directional guidance only — not implementation specification

struct SnakeNode {
    Vector3 position;
    Vector3 previousPosition;
    float radius;        // 节点半径，随蛇身位置递减（头部大，尾部小）
}

class VerletSnakeBody {
    SnakeNode[] nodes;
    float segmentLength;
    float stiffness;
    
    void Update(float dt) {
        // 1. Verlet 积分 — 所有节点
        for each node:
            Vector3 velocity = node.position - node.previousPosition;
            node.previousPosition = node.position;
            node.position += velocity + gravity * dt * dt;
        
        // 2. 头部约束 — 由 SnakeController 驱动
        nodes[0].position = headTransform.position;
        
        // 3. 距离约束 — 迭代求解（3-5 次）
        for iteration in 0..4:
            for i in 1..nodes.length-1:
                Vector3 delta = nodes[i].position - nodes[i-1].position;
                float error = delta.magnitude - segmentLength;
                Vector3 correction = delta.normalized * error * stiffness;
                nodes[i].position -= correction * 0.5;
                nodes[i-1].position += correction * 0.5;
        
        // 4. 碰撞约束 — 蛇身 vs 环境
        for each node:
            if (CapsuleCast against environment hits):
                node.position = pushOutFromCollision(hit);
    }
    
    void Grow(int segments) {
        // 在尾部追加新节点，位置与最后一节重叠
        for i in 0..segments:
            nodes.Add(new SnakeNode { position = nodes[last].position });
    }
}
```

### 连击冲刺状态机

```
[Idle] --(eat food)--> [ComboBuilding]
[ComboBuilding] --(combo meter full)--> [NeonDashReady]
[ComboBuilding] --(timeout without eating)--> [Idle] (combo reset)
[NeonDashReady] --(player activates)--> [NeonDashing]
[NeonDashing] --(duration expires)--> [Idle]
[NeonDashing] --(collision detected)--> [DeathPrevented] --(consume dash)--> [Idle]
[NeonDashing] --(collision detected, no dash charges)--> [Dead]
```

## Implementation Units

### Phase 1: MVP 原型（2–3 个月）

---

- [ ] **Unit 1: 项目搭建与渲染管线**

**Goal:** 建立 Unity 项目骨架，配置 URP 渲染管线，实现基础赛博朋克霓虹视觉（Bloom、后处理、霓虹材质）。

**Requirements:** R11

**Dependencies:** None

**Files:**
- Create: `Assets/Settings/URP-Asset.asset`
- Create: `Assets/Shaders/NeonEmission.shadergraph`
- Create: `Assets/Materials/Environment/NeonWall.mat`
- Create: `Assets/Materials/Snake/SnakeBody_Emissive.mat`
- Create: `Assets/Scenes/Prototype.unity`
- Create: `Assets/Scripts/Core/GameConstants.cs`

**Approach:**
- 新建 Unity 2022.3 LTS 项目，切换至 URP
- 配置 URP Asset：MSAA 4x，主光源阴影，后处理启用
- 使用 Shader Graph 创建可参数化的霓虹发光材质（Emission + Fresnel rim）
- 配置 Volume Profile：Bloom（高强度，霓虹色阈值）、Vignette、Chromatic Aberration、Motion Blur
- 创建基础场景：简单立方体迷宫（10×10×3），带霓虹贴花

**Test scenarios:**
- Happy path: 场景在 Editor Play Mode 下以 60fps 运行，Bloom 效果在霓虹材质上可见
- Edge case: 同时渲染 50+ 发光物体，帧率不低于 55fps（目标硬件：GTX 1060）
- Error path: URP Asset 配置错误时，Console 输出清晰错误提示

**Verification:**
- 场景在目标硬件上稳定 60fps
- 霓虹材质可通过 Inspector 实时调整颜色和强度
- 后处理效果在 Game 视图中正确呈现

---

- [ ] **Unit 2: 第一人称控制器与 Verlet 蛇身**

**Goal:** 实现第一人称六向移动控制器和自定义 Verlet 积分蛇身系统。

**Requirements:** R1, R2, R6

**Dependencies:** Unit 1

**Files:**
- Create: `Assets/Scripts/Player/SnakeHeadController.cs`
- Create: `Assets/Scripts/Player/VerletSnakeBody.cs`
- Create: `Assets/Scripts/Player/SnakeBodyRenderer.cs`
- Create: `Assets/Scripts/Player/CameraController.cs`
- Create: `Assets/Prefabs/Player/SnakeHead.prefab`
- Create: `Assets/Prefabs/Player/SnakeSegment.prefab`
- Test: `Assets/Tests/PlayMode/VerletSnakeBodyTests.cs`

**Approach:**
- **SnakeHeadController**: 处理 Input System 输入，将 WASD + 空格/Shift 映射为 3D 空间中的头部移动向量。支持鼠标控制视角（Yaw/Pitch）。头部移动速度基础值 8 m/s，可配置。
- **VerletSnakeBody**: 自定义 Verlet 积分实现。节点数组管理，每帧执行：Verlet 积分 → 头部约束 → 距离约束（迭代 4 次）→ 环境碰撞约束。节点半径头部 0.5m，尾部递减至 0.2m。
- **SnakeBodyRenderer**: 根据 Verlet 节点生成平滑的管状 Mesh（使用 Catmull-Rom 样条插值）。支持动态分段数调整。
- **CameraController**: 第一人称相机挂载在蛇头，FOV 110°，支持头部轻微摆动（基于移动速度的正弦偏移）和撞击时的 FOV 冲击效果。

**Technical design:** *(directional guidance only)*
```
// VerletSnakeBody 核心循环（每帧）
1. 对所有节点应用 Verlet 积分: pos += (pos - prevPos) * damping + externalForces
2. 固定头部节点到 SnakeHeadController 的 transform.position
3. 迭代 4 次距离约束: 确保相邻节点距离 = segmentLength
4. 环境碰撞约束: 对每个节点执行 CapsuleCast，若碰撞则沿法线推出
5. 自撞检测: 头部节点对后续节点（跳过前 N 个）做距离检查
```

**Patterns to follow:**
- Unity Input System 的 `PlayerInput` 组件模式
- Job System 可选：若 Verlet 积分在 CPU 上成为瓶颈，迁移至 `IJobParallelFor`

**Test scenarios:**
- Happy path: 玩家输入 WASD，蛇头向对应方向平滑移动，蛇身自然跟随
- Happy path: 蛇身增长至 50 节，Verlet 约束保持相邻节点距离一致
- Edge case: 蛇头急转 180°，蛇身外侧重叠但不穿模
- Edge case: 长蛇身（100 节）在复杂环境中，帧率保持 60fps
- Error path: 蛇头穿过薄墙（< 0.1m）— SphereCast 连续检测应阻止此情况
- Integration: SnakeHeadController → VerletSnakeBody → SnakeBodyRenderer 端到端运行，无帧延迟

**Verification:**
- 第一人称移动手感流畅，无眩晕感
- 蛇身在各种转向角度下保持视觉连贯性
- 100 节蛇身 @ 60fps 在目标硬件上稳定运行

---

- [ ] **Unit 3: 贪吃蛇核心规则与死亡系统**

**Goal:** 实现吃食物增长、撞墙/自撞死亡、死亡回放和快速重生。

**Requirements:** R3, R13

**Dependencies:** Unit 2

**Files:**
- Create: `Assets/Scripts/Gameplay/Food.cs`
- Create: `Assets/Scripts/Gameplay/FoodSpawner.cs`
- Create: `Assets/Scripts/Gameplay/DeathManager.cs`
- Create: `Assets/Scripts/Gameplay/GameStateManager.cs`
- Create: `Assets/Prefabs/Gameplay/EnergyCore.prefab`
- Create: `Assets/Scripts/UI/HUDController.cs`
- Test: `Assets/Tests/PlayMode/GameplayRuleTests.cs`

**Approach:**
- **Food / EnergyCore**: 可收集的发光球体，带有脉冲动画和粒子效果。被头部碰撞触发收集。
- **FoodSpawner**: 在关卡有效空间内随机生成食物，确保不与蛇身或障碍物重叠。使用空间查询（Physics.OverlapSphere）验证生成位置。
- **DeathManager**: 监听碰撞事件。撞墙/自撞时：触发时间膨胀（Time.timeScale = 0.2）持续 1.5 秒，相机围绕撞击点轨道旋转展示死亡瞬间，随后快速淡入淡出重生。支持检查点系统（每关预设）。
- **GameStateManager**: 管理 Playing / Paused / Dead / LevelComplete 状态。状态切换时触发对应 UI 和音频。
- **HUD**: 显示当前速度、蛇身长度、连击数、迷你地图（俯视图显示蛇身轨迹）。

**Test scenarios:**
- Happy path: 蛇头碰撞食物，蛇身增长 1 节，分数增加，新食物在 0.5 秒内生成
- Happy path: 撞墙触发死亡 → 子弹时间慢放 → 相机轨道展示 → 重生在检查点
- Edge case: 食物生成位置被蛇身完全包围 → 重新随机生成（最多 10 次尝试）
- Edge case: 蛇身极长时自撞 — 头部与尾部节点距离检测精度
- Error path: 死亡状态期间玩家输入应被忽略
- Integration: 死亡 → 重生 → 游戏状态正确重置，蛇身长度和分数归零或恢复检查点值

**Verification:**
- 吃食物 → 增长 → 新食物生成循环在 60fps 下无卡顿
- 死亡回放视觉冲击力达标（时间膨胀 + 相机轨道）
- 重生时间 < 3 秒，支持即时重开

---

- [ ] **Unit 4: 连击冲刺系统与动态音频**

**Goal:** 实现连击槽积累、霓虹冲刺激活、动态 BPM 音乐切换。

**Requirements:** R4, R12

**Dependencies:** Unit 3

**Files:**
- Create: `Assets/Scripts/Gameplay/ComboSystem.cs`
- Create: `Assets/Scripts/Gameplay/NeonDashAbility.cs`
- Create: `Assets/Scripts/Audio/MusicManager.cs`
- Create: `Assets/Scripts/Audio/SFXManager.cs`
- Create: `Assets/Audio/Music/Stem_Ambient.wav` (placeholder)
- Create: `Assets/Audio/Music/Stem_DnB.wav` (placeholder)
- Test: `Assets/Tests/PlayMode/ComboSystemTests.cs`

**Approach:**
- **ComboSystem**: 吃食物增加连击槽（+20%），槽随时间衰减（2%/秒）。连续吃食物不中断可快速填满。满槽时进入 `DashReady` 状态，UI 高亮提示。
- **NeonDashAbility**: 玩家按键激活冲刺。冲刺期间：速度 ×2.5，蛇身发光强度 ×3，拖尾粒子增强。持续时间 3 秒。冲刺期间若发生碰撞，消耗冲刺抵消死亡（进入 `DeathPrevented` 状态，短暂无敌 0.5 秒）。
- **MusicManager**: 两轨音乐（Ambient 氛围电子 + DnB 高速鼓打贝斯）。BPM 随连击数和速度动态交叉淡化。使用 Unity AudioMixer 的 Snapshot 切换实现平滑过渡。
- **SFXManager**: 吃食物音效（音高随连击数递增）、冲刺激活音效、死亡撞击音效、环境空间音频。

**Test scenarios:**
- Happy path: 连续吃 5 个食物，连击槽从 0% → 100%，UI 高亮
- Happy path: 激活冲刺，速度从 8 m/s → 20 m/s，持续 3 秒后恢复正常
- Happy path: 冲刺期间撞墙，死亡被抵消，蛇身闪烁无敌 0.5 秒
- Edge case: 连击槽在 99% 时食物效果过期 — 槽开始衰减，吃食物重置衰减
- Edge case: 冲刺期间连续碰撞 — 仅第一次碰撞被抵消，第二次正常死亡
- Error path: 冲刺冷却期间（若有）玩家按键无响应
- Integration: 连击数增加 → 音乐 BPM 渐强 → 冲刺激活 → 音乐切换至 DnB 轨

**Verification:**
- 连击冲刺循环在 playtest 中 feels good（主观评估 + 5 名测试者反馈）
- 音乐切换无明显跳变或卡顿
- 冲刺视觉反馈足够明显（发光、粒子、FOV 拉伸）

---

- [ ] **Unit 5: 环境互动（墙面跑、滑铲、钩索）**

**Goal:** 实现墙面跑、滑铲和钩索三种环境互动，扩展移动策略空间。

**Requirements:** R5

**Dependencies:** Unit 2

**Files:**
- Create: `Assets/Scripts/Player/WallRunAbility.cs`
- Create: `Assets/Scripts/Player/SlideAbility.cs`
- Create: `Assets/Scripts/Player/GrappleAbility.cs`
- Create: `Assets/Scripts/Environment/WallRunSurface.cs`
- Create: `Assets/Scripts/Environment/GrapplePoint.cs`
- Create: `Assets/Prefabs/Environment/WallRunZone.prefab`
- Test: `Assets/Tests/PlayMode/EnvironmentInteractionTests.cs`

**Approach:**
- **WallRun**: 玩家以一定角度（30°–60°）接近墙面时自动触发。蛇头沿墙面法线方向的垂直平面移动，重力暂时抵消。持续时间最多 2 秒或直到墙面结束。墙面跑期间蛇身自然"贴墙"。
- **Slide**: 玩家按住 Ctrl/Crouch 键触发。蛇头高度降低 50%，速度增加 20%，持续直到按键释放或 1 秒超时。用于通过低矮通道。
- **Grapple**: 玩家瞄准钩索点（自动吸附最近的有效目标）并按键发射。蛇头沿抛物线快速飞向目标点（0.5 秒），期间玩家无控制。落地后恢复正常移动。钩索点用特定标记的 GameObject 标识。

**Test scenarios:**
- Happy path: 以 45° 角跑向墙面，自动触发墙面跑，蛇身沿墙面延伸
- Happy path: 滑铲通过 1m 高通道，蛇头高度正确降低
- Happy path: 瞄准钩索点，0.5 秒飞抵目标，落地无硬直
- Edge case: 墙面跑时墙面突然中断 — 正常坠落，不触发异常状态
- Edge case: 滑铲时通道高度不足 — 碰撞检测阻止通过
- Error path: 钩索目标被障碍物遮挡 — 不触发钩索，UI 提示"目标不可见"
- Integration: 墙面跑 → 滑铲落地 → 钩索跨越深渊的连续动作链

**Verification:**
- 三种互动在原型关卡中可流畅串联
- 蛇身在墙面跑/滑铲/钩索期间无穿模或断裂
- 互动触发条件清晰，玩家可通过视觉反馈（UI 提示、目标高亮）预判

---

### Phase 2: 完整版（6–9 个月）

---

- [ ] **Unit 6: 战役模式与进度系统**

**Goal:** 实现 50–80 关战役模式、关卡评分、解锁系统和保存系统。

**Requirements:** R7, R14, R15

**Dependencies:** Phase 1 全部完成

**Files:**
- Create: `Assets/Scripts/Level/LevelData.cs`
- Create: `Assets/Scripts/Level/LevelManager.cs`
- Create: `Assets/Scripts/Level/LevelScoring.cs`
- Create: `Assets/Scripts/Progression/UnlockSystem.cs`
- Create: `Assets/Scripts/Progression/SaveSystem.cs`
- Create: `Assets/Scripts/UI/LevelSelectUI.cs`
- Create: `Assets/Scripts/UI/UnlockNotificationUI.cs`
- Create: `Assets/LevelData/Campaign/` (关卡数据目录)
- Test: `Assets/Tests/EditMode/LevelScoringTests.cs`

**Approach:**
- **LevelData (ScriptableObject)**: 存储关卡元数据（名称、主题、目标长度、时间限制、评分阈值 S/A/B/C、解锁条件）。
- **关卡主题分区**（建议）：
  - Zone 1: 霓虹都市（Neon City）— 1–15 关，基础移动教学
  - Zone 2: 数据核心（Data Core）— 16–30 关，引入墙面跑和激光障碍
  - Zone 3: 深渊裂隙（Abyss Rift）— 31–50 关，钩索和垂直空间
  - Zone 4: 核心熔炉（Core Furnace）— 51–65 关，高温区域和收缩空间
  - Zone 5: 虚空边界（Void Edge）— 66–80 关，综合挑战和 Boss 关卡
- **LevelScoring**: 基于完成时间、收集率（食物/隐藏收集品）、死亡次数计算评级。
- **UnlockSystem**: 通关解锁新皮肤（发光纹理、粒子尾迹）、新环境主题、新音乐。
- **SaveSystem**: JSON 序列化玩家进度，支持 Steam 云存档。

**Test scenarios:**
- Happy path: 完成关卡，时间 < S 级阈值，死亡 0 次 → 评级 S
- Happy path: 通关 Zone 1 解锁 Zone 2 和新皮肤
- Edge case: 玩家删除本地存档 → 从 Steam 云存档恢复
- Edge case: 关卡评分边界值 — 时间恰好等于阈值时取较低评级
- Error path: 关卡数据文件损坏 → 加载失败，显示错误提示，不崩溃

**Verification:**
- 战役模式从第 1 关到第 80 关可完整通关
- 保存/加载系统在 Editor 和 Build 中均正常工作
- Steam 云存档跨设备同步验证

---

- [ ] **Unit 7: 无尽模式、挑战模式与排行榜**

**Goal:** 实现程序化无尽关卡、每日挑战和全球排行榜。

**Requirements:** R8, R9

**Dependencies:** Unit 6

**Files:**
- Create: `Assets/Scripts/Level/ProceduralGenerator.cs`
- Create: `Assets/Scripts/Level/EndlessModeManager.cs`
- Create: `Assets/Scripts/Level/ChallengeModeManager.cs`
- Create: `Assets/Scripts/Steam/LeaderboardManager.cs`
- Test: `Assets/Tests/EditMode/ProceduralGeneratorTests.cs`

**Approach:**
- **ProceduralGenerator**: 基于规则的瓦片拼接。输入参数：通道宽度、障碍物密度、垂直变化幅度、主题风格。随蛇身长度动态增加难度（更窄通道、更多障碍、更快食物衰减）。
- **EndlessModeManager**: 管理无尽模式状态，记录最高长度/分数，死亡后提交至 Steam 排行榜。
- **ChallengeModeManager**: 每日/每周轮换特殊规则（如"无冲刺"、"双倍速度"、"镜像控制"）。规则用 ScriptableObject 定义。
- **LeaderboardManager**: Steamworks 排行榜 API 封装。支持上传分数、下载全球/好友排行。

**Test scenarios:**
- Happy path: 无尽模式生成 1000m 地图无重复/死胡同
- Happy path: 挑战模式"双倍速度"规则下，蛇头速度正确翻倍
- Edge case: 无尽模式难度随蛇身长度平滑递增，无突兀跳跃
- Edge case: 排行榜上传失败（网络问题）→ 本地缓存，下次启动重试
- Error path: 生成器无法找到有效路径 → 回退到更宽松的参数重试

**Verification:**
- 无尽模式可连续游玩 30 分钟以上无性能下降
- 排行榜数据正确显示在 UI 中
- 挑战模式规则切换无状态残留

---

- [ ] **Unit 8: 关卡编辑器与 Steam Workshop**

**Goal:** 实现可视化关卡编辑器和 Steam Workshop 集成。

**Requirements:** R10

**Dependencies:** Unit 6

**Files:**
- Create: `Assets/Scripts/Editor/LevelEditorWindow.cs`
- Create: `Assets/Scripts/Editor/TilePlacementTool.cs`
- Create: `Assets/Scripts/Editor/LevelExporter.cs`
- Create: `Assets/Scripts/Steam/WorkshopUploader.cs`
- Create: `Assets/Scenes/LevelEditor.unity`

**Approach:**
- **LevelEditor**: 运行时/编辑器双模式。3D 网格放置工具（墙面、障碍物、食物生成点、玩家起点、钩索点）。支持撤销/重做、关卡预览测试。
- **LevelExporter**: 将关卡几何导出为 JSON + 缩略图 PNG。
- **WorkshopUploader**: Facepunch.Steamworks 封装，支持上传关卡包（JSON + 缩略图）至 Steam Workshop，带标题、描述、标签。

**Test scenarios:**
- Happy path: 在编辑器中放置 10×10 关卡，导出 JSON，重新导入后完全一致
- Happy path: 上传关卡至 Workshop，Steam 客户端中可见
- Edge case: 导出时关卡无玩家起点 → 提示错误，阻止导出
- Edge case: Workshop 上传超大关卡（> 10MB）→ 压缩或分块处理

**Verification:**
- 编辑器 UI 直观，新用户 10 分钟内可创建简单关卡
- Workshop 上传/下载流程端到端验证

---

- [ ] **Unit 9: Steam 集成、成就与发布准备**

**Goal:** 集成 Steamworks（认证、云存档、成就、卡牌、远程同乐），完成发布准备。

**Requirements:** R16, R17, R19

**Dependencies:** Unit 6, Unit 7, Unit 8

**Files:**
- Create: `Assets/Scripts/Steam/SteamManager.cs`
- Create: `Assets/Scripts/Steam/AchievementManager.cs`
- Create: `Assets/Scripts/Steam/SteamCloudSave.cs`
- Create: `Assets/Scripts/Platform/BuildConfig.cs`
- Create: `Assets/Editor/BuildPipeline.cs`

**Approach:**
- **SteamManager**: 初始化 Steamworks，处理回调，管理玩家会话。
- **AchievementManager**: 30–40 个成就定义（ScriptableObject），触发条件监听（通关、速通、无伤、隐藏收集、连击上限等）。
- **SteamCloudSave**: 将 SaveSystem 的 JSON 存档自动同步至 Steam 云。
- **BuildPipeline**: 自动化构建脚本（Windows/macOS/Linux），版本号管理，Steam 上传。

**Test scenarios:**
- Happy path: 启动游戏，Steam 认证成功，玩家昵称显示在主菜单
- Happy path: 触发成就，Steam 弹出通知，成就数据持久化
- Edge case: Steam 离线模式 — 游戏可正常游玩，成就本地缓存，上线后同步
- Edge case: 云存档冲突（本地 vs 云端更新）— 提示玩家选择版本
- Integration: 成就触发 → 保存系统更新 → 云存档同步的完整链路

**Verification:**
- Steam 认证在三种平台上均正常工作
- 成就系统 100% 覆盖（所有成就可触发）
- 构建管道可一键生成三平台安装包

---

## System-Wide Impact

- **Interaction graph:** SnakeHeadController → VerletSnakeBody → SnakeBodyRenderer 是每帧核心链路，任何修改都需验证帧率。GameStateManager 是全局状态中枢，所有游戏模式、UI、音频都订阅其状态变更事件。
- **Error propagation:** 物理碰撞检测失败（穿墙）应触发断言级错误并暂停游戏，避免不可恢复状态。Steam API 调用失败应降级为离线模式，不阻塞游戏。
- **State lifecycle risks:** 死亡 → 重生状态切换需确保蛇身节点数组正确重置，避免残留节点导致自撞误判。关卡切换时需卸载旧关卡几何并释放 Mesh 内存。
- **API surface parity:** 输入系统需同时支持键鼠和手柄，所有可交互操作都需绑定两种输入方案。
- **Integration coverage:** Steam 云存档 ↔ 本地 SaveSystem ↔ 游戏内进度 UI 的跨层同步需端到端测试。Workshop 关卡下载 → 导入 → 游玩 → 评分的完整链路需验证。
- **Unchanged invariants:** 贪吃蛇核心规则（吃食物增长、撞墙/自撞死亡）在所有游戏模式中保持不变。环境互动（墙面跑、滑铲、钩索）不改变增长/死亡规则。

## Risks & Dependencies

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Verlet 蛇身在长链下性能不达标 | 中 | 高 | 早期原型验证（Unit 2），若瓶颈则迁移至 Burst/Job System |
| 第一人称视角导致玩家无法感知蛇身位置 | 高 | 高 | 迷你地图 + 音频提示 + 蛇身边缘视觉指示器（半透明轮廓） |
| 写实赛博朋克美术成本超支 | 中 | 高 | 使用程序化生成建筑 + 高质量材质库，控制手工建模比例 |
| Steam 审核/发布延迟 | 低 | 中 | 提前 2 个月提交 Steam 页面和 Build，预留审核缓冲 |
| 核心玩法循环不够有趣 | 中 | 高 | MVP 阶段尽早进行外部 playtest（目标 20+ 测试者），根据反馈迭代 |
| 动态音乐 BPM 切换技术复杂 | 低 | 中 | 使用 FMOD 或 Wwise 替代 Unity 内置音频，专业工具支持动态音乐 |

## Phased Delivery

### Phase 1: MVP 原型（2–3 个月）
目标：验证核心玩法循环是否有趣
交付物：
- 可玩的 3D 第一人称贪吃蛇原型（1 个测试关卡）
- 第一人称移动 + Verlet 蛇身 + 吃食物增长 + 撞墙死亡
- 连击冲刺系统（基础版）
- 赛博朋克霓虹视觉（基础版）
- 动态音乐（2 轨切换原型）
- 外部 playtest 报告

### Phase 2: 完整版（6–9 个月）
目标：Steam 发售就绪
交付物：
- 战役模式 50–80 关
- 无尽模式 + 挑战模式
- 关卡编辑器 + Steam Workshop
- Steam 集成（成就、排行榜、云存档、卡牌）
- 完整音频资产（音乐 + SFX）
- 营销素材（预告片、截图、Store 页面）
- 三平台构建（Win/macOS/Linux）

## Documentation / Operational Notes

- **开发文档**: 在 `docs/dev/` 维护架构决策记录（ADR）和编码规范
- **美术管线**: 使用 Blender → FBX → Unity 的工作流，Git LFS 管理大型资产
- **测试策略**: Play Mode 测试覆盖核心玩法循环，Edit Mode 测试覆盖数据系统和生成器
- **Playtest 计划**: MVP 完成后招募 20+ 测试者，收集核心循环反馈；完整版 Beta 前再招募 50+ 测试者
- **营销时间线**: 发售前 3 个月启动社交媒体（Twitter/X、Reddit、Discord），发售前 1 个月发布 Steam 页面和 Demo

## Sources & References

- **Origin document:** [docs/brainstorms/2026-05-28-3d-snake-ghostrunner-fusion-requirements.md](docs/brainstorms/2026-05-28-3d-snake-ghostrunner-fusion-requirements.md)
- **参考游戏:** Snake Pass, Ghostrunner, Ghostrunner 2, Thumper, Super Hexagon
- **技术参考:** Unity URP Documentation, Steamworks.NET, Verlet Integration (Jakobsen 2001)
