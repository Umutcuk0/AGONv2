using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleResultManager : MonoBehaviour
{
    [Header("Victory Settings")]
    [Tooltip("Düþmanlar bittiðinde geçilecek sahnenin Index numarasý.")]
    public int victorySceneIndex;

    [Tooltip("Sahneler arasý geçiþte görünecek Fade/Animasyon paneli.")]
    public GameObject transitionPanel;

    [Tooltip("Sahne deðiþmeden önce beklenecek süre (saniye).")]
    public float transitionDelay = 1.0f;

    [Header("Defeat Settings")]
    [Tooltip("Tüm oyuncular öldüðünde açýlacak Canvas/Panel.")]
    public GameObject defeatCanvas;

    private bool isGameOver = false;

    private void Start()
    {
        // Oyun baþýnda panellerin kapalý olduðundan kod ile emin oluyoruz
        if (transitionPanel != null)
            transitionPanel.SetActive(false);

        if (defeatCanvas != null)
            defeatCanvas.SetActive(false);
    }

    private void Update()
    {
        // Eðer oyun bittiyse (Victory veya Defeat) Update'i durdur
        if (isGameOver) return;

        // TurnManager hazýr deðilse bekle
        if (TurnManager.Instance == null) return;

        // VICTORY: Düþmanlar bitti
        if (TurnManager.Instance.enemyUnits.Count == 0 && TurnManager.Instance.playerUnits.Count > 0)
        {
            isGameOver = true; // Döngüyü anýnda kýr
            StartCoroutine(HandleVictoryWithTransition());
        }
        // DEFEAT: Oyuncular bitti
        else if (TurnManager.Instance.playerUnits.Count == 0)
        {
            isGameOver = true;
            HandleDefeat();
        }
    }

    private IEnumerator HandleVictoryWithTransition()
    {
        Debug.Log("Victory! Panel açýlýyor...");

        if (transitionPanel != null)
        {
            // Paneli aktif et (Animasyonun 'Play on Awake' seçeneði iþaretli olmalý)
            transitionPanel.SetActive(true);
        }

        // Animasyon süresi (1 sn) boyunca bekle
        yield return new WaitForSeconds(transitionDelay);

        Debug.Log("Süre doldu, sahne yükleniyor...");
        SceneManager.LoadScene(victorySceneIndex);
    }

    private void HandleDefeat()
    {
        Debug.Log("Defeat! Kaybetme ekraný açýlýyor...");
        if (defeatCanvas != null)
        {
            defeatCanvas.SetActive(true);
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}