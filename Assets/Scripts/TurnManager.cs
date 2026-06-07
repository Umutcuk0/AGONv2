using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI işlemleri (Fill ve Buton) için ŞART

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

    [Header("UI Elements")]
    public GameObject sniperAimButton;

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

        if (sniperAimButton != null) sniperAimButton.SetActive(false);

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

        u.BeginTurn();

        string side = IsPlayerTurn ? "PLAYER" : "ENEMY";
        Debug.Log($"-- {side} TURN: {u.name} | AP={u.ap}/{u.characterClass.maxAP}");

        FindFirstObjectByType<UnitMovementController>()?.RefreshHighlight();

        // =================================================================
        // 🔥 SNIPER COOLDOWN VE UI DOLUM MANTIĞI 🔥
        // =================================================================
        if (sniperAimButton != null)
        {
            bool isSniper = (u.characterClass != null && u.characterClass.name == "Sniper");
            sniperAimButton.SetActive(IsPlayerTurn && isSniper);

            if (IsPlayerTurn && isSniper)
            {
                // Sahnede SniperMechanic kodunu bul
                SniperMechanic sniperMech = FindFirstObjectByType<SniperMechanic>();

                if (sniperMech != null)
                {
                    // Turu 1 artır
                    sniperMech.IncreaseTurnCharge();

                    Button btn = sniperAimButton.GetComponent<Button>();
                    Image img = sniperAimButton.GetComponent<Image>();

                    if (btn != null)
                    {
                        // Sadece currentTurns 3'e ulaştığında buton tıklanabilir olur
                        btn.interactable = (sniperMech.currentTurns >= sniperMech.requiredTurns);
                    }

                    if (img != null)
                    {
                        // Butonun doluluk oranını hesapla (Örn: 1. tur = 0.33, 2. tur = 0.66, 3. tur = 1.0)
                        img.fillAmount = (float)sniperMech.currentTurns / sniperMech.requiredTurns;
                    }
                }
            }
        }
        // =================================================================

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

        FindFirstObjectByType<TurnOrderUI>()?.UpdateTimeline(turnOrder, turnIndex);
    }

    IEnumerator EnemyActAfterDelay(EnemyMover enemyMover, Unit enemy)
    {
        if (enemyActionDelay > 0f)
            yield return new WaitForSeconds(enemyActionDelay);

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