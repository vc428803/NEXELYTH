using UnityEngine;

// Portal 或其他呼叫端
// NexelythWorldTravel.Service?.TravelTo(targetLocation);
//只依賴 IWorldTravelService 介面和 NexelythWorldTravel 靜態類別
//實現了依賴反轉：高層模組（Portal）不依賴低層模組（Manager），雙方都依賴抽象（介面）

//之後會變成：
//Portal
//→ NexelythWorldTravel
//→ IWorldTravelService
//→ 實際傳送系統

public static class NexelythWorldTravel
{
    //服務定位器(Service Locator)」的簡化版本，作為「服務註冊表」，儲存當前的 IWorldTravelService 實例
    //提供註冊與取消註冊的 API
    //建立一個「全域存取點」，讓系統任何地方都能獲取傳送服務，但不直接耦合到具體的 Manager 類別。
    public static IWorldTravelService Service { get; private set; }

    public static void Register(
        IWorldTravelService service
    )
    {
        // 修改原因：集中保存目前使用中的世界傳送服務，
        // 讓 Portal 不需要直接依賴 NexelythWorldSceneManager 具體類別。
        Service = service;
    }

    public static void Unregister(
        IWorldTravelService service
    )
    {
        // 修改原因：只有目前註冊中的同一個服務才能解除註冊，
        // 避免其他傳送服務被錯誤清除。
        //情境模擬：
        //服務 A 註冊為當前服務(Service = A)
        //服務 B 嘗試呼叫 Unregister(B)
        //因為 Service != B，所以 B 不會被清除
        //只有當 Service == service（即 A 自己呼叫時），才會設為 null

        if (Service == service)
        {
            Service = null;
        }
    }
}

//步驟一：遊戲啟動時註冊服務

//csharp
//// 在 Bootstrap 或 GameManager 的 Awake() 中
//void Awake()
//{
//    IWorldTravelService travelService = 
//        NexelythWorldSceneManager.Instance;

//NexelythWorldTravel.Register(travelService);
//}


//步驟二：Portal 使用服務

//csharp
//private void OnTriggerEnter(Collider other)
//{
//    // ... 玩家判斷 ...

//    // 透過靜態類別取得服務並執行傳送
//    NexelythWorldTravel.Service?.TravelTo(targetLocation);
//}

//步驟三：場景卸載時清理（可選）

//csharp
//void OnDestroy()
//{
//    IWorldTravelService currentService = 
//        NexelythWorldTravel.Service;

//NexelythWorldTravel.Unregister(currentService);
//}