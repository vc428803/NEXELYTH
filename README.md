# NEXELYTH

## Current Folder Direction

```txt
Assets/NEXELYTH/Scripts/

Bootstrap/
    NexelythBootstrapLoader.cs
        用途：
        - NEXELYTH 的啟動入口
        - 從 NexelythBootstrap 載入初始 World Scene
        - 目前預設載入 Aurevane
        - Bootstrap 本身長駐，不隨 World Scene 切換而消失


Core/
    World/

        IWorldTravelService.cs
            用途：
            - 統一世界傳送介面
            - Portal、Memo、未來 Respawn / Fast Travel
              不需要直接依賴 NexelythWorldSceneManager

            目前支援：
            - NexelythWorldLocationSO
              → 固定世界地點，例如 Portal Entrance
            - NexelythWorldLocation
              → Runtime 動態位置，例如 Memo 存點


        NexelythWorldSceneManager.cs
            用途：
            - 負責 World Scene 的 Additive Load / Unload
            - 管理目前所在 World Scene
            - 將 Map X/Y 轉成 Unity World Position
            - 移動長駐 XR Origin 到指定座標
            - 傳送後更新 Player Location

            目前完成：
            - Aurevane
              → SylvarisFields
              → ObsidianHollow
              → Aurevane

            都可以使用：
            MapId + X/Y
            指定目的地座標傳送


        NexelythWorldTravel.cs
            用途：
            - 保存目前有效的 IWorldTravelService
            - Portal / Memo 不需要直接取得具體 Scene Manager

            架構：
            Portal / Memo
                ↓
            NexelythWorldTravel.Service
                ↓
            IWorldTravelService
                ↓
            NexelythWorldSceneManager


        NexelythWorldLocation.cs
            用途：
            - Runtime 世界位置資料

            格式：
            MapId
            X
            Y

            例如：
            Aurevane (20,35)

            目前主要使用於：
            - Player Current Location
            - Memo Runtime Location
            - 未來 Save / Load
            - Respawn


        NexelythWorldLocationSO.cs
            用途：
            - ScriptableObject 版固定世界地點
            - 可以被多個 Portal / 系統共同引用

            例如：
            Aurevane_Entrance.asset
            SylvarisFields_Entrance.asset
            ObsidianHollow_Entrance.asset

            好處：
            - 多個 Portal 指向同一個地點時不用重複填座標
            - 目的地座標修改時只改一份 Asset


        NexelythMapCoordinateSystem.cs
            用途：
            - NEXELYTH 地圖座標系
            - Unity World Position ↔ Map X/Y

            目前規則：
            Unity X → Map X
            Unity Z → Map Y

            World Scene 沒有 MapSystem 時：
            NexelythWorldSceneManager
            會在 Runtime 自動建立


Player/
    Location/

        NexelythPlayerLocationService.cs
            用途：
            - 保存玩家目前所在的：
              MapId + X/Y
            - 作為玩家位置的 Single Source of Truth

            目前完成：
            - 遊戲剛進 Aurevane 時就初始化位置
            - Portal 傳送完成後更新位置
            - Memo 需要存點時，可即時計算玩家真正當下的位置
            - XR Origin 已做 Cache，
              避免每次查位置都 FindFirstObjectByType

            未來可提供：
            - Save / Load
            - Respawn
            - Quest
            - Memo
            - Fast Travel


        NexelythPlayerCoordinateDebug.cs
            用途：
            - 開發階段 Debug 玩家 Map X/Y
            - Console 顯示目前玩家所在座標

            性質：
            - Debug Tool
            - 不屬於正式 Gameplay 核心


World/
    Portals/

        NexelythPortal.cs
            用途：
            - 世界中的 Portal Trigger
            - 偵測玩家進入
            - 取得 Target World Location
            - 發出 Travel Request

            Portal 本身不負責：
            - Load Scene
            - Server Logic
            - 玩家位置同步

            目前使用：
            NexelythWorldLocationSO

            流程：
            Player Enter Portal
                ↓
            Target Location Asset
                ↓
            IWorldTravelService
                ↓
            World Travel

            已實測：
            Aurevane
                ↓
            SylvarisFields
                ↓
            ObsidianHollow
                ↓
            Aurevane


Gameplay/
    Memo/

        NexelythMemoLocationService.cs
            用途：
            - 管理玩家 Memo / 傳送存點
            - 從 PlayerLocationService 取得玩家當下位置
            - 保存指定 Memo Slot
            - 指定 Memo Slot 進行傳送
            - 透過 IWorldTravelService 執行傳送

            目前完成：
            - 多存點架構
            - 固定 3 個 Memo Slot
            - 每一格可以保存不同：
              MapId + X/Y
            - 可以指定 Slot 傳送回去

            目前測試成功：
            Slot 0
                → 保存位置 A
                → 可傳回位置 A

            Slot 1
                → 保存位置 B
                → 可傳回位置 B

            Slot 0 / Slot 1
                → 彼此獨立
                → 不會互相覆蓋

            Slot 2
                → 架構已支援
                → 同樣可以使用


        NexelythMemoSlot.cs
            用途：
            - 代表「一個 Memo Slot」
            - 每個 Slot 自己管理自己的狀態

            目前資料：
            IsSaved
            Location

            行為：
            Save(...)
            Clear()

            設計目的：
            - MemoLocationService 不直接用 null 管理所有狀態
            - 未來容易加入：
              Slot 名稱
              建立時間
              Lock
              Overwrite Confirmation
              UI 顯示資訊
```