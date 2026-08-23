using UnityEngine;
using Unity.XR.CoreUtils;

public class NexelythPlayerCoordinateDebug : MonoBehaviour
{
    [SerializeField]
    private NexelythMapCoordinateSystem mapCoordinateSystem;

    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private float logInterval = 0.5f;

    private float nextLogTime;

    private void Start()
    {
        // 修改原因：World Scene 與 Bootstrap 分離後，
        // 玩家 XR Origin 位於另一張長駐 Scene，
        // 因此不要求每張地圖手動跨 Scene 綁定 Player Transform。
        if (playerTransform == null)
        {
            XROrigin xrOrigin =
                Object.FindFirstObjectByType<XROrigin>();

            if (xrOrigin != null)
            {
                playerTransform =
                    xrOrigin.transform;
            }
        }

        // 修改原因：若 Inspector 尚未指定 Map Coordinate System，
        // 優先從同一個 MapSystem GameObject 自動取得。
        if (mapCoordinateSystem == null)
        {
            mapCoordinateSystem =
                GetComponent<NexelythMapCoordinateSystem>();
        }

        if (playerTransform == null)
        {
            Debug.LogError(
                "[NEXELYTH Position] XR Origin not found."
            );
        }

        if (mapCoordinateSystem == null)
        {
            Debug.LogError(
                "[NEXELYTH Position] Map Coordinate System not found."
            );
        }
    }

    private void Update()
    {
        if (mapCoordinateSystem == null ||
            playerTransform == null)
        {
            return;
        }

        if (Time.time < nextLogTime)
            return;

        nextLogTime =
            Time.time + logInterval;

        // 修改原因：將 XR 玩家目前的 Unity 世界位置轉換成
        // NEXELYTH 地圖座標，驗證每張 World Scene 的座標系是否正確運作。
        Vector2Int mapCoordinate =
            mapCoordinateSystem.WorldToMapCoordinate(
                playerTransform.position
            );

        Debug.Log(
            $"[NEXELYTH Position] " +
            $"Map={mapCoordinateSystem.MapId}, " +
            $"Coordinate=({mapCoordinate.x}, {mapCoordinate.y}), " +
            $"World={playerTransform.position}"
        );
    }
}