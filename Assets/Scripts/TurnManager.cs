using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("Units (drag & drop)")]
    public List<Unit> playerUnits = new();
    public List<Unit> enemyUnits = new();

    [Header("State")]
    public int roundNumber = 1;
    public Unit currentUnit;

    [Header("Enemy Timing")]
    [Tooltip("Enemy turu başlayınca kamera enemy'e kayabilsin diye bekleme (sn).")]
    public float enemyActionDelay = 1.0f;

    private List<Unit> turnOrder = new();
    private int turnIndex = -1;

    private bool waitingForEnemyAction = false;

    private Coroutine enemyDelayRoutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        BuildTurnOrder();
        NextUnitTurn();
    }

    public bool IsPlayerTurn => currentUnit != null && playerUnits.Contains(currentUnit);
    public bool IsEnemyTurn => currentUnit != null && enemyUnits.Contains(currentUnit);

    public void EndCurrentUnitTurn()
    {
        if (waitingForEnemyAction) return;

        NextUnitTurn();
    }

    public void NotifyEnemyFinished()
    {
        waitingForEnemyAction = false;

        if (enemyDelayRoutine != null)
        {
            StopCoroutine(enemyDelayRoutine);
            enemyDelayRoutine = null;
        }

        NextUnitTurn();
    }

    void NextUnitTurn()
    {
        CleanupDeadFromLists();

        if (playerUnits.Count == 0)
        {
            Debug.Log("GAME OVER: All players dead");
            currentUnit = null;
            return;
        }
        if (enemyUnits.Count == 0)
        {
            Debug.Log("VICTORY: All enemies dead");
            currentUnit = null;
            return;
        }

        if (turnOrder.Count == 0 || turnIndex >= turnOrder.Count - 1)
        {
            roundNumber++;
            Debug.Log($"=== ROUND {roundNumber} ===");
            BuildTurnOrder();
            turnIndex = -1;
        }

        currentUnit = null;
        while (currentUnit == null)
        {
            turnIndex++;
            if (turnIndex >= turnOrder.Count)
            {
                roundNumber++;
                Debug.Log($"=== ROUND {roundNumber} ===");
                BuildTurnOrder();
                turnIndex = 0;
            }

            Unit candidate = turnOrder[turnIndex];
            if (candidate != null && !candidate.IsDead)
                currentUnit = candidate;
        }

        StartUnitTurn(currentUnit);
    }

    void StartUnitTurn(Unit u)
    {
        if (u == null) return;

        // AP reset
        u.BeginTurn();

        string side = IsPlayerTurn ? "PLAYER" : "ENEMY";
        Debug.Log($"-- {side} TURN: {u.name} | AP={u.ap}/{u.characterClass.maxAP}");

        // Highlight refresh
        FindFirstObjectByType<UnitMovementController>()?.RefreshHighlight();

        if (IsEnemyTurn)
        {
            var enemyMover = FindFirstObjectByType<EnemyMover>();
            if (enemyMover != null)
            {
                waitingForEnemyAction = true;

                if (enemyDelayRoutine != null)
                {
                    StopCoroutine(enemyDelayRoutine);
                    enemyDelayRoutine = null;
                }

                enemyDelayRoutine = StartCoroutine(EnemyActAfterDelay(enemyMover, u));
            }
            else
            {
                NextUnitTurn();
            }
        }
        else
        {
            waitingForEnemyAction = false;
        }
    }

    IEnumerator EnemyActAfterDelay(EnemyMover enemyMover, Unit enemy)
    {
        if (enemyActionDelay > 0f)
            yield return new WaitForSeconds(enemyActionDelay);

        // Enemy ölmediyse oynat
        if (enemy != null && !enemy.IsDead)
        {
            enemyMover.DoEnemyTurn(enemy);
        }
        else
        {
            NotifyEnemyFinished();
        }

        enemyDelayRoutine = null;
    }

    void BuildTurnOrder()
    {
        CleanupDeadFromLists();
        turnOrder.Clear();

        int i = 0;
        int j = 0;
        while (i < playerUnits.Count || j < enemyUnits.Count)
        {
            if (i < playerUnits.Count) turnOrder.Add(playerUnits[i++]);
            if (j < enemyUnits.Count) turnOrder.Add(enemyUnits[j++]);
        }

        Debug.Log($"TurnOrder built: {turnOrder.Count} units");
    }

    void CleanupDeadFromLists()
    {
        playerUnits.RemoveAll(u => u == null || u.IsDead);
        enemyUnits.RemoveAll(u => u == null || u.IsDead);
    }
}
