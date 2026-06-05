using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitHubManager : MonoBehaviour
{
    public static ExitHubManager Instance { get; private set; }

    [Header("Runtime Verisi")]
    public Vector2Int exitGridPos;

    // Çift tetiklenmeyi önleme kilidi
    private bool isExiting = false;

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
            exitGridPos = grid.WorldToGrid(transform.position);
            Debug.LogWarning($"[EXIT HUB] Çýkýþ Kapýsý Merkez Koordinatý: {exitGridPos}");

            Tile t = grid.GetTile(exitGridPos);
            if (t != null)
            {
                t.walkable = true;
                t.cover = CoverType.None;
            }
        }
    }

    // YÖNTEM 1: Fiziksel Tetikleyici (Ýçinden geçerken)
    private void OnTriggerEnter(Collider other)
    {
        if (isExiting) return;

        Unit unit = other.GetComponentInParent<Unit>();

        // isPlayerUnit tikini unutsan bile Hub'da çalýþmasý için o þartý sildik!
        if (unit != null)
        {
            Debug.LogWarning($"[EXIT TRIGGER] {unit.name} çýkýþ kapýsýna fiziksel olarak dokundu!");
            LoadNextScene();
        }
    }

    // YÖNTEM 2: Yürüme sonu koordinat kontrolü (ESNEK BÖLGE / ZONE MANTIÐI)
    public void CheckAndExitScene(Vector2Int playerGridPos)
    {
        if (isExiting) return;

        // Karakterin durduðu yer ile kapýnýn merkezi arasýndaki uzaklýðý ölçüyoruz
        int distanceX = Mathf.Abs(playerGridPos.x - exitGridPos.x);
        int distanceY = Mathf.Abs(playerGridPos.y - exitGridPos.y);

        // Yeþil alanýmýz büyük olduðu için, merkezin X veya Y ekseninde 
        // 2 kare (tile) yakýnýna kadar her yeri "ÇIKIÞ BÖLGESÝ" sayýyoruz!
        if (distanceX <= 2 && distanceY <= 2)
        {
            Debug.LogWarning($"[EXIT HUB] Karakter geniþletilmiþ çýkýþ bölgesinde durdu! (Karakter: {playerGridPos}, Merkez: {exitGridPos})");
            LoadNextScene();
        }
        else
        {
            Debug.LogWarning($"[EXIT HUB ES GEÇÝLDÝ] Karakter kapýdan çok uzakta. MesafeX: {distanceX}, MesafeY: {distanceY}");
        }
    }

    private void LoadNextScene()
    {
        isExiting = true;

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[EXIT HUB] Sahne Yükleniyor: Index {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("[EXIT HUB] Sonraki sahne Build Settings listesinde YOK! (Lütfen 'File > Build Settings' kýsmýndan sahne ekleyin)");
        }
    }
}