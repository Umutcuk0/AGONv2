using System.Collections.Generic;
using UnityEngine;

public class GridClickTest : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private GridManager grid;
    [SerializeField] private Pathfinder pathfinder;

    [Header("Start (temporary)")]
    [SerializeField] private Vector2Int startGridPos = new Vector2Int(0, 0);

    private Tile startTile;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        if (!grid.TryGetTile(startGridPos, out startTile))
            Debug.LogError("Start tile bulunamadý! startGridPos yanlýþ olabilir.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f))
            {
                Tile targetTile = hit.collider.GetComponent<Tile>();
                if (targetTile == null) return;

                List<Tile> path = pathfinder.FindPath(startTile, targetTile);
                Debug.Log(path == null ? "Path yok" : $"Path len: {path.Count} | Target: {targetTile.gridPos}");
            }
        }
    }
}
