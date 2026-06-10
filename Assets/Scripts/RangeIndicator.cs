using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    [Header("Menzil Silindiri")]
    [Tooltip("Karakterin altýndaki ezilmiþ 3D Silindir (Cylinder) objesini buraya sürükleyin.")]
    public GameObject rangeCylinderObj;

    [Header("Menzil Ayarlarý")]
    [Tooltip("Bu karakterin vurabileceði kare/birim menzili.")]
    public float attackRange = 5f;

    private void Awake()
    {
        // Oyun baþýnda menzil halkasý kapalý baþlasýn
        // Awake içinde olduðu için TurnManager sýrayý kime verirse versin burasý hep daha önce çalýþýr!
        HideRange();

        // Silindirin boyutunu menzile göre ayarla
        UpdateCylinderScale();
    }

    public void ShowRange()
    {
        if (rangeCylinderObj != null)
        {
            rangeCylinderObj.SetActive(true);
        }
    }

    public void HideRange()
    {
        if (rangeCylinderObj != null)
        {
            rangeCylinderObj.SetActive(false);
        }
    }

    public void UpdateCylinderScale()
    {
        if (rangeCylinderObj != null)
        {
            // Çap = Yarýçap * 2
            float scaleSize = attackRange * 2f;

            // 3D Silindirde X ve Z geniþliði belirler, Y ise kalýnlýðý (0.01f) korur!
            rangeCylinderObj.transform.localScale = new Vector3(scaleSize, 0.01f, scaleSize);
        }
    }
}