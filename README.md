# NEXELYTH

> **Next generation. Connected reality. A world beyond the login screen.**

NEXELYTH 是一個以 VR 為核心的原創 RPG 世界企劃。

它的目標不是單純製作一款「可以用 VR 玩的 RPG」，而是逐步建立一個讓玩家感受到：

> **不是打開遊戲，而是登入另一個世界。**

---

# Quick Resume

如果一段時間沒有維護 NEXELYTH，先看這一區。

## Current Development Status

目前核心開發已完成：

```txt
Persistent VR Bootstrap
        ↓
Additive World Scene Loading
        ↓
World Map Coordinate System
        ↓
MapId + X/Y Travel
        ↓
Reusable World Location Assets
        ↓
IWorldTravelService
        ↓
Player Location Tracking
```

目前已實測的 World Scene 傳送循環：

```txt
Aurevane
    ↓
SylvarisFields
    ↓
ObsidianHollow
    ↓
Aurevane
```

每一段 Portal 都可以：

```txt
指定 Target World
+
指定 Target X/Y
```

而不是只能傳送到每張 Scene 唯一固定的 Spawn Point。

---

## Current Main Architecture

```txt
NexelythBootstrap
│
├─ XR Origin
├─ XR Interaction Manager
├─ XR Interaction Simulator
│
└─ Bootstrap
   ├─ NexelythBootstrapLoader
   ├─ NexelythWorldSceneManager
   └─ NexelythPlayerLocationService
```

Bootstrap 是長駐系統層。

World Scene：

```txt
Aurevane
SylvarisFields
ObsidianHollow
```

以 Additive 方式載入及卸載。

XR Origin 不會因為切換地圖而重新建立。

---

## Current Travel Architecture

目前 Portal 傳送流程：

```txt
Portal
   ↓
World Location Asset
   ↓
NexelythWorldTravel.Service
   ↓
IWorldTravelService
   ↓
NexelythWorldSceneManager
   ↓
Load Target World Scene
   ↓
Map X/Y → Unity World Position
   ↓
Move XR Origin
   ↓
Unload Previous World Scene
```

Portal 本身只負責：

```txt
偵測玩家
+
提供 Destination
+
提出 Travel Request
```

Portal 不需要知道 Scene 如何載入，也不需要知道未來是否改成 Multiplayer / Server Travel。

---

## Current Location Architecture

世界位置統一使用：

```txt
MapId + X + Y
```

例如：

```txt
SylvarisFields
X = 20
Y = 35
```

Runtime 資料：

```txt
NexelythWorldLocation
```

Unity 可共用地點 Asset：

```txt
NexelythWorldLocationSO
```

目前已有：

```txt
Aurevane_Entrance.asset
SylvarisFields_Entrance.asset
ObsidianHollow_Entrance.asset
```

多個 Portal 或系統可以引用同一個 Location Asset。

因此如果某個入口座標改變：

```txt
SylvarisFields Entrance
(20,35)
        ↓
改成
(25,40)
```

只需要修改 Location Asset。

不需要逐張 Scene 尋找所有 Portal 修改座標。

---

## Current Player Location System

`NexelythPlayerLocationService` 負責保存玩家目前位置：

```txt
Current Map
Current X
Current Y
```

目前已完成：

```txt
遊戲啟動
↓
立即建立 Initial Location

Portal Travel
↓
Travel 完成
↓
更新 Current Location
```

因此現在不是：

> 「玩家傳送過一次之後，系統才知道他在哪。」

而是：

> **「玩家一進遊戲，系統就知道他目前在哪張 Map、哪個 X/Y。」**

這份資料未來可以直接提供給：

```txt
Memo
Save / Load
Respawn
Fast Travel
Quest
Logout / Login Position
Multiplayer Position State
```

---

## Next Planned Feature

下一階段目前預計：

# Memo / Saved Location System

第一版目標：

```txt
Player Current Location
        ↓
Save Memo Location
        ↓
MapId + X/Y
```

先做到：

> **玩家站在某個位置時，可以把目前位置記錄下來。**

第一版暫時不需要：

```txt
UI
Skill Animation
Cooldown
MP Cost
Server Persistence
```

先完成資料與服務層。

預計使用：

```txt
NexelythMemoLocationService
```

後續再延伸成真正的：

```txt
Memo
Teleport Skill
Respawn Point
Fast Travel
Saved Location
```

---

# Name Origin

## NEX

`NEX` 來自兩個核心概念：

- **Next**：下一世代
- **Nexus**：連結、交會點、核心節點

它代表現實世界與虛擬世界之間的連結，也象徵一種新的遊戲世代。

## ELYTH

`ELYTH` 是一個帶有奇幻世界感的原創名稱。

它希望讓人聯想到未知文明、神秘領域與可以被探索的另一個世界。

因此，NEXELYTH 代表的是：

> **通往下一世代虛擬世界的連結。**

當玩家戴上 VR 裝置登入後，希望感受到的不是「打開一款遊戲」，而是：

> **進入另一個世界。**

---

# Character Progression

NEXELYTH 希望讓玩家在現實世界中的能力、知識與經驗，部分影響自己在虛擬世界中的角色身份與成長方式。

```txt
現實中的能力與經驗
        ↓
虛擬世界中的角色身份
        ↓
職業專屬任務與貢獻
        ↓
角色成長、聲望與世界影響力
```

玩家不只透過打怪與刷經驗值升級。

不同職業可以透過不同方式成長，例如：

```txt
建造
治療
創作
交易
研究
探索
社群合作
```

> **玩家不只因為刷怪時間比較長而變強，也可以因為自己對世界產生的價值而成長。**

---

# How NEXELYTH Differs From Traditional RPGs

傳統 RPG 常見的成長模式：

```txt
Combat
  ↓
EXP
  ↓
Level Up
  ↓
Better Gear
  ↓
Stronger Enemies
```

NEXELYTH 不打算完全捨棄戰鬥，但戰鬥不會是唯一的成長來源。

NEXELYTH 希望加入另一條成長方式：

```txt
Knowledge / Skills / Creation / Contribution
                    ↓
              Identity Path
                    ↓
             Class Challenges
                    ↓
       Contribution & Reputation
                    ↓
      Character & World Progression
```

角色的價值不只來自：

```txt
Level
Gear
DPS
```

也可以來自：

```txt
Knowledge
Creativity
Cooperation
Contribution
```

NEXELYTH 真正想問的是：

> **你希望自己在這個世界成為什麼樣的人？**

---

# Scene Architecture

NEXELYTH 使用：

```txt
Persistent Bootstrap
+
Additive World Scenes
```

而不是每張地圖都各自建立 XR Player。

---

## Bootstrap Scene

正式遊戲入口：

```txt
NexelythBootstrap
```

Bootstrap 負責：

```txt
XR Origin
XR Input
XR Interaction
World Scene Management
Player Location Services
Future Global Services
```

遊戲測試原則上應從：

```txt
NexelythBootstrap
```

按 Play。

不要直接把 World Scene 當正式 Entry Point。

---

## World Scenes

目前正式 World Scene：

```txt
Aurevane
SylvarisFields
ObsidianHollow
```

World Scene 只放：

```txt
Terrain / Ground
Buildings
Environment
NPC
Monster
Portal
Quest Objects
World Objects
```

World Scene 不應重複放：

```txt
XR Origin
Main Camera
Audio Listener
XR Interaction Manager
```

這些屬於 Bootstrap。

---

# World Coordinate System

NEXELYTH 的世界定位概念不是單純保存 Unity：

```txt
Vector3(x, y, z)
```

而是建立 RPG 世界層自己的：

```txt
MapId + X + Y
```

例如：

```txt
Aurevane (100,89)
```

Unity 世界座標主要屬於 Rendering / Physics 層。

遊戲邏輯則盡量使用：

```txt
Map Coordinate
```

這讓未來以下系統可以共用：

```txt
Portal
Memo
NPC Position
Monster Spawn
Quest Target
Fast Travel
Respawn
GM Teleport
Party Position
Save / Load
Multiplayer
```

目前：

```txt
Unity X → Map X
Unity Z → Map Y
```

轉換由：

```txt
NexelythMapCoordinateSystem
```

負責。

每張 World Scene 如果沒有手動建立 MapSystem：

```txt
NexelythWorldSceneManager
```

會在 Runtime 自動建立。

MapId 預設使用 Scene Name。

---

# World Location Assets

固定世界地點使用：

```txt
NexelythWorldLocationSO
```

也就是 Unity ScriptableObject。

例如：

```txt
Aurevane_Entrance.asset
```

內部資料：

```txt
MapId = Aurevane
X = 20
Y = 15
```

Portal 不需要自己保存：

```txt
Target Scene
Target X
Target Y
```

而是直接：

```txt
Target Location
    ↓
Aurevane_Entrance.asset
```

---

## Why ScriptableObject?

假設：

```txt
Dungeon A
Dungeon B
Field C
Town D
```

全部都有 Portal 指向：

```txt
Aurevane South Gate
```

四個 Portal 可以共同引用：

```txt
Aurevane_SouthGate.asset
```

如果入口座標改變，只改：

```txt
Aurevane_SouthGate.asset
```

即可。

核心原則：

> **Portal 不保存座標；Portal 保存「我要去哪個地點」。**

---

# World Travel Abstraction

NEXELYTH 不讓 Portal 直接綁死：

```txt
NexelythWorldSceneManager
```

而是透過：

```txt
IWorldTravelService
```

目前：

```txt
Portal
    ↓
NexelythWorldTravel.Service
    ↓
IWorldTravelService
    ↓
NexelythWorldSceneManager
```

---

## Why?

單機 Prototype 目前可能只是：

```txt
Travel Request
↓
LoadSceneAsync
↓
Move XR Origin
```

但未來 Multiplayer 有可能變成：

```txt
Travel Request
        ↓
Send Request To Server
        ↓
Server Validation
        ↓
Instance / World Assignment
        ↓
Client Scene Load
        ↓
Position Synchronization
```

Portal 不應該知道這些細節。

它只需要：

> **呼叫統一的 Travel Interface，並傳入 Destination。**

因此未來底層傳送系統被抽換時：

```txt
Portal
Respawn
Memo
Fast Travel
Quest Teleport
Dungeon Exit
```

不需要全部重新修改。

---

# Project Structure

NEXELYTH 的 Script 採用：

> **Feature / Domain Based Organization**

而不是把所有 `.cs` 平鋪在：

```txt
Scripts/
```

---

## Current Folder Direction

```txt
Assets/NEXELYTH/Scripts/

Bootstrap/
    NexelythBootstrapLoader.cs

Core/
    World/
        IWorldTravelService.cs
        NexelythWorldSceneManager.cs
        NexelythWorldTravel.cs
        NexelythWorldLocation.cs
        NexelythWorldLocationSO.cs
        NexelythMapCoordinateSystem.cs

Player/
    Location/
        NexelythPlayerLocationService.cs
        NexelythPlayerCoordinateDebug.cs

World/
    Portals/
        NexelythPortal.cs

Gameplay/
    Memo/
        NexelythMemoLocationService.cs
```

---

# Folder Design Philosophy

一個 Class 一個 `.cs` 檔案本身不是問題。

真正需要避免的是：

```txt
Scripts/
    001.cs
    002.cs
    003.cs
    ...
    500.cs
```

全部平鋪在同一層。

NEXELYTH 依照「系統責任」分類。

---

## Bootstrap

```txt
Bootstrap/
```

負責：

```txt
Application Startup
Persistent Systems
World Initialization
```

這些功能跟著整個遊戲生命週期存在。

---

## Core

```txt
Core/
```

放置多個系統共同依賴的底層能力。

例如：

```txt
World Location
World Travel
Coordinate System
Scene Management
```

這些不是某一個 Gameplay Feature 專屬的功能。

---

## Player

```txt
Player/
```

與玩家本身狀態直接相關。

例如：

```txt
Player/
    Location/
    Stats/
    Interaction/
```

目前 Location 系統負責：

```txt
Current Map
Current X
Current Y
```

---

## World

```txt
World/
```

放置實際存在於世界中的 World Object / World Mechanic。

例如：

```txt
World/
    Portals/
    NPC/
    Monsters/
    Spawning/
    Environment/
```

Portal 屬於 World Object，因此放在：

```txt
World/Portals/
```

---

## Gameplay

```txt
Gameplay/
```

放置真正的 RPG Gameplay Feature。

例如：

```txt
Gameplay/
    Memo/
    Combat/
    Skills/
    Quest/
    Inventory/
    Party/
    Guild/
    Crafting/
```

每個 Feature 可以逐步擁有自己的：

```txt
Data
Service
UI
Rules
Runtime Logic
```

---

# Long-Term Folder Direction

如果 NEXELYTH 規模持續擴大，預計可以自然演化成：

```txt
Scripts/

Bootstrap/

Core/
    World/
    Networking/
    Save/
    Audio/

Player/
    Location/
    Interaction/
    Stats/

World/
    Portals/
    NPC/
    Monsters/
    Spawning/
    Environment/

Gameplay/
    Memo/
    Combat/
    Skills/
    Quest/
    Inventory/
    Party/
    Guild/
    Crafting/

UI/
    HUD/
    Menus/
    VR/
```

不需要現在一次建立全部資料夾。

> **Feature 出現時，再建立它需要的模組。**

避免為了「看起來很完整」而提前建立大量沒有內容的架構。

---

# Architecture Principles

目前 NEXELYTH 的主要架構原則：

```txt
Clear Responsibility
        ↓
Reusable Data
        ↓
Reusable Services
        ↓
Loose Coupling
        ↓
Replaceable Implementations
        ↓
Scalable Features
```

---

## 1. Clear Responsibility

例如：

```txt
Portal
```

只負責：

```txt
Trigger Detection
Destination
Travel Request
```

它不負責：

```txt
Scene Loading Details
Server Validation
Save Data
Multiplayer Sync
```

---

## 2. Reusable Data

例如：

```txt
NexelythWorldLocation
```

統一表示：

```txt
MapId + X/Y
```

不要讓 Portal、Memo、Quest、Respawn 各自發明自己的 Location Format。

---

## 3. Reusable Assets

固定世界地點：

```txt
NexelythWorldLocationSO
```

可以被：

```txt
Portal
Quest
Fast Travel
Respawn
```

共同引用。

---

## 4. Loose Coupling

Gameplay Feature 儘量依賴：

```txt
Interface / Service Contract
```

而不是直接綁死底層具體實作。

例如：

```txt
IWorldTravelService
```

---

## 5. Avoid Premature Overengineering

NEXELYTH 不追求一開始就導入：

```txt
Large DI Framework
Complex Enterprise Architecture
Massive Service Locator
Premature Networking Abstraction
```

原則是：

> **有明確擴充價值時才抽象，有實際功能需求時才增加架構。**

Prototype 階段仍然要保持開發速度。

---

# Development Workflow

NEXELYTH 採用小步驟開發。

每一個獨立功能：

```txt
Inspect Existing Code
        ↓
Small Change
        ↓
Unity Compile
        ↓
Play Test
        ↓
git status
        ↓
Commit
        ↓
Push
```

避免一次修改過多系統。

---

## Git Rule

重要功能完成後建立 checkpoint。

例如目前已完成的方向包括：

```txt
OpenXR / XRI Setup
XR Simulator & Locomotion
Persistent XR Bootstrap
Multi-Scene Portal Flow
Map Coordinate System
Coordinate-Based World Travel
Reusable World Location Assets
World Travel Interface
Player Location Tracking
Initial Player Location
```

---

## Unity File Moving Rule

重新整理 Script Folder 時：

> **`.cs` 與 `.meta` 必須一起移動。**

建議使用：

```bash
git mv
```

例如：

```bash
git mv \
Assets/NEXELYTH/Scripts/NexelythPortal.cs \
Assets/NEXELYTH/Scripts/World/Portals/

git mv \
Assets/NEXELYTH/Scripts/NexelythPortal.cs.meta \
Assets/NEXELYTH/Scripts/World/Portals/
```

不要刪掉 `.meta` 再讓 Unity 重建，避免 Script GUID 改變造成 Scene / Prefab Missing Script。

---

# Current World Test Setup

目前主要測試：

```txt
Aurevane
→ SylvarisFields
→ ObsidianHollow
→ Aurevane
```

Portal 使用不同 Material 顏色協助 Prototype 辨識。

目前概念：

```txt
Aurevane Destination
→ Blue

SylvarisFields Destination
→ Green

ObsidianHollow Destination
→ Red
```

顏色目前只是開發辨識用途，不代表最終遊戲美術。

---

# Known Prototype Notes

目前 Map Coordinate 使用：

```txt
Unity X → Map X
Unity Z → Map Y
```

並使用整數格子座標。

目前仍可能出現 Unity Float 邊界，例如：

```txt
World Z = 14.99
FloorToInt
→ Map Y = 14
```

未來可以考慮讓 Teleport Landing Point 使用 Cell Center，例如：

```txt
(20,15)
→ World (20.5, ?, 15.5)
```

目前這不是阻塞性問題，因此暫未優先處理。

---

# Commercial Direction

NEXELYTH 是原創、可商業化方向的 VR RPG。

專案的：

```txt
World
Map
Character
Monster
Story
Location
Progression System
```

應以原創內容為主。

Prototype 可以研究其他 RPG 的設計概念，但正式 NEXELYTH 不應依賴第三方受版權保護的遊戲素材。

---

# Vision

NEXELYTH 希望建立一個知識、技能、創造力、身份與社群，都能成為角色力量來源的 VR 世界。

```txt
Not just grinding.
Not just leveling.
Not just another game world.

Enter NEXELYTH.
```