using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Class Data")]
    public CharacterClassSO characterClass;

    [Header("Overwatch")]
    public bool isOverwatch;
    public bool overwatchUsedThisRound;

    [Header("Runtime")]
    public int hp;
    public int ap;
    public int ammo;

    public Vector2Int gridPos;

    [Header("Grid")]
    [SerializeField] private GridManager grid;

    [Header("Animation Runtime")]
    public bool Anim_IsMoving;

    [Header("Death")]
    [SerializeField] private float destroyDelayAfterDeath = 1f;
    private bool isDying;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 720f; // derece/sn (Inspector’dan ayarlanır)

    public void RotateTowards(Vector3 worldDirection)
    {
        if (worldDirection == Vector3.zero) return;

        // Y ekseninde döndür
        Quaternion targetRot = Quaternion.LookRotation(worldDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }


    private void Awake()
    {
        InitFromClass();
    }

    private void Start()
    {
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
        SnapToGridFromWorld();
    }

    public void InitFromClass()
    {
        if (characterClass == null)
        {
            Debug.LogError($"{name}: CharacterClassSO atanmadı!");
            return;
        }

        hp = characterClass.maxHP;
        ap = characterClass.maxAP;
        ammo = characterClass.maxAmmo;
    }

    public void BeginTurn() => ap = characterClass.maxAP;

    public bool SpendAP(int cost)
    {
        if (ap < cost) return false;
        ap -= cost;
        return true;
    }

    public void SnapToGridFromWorld()
    {
        if (grid == null) return;

        Vector2Int gp = grid.WorldToGrid(transform.position);
        PlaceOnTile(gp);
    }

    public Vector3 GetWorldPos(Vector2Int gp) => grid != null ? grid.GridToWorld(gp) : transform.position;


    public bool PlaceOnTile(Vector2Int targetPos)
    {
        if (grid == null) return false;

        Tile target = grid.GetTile(targetPos);
        if (target == null) return false;
        if (!target.walkable) return false;
        if (target.IsOccupied) return false;

        Tile old = grid.GetTile(gridPos);
        if (old != null && old.occupant == this)
            old.occupant = null;

        gridPos = targetPos;
        target.occupant = this;

        //transform.position = grid.GridToWorld(gridPos);

        return true;
    }

    public bool IsDead => hp <= 0;

    public void TakeDamage(int dmg)
    {
        if (IsDead || isDying) return;

        hp -= dmg;
        if (hp < 0) hp = 0;

        Debug.Log($"{name} took {dmg} dmg. HP={hp}");

        if (IsDead)
        {
            isDying = true;
            Debug.Log($"{name} DIED!");

            // Overwatch, input vs. kapat
            ClearOverwatch();

            // Hareket animasyonu varsa durdur
            Anim_IsMoving = false;

            StartCoroutine(DeathRoutine());
        }
    }

    public bool Reload()
    {
        int cost = 1;
        if (!SpendAP(cost)) return false;

        ammo = characterClass.maxAmmo;
        Debug.Log($"{name} reloaded. Ammo={ammo}");
        return true;
    }

    public bool EnterOverwatch(int apCost = 1)
    {
        isOverwatch = true;
        if (!SpendAP(apCost)) return false;

        isOverwatch = true;
        overwatchUsedThisRound = false;
        Debug.Log($"{name} is now on OVERWATCH");
        return true;
    }

    public void ClearOverwatch()
    {
        isOverwatch= false;
        isOverwatch = false;
        overwatchUsedThisRound = false;
    }

    public void SetAnimMoving(bool moving)
    {
        Anim_IsMoving = moving;
    }

    IEnumerator DeathRoutine()
    {
        // Animator varsa animasyon süresini bul
        float deathAnimLength = 0f;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            // Animator state'e geçmesi için 1 frame bekle
            yield return null;

            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            deathAnimLength = state.length;
        }

        // Animasyon + ekstra delay
        yield return new WaitForSeconds(deathAnimLength + destroyDelayAfterDeath);

        Destroy(gameObject);
    }

}
