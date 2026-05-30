using System.Collections.Generic;
using UnityEngine;

public class IsoTacticalCamera : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Vector3 pivotOffset = Vector3.zero;
    [SerializeField] private float followSmoothTime = 0.18f;

    [Header("WASD Pan Settings")]
    [SerializeField] private bool enablePan = true;
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float panSmoothTime = 0.1f;
    [SerializeField] private KeyCode returnToActiveUnitKey = KeyCode.Q;

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

    // Q tuþuna basýnca yönün ne kadar sürede (gecikmeyle) sýfýrlanacaðýný belirler. 
    [SerializeField] private float qResetSmoothTime = 0.3f;

    [Header("Cycle Focus (All Units)")]
    [SerializeField] private KeyCode prevUnitKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode nextUnitKey = KeyCode.RightShift;

    [Header("Refs (optional)")]
    [SerializeField] private TurnManager turnManager;

    private Transform focusTarget;
    private Vector3 followVel;
    private float yawVel;

    private int manualIndex = -1;
    private bool manualOverride;
    private Unit lastTurnUnit;

    private Vector3 customPivotPosition;
    private bool isPanningFree;

    private float defaultYaw;
    private float targetYaw;

    void Start()
    {
        if (turnManager == null) turnManager = TurnManager.Instance;

        defaultYaw = yaw;
        targetYaw = yaw;

        FocusTurnUnit(force: true);

        if (focusTarget != null)
            customPivotPosition = focusTarget.position + pivotOffset;
    }

    void LateUpdate()
    {
        if (turnManager == null) turnManager = TurnManager.Instance;
        if (turnManager == null) return;

        if (turnManager.currentUnit != lastTurnUnit)
            FocusTurnUnit(force: true);

        // Q Tuþuna basýldýðýnda
        if (Input.GetKeyDown(returnToActiveUnitKey))
        {
            FocusTurnUnit(force: true);
            targetYaw = defaultYaw;
        }

        if (Input.GetKeyDown(prevUnitKey)) { isPanningFree = false; FocusPrevUnit(); }
        if (Input.GetKeyDown(nextUnitKey)) { isPanningFree = false; FocusNextUnit(); }

        HandlePanInput();
        HandleRotate();

        Vector3 targetPivot;
        if (isPanningFree)
        {
            targetPivot = customPivotPosition;
        }
        else if (focusTarget != null)
        {
            targetPivot = focusTarget.position + pivotOffset;
            customPivotPosition = targetPivot;
        }
        else
        {
            return;
        }

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = targetPivot + rot * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref followVel,
            isPanningFree ? panSmoothTime : followSmoothTime);

        transform.rotation = rot;
    }

    void HandlePanInput()
    {
        if (!enablePan) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
        {
            isPanningFree = true;

            Vector3 camForward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 camRight = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 movement = (camForward * v + camRight * h).normalized * panSpeed * Time.deltaTime;
            customPivotPosition += movement;

            targetYaw = yaw;
        }
    }

    void HandleRotate()
    {
        if (!enableRotate) return;

        float input = 0f;
        if (Input.GetKey(rotateLeftKey)) input -= 1f;
        if (Input.GetKey(rotateRightKey)) input += 1f;

        if (!Mathf.Approximately(input, 0f))
        {
            targetYaw += input * yawRotateSpeed * Time.deltaTime;
        }

        if (!smoothYaw)
        {
            yaw = targetYaw;
            return;
        }

        float currentSmoothTime = Mathf.Approximately(targetYaw, defaultYaw) ? qResetSmoothTime : yawSmoothTime;
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVel, currentSmoothTime);
    }

    void FocusTurnUnit(bool force)
    {
        lastTurnUnit = turnManager.currentUnit;
        if (turnManager.currentUnit == null) return;

        focusTarget = turnManager.currentUnit.transform;
        manualOverride = false;
        isPanningFree = false;

        SyncManualIndexToCurrent();
    }

    // --- YENÝ EKLENEN FONKSÝYON: UI'DAN ÇAÐRILAN ODAKLAMA MANTIÐI ---
    /// <summary>
    /// TurnOrderUI çizelgesinden bir ikona týklandýðýnda kamerayý o üniteye pürüzsüzce odaklar.
    /// </summary>
    public void FocusOnUnitFromUI(Unit targetUnit)
    {
        if (targetUnit == null) return;

        focusTarget = targetUnit.transform;
        isPanningFree = false; // WASD serbest gezinmesini iptal et ve üniteye kilitle
        manualOverride = true;

        // Manuel Shift geçiþ indeksini de bu üniteyle senkronize et ki sistem þaþýrmasýn
        List<Unit> all = GetAllUnits();
        int idx = all.IndexOf(targetUnit);
        if (idx >= 0) manualIndex = idx;

        Debug.Log($"Kamera Odaklandý (UI): {targetUnit.gameObject.name}");
    }
    // ---------------------------------------------------------------

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
        targetYaw = yaw;
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
        targetYaw = yaw;
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