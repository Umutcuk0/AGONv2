using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPos;
    public bool walkable = true;

    public CoverType cover = CoverType.None;

    public Unit occupant;
    public bool IsOccupied => occupant != null;
}

public enum CoverType { None, Half, Full }
