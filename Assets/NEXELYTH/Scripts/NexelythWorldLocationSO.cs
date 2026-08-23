using UnityEngine;

[CreateAssetMenu(
    fileName = "WorldLocation",
    menuName = "NEXELYTH/World Location"
)]
public class NexelythWorldLocationSO : ScriptableObject
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

    public NexelythWorldLocation ToWorldLocation()
    {
        //這個 ToWorldLocation() 方法擔任了「不可變資料來源」與「可變執行個體」之間的橋樑。SO 負責提供純淨的原始設定，而 Runtime 物件負責承載執行時期的變化。
        //清楚表達：我把「ScriptableObject 形式的 WorldLocation」轉成「一般類別的 WorldLocation」
        //未來若 NexelythWorldLocation 需要更多建構參數（例如 IsLocked、RequiredLevel），可以輕鬆修改此方法，同時所有呼叫端無需更動
        return new NexelythWorldLocation(
            mapId,
            x,
            y
        );
    }
}