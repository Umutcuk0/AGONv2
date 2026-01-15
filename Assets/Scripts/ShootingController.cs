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

                StartCoroutine(RotateAndFire(attacker, targetUnit));
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            attacker.Reload();
        }
    }

    public static bool Fire(Unit attacker, Unit target, GridManager grid, int fireAPCost = 1, int maxRange = 8)
    {
        bool hit;
        Vector3 endPos;

        bool success = FireCore(attacker, target, grid, fireAPCost, maxRange, out hit, out endPos);
        if (!success) return false;

        if (Instance != null)
            Instance.SpawnProjectileVisual(attacker, endPos);

        return true;
    }

    public static bool OverwatchFire(Unit attacker, Unit target, GridManager grid, int maxRange = 8)
    {
        if (attacker == null || target == null) return false;
        if (attacker.IsDead || target.IsDead) return false;
        if (attacker.ammo <= 0) return false;

        int coverPenalty;
        bool blocked;
        string debugInfo;
        if (Instance != null)
        {
            Instance.EvaluateLineOfFire(attacker, target, out coverPenalty, out blocked, out debugInfo);
            if (blocked) return false;
        }
        else
        {
            coverPenalty = 0;
            blocked = false;
            debugInfo = "Instance NULL (no LOS debug)";
        }

        int dist = Mathf.Abs(attacker.gridPos.x - target.gridPos.x) + Mathf.Abs(attacker.gridPos.y - target.gridPos.y);
        if (dist > maxRange) return false;

        int hitChance = attacker.characterClass.aim - coverPenalty;
        hitChance = Mathf.Clamp(hitChance, 5, 95);

        attacker.ammo--;

        int roll = Random.Range(1, 101);
        bool hit = roll <= hitChance;

        if (Instance != null && Instance.debugLOS)
            Debug.Log($"[OVERWATCH] {attacker.name} -> {target.name} | Penalty={coverPenalty} | {debugInfo}");

        Vector3 endPos = target.transform.position;
        if (!hit)
        {
            float radius = (Instance != null) ? Instance.missOffsetRadius : 0.6f;
            Vector2 r = Random.insideUnitCircle * radius;
            endPos = target.transform.position + new Vector3(r.x, 0f, r.y);
        }

        if (Instance != null)
            Instance.SpawnProjectileVisual(attacker, endPos);

        Debug.Log($"[OVERWATCH RESULT] Hit%={hitChance} Roll={roll} => {(hit ? "HIT" : "MISS")} | Ammo={attacker.ammo}");

        if (hit)
            target.TakeDamage(attacker.characterClass.damage);

        return true;
    }


    static bool FireCore(Unit attacker, Unit target, GridManager grid, int fireAPCost, int maxRange, out bool isHit, out Vector3 shotEndWorld)
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
        if (Instance != null)
        {
            Instance.EvaluateLineOfFire(attacker, target, out coverPenalty, out blocked, out debugInfo);
            if (blocked) return false;
        }

        int dist = Mathf.Abs(attacker.gridPos.x - target.gridPos.x) + Mathf.Abs(attacker.gridPos.y - target.gridPos.y);
        if (dist > maxRange) return false;

        int hitChance = attacker.characterClass.aim - coverPenalty;
        hitChance = Mathf.Clamp(hitChance, 5, 95);

        if (!attacker.SpendAP(fireAPCost)) return false;
        attacker.ammo--;

        int roll = Random.Range(1, 101);
        isHit = roll <= hitChance;

        if (Instance != null && Instance.debugLOS)
            Debug.Log($"[FIRE] {attacker.name} -> {target.name} | Penalty={coverPenalty} | {debugInfo}");

        if (isHit)
        {
            target.TakeDamage(attacker.characterClass.damage);
            shotEndWorld = target.transform.position;
        }
        else
        {
            float radius = (Instance != null) ? Instance.missOffsetRadius : 0.6f;
            Vector2 r = Random.insideUnitCircle * radius;
            shotEndWorld = target.transform.position + new Vector3(r.x, 0f, r.y);
        }

        Debug.Log($"[FIRE RESULT] Hit%={hitChance} Roll={roll} => {(isHit ? "HIT" : "MISS")} | Ammo={attacker.ammo}");

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

            if (debugLOS)
            {
                Debug.DrawLine(start, end, Color.red, debugLineDuration);
                debugInfo = "BLOCKED by Obstacle";
                Debug.Log($"[LOS] BLOCKED: {attacker.name} -> {target.name}");
            }
            else debugInfo = "BLOCKED";

            return;
        }

        RaycastHit[] coverHits = Physics.RaycastAll(start, dir, dist, coverMask, QueryTriggerInteraction.Ignore);

        int best = 0;
        string bestName = "None";

        foreach (var h in coverHits)
        {
            if (h.collider == null) continue;

            CoverMarker marker = h.collider.GetComponentInParent<CoverMarker>();
            if (marker == null) continue;

            if (marker.coverType == CoverType.Full)
            {
                best = Mathf.Max(best, 50);
                bestName = h.collider.name;
            }
            else if (marker.coverType == CoverType.Half)
            {
                best = Mathf.Max(best, 25);
                if (best < 50) bestName = h.collider.name;
            }
        }

        coverPenalty = best;

        if (debugLOS)
        {
            Color c = Color.green;
            if (coverPenalty == 25) c = Color.yellow;
            else if (coverPenalty == 50) c = new Color(1f, 0.5f, 0f); // turuncu

            Debug.DrawLine(start, end, c, debugLineDuration);

            if (coverPenalty > 0)
                debugInfo = $"COVER penalty={coverPenalty} hit={bestName}";
            else
                debugInfo = "CLEAR (no cover/obstacle between)";
        }
        else
        {
            debugInfo = coverPenalty > 0 ? $"COVER {coverPenalty}" : "CLEAR";
        }
    }

    IEnumerator RotateAndFire(Unit attacker, Unit target)
    {
        if (attacker == null || target == null) yield break;

        Vector3 dir = (target.transform.position - attacker.transform.position);
        dir.y = 0f;

        float t = 0f;
        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            attacker.RotateTowards(dir);
            yield return null;
        }

        Fire(attacker, target, grid, fireAPCost, maxRange);
    }


    void SpawnProjectileVisual(Unit attacker, Vector3 endPos)
    {
        if (projectilePrefab == null) return;

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

            if (destroyProjectileOnArrival)
                Destroy(proj);
        }
    }
}
