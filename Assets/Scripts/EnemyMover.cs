using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    [SerializeField] private GridManager grid;
    [SerializeField] private Pathfinder pathfinder;

    [Header("Timing")]
    [SerializeField] private float stepTime = 0.18f;

    [Header("Combat")]
    [SerializeField] private int overwatchRange = 8;
    [SerializeField] private int enemyFireRange = 8;
    [SerializeField] private int enemyFireCost = 1;

    [Header("Rotate")]
    [SerializeField] private float aimRotateDuration = 0.12f;

    void Start()
    {
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
        if (pathfinder == null) pathfinder = FindFirstObjectByType<Pathfinder>();
    }

    public void DoEnemyTurn(Unit enemy)
    {
        StartCoroutine(EnemyTurnRoutine(enemy));
    }

    IEnumerator EnemyTurnRoutine(Unit enemy)
    {
        if (TurnManager.Instance == null) yield break;

        if (enemy == null || enemy.IsDead || grid == null || pathfinder == null)
        {
            TurnManager.Instance.NotifyEnemyFinished();
            yield break;
        }

        EnemyAIWeights w = enemy.GetComponent<EnemyAIWeights>();
        if (w == null) w = enemy.gameObject.AddComponent<EnemyAIWeights>();

        while (enemy.ap > 0 && !enemy.IsDead)
        {
            if (enemy.ammo <= 0)
            {
                bool reloaded = enemy.Reload();
                if (!reloaded) break;
                yield return new WaitForSeconds(0.05f);
                continue;
            }

            Unit target = FindNearestPlayer(enemy);
            bool canShoot = (target != null) && InRange(enemy, target, enemyFireRange) && enemy.ap >= enemyFireCost && enemy.ammo > 0;

            EnemyAction action = ChooseAction(w, canShoot);

            if (action == EnemyAction.Shoot)
            {
                if (target != null)
                {
                    Vector3 dir = (target.transform.position - enemy.transform.position);
                    dir.y = 0f;
                    yield return RotateFor(enemy, dir, aimRotateDuration);

                    ShootingController.Fire(enemy, target, grid, enemyFireCost, enemyFireRange);
                }

                yield return new WaitForSeconds(0.1f);
            }
            else if (action == EnemyAction.Move)
            {
                if (target != null)
                    yield return MoveTowards(enemy, target, enemy.characterClass.moveRange);
                else
                    yield return RandomStep(enemy, enemy.characterClass.moveRange);

                int moveCost = enemy.characterClass.moveAPCost;
                enemy.SpendAP(moveCost);
            }
            else if (action == EnemyAction.Cover)
            {
                if (target != null)
                    yield return MoveToBestCover(enemy, target, enemy.characterClass.moveRange);
                else
                    yield return RandomStep(enemy, enemy.characterClass.moveRange);

                int moveCost = enemy.characterClass.moveAPCost;
                enemy.SpendAP(moveCost);
            }
            else if (action == EnemyAction.Overwatch)
            {
                bool ok = enemy.EnterOverwatch(1);
                if (ok) enemy.ap = 0;
                yield return null;
            }
        }

        TurnManager.Instance.NotifyEnemyFinished();
    }

    enum EnemyAction { Shoot, Move, Cover, Overwatch }

    EnemyAction ChooseAction(EnemyAIWeights w, bool canShoot)
    {
        int shootW = canShoot ? w.shoot : 0;
        int moveW = w.move;
        int coverW = w.cover;
        int overW = w.overwatch;

        int total = shootW + moveW + coverW + overW;
        if (total <= 0) return canShoot ? EnemyAction.Shoot : EnemyAction.Move;

        int r = Random.Range(1, total + 1);
        if (r <= shootW) return EnemyAction.Shoot;
        r -= shootW;

        if (r <= moveW) return EnemyAction.Move;
        r -= moveW;

        if (r <= coverW) return EnemyAction.Cover;
        return EnemyAction.Overwatch;
    }

    Unit FindNearestPlayer(Unit enemy)
    {
        if (TurnManager.Instance == null) return null;

        Unit best = null;
        int bestDist = int.MaxValue;

        foreach (var p in TurnManager.Instance.playerUnits)
        {
            if (p == null || p.IsDead) continue;
            int d = Manhattan(enemy.gridPos, p.gridPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }

    bool InRange(Unit a, Unit b, int range) => Manhattan(a.gridPos, b.gridPos) <= range;

    int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    IEnumerator MoveTowards(Unit enemy, Unit target, int moveRange)
    {
        enemy.SetAnimMoving(true);

        Tile start = grid.GetTile(enemy.gridPos);
        Tile goal = grid.GetTile(target.gridPos);
        if (start == null || goal == null) { enemy.SetAnimMoving(false); yield break; }

        Tile bestTile = FindBestApproachTile(start, target.gridPos, moveRange);
        if (bestTile == null) { enemy.SetAnimMoving(false); yield break; }

        List<Tile> path = pathfinder.FindPath(start, bestTile);
        if (path == null || path.Count <= 1) { enemy.SetAnimMoving(false); yield break; }

        path = TrimPathBySteps(path, moveRange);

        for (int i = 1; i < path.Count; i++)
        {
            Tile next = path[i];
            if (!next.walkable || next.IsOccupied) break;

            bool ok = enemy.PlaceOnTile(next.gridPos);
            if (!ok) break;

            yield return MoveStepSmooth(enemy, grid.GridToWorld(next.gridPos));

            TryTriggerOverwatch(enemy);

            if (enemy.IsDead) { enemy.SetAnimMoving(false); yield break; }
        }

        enemy.SetAnimMoving(false);
    }

    IEnumerator MoveToBestCover(Unit enemy, Unit target, int moveRange)
    {
        enemy.SetAnimMoving(true);

        Tile start = grid.GetTile(enemy.gridPos);
        if (start == null) { enemy.SetAnimMoving(false); yield break; }

        Tile bestCover = FindBestCoverTile(start, target.gridPos, moveRange);
        if (bestCover == null)
        {
            yield return MoveTowards(enemy, target, moveRange);
            enemy.SetAnimMoving(false);
            yield break;
        }

        List<Tile> path = pathfinder.FindPath(start, bestCover);
        if (path == null || path.Count <= 1) { enemy.SetAnimMoving(false); yield break; }

        path = TrimPathBySteps(path, moveRange);

        for (int i = 1; i < path.Count; i++)
        {
            Tile next = path[i];
            if (!next.walkable || next.IsOccupied) break;

            bool ok = enemy.PlaceOnTile(next.gridPos);
            if (!ok) break;

            yield return MoveStepSmooth(enemy, grid.GridToWorld(next.gridPos));

            TryTriggerOverwatch(enemy);

            if (enemy.IsDead) { enemy.SetAnimMoving(false); yield break; }
        }

        enemy.SetAnimMoving(false);
    }

    IEnumerator RandomStep(Unit enemy, int moveRange)
    {
        enemy.SetAnimMoving(true);

        Tile start = grid.GetTile(enemy.gridPos);
        if (start == null) { enemy.SetAnimMoving(false); yield break; }

        for (int k = 0; k < 20; k++)
        {
            int dx = Random.Range(-moveRange, moveRange + 1);
            int dy = Random.Range(-moveRange, moveRange + 1);

            Vector2Int gp = start.gridPos + new Vector2Int(dx, dy);
            Tile t = grid.GetTile(gp);
            if (t == null) continue;
            if (!t.walkable || t.IsOccupied) continue;

            List<Tile> path = pathfinder.FindPath(start, t);
            if (path == null || path.Count <= 1) continue;

            path = TrimPathBySteps(path, moveRange);

            for (int i = 1; i < path.Count; i++)
            {
                Tile next = path[i];
                if (!next.walkable || next.IsOccupied) break;

                bool ok = enemy.PlaceOnTile(next.gridPos);
                if (!ok) break;

                yield return MoveStepSmooth(enemy, grid.GridToWorld(next.gridPos));

                TryTriggerOverwatch(enemy);

                if (enemy.IsDead) { enemy.SetAnimMoving(false); yield break; }
            }
            break;
        }

        enemy.SetAnimMoving(false);
    }

    IEnumerator MoveStepSmooth(Unit unit, Vector3 endPos)
    {
        Vector3 startPos = unit.transform.position;

        Vector3 dir = (endPos - startPos);
        dir.y = 0f;

        float duration = Mathf.Max(0.01f, stepTime);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            unit.RotateTowards(dir);

            unit.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        unit.transform.position = endPos;
    }

    IEnumerator RotateFor(Unit unit, Vector3 worldDir, float duration)
    {
        if (worldDir == Vector3.zero) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            unit.RotateTowards(worldDir);
            yield return null;
        }
    }

    Tile FindBestApproachTile(Tile start, Vector2Int targetPos, int range)
    {
        List<Tile> reachable = CollectReachable(start, range);
        Tile best = null;
        int bestDist = int.MaxValue;

        foreach (var t in reachable)
        {
            if (t == null || !t.walkable || t.IsOccupied) continue;
            int d = Manhattan(t.gridPos, targetPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    Tile FindBestCoverTile(Tile start, Vector2Int targetPos, int range)
    {
        List<Tile> reachable = CollectReachable(start, range);
        Tile best = null;
        int bestScore = int.MinValue;

        foreach (var t in reachable)
        {
            if (t == null || !t.walkable || t.IsOccupied) continue;

            int coverScore = 0;
            if (t.cover == CoverType.Full) coverScore = 2;
            else if (t.cover == CoverType.Half) coverScore = 1;

            int dist = Manhattan(t.gridPos, targetPos);
            int score = coverScore * 100 - dist;

            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }
        if (best != null && best.cover == CoverType.None) return null;
        return best;
    }

    List<Tile> CollectReachable(Tile start, int range)
    {
        var result = new List<Tile>();
        var q = new Queue<(Tile t, int d)>();
        var visited = new HashSet<Tile>();

        q.Enqueue((start, 0));
        visited.Add(start);

        while (q.Count > 0)
        {
            var (t, d) = q.Dequeue();
            result.Add(t);

            if (d >= range) continue;

            foreach (var n in grid.GetNeighbors(t))
            {
                if (n == null) continue;
                if (visited.Contains(n)) continue;
                visited.Add(n);
                q.Enqueue((n, d + 1));
            }
        }
        return result;
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

    void TryTriggerOverwatch(Unit movingEnemy)
    {
        if (TurnManager.Instance == null) return;

        foreach (var p in TurnManager.Instance.playerUnits)
        {
            if (p == null || p.IsDead) continue;
            if (!p.isOverwatch) continue;
            if (p.overwatchUsedThisRound) continue;

            int dist = Manhattan(p.gridPos, movingEnemy.gridPos);
            if (dist > overwatchRange) continue;

            bool fired = ShootingController.OverwatchFire(p, movingEnemy, grid, overwatchRange);
            if (fired)
            {
                p.overwatchUsedThisRound = true;
                break;
            }
        }
    }
}
