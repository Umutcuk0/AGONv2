using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadNextLevel()
    {
        // Aktif sahnenin build indeksini alýp 1 ekleyerek bir sonraki sahnenin indeksini hesaplýyoruz
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // GÜVENLÝK KONTROLÜ: Eðer hesaplanan indeks Build Settings'teki toplam sahne sayýsýndan küçükse yükle
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[SceneChanger] Diyalog bitti. Otomatik olarak sýradaki sahne yükleniyor. Ýndeks: {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("[SceneChanger] Sýradaki sahne yüklenemedi! Build Settings listesindeki son sahnedesiniz.");
        }
    }
}