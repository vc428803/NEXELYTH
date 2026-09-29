NEXELYTH

NEXELYTH 是一個以 Unity VR 世界系統為核心的 Prototype，主要探索 跨場景傳送、世界座標系統，以及玩家位置狀態管理。

專案並非讓每個 Portal 各自控制 Scene 載入與玩家座標，而是將 傳送請求、世界位置、座標轉換、Scene 管理與玩家狀態拆分成獨立系統，建立可持續擴充的 VR 世界架構。

---

🎮 目前完成內容

🌍 三地圖 VR 世界傳送

目前已完成並實際測試三個 World Scene 之間的 Portal 循環傳送：

Aurevane
   ↓
SylvarisFields
   ↓
ObsidianHollow
   ↓
Aurevane

每個世界目的地統一使用：

MapId + X/Y

目前已完成：

- World Scene Additive Load / Unload
- XR Origin 跨 Scene 移動
- 指定目的地座標傳送
- 傳送後玩家位置更新
- Portal 目的地共用

---

🌀 Portal 傳送系統

Portal 本身只負責：

偵測玩家 → 取得目的地 → 發出傳送請求

Portal 不直接處理 Scene Loading 或玩家位置狀態。

Player 進入 Portal
        ↓
Target World Location
        ↓
IWorldTravelService
        ↓
NexelythWorldSceneManager
        ↓
載入 World Scene
        ↓
移動 XR Origin

透過這種設計，Portal Gameplay Logic 不需要依賴具體的 Scene Management 實作。

---

🗺️ 世界座標系統

NEXELYTH 建立自己的 Map Coordinate System，而不是讓 Gameplay 系統直接依賴 Unity World Position。

目前座標規則：

Unity X → Map X
Unity Z → Map Y

因此世界中的位置可以統一表示成：

MapId
X
Y

例如：

Aurevane (20, 35)

"NexelythMapCoordinateSystem" 負責：

Unity World Position ↔ Map X/Y

之間的轉換。

Portal、玩家位置以及 Runtime 傳送點因此可以共用同一套世界座標模型。

---

📍 玩家位置系統

"NexelythPlayerLocationService" 作為玩家目前世界位置的 Single Source of Truth。

統一保存：

MapId + X/Y

目前已完成：

- 玩家進入世界時初始化位置
- Portal 傳送完成後更新位置
- Runtime 取得玩家目前實際座標
- XR Origin Reference Cache

這套位置資料未來可以繼續提供給：

- Save / Load
- Respawn
- Quest
- Fast Travel

等系統使用。

---

📝 Memo 多存點傳送

除了固定 Portal，NEXELYTH 目前也使用相同的 World Travel Architecture 實作 Memo 傳送系統。

玩家可以將目前位置保存至 3 個獨立 Memo Slot，之後再次傳送回該位置。

Memo Slot
    ↓
MapId + X/Y
    ↓
IWorldTravelService
    ↓
World Travel

目前已驗證：

- Slot 0 可保存並返回 Location A
- Slot 1 可保存並返回 Location B
- Slot 0 / Slot 1 狀態彼此獨立
- Slot 2 已由相同架構支援

這也驗證目前的 Travel System 並不只適用於預先設定好的 Portal，而能處理 Runtime 動態產生的位置。

---

🏗️ 系統架構

目前核心設計將 世界位置資料、傳送請求與 Scene 執行分離：

Portal ─────────────┐
                   │
Memo ───────────────┤
                   ↓
         IWorldTravelService
                   ↓
      NexelythWorldSceneManager
             ↙           ↘
   Coordinate System   Player Location
             ↓
          XR Origin

核心元件

元件| 職責
"IWorldTravelService"| 統一世界傳送介面
"NexelythWorldSceneManager"| World Scene 載入、卸載與 XR Origin 移動
"NexelythWorldLocation"| Runtime "MapId + X/Y" 世界位置
"NexelythWorldLocationSO"| 可重複使用的固定世界目的地
"NexelythMapCoordinateSystem"| Unity ↔ Map 座標轉換
"NexelythPlayerLocationService"| 玩家目前世界位置
"NexelythPortal"| Portal Trigger 與 Travel Request
"NexelythMemoLocationService"| Runtime 位置保存與傳送
"NexelythMemoSlot"| 獨立 Memo Slot 狀態

---

💡 設計重點

Travel System 解耦

Portal、Memo 等 Gameplay 系統不直接操作 Unity Scene Management，而是統一依賴：

IWorldTravelService

因此不同 Gameplay Feature 可以共用相同的世界傳送流程。

統一世界位置模型

無論固定 Portal 或 Runtime Memo Location，世界位置最終都統一表示成：

MapId + X/Y

Gameplay Logic 不需要直接依賴 Unity Transform Position。

Persistent XR Origin

XR Origin 在 World Scene 切換期間保持長駐。

World Scene 使用 Additive Load / Unload 管理，再由中央 World System 控制 XR Origin 所在位置。

ScriptableObject 世界地點

固定世界位置可以建立成 "NexelythWorldLocationSO"。

因此多個 Portal 可以共用同一目的地，而不需要在不同物件上重複設定座標。

---

📁 專案結構

Assets/NEXELYTH/Scripts/

Bootstrap/
└── NexelythBootstrapLoader.cs

Core/
└── World/
    ├── IWorldTravelService.cs
    ├── NexelythWorldSceneManager.cs
    ├── NexelythWorldTravel.cs
    ├── NexelythWorldLocation.cs
    ├── NexelythWorldLocationSO.cs
    └── NexelythMapCoordinateSystem.cs

Player/
└── Location/
    ├── NexelythPlayerLocationService.cs
    └── NexelythPlayerCoordinateDebug.cs

World/
└── Portals/
    └── NexelythPortal.cs

Gameplay/
└── Memo/
    ├── NexelythMemoLocationService.cs
    └── NexelythMemoSlot.cs

---

🚧 專案目前範圍

已完成

- Unity VR 世界 Prototype
- 三個 World Scene Portal 循環傳送
- Additive Scene Management
- Persistent XR Origin
- 自訂 Map X/Y 世界座標系統
- Runtime 玩家位置追蹤
- ScriptableObject 世界目的地
- World Travel Interface 解耦
- 3 Slot Memo 位置保存／傳送

架構預留擴充

目前世界位置與 Travel Architecture 可繼續延伸至：

- Save / Load
- Respawn
- Fast Travel
- Quest
- 其他需要世界位置資訊的 Gameplay System
