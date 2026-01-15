using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Map Scan")]
    [SerializeField] private LayerMask scanMask;
    [SerializeField] private float scanRadius = 0.45f;
    [Header("Grid Size")]

    public int width = 12;
    public int height = 12;
    public float cellSize = 1f;

    [Header("Prefab")]
    public Tile tilePrefab;

    private Dictionary<Vector2Int, Tile> tiles = new();

    public IEnumerable<Tile> AllTiles => tiles.Values;


    private void Awake()
    {
        GenerateGrid();
    }

    void ScanMapForCoverAndObstacles()
    {
        foreach (var kv in tiles)
        {
            Tile tile = kv.Value;
            Vector3 center = GridToWorld(tile.gridPos) + Vector3.up * 0.5f;

            Collider[] hits = Physics.OverlapSphere(center, scanRadius, scanMask);

            tile.cover = CoverType.None;
            tile.walkable = true;

            foreach (var h in hits)
            {
                CoverMarker marker = h.GetComponentInParent<CoverMarker>();
                if (marker != null)
                {
                    tile.cover = marker.coverType;

                    if (marker.blocksMovement)
                        tile.walkable = false;

                    continue;
                }

                if (h.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                {
                    tile.walkable = false;
                    tile.cover = CoverType.Full;
                }
            }
        }
    }


    void GenerateGrid()
    {
        tiles.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 worldPos = GridToWorld(new Vector2Int(x, y));

                Tile t = Instantiate(tilePrefab, worldPos, tilePrefab.transform.rotation, transform);

                t.gridPos = new Vector2Int(x, y);
                tiles[t.gridPos] = t;
            }
        }
        ScanMapForCoverAndObstacles();
    }

    public Vector3 GridToWorld(Vector2Int gp)
        => new Vector3(gp.x * cellSize, 0f, gp.y * cellSize);

    public Vector2Int WorldToGrid(Vector3 wp)
        => new Vector2Int(Mathf.RoundToInt(wp.x / cellSize), Mathf.RoundToInt(wp.z / cellSize));

    public bool TryGetTile(Vector2Int gp, out Tile tile)
        => tiles.TryGetValue(gp, out tile);

    public List<Tile> GetNeighbors(Tile tile)
    {
        var result = new List<Tile>(4);
        Vector2Int p = tile.gridPos;

        Vector2Int[] dirs = {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };

        foreach (var d in dirs)
        {
            if (TryGetTile(p + d, out Tile n) && n.walkable)
                result.Add(n);
        }
        return result;
    }

    public Tile GetTile(Vector2Int gp)
    {
        tiles.TryGetValue(gp, out Tile t);
        return t;
    }
}
