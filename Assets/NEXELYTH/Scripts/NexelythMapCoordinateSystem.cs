using UnityEngine;
using UnityEngine.SceneManagement;

public class NexelythMapCoordinateSystem : MonoBehaviour
{
    [Header("Map Identity")]
    [SerializeField]
    private string mapId;

    [Header("Coordinate Settings")]
    [SerializeField]
    private float tileSize = 1f;

    [SerializeField]
    private Vector3 coordinateOrigin = Vector3.zero;

    public string MapId => mapId;

    public float TileSize => tileSize;

    public Vector3 CoordinateOrigin => coordinateOrigin;

    private void Awake()
    {
        // 修改原因：World Scene 的 MapId 預設直接使用所在 Scene 名稱，
        // 讓大量地圖不需要逐張手動輸入 MapId。
        if (string.IsNullOrWhiteSpace(mapId))
        {
            mapId =
                gameObject.scene.name;
        }
    }

    public void Initialize(
        string newMapId,
        float newTileSize = 1f,
        Vector3? newCoordinateOrigin = null
    )
    {
        // 修改原因：允許 World Scene Manager 在載入新地圖後
        // 自動建立並初始化座標系，不需要每張 Scene 手動放置 MapSystem。
        mapId =
            string.IsNullOrWhiteSpace(newMapId)
                ? gameObject.scene.name
                : newMapId;

        tileSize =
            Mathf.Max(
                0.0001f,
                newTileSize
            );

        coordinateOrigin =
            newCoordinateOrigin ?? Vector3.zero;
    }

    // 修改原因：將 Unity 世界座標轉換成 NEXELYTH 二維地圖座標，
    // 讓傳送、Memo、NPC、怪物與 Quest 使用統一的 Map X/Y。
    public Vector2Int WorldToMapCoordinate(
        Vector3 worldPosition
    )
    {
        Vector3 localPosition =
            worldPosition - coordinateOrigin;

        int mapX =
            Mathf.FloorToInt(
                localPosition.x / tileSize
            );

        int mapY =
            Mathf.FloorToInt(
                localPosition.z / tileSize
            );

        return new Vector2Int(
            mapX,
            mapY
        );
    }

    // 修改原因：提供地圖座標到 Unity 世界位置的反向轉換，
    // 之後 Portal 與 Memo 可以直接指定 Map X/Y 傳送玩家。
    public Vector3 MapCoordinateToWorld(
        Vector2Int mapCoordinate,
        float worldY = 0f
    )
    {
        return coordinateOrigin +
               new Vector3(
                   mapCoordinate.x * tileSize,
                   worldY,
                   mapCoordinate.y * tileSize
               );
    }
}