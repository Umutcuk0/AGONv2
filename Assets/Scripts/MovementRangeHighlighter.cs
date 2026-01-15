using System.Collections.Generic;
using UnityEngine;

public class MovementRangeHighlighter : MonoBehaviour
{
    [SerializeField] private GridManager grid;

    private HashSet<Tile> lastHighlighted = new HashSet<Tile>();

    void Start()
    {
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
    }

    public void Clear()
    {
        foreach (var t in lastHighlighted)
        {
            var vis = t.GetComponent<TileVisual>();
            if (vis != null) vis.SetReachable(false);
        }
        lastHighlighted.Clear();
    }

    public void ShowFor(Unit unit)
    {
        Clear();
        if (unit == null || grid == null) return;


        int moveCost = unit.characterClass.moveAPCost;
        if (unit.ap < moveCost) return;

        Tile start = grid.GetTile(unit.gridPos);
        if (start == null) return;

        int range = unit.characterClass.moveRange;

        var q = new Queue<Tile>();
        var dist = new Dictionary<Tile, int>();

        q.Enqueue(start);
        dist[start] = 0;

        while (q.Count > 0)
        {
            var current = q.Dequeue();
            int cd = dist[current];

            foreach (var n in grid.GetNeighbors(current))
            {
                if (n != start && n.IsOccupied) continue;

                int nd = cd + 1;
                if (nd > range) continue;

                if (dist.ContainsKey(n)) continue;

                dist[n] = nd;
                q.Enqueue(n);
            }
        }

        foreach (var kv in dist)
        {
            Tile t = kv.Key;
            if (t == start) continue;

            var vis = t.GetComponent<TileVisual>();
            if (vis != null)
            {
                vis.SetReachable(true);
                lastHighlighted.Add(t);
            }
        }
    }
}
