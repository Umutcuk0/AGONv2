using System.Collections.Generic;
using UnityEngine;

public class IsoTacticalCamera : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;
    [SerializeField] private float followSmoothTime = 0.18f;

    [Header("Iso Orbit Settings")]
    [SerializeField] private float distance = 14f;
    [SerializeField] private float pitch = 45f;
    [SerializeField] private float yaw = 45f;

    [SerializeField] private float minPitch = 25f;
    [SerializeField] private float maxPitch = 75f;

    [Header("Rotate Input (optional)")]
    [SerializeField] private bool enableRotate = true;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Z;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.C;
    [SerializeField] private float yawRotateSpeed = 120f;
    [SerializeField] private bool smoothYaw = true;
    [SerializeField] private float yawSmoothTime = 0.12f;

    [Header("Cycle Focus (All Units)")]
    [SerializeField] private KeyCode prevUnitKey = KeyCode.Q;
    [SerializeField] private KeyCode nextUnitKey = KeyCode.E;

    [Header("Refs (optional)")]
    [SerializeField] private TurnManager turnManager;

    private Transform focusTarget;
    private Vector3 followVel;

    private float yawVel;

    private int manualIndex = -1;
    private bool manualOverride;

    private Unit lastTurnUnit;

    void Start()
    {
        if (turnManager == null) turnManager = TurnManager.Instance;

        FocusTurnUnit(force: true);
    }

    void LateUpdate()
    {
        if (turnManager == null) turnManager = TurnManager.Instance;
        if (turnManager == null) return;

        if (turnManager.currentUnit != lastTurnUnit)
            FocusTurnUnit(force: true);

        if (Input.GetKeyDown(prevUnitKey)) FocusPrevUnit();
        if (Input.GetKeyDown(nextUnitKey)) FocusNextUnit();

        HandleRotate();

        if (focusTarget != null)
        {
            Vector3 pivot = focusTarget.position + pivotOffset;

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPos = pivot + rot * new Vector3(0f, 0f, -distance);

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref followVel, followSmoothTime);
            transform.rotation = rot;
        }
    }

    void HandleRotate()
    {
        if (!enableRotate) return;

        float input = 0f;
        if (Input.GetKey(rotateLeftKey)) input -= 1f;
        if (Input.GetKey(rotateRightKey)) input += 1f;
        if (Mathf.Approximately(input, 0f)) return;

        float targetYaw = yaw + input * yawRotateSpeed * Time.deltaTime;

        if (!smoothYaw)
        {
            yaw = targetYaw;
            return;
        }

        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVel, yawSmoothTime);
    }

    void FocusTurnUnit(bool force)
    {
        lastTurnUnit = turnManager.currentUnit;
        if (turnManager.currentUnit == null) return;

        focusTarget = turnManager.currentUnit.transform;

        manualOverride = false;

        SyncManualIndexToCurrent();
    }

    void SyncManualIndexToCurrent()
    {
        List<Unit> all = GetAllUnits();
        if (all.Count == 0) { manualIndex = -1; return; }

        Unit u = turnManager.currentUnit;
        if (u == null) { manualIndex = 0; return; }

        int idx = all.IndexOf(u);
        manualIndex = (idx >= 0) ? idx : 0;
    }

    void FocusPrevUnit()
    {
        List<Unit> all = GetAllUnits();
        if (all.Count == 0) return;

        if (manualIndex < 0) manualIndex = 0;
        manualIndex = (manualIndex - 1 + all.Count) % all.Count;

        Unit u = all[manualIndex];
        if (u == null) return;

        focusTarget = u.transform;
        manualOverride = true;
    }

    void FocusNextUnit()
    {
        List<Unit> all = GetAllUnits();
        if (all.Count == 0) return;

        if (manualIndex < 0) manualIndex = 0;
        manualIndex = (manualIndex + 1) % all.Count;

        Unit u = all[manualIndex];
        if (u == null) return;

        focusTarget = u.transform;
        manualOverride = true;
    }

    List<Unit> GetAllUnits()
    {
        var list = new List<Unit>();

        if (turnManager != null)
        {
            if (turnManager.playerUnits != null)
            {
                foreach (var u in turnManager.playerUnits)
                    if (u != null && !u.IsDead) list.Add(u);
            }

            if (turnManager.enemyUnits != null)
            {
                foreach (var u in turnManager.enemyUnits)
                    if (u != null && !u.IsDead) list.Add(u);
            }
        }

        if (list.Count == 0)
        {
            var all = FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (var u in all)
                if (u != null && !u.IsDead) list.Add(u);
        }

        return list;
    }
}
