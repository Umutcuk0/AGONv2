using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneTransition : MonoBehaviour
{
    [Header("Geçiþ Ayarlarý")]
    [Tooltip("Sahne yüklenmeden önce beklenecek süre (Saniye). Animasyonunun bitiþ süresine göre ayarla.")]
    public float delayTime = 4.0f; // Ekran görüntüsüne göre 4 saniye ideal duruyor

    private void Start()
    {
        // Sahne baþlar baþlamaz sayacý (Coroutine) baþlatýyoruz
        StartCoroutine(WaitAndLoadNextScene());
    }

    private IEnumerator WaitAndLoadNextScene()
    {
        // Belirlenen süre kadar bekle (Kamera animasyonu bu sýrada oynayacak)
        yield return new WaitForSeconds(delayTime);

        // Mevcut sahnenin indeksini alýp 1 ekleyerek sýradakini bul
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Güvenlik kontrolü yap ve sahneyi yükle
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[SÝSTEM] {delayTime} saniye doldu. Kamera hareketi bitti, {nextSceneIndex}. sahneye geçiliyor.");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("[SÝSTEM] Sýradaki sahne yüklenemedi! Build Settings'te son sahnedesiniz.");
        }
    }
}