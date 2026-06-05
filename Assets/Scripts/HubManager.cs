using UnityEngine;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance { get; private set; }

    [Header("Grid Koordinatý")]
    public Vector2Int upgradeGridPos;

    [Header("Baðlantýlar")]
    [SerializeField] private GameObject exitDoor; // Çýkýþ kapýsý referansý

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GridManager grid = FindFirstObjectByType<GridManager>();
        if (grid != null)
        {
            upgradeGridPos = grid.WorldToGrid(transform.position);

            Tile t = grid.GetTile(upgradeGridPos);
            if (t != null) t.walkable = true;
        }
    }

    public void CheckAndOpenPanel(Vector2Int playerGridPos)
    {
        if (playerGridPos == upgradeGridPos)
        {

            UpgradePanelUI panelUI = FindFirstObjectByType<UpgradePanelUI>(FindObjectsInactive.Include);

            if (panelUI != null)
            {
                panelUI.OpenUpgradePanel();
            }
            else
            {
                Debug.LogError("[KRÝTÝK HATA] Sahnede 'UpgradePanelUI' scriptine sahip hiçbir nesne YOK!");
            }
        }
    }

    public void SelfDestroyAndClearGrid()
    {
        Debug.LogWarning("[HUB] Geliþtirme tamamlandý. HubManager sahneden siliniyor.");

        // --- ÇIKIÞ KAPISI AKTÝVASYONU ---
        // Yeþil küp silinirken çýkýþ kapýsýný (eðer atandýysa) uykudan uyandýrýr!
        if (exitDoor != null)
        {
            exitDoor.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[HUB DÝKKAT] Inspector'dan Çýkýþ Kapýsý (exitDoor) atanmamýþ!");
        }

        if (Instance == this) Instance = null;
        Destroy(gameObject);
    }
}