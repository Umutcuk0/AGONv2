using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CinematicManager : MonoBehaviour
{
    [Header("Video ve Sahne Ayarlarý")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName; // Geçiþ yapýlacak sahnenin adý
    [SerializeField] private float delayAfterVideo = 1.0f; // Bekleme süresi (Saniye)

    void Awake()
    {
        // Eðer Inspector'dan VideoPlayer atanmadýysa, bu objedekini otomatik bulmaya çalýþ text
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    void OnEnable()
    {
        if (videoPlayer != null)
        {
            // Video bittiðinde tetiklenecek fonksiyonu event'e baðlýyoruz
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnDisable()
    {
        if (videoPlayer != null)
        {
            // Hafýza sýzýntýsýný önlemek için event aboneliðini iptal ediyoruz
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    // Video bittiðinde Unity bu fonksiyonu otomatik çaðýrýr
    private void OnVideoFinished(VideoPlayer source)
    {
        StartCoroutine(LoadSceneWithDelay());
    }

    // Gecikmeli sahne geçiþini saðlayan Coroutine
    private IEnumerator LoadSceneWithDelay()
    {
        // Belirttiðin süre kadar (1 saniye) bekle
        yield return new WaitForSeconds(delayAfterVideo);

        // Yeni sahneyi yükle
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Geçiþ yapýlacak sahne adý (nextSceneName) boþ býrakýlmýþ!");
        }
    }
}