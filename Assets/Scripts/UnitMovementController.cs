using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovementController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private GridManager grid;
    [SerializeField] private Pathfinder pathfinder;
    [SerializeField] private MovementRangeHighlighter highlighter;

    [Header("Selected Unit (auto from TurnManager)")]
    [SerializeField] private Unit selectedUnit;

    [Header("Move Settings")]
    [SerializeField] private float stepTime = 0.08f;

    private bool isMoving;

    public Unit GetSelectedUnit() => selectedUnit;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
        if (pathfinder == null) pathfinder = FindFirstObjectByType<Pathfinder>();
        if (highlighter == null) highlighter = FindFirstObjectByType<MovementRangeHighlighter>();

        StartCoroutine(DelayedFirstHighlight());
    }

    IEnumerator DelayedFirstHighlight()
    {
        yield return null;
        SyncSelectedFromTurn();
        RefreshHighlight();
    }

    void Update()
    {
        if (isMoving) return;
        if (TurnManager.Instance == null) return;

        if (!TurnManager.Instance.IsPlayerTurn)
        {
            ClearHighlight();
            return;
        }

        SyncSelectedFromTurn();

        if (Input.GetMouseButtonDown(1))
        {
            if (selectedUnit == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f))
            {
                Tile targetTile = hit.collider.GetComponent<Tile>();
                if (targetTile == null) return;

                TryMoveSelectedTo(targetTile);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearHighlight();
            TurnManager.Instance.EndCurrentUnitTurn();
        }
    }

    void SyncSelectedFromTurn()
    {
        if (TurnManager.Instance == null) return;

        if (!TurnManager.Instance.IsPlayerTurn) return;

        Unit turnUnit = TurnManager.Instance.currentUnit;
        if (turnUnit != null && turnUnit != selectedUnit)
        {
            selectedUnit = turnUnit;
            RefreshHighlight();
        }
    }

    void TryMoveSelectedTo(Tile targetTile)
    {
        if (TurnManager.Instance == null) return;
        if (!TurnManager.Instance.IsPlayerTurn) return;

        if (selectedUnit == null || selectedUnit.characterClass == null) return;

        if (TurnManager.Instance.currentUnit != selectedUnit) return;

        if (!targetTile.walkable) return;
        if (targetTile.IsOccupied) return;

        int moveCost = selectedUnit.characterClass.moveAPCost;
        if (selectedUnit.ap < moveCost)
        {
            Debug.Log($"{selectedUnit.name}: AP yok, hareket edemez. AP={selectedUnit.ap}");
            RefreshHighlight();
            return;
        }

        Tile startTile = grid.GetTile(selectedUnit.gridPos);
        if (startTile == null)
        {
            selectedUnit.SnapToGridFromWorld();
            startTile = grid.GetTile(selectedUnit.gridPos);
            if (startTile == null) return;
        }

        List<Tile> fullPath = pathfinder.FindPath(startTile, targetTile);
        if (fullPath == null || fullPath.Count <= 1) return;

        int maxSteps = selectedUnit.characterClass.moveRange;
        List<Tile> trimmed = TrimPathBySteps(fullPath, maxSteps);

        if (!selectedUnit.SpendAP(moveCost))
            return;

        ClearHighlight();
        StartCoroutine(MoveRoutine(selectedUnit, trimmed));
    }

    List<Tile> TrimPathBySteps(List<Tile> path, int maxSteps)
    {
        int steps = Mathf.Min(maxSteps, path.Count - 1);
        int takeCount = steps + 1;

        var result = new List<Tile>(takeCount);
        for (int i = 0; i < takeCount; i++)
            result.Add(path[i]);

        return result;
    }

    IEnumerator MoveRoutine(Unit unit, List<Tile> path)
    {
        isMoving = true;
        unit.SetAnimMoving(true);

        for (int i = 1; i < path.Count; i++)
        {
            Tile next = path[i];
            if (!next.walkable || next.IsOccupied) break;

            // ✅ occupancy + gridPos güncelle (teleport yok)
            bool placed = unit.PlaceOnTile(next.gridPos);
            if (!placed) break;

            // ✅ görsel olarak yumuşak hareket
            Vector3 startPos = unit.transform.position;
            Vector3 endPos = unit.GetWorldPos(next.gridPos);

            float t = 0f;
            float duration = stepTime;              // adım süresi
            if (duration < 0.01f) duration = 0.01f; // güvenlik

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                Vector3 moveDir = (endPos - startPos).normalized;
                unit.RotateTowards(moveDir);
                unit.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            unit.transform.position = endPos;
        }

        unit.SetAnimMoving(false);
        isMoving = false;

        RefreshHighlight();
    }


    public void RefreshHighlight()
    {
        if (highlighter == null) return;
        if (TurnManager.Instance == null) return;

        if (!TurnManager.Instance.IsPlayerTurn)
        {
            highlighter.Clear();
            return;
        }

        if (selectedUnit == null)
        {
            highlighter.Clear();
            return;
        }

        selectedUnit.SnapToGridFromWorld();
        highlighter.ShowFor(selectedUnit);
    }

    public void ClearHighlight()
    {
        if (highlighter != null) highlighter.Clear();
    }
}
