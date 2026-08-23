using System;
using UnityEngine;

[Serializable]
public class NexelythWorldLocation
{
    [SerializeField]
    private string mapId;

    [SerializeField]
    private int x;

    [SerializeField]
    private int y;

    public string MapId => mapId;
    public int X => x;
    public int Y => y;

    public Vector2Int Coordinate =>
        new Vector2Int(
            x,
            y
        );

    public NexelythWorldLocation(
        string mapId,
        int x,
        int y
    )
    {
        // 修改原因：建立統一的世界位置資料格式，
        // 讓 Portal、Memo、復活點與存檔系統未來共用同一套 Map + X/Y 資料。
        this.mapId = mapId;
        this.x = x;
        this.y = y;
    }
}