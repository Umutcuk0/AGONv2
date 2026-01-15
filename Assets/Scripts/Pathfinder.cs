using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private GridManager grid;

    public List<Tile> FindPath(Tile start, Tile goal)
    {
        if (start == null || goal == null) return null;
        if (start == goal) return new List<Tile>() { start };
        if (!goal.walkable) return null;

        var cameFrom = new Dictionary<Tile, Tile>();
        var queue = new Queue<Tile>();
        var visited = new HashSet<Tile>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == goal) break;

            foreach (var n in grid.GetNeighbors(current))
            {
                if (visited.Contains(n)) continue;
                visited.Add(n);
                cameFrom[n] = current;
                queue.Enqueue(n);
            }
        }

        if (!cameFrom.ContainsKey(goal)) return null;

        var path = new List<Tile>();
        Tile c = goal;
        path.Add(c);

        while (c != start)
        {
            c = cameFrom[c];
            path.Add(c);
        }

        path.Reverse();
        return path;
    }
}
