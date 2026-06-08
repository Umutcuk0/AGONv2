using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Team Settings")]
    [Tooltip("Oyuncunun kendi askerlerinde bunu TİKLEYİN, düşmanlarda boş bırakın.")]
    public bool isPlayerUnit;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;

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
    [SerializeField] private float destroyDelayAfterDeath = 2.5f;
    private bool isDying;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 720f;

    public void RotateTowards(Vector3 worldDirection)
    {
        if (worldDirection == Vector3.zero) return;
        Quaternion targetRot = Quaternion.LookRotation(worldDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    private void Awake() => InitFromClass();

    private void Start()
    {
        if (grid == null) grid = FindFirstObjectByType<GridManager>();
        SnapToGridFromWorld();
    }

    public void InitFromClass()
    {
        if (characterClass == null) return;
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
        if (target == null || !target.walkable || target.IsOccupied) return false;

        Tile old = grid.GetTile(gridPos);
        if (old != null && old.occupant == this) old.occupant = null;

        gridPos = targetPos;
        target.occupant = this;
        return true;
    }

    public bool IsDead => hp <= 0;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] bodyRenderers;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private DamagePopup damagePopupPrefab;

    public void TakeDamage(int dmg)
    {
        if (IsDead || isDying) return;

        hp -= dmg;
        if (hp < 0) hp = 0;

        if (damagePopupPrefab != null)
        {
            DamagePopup popup = Instantiate(damagePopupPrefab, transform.position + (Vector3.up * 1.5f), Quaternion.identity);
            popup.Setup(dmg);
        }

        StartCoroutine(DamageFlashRoutine());

        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (!IsDead)
        {
            UnitAnimator unitAnimator = GetComponent<UnitAnimator>();
            if (unitAnimator != null)
            {
                unitAnimator.TriggerHitAnimation();
            }
        }

        if (IsDead)
        {
            isDying = true;
            ClearOverwatch();
            SetAnimMoving(false);

            if (BattleResultManager.Instance != null)
            {
                BattleResultManager.Instance.OnUnitDied(this);
            }

            StartCoroutine(DeathRoutine());
        }
    }

    public void SetAnimMoving(bool moving)
    {
        Anim_IsMoving = moving;

        if (audioSource == null || footstepSound == null) return;

        if (moving)
        {
            if (!audioSource.isPlaying || audioSource.clip != footstepSound)
            {
                audioSource.clip = footstepSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.clip == footstepSound)
            {
                audioSource.Stop();
            }
        }
    }

    public bool Reload()
    {
        if (!SpendAP(1)) return false;

        ammo = characterClass.maxAmmo;

        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        return true;
    }

    public bool EnterOverwatch(int apCost = 1)
    {
        if (!SpendAP(apCost)) return false;
        isOverwatch = true;
        overwatchUsedThisRound = false;
        return true;
    }

    public void ClearOverwatch()
    {
        isOverwatch = false;
        overwatchUsedThisRound = false;
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (bodyRenderers == null) yield break;
        Color[] originalColors = new Color[bodyRenderers.Length];
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] != null)
            {
                originalColors[i] = bodyRenderers[i].material.color;
                bodyRenderers[i].material.color = damageFlashColor;
            }
        }
        yield return new WaitForSeconds(flashDuration);
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] != null) bodyRenderers[i].material.color = originalColors[i];
        }
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(destroyDelayAfterDeath);
        Destroy(gameObject);
    }

    public void PlayFireSound()
    {
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    // =================================================================
    // 🔥 HUB GELİŞTİRME ENTEGRASYONU (DİĞER MEKANİKLERİ ETKİLEMEZ) 🔥
    // =================================================================
    public void HandleUpgrade(UpgradeType upgradeType, int amount)
    {
        // Karakterin bağlı olduğu bir sınıf (ScriptableObject) yoksa işlem yapma
        if (characterClass == null) return;

        switch (upgradeType)
        {
            case UpgradeType.MaxHP:
                characterClass.maxHP += amount; // Kalıcı Max HP'yi artır
                ap = characterClass.maxAP;      // (Opsiyonel) Canı fullemek istersen: hp = characterClass.maxHP;
                
                Debug.LogWarning($"[UPGRADE] {gameObject.name} Max HP kazandı! Yeni Max HP: {characterClass.maxHP}");
                
                // EĞER KARAKTERİN ÜSTÜNDE CAN BARI VARSA ONU DA YENİLE
                // HealthBar healthBar = GetComponentInChildren<HealthBar>();
                // if (healthBar != null) healthBar.UpdateHealth(hp, characterClass.maxHP);
                break;

            case UpgradeType.Damage:
                characterClass.damage += amount; // Karakterin hasarını kalıcı artır
                Debug.LogWarning($"[UPGRADE] {gameObject.name} Hasar kazandı! Yeni Hasar: {characterClass.damage}");
                break;

            case UpgradeType.AP:
                characterClass.maxAP += amount; // Karakterin hareket/aksiyon puanını kalıcı artır
                ap += amount; // O anki turda kullanabilsin diye anlık AP'sini de artır
                Debug.LogWarning($"[UPGRADE] {gameObject.name} Max AP kazandı! Yeni Max AP: {characterClass.maxAP}");
                break;
        }
    }

    }