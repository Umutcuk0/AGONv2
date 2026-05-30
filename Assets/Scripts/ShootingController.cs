using System.Collections;
using UnityEngine;

public class ShootingController : MonoBehaviour
{
    public static ShootingController Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private GridManager grid;

    [Header("Rules")]
    [SerializeField] private int fireAPCost = 1;
    [SerializeField] private int maxRange = 8;

    [Header("LOS (Cover/Obstacle Between)")]
    [SerializeField] private LayerMask coverMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float rayHeight = 1.2f;

    [Header("Debug")]
    [SerializeField] private bool debugLOS = true;
    [SerializeField] private float debugLineDuration = 1.5f;

    [Header("Projectile Visual (NO DAMAGE)")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzleOverride;
    [SerializeField] private float projectileFlightTime = 0.12f;
    [SerializeField] private float missOffsetRadius = 0.6f;
    [SerializeField] private bool destroyProjectileOnArrival = true;

    [Header("Rotate Before Fire")]
    [SerializeField] private float rotateDuration = 0.12f;

    [Tooltip("Karakterin atış anındaki bakış açısını manuel düzeltmek için (Derece cinsinden)")]
    [SerializeField] private float rotationOffset = 0f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
    }

    void Update()
    {
        if (TurnManager.Instance == null) return;
        if (!TurnManager.Instance.IsPlayerTurn) return;

        Unit attacker = TurnManager.Instance.currentUnit;
        if (attacker == null || attacker.IsDead) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f))
            {
                Unit targetUnit = hit.collider.GetComponentInParent<Unit>();
                if (targetUnit == null) return;
                if (targetUnit == attacker) return;
                if (targetUnit.IsDead) return;

                // DOST ATEŞİ ENGELİ: Saldıran ve hedef aynı takımdaysa (İkisi de oyuncu veya ikisi de düşmansa) atışı iptal et
                if (attacker.isPlayerUnit == targetUnit.isPlayerUnit) return;

                StartCoroutine(RotateAndFireRoutine(attacker, targetUnit, fireAPCost, maxRange));
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            attacker.Reload();
        }
    }

    public bool Fire(Unit attacker, Unit target, GridManager grid, int fireAPCost = 1, int maxRange = 8)
    {
        if (attacker == null || target == null) return false;
        if (attacker.IsDead || target.IsDead) return false;

        // DOST ATEŞİ ENGELİ
        if (attacker.isPlayerUnit == target.isPlayerUnit) return false;

        StartCoroutine(RotateAndFireRoutine(attacker, target, fireAPCost, maxRange));
        return true;
    }

    public bool OverwatchFire(Unit attacker, Unit target, GridManager grid, int maxRange = 8)
    {
        if (attacker == null || target == null) return false;
        if (attacker.IsDead || target.IsDead) return false;
        if (attacker.ammo <= 0) return false;

        // DOST ATEŞİ ENGELİ: Pusuya yatan asker kendi arkadaşı önünden geçerken tetiklenmez
        if (attacker.isPlayerUnit == target.isPlayerUnit) return false;

        StartCoroutine(RotateAndOverwatchFireRoutine(attacker, target, maxRange));
        return true;
    }

    IEnumerator RotateAndFireRoutine(Unit attacker, Unit target, int fireAPCost = 1, int maxRange = 8)
    {
        Vector3 dir = (target.transform.position - attacker.transform.position);
        dir.y = 0f;

        if (dir == Vector3.zero) dir = attacker.transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        targetRotation *= Quaternion.Euler(0, rotationOffset, 0);

        float t = 0f;
        Quaternion startRotation = attacker.transform.rotation;

        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / rotateDuration;
            attacker.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, normalizedTime);
            yield return null;
        }

        attacker.transform.rotation = targetRotation;

        bool hit;
        Vector3 endPos;
        bool success = FireCore(attacker, target, grid, fireAPCost, maxRange, out hit, out endPos);
        if (success)
        {
            SpawnProjectileVisual(attacker, endPos);
        }
    }

    IEnumerator RotateAndOverwatchFireRoutine(Unit attacker, Unit target, int maxRange = 8)
    {
        Vector3 dir = (target.transform.position - attacker.transform.position);
        dir.y = 0f;

        if (dir == Vector3.zero) dir = attacker.transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        targetRotation *= Quaternion.Euler(0, rotationOffset, 0);

        float t = 0f;
        Quaternion startRotation = attacker.transform.rotation;

        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = t / rotateDuration;
            attacker.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, normalizedTime);
            yield return null;
        }

        attacker.transform.rotation = targetRotation;

        int coverPenalty;
        bool blocked;
        string debugInfo;
        EvaluateLineOfFire(attacker, target, out coverPenalty, out blocked, out debugInfo);

        if (blocked) yield break;

        int dist = Mathf.Abs(attacker.gridPos.x - target.gridPos.x) + Mathf.Abs(attacker.gridPos.y - target.gridPos.y);
        if (dist > maxRange) yield break;

        int hitChance = attacker.characterClass.aim - coverPenalty;
        hitChance = Mathf.Clamp(hitChance, 5, 95);

        attacker.ammo--;

        int roll = Random.Range(1, 101);
        bool hit = roll <= hitChance;

        if (debugLOS)
            Debug.Log($"[OVERWATCH] {attacker.name} -> {target.name} | Penalty={coverPenalty} | {debugInfo}");

        Vector3 endPos = target.transform.position;
        if (!hit)
        {
            float radius = missOffsetRadius;
            Vector2 r = Random.insideUnitCircle * radius;
            endPos = target.transform.position + new Vector3(r.x, 0f, r.y);
        }

        SpawnProjectileVisual(attacker, endPos);

        if (hit)
            target.TakeDamage(attacker.characterClass.damage);
    }

    bool FireCore(Unit attacker, Unit target, GridManager grid, int fireAPCost, int maxRange, out bool isHit, out Vector3 shotEndWorld)
    {
        isHit = false;
        shotEndWorld = target != null ? target.transform.position : Vector3.zero;

        if (attacker == null || target == null) return false;
        if (attacker.IsDead || target.IsDead) return false;

        if (attacker.ap < fireAPCost) return false;
        if (attacker.ammo <= 0) return false;

        int coverPenalty = 0;
        bool blocked = false;
        string debugInfo = "";
        EvaluateLineOfFire(attacker, target, out coverPenalty, out blocked, out debugInfo);
        if (blocked) return false;

        int dist = Mathf.Abs(attacker.gridPos.x - target.gridPos.x) + Mathf.Abs(attacker.gridPos.y - target.gridPos.y);
        if (dist > maxRange) return false;

        int hitChance = attacker.characterClass.aim - coverPenalty;
        hitChance = Mathf.Clamp(hitChance, 5, 95);

        if (!attacker.SpendAP(fireAPCost)) return false;
        attacker.ammo--;

        int roll = Random.Range(1, 101);
        isHit = roll <= hitChance;

        if (isHit)
        {
            target.TakeDamage(attacker.characterClass.damage);
            shotEndWorld = target.transform.position;
        }
        else
        {
            float radius = missOffsetRadius;
            Vector2 r = Random.insideUnitCircle * radius;
            shotEndWorld = target.transform.position + new Vector3(r.x, 0f, r.y);
        }

        return true;
    }

    void EvaluateLineOfFire(Unit attacker, Unit target, out int coverPenalty, out bool blocked, out string debugInfo)
    {
        coverPenalty = 0;
        blocked = false;

        Vector3 start = attacker.transform.position + Vector3.up * rayHeight;
        Vector3 end = target.transform.position + Vector3.up * rayHeight;

        Vector3 dir = end - start;
        float dist = dir.magnitude;

        if (dist <= 0.01f)
        {
            debugInfo = "Too close";
            return;
        }

        dir /= dist;

        if (Physics.Raycast(start, dir, dist, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            blocked = true;
            if (debugLOS) Debug.DrawLine(start, end, Color.red, debugLineDuration);
            debugInfo = "BLOCKED";
            return;
        }

        RaycastHit[] coverHits = Physics.RaycastAll(start, dir, dist, coverMask, QueryTriggerInteraction.Ignore);
        int best = 0;
        foreach (var h in coverHits)
        {
            CoverMarker marker = h.collider.GetComponentInParent<CoverMarker>();
            if (marker == null) continue;
            if (marker.coverType == CoverType.Full) best = Mathf.Max(best, 50);
            else if (marker.coverType == CoverType.Half) best = Mathf.Max(best, 25);
        }

        coverPenalty = best;
        debugInfo = coverPenalty > 0 ? $"COVER {coverPenalty}" : "CLEAR";
    }

    void SpawnProjectileVisual(Unit attacker, Vector3 endPos)
    {
        if (projectilePrefab == null) return;

        attacker.PlayFireSound();

        UnitAnimator unitAnimator = attacker.GetComponent<UnitAnimator>();
        if (unitAnimator != null)
        {
            unitAnimator.PlayShootAnimation();
        }

        Vector3 startPos;
        if (muzzleOverride != null) startPos = muzzleOverride.position;
        else startPos = attacker.transform.position + Vector3.up * 1.2f;

        GameObject proj = Instantiate(projectilePrefab, startPos, Quaternion.identity);

        Vector3 dir = (endPos - startPos);
        dir.y = 0f;
        if (dir != Vector3.zero)
            proj.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);

        StartCoroutine(MoveProjectile(proj, startPos, endPos, projectileFlightTime));
    }

    IEnumerator MoveProjectile(GameObject proj, Vector3 start, Vector3 end, float flightTime)
    {
        if (proj == null) yield break;

        float duration = Mathf.Max(0.01f, flightTime);
        float t = 0f;

        while (t < 1f && proj != null)
        {
            t += Time.deltaTime / duration;
            proj.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        if (proj != null)
        {
            proj.transform.position = end;
            if (destroyProjectileOnArrival) Destroy(proj);
        }
    }
}