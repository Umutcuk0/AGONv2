using UnityEngine;

public class OverwatchController : MonoBehaviour
{
    [SerializeField] private UnitMovementController mover;
    [SerializeField] private KeyCode overwatchKey = KeyCode.O;
    [SerializeField] private int overwatchAPCost = 1;

    void Start()
    {
        if (mover == null) mover = FindFirstObjectByType<UnitMovementController>();
    }

    void Update()
    {
        if (TurnManager.Instance == null) return;

        if (!TurnManager.Instance.IsPlayerTurn) return;

        Unit u = TurnManager.Instance.currentUnit;
        if (u == null || u.IsDead) return;

        if (Input.GetKeyDown(overwatchKey))
        {
            bool ok = u.EnterOverwatch(overwatchAPCost);
            if (!ok)
            {
                Debug.Log($"{u.name}: AP yetmedi, overwatch yapamaz.");
                return;
            }
            mover?.RefreshHighlight();
        }
    }
}
