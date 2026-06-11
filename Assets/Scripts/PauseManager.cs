using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panelleri")]
    [Tooltip("ESC'ye basýldýðýnda açýlacak olan Kontrol Þemasý (HowToCanvas) objesi.")]
    public GameObject howToCanvas;

    private bool isPaused = false;

    void Start()
    {
        // Oyun baþladýðýnda panelin kapalý olduðundan ve zamanýn normal aktýðýndan emin olalým
        if (howToCanvas != null)
        {
            howToCanvas.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Klavyeden ESC tuþuna basýldýðýnda tetiklenir
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Oyunu durdurma ve paneli açma fonksiyonu
    public void PauseGame()
    {
        if (howToCanvas != null)
        {
            howToCanvas.SetActive(true);
        }
        Time.timeScale = 0f; // Oyun içi zamaný tamamen durdurur
        isPaused = true;
    }

    // Oyuna devam etme ve paneli kapatma fonksiyonu
    public void ResumeGame()
    {
        if (howToCanvas != null)
        {
            howToCanvas.SetActive(false);
        }
        Time.timeScale = 1f; // Zamaný normal hýzýna (1) döndürür
        isPaused = false;
    }
}