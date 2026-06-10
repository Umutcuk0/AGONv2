using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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

    public void OnUnitDied(Unit deadUnit)
    {
        if (isGameOver || TurnManager.Instance == null) return;

        if (TurnManager.Instance.playerUnits.Contains(deadUnit))
            TurnManager.Instance.playerUnits.Remove(deadUnit);

        if (TurnManager.Instance.enemyUnits.Contains(deadUnit))
            TurnManager.Instance.enemyUnits.Remove(deadUnit);

        CheckGameCondition();
    }

    private void CheckGameCondition()
    {
        // VICTORY KONTROLÜ
        if (TurnManager.Instance.enemyUnits.Count == 0 && TurnManager.Instance.playerUnits.Count > 0)
        {
            isGameOver = true;
            HideTimelineUI();
            StartCoroutine(TriggerAutomaticVictory());
        }
        // DEFEAT KONTROLÜ
        else if (TurnManager.Instance.playerUnits.Count == 0)
        {
            isGameOver = true;
            HideTimelineUI();
            StartCoroutine(TriggerAutomaticDefeat());
        }
    }

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
    // --- BUTONLAR TARAFINDAN ÇAÐRILACAK FONKSÝYONLAR ---
    // =================================================================

    public void RestartLevel()
    {
        Debug.Log("Level yeniden baþlatýlýyor...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// --- DÜZELTÝLDÝ: Artýk elle sayý girilmesine gerek yok! ---
    /// Zafer ekranýndaki butona basýldýðýnda otomatik olarak sýradaki sahneyi bulur ve yükler.
    /// </summary>
    public void LoadNextSceneAutomatically()
    {
        // Mevcut sahnenin numarasýný al
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // Bir sonraki sahnenin numarasýný hesapla
        int nextSceneIndex = currentSceneIndex + 1;

        Debug.Log($"[SÝSTEM] Mevcut Sahne: {currentSceneIndex}. Otomatik olarak {nextSceneIndex}. sahneye geçiliyor!");

        // Sýradaki sahneyi yükle
        SceneManager.LoadScene(nextSceneIndex);
    }
}