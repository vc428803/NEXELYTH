# NEXELYTH

NEXELYTH 是一個以 **Unity VR 世界系統**為核心的 Prototype，主要探索 **跨場景傳送、世界座標系統與玩家位置狀態管理**。

專案將 **傳送請求、世界位置、座標轉換、Scene 管理與玩家狀態**拆分為獨立系統，建立可持續擴充的 VR 世界架構。

---

## 📌 Project Overview

目前核心目標是建立一套可重複使用的 **World Travel Architecture**。

玩家可以透過 Portal 在不同 World Scene 之間移動，系統負責 Scene 切換、座標轉換、XR Origin 移動，以及玩家世界位置更新。

除了固定 Portal，這套架構目前也已延伸至 Memo 系統，支援保存 Runtime 世界位置並再次傳送回該位置。

### 目前完成

- 三個 World Scene 之間的 Portal 循環傳送
- World Scene Additive Load / Unload
- Persistent XR Origin 跨 Scene 移動
- 指定目的地座標傳送
- `MapId + X/Y` 世界座標模型
- Unity World Position ↔ Map Coordinate 轉換
- Runtime 玩家位置追蹤
- ScriptableObject 固定世界目的地
- Portal / Memo 共用 World Travel Architecture
- 3 個獨立 Memo Slot 位置保存與傳送

---

# 🌍 World System

## World Scenes

目前已完成並實際測試三個 World Scene 之間的循環傳送：

```text
Aurevane
   │
   ▼
SylvarisFields
   │
   ▼
ObsidianHollow
   │
   ▼
Aurevane
```

玩家進入 Portal 後，系統會根據目的地的 `MapId + X/Y` 載入對應 World Scene，並將 XR Origin 移動至指定位置。

---

## World Location Model

NEXELYTH 不讓 Gameplay 系統直接依賴 Unity Transform Position。

世界中的位置統一表示為：

```text
MapId + X/Y
```

例如：

```text
Aurevane (20, 35)
```

這套 World Location Model 目前提供給：

- Portal Destination
- Player Current Location
- Memo Runtime Location

未來亦可提供給：

- Save / Load
- Respawn
- Fast Travel
- Quest

---

## Coordinate System

NEXELYTH 使用自己的 Map Coordinate System，將 Gameplay 使用的地圖座標與 Unity World Position 分離。

目前座標規則：

```text
Unity X ──→ Map X
Unity Z ──→ Map Y
```

`NexelythMapCoordinateSystem` 負責：

```text
Unity World Position
        ↕
     Map X/Y
```

因此 Portal、玩家位置與 Runtime 傳送點皆可以使用相同的世界座標模型。

Gameplay Logic 不需要直接處理 Unity Scene 中的 Transform Position。

---

# 🌀 World Travel System

## Travel Flow

Portal 與 Memo 不直接操作 Unity Scene Management。

所有傳送行為統一透過 `IWorldTravelService`：

```text
Portal / Memo
      │
      ▼
IWorldTravelService
      │
      ▼
NexelythWorldSceneManager
      │
      ├── Load Target World Scene
      │
      ├── Unload Previous World Scene
      │
      ├── Convert Map Coordinate
      │
      ├── Move XR Origin
      │
      └── Update Player Location
```

這讓 Gameplay Feature 與底層 Scene Management 保持分離。

---

## Portal System

`NexelythPortal` 的職責維持單純：

1. 偵測玩家進入 Portal
2. 取得 Target World Location
3. 發出 Travel Request

Portal 本身不負責：

- Scene Load / Unload
- XR Origin 移動
- Coordinate Conversion
- Player Location 更新

因此新增不同 Portal 時，不需要重新實作完整的 World Travel Logic。

---

# 📍 Player Location System

## Single Source of Truth

`NexelythPlayerLocationService` 負責保存玩家目前所在的：

```text
MapId + X/Y
```

並作為 Player Location 的 **Single Source of Truth**。

目前支援：

- 遊戲開始時初始化玩家位置
- Portal 傳送完成後更新位置
- Runtime 計算玩家目前 Map Coordinate
- XR Origin Reference Cache

其他 Gameplay System 不需要各自維護另一份玩家位置資料。

---

# 📝 Memo Travel System

## Runtime Location Save

除了固定 Portal Destination，NEXELYTH 也支援 Runtime 動態位置。

玩家可以將目前所在位置保存至 Memo Slot：

```text
Player Current Position
          │
          ▼
NexelythPlayerLocationService
          │
          ▼
      Memo Slot
     MapId + X/Y
          │
          ▼
IWorldTravelService
          │
          ▼
     World Travel
```

目前提供 **3 個獨立 Memo Slot**。

### 已驗證

- Slot 0 可保存 Location A
- Slot 0 可重新傳送回 Location A
- Slot 1 可保存 Location B
- Slot 1 可重新傳送回 Location B
- Slot 0 / Slot 1 狀態彼此獨立
- Slot 2 使用相同架構支援

Memo System 驗證了 World Travel Architecture 不只適用於預先建立的 Portal Destination，也能處理 Runtime 動態世界位置。

---

# 🏗️ Architecture

## Core Architecture

```text
                         ┌───────────────────┐
                         │   Gameplay Layer  │
                         └─────────┬─────────┘
                                   │
                    ┌──────────────┴──────────────┐
                    │                             │
                    ▼                             ▼
                 Portal                         Memo
                    │                             │
                    └──────────────┬──────────────┘
                                   │
                                   ▼
                         IWorldTravelService
                                   │
                                   ▼
                    NexelythWorldSceneManager
                                   │
              ┌────────────────────┼────────────────────┐
              │                    │                    │
              ▼                    ▼                    ▼
       Scene Management     Coordinate System     Player Location
              │                    │                    │
              └────────────────────┼────────────────────┘
                                   │
                                   ▼
                               XR Origin
```

核心設計概念：

> **Gameplay 負責提出 Travel Request，World System 負責如何完成 Travel。**

Portal 與 Memo 不需要知道 Scene 如何切換，也不需要知道 XR Origin 如何移動。

---

## Core Components

| Component | Responsibility |
| --- | --- |
| `IWorldTravelService` | 統一世界傳送介面 |
| `NexelythWorldSceneManager` | World Scene Load / Unload 與 XR Origin 移動 |
| `NexelythWorldTravel` | 提供目前有效的 World Travel Service |
| `NexelythWorldLocation` | Runtime `MapId + X/Y` 世界位置 |
| `NexelythWorldLocationSO` | ScriptableObject 固定世界目的地 |
| `NexelythMapCoordinateSystem` | Unity ↔ Map Coordinate 轉換 |
| `NexelythPlayerLocationService` | 玩家目前世界位置 |
| `NexelythPortal` | Portal Trigger 與 Travel Request |
| `NexelythMemoLocationService` | Memo Location 保存與傳送 |
| `NexelythMemoSlot` | 單一 Memo Slot 狀態 |

---

# 💡 Design Principles

## 1. Travel System 解耦

Portal、Memo 等 Gameplay System 不直接依賴 `NexelythWorldSceneManager`。

而是統一依賴：

```text
IWorldTravelService
```

因此不同 Gameplay Feature 可以共用相同的 World Travel Pipeline。

---

## 2. 統一 World Location Model

Portal Destination、Player Location 與 Memo Location 都統一使用：

```text
MapId + X/Y
```

避免不同系統各自建立自己的位置格式。

---

## 3. Gameplay 與 Unity Coordinate 分離

Gameplay 使用：

```text
Map X/Y
```

Unity Scene 使用：

```text
World Position
```

兩者透過 `NexelythMapCoordinateSystem` 進行轉換。

因此 Gameplay Logic 不需要直接依賴 Unity Transform Position。

---

## 4. Persistent XR Origin

XR Origin 不隨 World Scene 切換而銷毀。

World Scene 使用 Additive Load / Unload 管理，再由中央 World System 控制 XR Origin 的位置。

因此：

```text
Persistent XR Origin
        │
        ├── World Scene A
        │
        ├── World Scene B
        │
        └── World Scene C
```

VR Player Rig 與 World Scene Lifecycle 可以分開管理。

---

## 5. Reusable World Location

固定世界位置使用 `NexelythWorldLocationSO` 建立成 ScriptableObject Asset。

例如：

```text
Aurevane_Entrance
SylvarisFields_Entrance
ObsidianHollow_Entrance
```

多個 Gameplay Object 可以共用相同的 World Location，不需要重複設定目的地座標。

---

# 📁 Project Structure

```text
Assets/NEXELYTH/Scripts/

├── Bootstrap/
│   └── NexelythBootstrapLoader.cs
│
├── Core/
│   └── World/
│       ├── IWorldTravelService.cs
│       ├── NexelythWorldSceneManager.cs
│       ├── NexelythWorldTravel.cs
│       ├── NexelythWorldLocation.cs
│       ├── NexelythWorldLocationSO.cs
│       └── NexelythMapCoordinateSystem.cs
│
├── Player/
│   └── Location/
│       ├── NexelythPlayerLocationService.cs
│       └── NexelythPlayerCoordinateDebug.cs
│
├── World/
│   └── Portals/
│       └── NexelythPortal.cs
│
└── Gameplay/
    └── Memo/
        ├── NexelythMemoLocationService.cs
        └── NexelythMemoSlot.cs
```

---

# 🚧 Project Status

## Completed

- Unity VR World Prototype
- 三個 World Scene Portal 循環傳送
- Additive Scene Management
- Persistent XR Origin
- Map X/Y Coordinate System
- Unity ↔ Map Coordinate Conversion
- Runtime Player Location Tracking
- ScriptableObject World Location
- `IWorldTravelService` Travel Abstraction
- 3 Slot Memo Location System

---

## Future Extensions

目前 World Location 與 Travel Architecture 預留以下擴充方向：

- Save / Load
- Respawn
- Fast Travel
- Quest Integration
- 更多使用 World Location 的 Gameplay System

---

# 🛠️ Tech Stack

- Unity
- C#
- XR / VR
- Unity Scene Management
- ScriptableObject
- Additive Scene Loading
