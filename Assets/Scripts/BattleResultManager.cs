using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Sahneleri yönetmek için bu kütüphane ÞART!

public class BattleResultManager : MonoBehaviour
{
    public static BattleResultManager Instance;

    [Header("Victory UI")]
    [Tooltip("Düþmanlar bittiðinde otomatik açýlacak Zafer Paneli.")]
    public GameObject victoryPanel;

    [Header("Defeat UI")]
    [Tooltip("Tüm oyuncular öldüðünde otomatik açýlacak Yenilgi Paneli.")]
    public GameObject defeatPanel;

    [Header("Transition Settings")]
    [Tooltip("Arka planda belirecek siyah fade ekraný.")]
    public GameObject fadeTransitionPanel;

    [Tooltip("Paneller otomatik açýlmadan önce beklenecek süre.")]
    public float panelDisplayDelay = 1.0f;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (fadeTransitionPanel != null) fadeTransitionPanel.SetActive(false);
    }

    // Üniteler öldüðü an bu fonksiyonu çaðýracaðýz
    public void OnUnitDied(Unit deadUnit)
    {
        if (isGameOver || TurnManager.Instance == null) return;

        // TurnManager'ýn listelerinden anýnda temizliyoruz ki yok olmasýný (Destroy) beklemeyelim
        if (TurnManager.Instance.playerUnits.Contains(deadUnit))
            TurnManager.Instance.playerUnits.Remove(deadUnit);

        if (TurnManager.Instance.enemyUnits.Contains(deadUnit))
            TurnManager.Instance.enemyUnits.Remove(deadUnit);

        // Kalan canlý sayýlarýna göre durumu kontrol et
        CheckGameCondition();
    }

    private void CheckGameCondition()
    {
        // VICTORY KONTROLÜ
        if (TurnManager.Instance.enemyUnits.Count == 0 && TurnManager.Instance.playerUnits.Count > 0)
        {
            isGameOver = true;
            HideTimelineUI(); // <-- Savaþ bitti, timeline'ý hemen temizle ve gizle
            StartCoroutine(TriggerAutomaticVictory());
        }
        // DEFEAT KONTROLÜ
        else if (TurnManager.Instance.playerUnits.Count == 0)
        {
            isGameOver = true;
            HideTimelineUI(); // <-- Savaþ bitti, timeline'ý hemen temizle ve gizle
            StartCoroutine(TriggerAutomaticDefeat());
        }
    }

    // =================================================================
    // =================================================================
    private void HideTimelineUI()
    {
        TurnOrderUI turnUI = FindFirstObjectByType<TurnOrderUI>();
        if (turnUI != null)
        {
            turnUI.HideTimeline();
        }
        else
        {
            Debug.LogWarning("[BATTLE RESULT] Sahnede TurnOrderUI bulunamadýðý için Timeline gizlenemedi.");
        }
    }

    private IEnumerator TriggerAutomaticVictory()
    {
        Debug.Log("Zafer Þartlarý Saðlandý! Panel hazýrlanýyor...");
        if (fadeTransitionPanel != null) fadeTransitionPanel.SetActive(true);

        yield return new WaitForSeconds(panelDisplayDelay);

        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    private IEnumerator TriggerAutomaticDefeat()
    {
        Debug.Log("Yenilgi Þartlarý Saðlandý! Panel hazýrlanýyor...");
        if (fadeTransitionPanel != null) fadeTransitionPanel.SetActive(true);

        yield return new WaitForSeconds(panelDisplayDelay);

        if (defeatPanel != null) defeatPanel.SetActive(true);
    }

    // =================================================================
    // --- BUTONLAR TARAFINDAN ÇAÐRILACAK YENÝ EKLENEN FONKSÝYONLAR ---
    // =================================================================

    /// <summary>
    /// Yenilgi ekranýndaki "Tekrar Dene / Restart" butonu için geçerli sahneyi yeniden yükler.
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("Level yeniden baþlatýlýyor...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Zafer ekranýndaki "Sonraki Bölüm" butonu için belirtilen sahne indexini yükler.
    /// </summary>
    /// <param name="nextSceneIndex">Gidilmek istenen sahnenin Build Settings'teki numarasý.</param>
    public void LoadNextScene(int nextSceneIndex)
    {
        Debug.Log("Sonraki sahne yükleniyor: " + nextSceneIndex);
        SceneManager.LoadScene(nextSceneIndex);
    }
}