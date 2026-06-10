using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadNextLevel()
    {
        // 1. seviyenin Build Settings'teki adýný veya indeksini yaz
        SceneManager.LoadScene("2vs2");
    }
}